using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace ExpensesService.Utility.CosmosDB
{
    public class ContainerWrapper : IContainer
    {
        private readonly Container _container;

        public ContainerWrapper(Container container)
        {
            _container = container;
        }

        public async Task<IEnumerable<T>> GetItemLinqQueryable<T>(CancellationToken cancellationToken = default)
        {
            var items = new List<T>();
            using (var iterator = _container.GetItemQueryIterator<T>())
            {
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);

                    items.AddRange(response.ToList());
                }
            }

            return items;
        }

        public Task<ItemResponse<T>> ReadItemAsync<T>(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReadItemAsync<T>(id, partitionKey, requestOptions, cancellationToken);
        }

        public Task<ItemResponse<T>> CreateItemAsync<T>(T item, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.CreateItemAsync<T>(item, partitionKey, requestOptions, cancellationToken);
        }

        public Task UpsertItemAsync<T>(T item, PartitionKey? partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.UpsertItemAsync(item, partitionKey, requestOptions, cancellationToken);
        }
    }
}