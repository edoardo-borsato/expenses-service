namespace ExpensesService.Utility.CosmosDB
{
    public interface ICosmosClient
    {
        IContainer GetContainer(string databaseId, string containerId);
    }
}