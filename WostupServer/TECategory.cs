namespace WostupServer
{
    public sealed record TECategory
    {
        public required bool MediaRelativeClientPath { get; set; }
        public required string ClientSourcePath { get; set; }
        public required int Limit { get; set; }
        public required string Sort { get; set; }
        public required string ServerDestPath { get; set; }
    }
}
