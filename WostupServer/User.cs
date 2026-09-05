namespace WostupServer
{
    public sealed record User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Number { get; set; }
    }
}
