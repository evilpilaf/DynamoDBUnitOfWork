using Amazon.DynamoDBv2.Model;
using System.Threading;
using System.Threading.Tasks;

namespace DynamoDBUnitOfWork
{
    public interface IDynamoUnitOfWork
    {
        void Start();
        Task Commit(CancellationToken cancellationToken = CancellationToken.None);
        void AddOperation(TransactWriteItem transactWriteItem);
    }
}
