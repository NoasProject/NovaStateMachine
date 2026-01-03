using UnityEditor;
using UnityEngine;

namespace NovaStateMachine.Extensions
{
    /// <summary>
    /// StateMchineの監視を行うEditorWindow用クラス
    /// </summary>
    public sealed class StateMachineTrackerEditorWindow : EditorWindow
    {
        private const float kLeftPanelWidth = 260f;
        private const float kGridSmall = 20f;
        private const float kGridLarge = 100f;
        private const float kZoomMin = 0.5f;
        private const float kZoomMax = 2.0f;
        private const float kZoomSpeed = 0.03f;

        private const int kNodeTextureWidth = 96;
        private const int kNodeTextureHeight = 32;
        private const int kNodeCornerRadius = 7;
        private const float kNodeShadowOffset = 2f;
        private const int kNodeLabelBaseSize = 10;
        private const int kNodeLabelMinSize = 6;
        private const int kNodeLabelMaxSize = 14;

        private static readonly Color GraphBackground = new Color(0.18f, 0.19f, 0.20f, 1f);
        private static readonly Color GridSmallColor = new Color(1f, 1f, 1f, 0.06f);
        private static readonly Color GridLargeColor = new Color(1f, 1f, 1f, 0.12f);
        private static readonly Color PanelBackground = new Color(0.22f, 0.22f, 0.22f, 1f);
        private static readonly Color NodeShadowColor = new Color(0f, 0f, 0f, 0.35f);

        private Vector2 _leftScroll;
        private Vector2 _graphPan;
        private float _graphZoom = 1f;
        private bool _isPanning;

        private GUIStyle _nodeLabelStyle;
        private GUIStyle _watermarkStyle;

        private Texture2D _anyStateTexture;
        private Texture2D _entryTexture;
        private Texture2D _shadowTexture;

        private StateMachineTracker stateMachineTracker;

        /// <summary>
        /// ウィンドウを開いて初期設定する。
        /// </summary>
        [MenuItem("Tools/NovaStateMachine/State Machine Tracker")]
        public static void Open()
        {
            var window = GetWindow<StateMachineTrackerEditorWindow>();
            window.titleContent = new GUIContent("State Machine Tracker");
            window.minSize = new Vector2(720f, 420f);
            window.Show();
        }

        /// <summary>
        /// エディタ有効化時にリソースを準備する。
        /// </summary>
        private void OnEnable()
        {
            EnsureResources();
        }

        /// <summary>
        /// エディタ無効化時にリソースを破棄する。
        /// </summary>
        private void OnDisable()
        {
            CleanupTextures();
        }

        /// <summary>
        /// ウィンドウ全体のGUIを描画する。
        /// </summary>
        private void OnGUI()
        {
            // StateMachineTrackerのインスタンスを取得
            this.stateMachineTracker ??= StateMachineTracker.Instance;

            EnsureResources();

            var leftRect = new Rect(0f, 0f, kLeftPanelWidth, position.height);
            var graphRect = new Rect(kLeftPanelWidth, 0f, position.width - kLeftPanelWidth, position.height);

            DrawLeftPanel(leftRect);
            DrawGraph(graphRect);
        }

        /// <summary>
        /// 描画に必要なテクスチャとスタイルを確保する。
        /// </summary>
        private void EnsureResources()
        {
            if (_anyStateTexture == null || _entryTexture == null || _shadowTexture == null)
            {
                CleanupTextures();
                _anyStateTexture = CreateRoundedGradientTexture(kNodeTextureWidth, kNodeTextureHeight, kNodeCornerRadius,
                    new Color(0.34f, 0.64f, 0.62f, 1f));
                _entryTexture = CreateRoundedGradientTexture(kNodeTextureWidth, kNodeTextureHeight, kNodeCornerRadius,
                    new Color(0.15f, 0.55f, 0.20f, 1f));
                _shadowTexture = CreateRoundedSolidTexture(kNodeTextureWidth, kNodeTextureHeight, kNodeCornerRadius, NodeShadowColor);
            }

            if (_nodeLabelStyle == null)
            {
                _nodeLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };
                _nodeLabelStyle.normal.textColor = Color.white;
            }

            if (_watermarkStyle == null)
            {
                _watermarkStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.LowerRight,
                    fontSize = 24
                };
                _watermarkStyle.normal.textColor = new Color(1f, 1f, 1f, 0.15f);
            }
        }

        /// <summary>
        /// 生成したテクスチャとスタイルを解放する。
        /// </summary>
        private void CleanupTextures()
        {
            if (_anyStateTexture != null)
            {
                DestroyImmediate(_anyStateTexture);
            }

            if (_entryTexture != null)
            {
                DestroyImmediate(_entryTexture);
            }

            if (_shadowTexture != null)
            {
                DestroyImmediate(_shadowTexture);
            }

            _anyStateTexture = null;
            _entryTexture = null;
            _shadowTexture = null;
            _nodeLabelStyle = null;
        }

        /// <summary>
        /// 左ペインの内容を描画する。
        /// </summary>
        private void DrawLeftPanel(Rect rect)
        {
            EditorGUI.DrawRect(rect, PanelBackground);

            GUILayout.BeginArea(rect);
            GUILayout.Space(8f);

            using (var scroll = new GUILayout.ScrollViewScope(_leftScroll))
            {
                _leftScroll = scroll.scrollPosition;
                GUILayout.Label("New StateMachine", EditorStyles.boldLabel);
                GUILayout.Space(6f);
                GUILayout.Label("- Any State", EditorStyles.label);
                GUILayout.Label("- Entry", EditorStyles.label);
                GUILayout.Label("- Idle", EditorStyles.label);
                GUILayout.Label("- Move", EditorStyles.label);
                GUILayout.Space(12f);
                GUILayout.Label("Mini Map", EditorStyles.miniBoldLabel);
                GUILayout.Box(GUIContent.none, GUILayout.Height(90f), GUILayout.ExpandWidth(true));
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// グラフ領域を描画する。
        /// </summary>
        private void DrawGraph(Rect rect)
        {
            EditorGUI.DrawRect(rect, GraphBackground);
            HandleGraphInput(rect);

            GUI.BeginClip(rect);
            DrawGrid(new Rect(0f, 0f, rect.width, rect.height), kGridSmall, GridSmallColor, _graphPan, _graphZoom);
            DrawGrid(new Rect(0f, 0f, rect.width, rect.height), kGridLarge, GridLargeColor, _graphPan, _graphZoom);

            this.DrawNode();

            var watermarkRect = new Rect(0f, 0f, rect.width - 12f, rect.height - 12f);
            GUI.EndClip();
        }

        private void DrawNode()
        {
            var stateSize = new Vector2(220f, 32f);
            var anyStateRect = new Rect(320f, 120f, stateSize.x, stateSize.y);
            var entryRect = new Rect(320f, 200f, stateSize.x, stateSize.y);

            DrawNode(GraphToScreen(anyStateRect), "Any State", _anyStateTexture, _graphZoom);
            DrawNode(GraphToScreen(entryRect), "Entry", _entryTexture, _graphZoom);            
        }

        /// <summary>
        /// グラフ領域の入力操作を処理する。
        /// </summary>
        private void HandleGraphInput(Rect rect)
        {
            var current = Event.current;
            if (!rect.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseUp && _isPanning)
                {
                    _isPanning = false;
                }
                return;
            }

            if (current.type == EventType.ScrollWheel)
            {
                var localMouse = current.mousePosition - rect.position;
                var zoomDelta = -current.delta.y * kZoomSpeed;
                var newZoom = Mathf.Clamp(_graphZoom + zoomDelta, kZoomMin, kZoomMax);

                if (!Mathf.Approximately(newZoom, _graphZoom))
                {
                    var graphMouse = (localMouse - _graphPan) / _graphZoom;
                    _graphZoom = newZoom;
                    _graphPan = localMouse - graphMouse * _graphZoom;
                    Repaint();
                }

                current.Use();
            }
            else if (current.type == EventType.MouseDown && (current.button == 2 || (current.button == 0 && current.alt)))
            {
                _isPanning = true;
                current.Use();
            }
            else if (current.type == EventType.MouseUp && _isPanning && (current.button == 2 || current.button == 0))
            {
                _isPanning = false;
                current.Use();
            }
            else if (_isPanning && current.type == EventType.MouseDrag)
            {
                _graphPan += current.delta;
                current.Use();
                Repaint();
            }
        }

        /// <summary>
        /// グリッドを描画する。
        /// </summary>
        private static void DrawGrid(Rect rect, float spacing, Color color, Vector2 offset, float zoom)
        {
            Handles.BeginGUI();
            Handles.color = color;

            var scaledSpacing = Mathf.Max(4f, spacing * zoom);
            var widthDivs = Mathf.CeilToInt(rect.width / scaledSpacing);
            var heightDivs = Mathf.CeilToInt(rect.height / scaledSpacing);

            var xOffset = Mathf.Repeat(offset.x, scaledSpacing);
            var yOffset = Mathf.Repeat(offset.y, scaledSpacing);

            for (var i = 0; i <= widthDivs; i++)
            {
                var x = rect.x + scaledSpacing * i + xOffset;
                Handles.DrawLine(new Vector3(x, rect.y, 0f), new Vector3(x, rect.y + rect.height, 0f));
            }

            for (var j = 0; j <= heightDivs; j++)
            {
                var y = rect.y + scaledSpacing * j + yOffset;
                Handles.DrawLine(new Vector3(rect.x, y, 0f), new Vector3(rect.x + rect.width, y, 0f));
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        /// <summary>
        /// ノードの背景とラベルを描画する。
        /// </summary>
        private void DrawNode(Rect rect, string title, Texture2D texture, float zoom)
        {
            if (texture == null || _shadowTexture == null || _nodeLabelStyle == null)
            {
                return;
            }

            var previousColor = GUI.color;
            GUI.color = Color.white;

            var fontSize = Mathf.Clamp(Mathf.RoundToInt(kNodeLabelBaseSize * zoom), kNodeLabelMinSize, kNodeLabelMaxSize);
            if (_nodeLabelStyle.fontSize != fontSize)
            {
                _nodeLabelStyle.fontSize = fontSize;
            }

            var shadowOffset = kNodeShadowOffset * zoom;
            var shadowRect = new Rect(rect.x + shadowOffset, rect.y + shadowOffset, rect.width, rect.height);
            GUI.DrawTexture(shadowRect, _shadowTexture, ScaleMode.StretchToFill, true);
            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
            GUI.Label(rect, title, _nodeLabelStyle);

            GUI.color = previousColor;
        }

        /// <summary>
        /// グラフ座標を画面座標に変換する。
        /// </summary>
        private Rect GraphToScreen(Rect rect)
        {
            return new Rect(
                rect.x * _graphZoom + _graphPan.x,
                rect.y * _graphZoom + _graphPan.y,
                rect.width * _graphZoom,
                rect.height * _graphZoom);
        }

        /// <summary>
        /// 角丸グラデーションのテクスチャを生成する。
        /// </summary>
        private static Texture2D CreateRoundedGradientTexture(int width, int height, int radius, Color baseColor)
        {
            var topColor = Color.Lerp(baseColor, Color.white, 0.22f);
            var bottomColor = Color.Lerp(baseColor, Color.black, 0.25f);
            var borderColor = new Color(0f, 0f, 0f, 0.35f);

            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color[width * height];
            var radiusF = radius - 0.5f;

            for (var y = 0; y < height; y++)
            {
                var t = height > 1 ? (float)y / (height - 1) : 0f;
                var gradient = Color.Lerp(bottomColor, topColor, t);

                for (var x = 0; x < width; x++)
                {
                    var color = IsBorderPixel(x, y, width, height) ? borderColor : gradient;
                    color.a *= ComputeRoundedAlpha(x, y, width, height, radiusF);
                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 角丸単色のテクスチャを生成する。
        /// </summary>
        private static Texture2D CreateRoundedSolidTexture(int width, int height, int radius, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.ARGB32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color[width * height];
            var radiusF = radius - 0.5f;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var shade = color;
                    shade.a *= ComputeRoundedAlpha(x, y, width, height, radiusF);
                    pixels[y * width + x] = shade;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 境界ピクセルか判定する。
        /// </summary>
        private static bool IsBorderPixel(int x, int y, int width, int height)
        {
            return x == 0 || y == 0 || x == width - 1 || y == height - 1;
        }

        /// <summary>
        /// 角丸用のアルファ値を計算する。
        /// </summary>
        private static float ComputeRoundedAlpha(int x, int y, int width, int height, float radius)
        {
            var alpha = 1f;
            var soft = 1f;

            if (x < radius && y < radius)
            {
                alpha = CornerAlpha(x, y, radius, soft);
            }
            else if (x >= width - radius && y < radius)
            {
                alpha = CornerAlpha(width - 1 - x, y, radius, soft);
            }
            else if (x < radius && y >= height - radius)
            {
                alpha = CornerAlpha(x, height - 1 - y, radius, soft);
            }
            else if (x >= width - radius && y >= height - radius)
            {
                alpha = CornerAlpha(width - 1 - x, height - 1 - y, radius, soft);
            }

            return alpha;
        }

        /// <summary>
        /// 角丸角のアルファ値を計算する。
        /// </summary>
        private static float CornerAlpha(float x, float y, float radius, float soft)
        {
            var dx = radius - x - 1f;
            var dy = radius - y - 1f;
            var dist = Mathf.Sqrt(dx * dx + dy * dy);
            if (dist <= radius - soft)
            {
                return 1f;
            }

            return Mathf.Clamp01(radius - dist);
        }
    }
}
