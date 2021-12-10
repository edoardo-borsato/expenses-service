namespace ExpensesService.Settings
{
    public record CosmosDb
    {
        public string DatabaseName { get; init; }
        public string ContainerName { get; init; }
        public string Account { get; init; }
        public string Key { get; init; }
    }
}