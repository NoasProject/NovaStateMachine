namespace NovaStateMachine
{
    internal sealed class TransitionIdentity
    {
        public const string kDefaultName = "__to__next__";

        public string From { get; }
        public string To { get; }
        public string Name { get; }
        public Action Callback { get; }

        public TransitionIdentity(string name, string from, string to, Action callback)
        {
            this.Name = name;
            this.From = from;
            this.To = to;
            this.Callback = callback;
        }
    }
}
