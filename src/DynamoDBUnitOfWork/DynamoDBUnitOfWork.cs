using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DynamoDBUnitOfWork
{
    public sealed class DynamoDBUnitOfWork : IDynamoUnitOfWork
    {
        private readonly AmazonDynamoDBClient _client;

        private const int _maximumTransactionOperations = 10;
        private List<TransactWriteItem> _operations;
        private UoWState _uoWState;
        private int _trackedOperations;

        public DynamoDBUnitOfWork(AmazonDynamoDBClient client)
        {
            _uoWState = UoWState.New;
            _client = client;
        }

        public void AddOperation(TransactWriteItem transactWriteItem)
        {
            if (_uoWState != UoWState.Started)
            {
                throw new Exception();
            }
            if (_trackedOperations >= _maximumTransactionOperations)
            {
                throw new Exception();
            }
            _operations[++_trackedOperations] = transactWriteItem;
        }

        public async Task<TransactWriteItemsResponse> Commit(string clientRequestToken, CancellationToken cancellationToken = default)
        {
            var request = new TransactWriteItemsRequest
            {
                TransactItems = _operations,
                ClientRequestToken = clientRequestToken
            };
            _uoWState = UoWState.Committed;
            return await _client.TransactWriteItemsAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public void Start()
        {
            _operations = new List<TransactWriteItem>(_maximumTransactionOperations);
            _uoWState = UoWState.Started;
            _trackedOperations = 0;
        }
    }

    internal enum UoWState
    {
        New,
        Started,
        Committed
    }
}
