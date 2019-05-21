using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBUnitOfWork.Exceptions;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DynamoDBUnitOfWork.Tests
{
    public sealed class DynamoDBUnitOfWorkTests
    {
        private readonly Mock<AmazonDynamoDBClient> _dynamoDBClientMock = new Mock<AmazonDynamoDBClient>();

        [Fact]
        public void AddOperation_WhenUoWNotStarted_ThrowsException()
        {
            var sut = CreateSut();
            Action action = () => sut.AddOperation(Mock.Of<TransactWriteItem>());

            action.Should().Throw<UnitOfWorkNotStartedException>();
        }

        [Fact]
        public async Task Commit_WhenUoWNotStarted_ThrowsExceptionAsync()
        {
            const string clientRequestToken = nameof(clientRequestToken);

            var sut = CreateSut();
            Func<Task> action = () => sut.Commit(clientRequestToken);

            await action.Should().ThrowAsync<UnitOfWorkNotStartedException>();
        }

        [Fact]
        public void AddOperation_WhenOperationLimitReached_ThrowsException()
        {
            var sut = CreateSut();

            sut.Start();
            for (var i = 0; i < Constants.MaximumTransactionOperations; i++)
            {
                sut.AddOperation(Mock.Of<TransactWriteItem>());
            }
            Action action = () => sut.AddOperation(Mock.Of<TransactWriteItem>());
            action.Should().Throw<MaximumTransactionOperationsException>();
        }

        private IDynamoUnitOfWork CreateSut() => new DynamoDBUnitOfWork(_dynamoDBClientMock.Object);
    }
}