using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace ExpensesService.Utility.CosmosDB
{
    public interface IContainer
    {
        Task<IEnumerable<T>> GetItemLinqQueryable<T>(CancellationToken cancellationToken = default);
        Task<T> ReadItemAsync<T>(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task CreateItemAsync<T>(T item, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task UpsertItemAsync<T>(T item, PartitionKey? partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
        Task DeleteItemAsync<T>(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default);
    }

    public class ContainerException : Exception
    {
        public ContainerException(string message) : base(message)
        {
        }
    }
}