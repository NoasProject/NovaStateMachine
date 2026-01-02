using System;
using System.Collections.Generic;
using System.Linq;
using NovaStateMachine.TrafficSignalExsample;

namespace NovaStateMachine.TrafficSignalExsample
{
    /// <summary>
    /// 信号の種類
    /// </summary>
    public enum TrafficSignalType
    {
        None = 0,
        Green,  // 緑
        Yellow, // 黄
        Red,    // 赤
    }
}
