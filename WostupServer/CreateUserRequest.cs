namespace WostupServer
{
    public sealed record CreateUserRequest
    {
        public required string Name { get; set; }
        public required string Number { get; set; }
    }
}
