namespace WostupServer
{
    public sealed record RawBytesChunkRequest
    {
        public required List<byte> Bytes { get; set; }
        public required bool IsLast { get; set; }
        public required string Uuid { get; set; }
        public required string UserName { get; set; }
        public required string UserNumber { get; set; }
        public required string ServerDestPath { get; set; }
    }
}
