namespace Screenplay.Nodes
{
    public struct Signal
    {
        public DelayType Type { get; private set; }
        public IAction? Action { get; private set; }


        public enum DelayType
        {
            NextFrame,
            None,
            SwapToAction,
            SoftBreak
        }

        public static Signal NextFrame => new(){ Type = DelayType.NextFrame };
        public static Signal BreakInto(IAction? action) => new() { Type = action == null ? DelayType.SoftBreak : DelayType.SwapToAction, Action = action };
    }
}
