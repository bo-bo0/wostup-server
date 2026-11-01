namespace WostupServer
{
    public sealed record RawBytesChunkRequest
    {
        public required List<byte> Bytes;
        public required bool IsLast;
        public required string Uuid;
        public required string UserName;
        public required string UserNumber;
        public required string ServerDestPath;
    }
}
