using Microsoft.Azure.Cosmos;

namespace ExpensesService.Utility.CosmosDB
{
    public class CosmosClientWrapper : ICosmosClient
    {
        private readonly CosmosClient _client;

        public CosmosClientWrapper(CosmosClient client)
        {
            _client = client;
        }

        public IContainer GetContainer(string databaseId, string containerId)
        {
            var container = _client.GetContainer(databaseId, containerId);
            return new ContainerWrapper(container);
        }
    }
}