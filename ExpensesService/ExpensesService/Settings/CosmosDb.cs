namespace ExpensesService.Settings
{
    public record CosmosDb
    {
        public string DatabaseName { get; init; }
        public string ContainerName { get; init; }
        public string AccountEndpoint { get; init; }
        public string Key { get; init; }
    }
}