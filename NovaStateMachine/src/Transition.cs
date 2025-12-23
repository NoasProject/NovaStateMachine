namespace NovaStateMachine
{
    public partial class StateMachine
    {
        private sealed class Transition
        {
            public string From { get; }
            public string To { get; }

            public Transition(string from, string to)
            {
                this.From = from;
                this.To = to;
            }
        }
    }
}
