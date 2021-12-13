namespace ExpensesService.Settings
{
    public record Authentication
    {
        public string Username { get; init; }
        public string Password { get; init; }
    }
}