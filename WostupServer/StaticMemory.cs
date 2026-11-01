namespace WostupServer
{
    public sealed class StaticMemory
    {
        private StaticMemory() { }

        public static Dictionary<string, string> PendingFiles { get; } = [];
    }
}
