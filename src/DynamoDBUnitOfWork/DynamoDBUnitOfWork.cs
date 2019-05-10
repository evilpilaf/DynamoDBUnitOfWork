using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DynamoDBUnitOfWork
{
    internal sealed class DynamoDBUnitOfWork : IDynamoUnitOfWork
    {
        private const int _maximumTransactionOperations = 10;
        private UoWState _uoWState = UoWState.New;

        private readonly List<TransactWriteItem> _operations;
        private readonly AmazonDynamoDBClient _client;
        private int _trackedOperations;

        public DynamoDBUnitOfWork(AmazonDynamoDBClient client)
        {
            _operations = new List<TransactWriteItem>(_maximumTransactionOperations);
            _trackedOperations = 0;
            _client = client;
        }

        public void AddOperation(TransactWriteItem transactWriteItem)
        {
            if(_uoWState != UoWState.Started)
            {
                throw new Exception();
            }
            if(_trackedOperations>= _maximumTransactionOperations)
            {
                throw new Exception();
            }
            _operations[++_trackedOperations] = transactWriteItem;
        }

        public async Task Commit(string clientRequestToken, CancellationToken cancellationToken = default)
        {
            var request = new TransactWriteItemsRequest
            {
                TransactItems = _operations,
                ClientRequestToken = clientRequestToken
            };
            TransactWriteItemsResponse response = await _client.TransactWriteItemsAsync(request, cancellationToken);
            _uoWState = UoWState.Committed;
        }

        public void Start()
        {
            _uoWState = UoWState.Started;
        }
    }

    internal enum UoWState
    {
        New,
        Started,
        Committed
    }
}
