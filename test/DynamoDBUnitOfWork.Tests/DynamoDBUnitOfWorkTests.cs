using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using DynamoDBUnitOfWork.Exceptions;
using FluentAssertions;
using Moq;
using System;
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
        public void AddOperation_WhenOperationLimitReached_ThrowsException()
        {
            var sut = CreateSut();

            sut.Start();
            for (var i = 0; i < Constants.MaximumTransactionOperations; i++)
            {
                sut.AddOperation(Mock.Of<TransactWriteItem>());
            }
            Action action = () => sut.AddOperation(Mock.Of<TransactWriteItem>());
            action.Should().Throw<Exception>();
        }

        private IDynamoUnitOfWork CreateSut() => new DynamoDBUnitOfWork(_dynamoDBClientMock.Object);
    }
}