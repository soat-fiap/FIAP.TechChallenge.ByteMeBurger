// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using System.Data;
using Dapper;
using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;
using FIAP.TechChallenge.ByteMeBurger.Domain.ValueObjects;
using FIAP.TechChallenge.ByteMeBurger.Infrastructure.Dto;
using FIAP.TechChallenge.ByteMeBurger.Infrastructure.Repository;
using FluentAssertions;
using FluentAssertions.Execution;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Dapper;

namespace FIAP.TechChallenge.ByteMeBurger.Infrastructure.Test.Repository;

[TestSubject(typeof(OrderRepositoryDapper))]
public class OrderRepositoryDapperTest
{
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly OrderRepositoryDapper _target;

    public OrderRepositoryDapperTest()
    {
        _mockConnection = new Mock<IDbConnection>();
        _target = new OrderRepositoryDapper(_mockConnection.Object, Mock.Of<ILogger<OrderRepositoryDapper>>());
    }

    [Fact(Skip = "I'll double check it later")]
    public async Task GetAll_Success()
    {
        // Arrange
        var order = new Order(Guid.NewGuid());
        order.AddOrderItem(Guid.NewGuid(), "banana", 10, 1);
        order.Create();
        order.ConfirmPayment();

        var expected = new[]
        {
            order
        };

        _mockConnection.SetupDapperAsync(c => c.QueryAsync(
                It.IsAny<string>(), It.IsAny<Func<OrderListDto, CustomerDto, Order>>(), null, null, false,
                "Id", null, null))
            .ReturnsAsync(expected);

        // Act
        var result = await _target.GetAllAsync();

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().BeEquivalentTo(expected);
        }
    }

    [Fact]
    public async Task Create_Success()
    {
        // Arrange
        var order = new Order(Guid.NewGuid());
        order.AddOrderItem(Guid.NewGuid(), "banana", 10, 1);
        order.Create();

        _mockConnection.Setup(c => c.BeginTransaction()).Returns(Mock.Of<IDbTransaction>());

        _mockConnection.SetupDapperAsync(c =>
                c.ExecuteAsync(
                    "insert into Orders (Id, CustomerId, Status, Created) values (@Id, @CustomerId, @Status, @Created);",
                    null, null, null, null))
            .ReturnsAsync(1);
        _mockConnection.SetupDapperAsync(c =>
                c.ExecuteAsync(
                    "insert into OrderItems (OrderId, ProductId, ProductName, UnitPrice, Quantity) values (@OrderId, @ProductId, @ProductName, @UnitPrice, @Quantity);",
                    null, null, null, null))
            .ReturnsAsync(1);

        // Act
        var result = await _target.CreateAsync(order);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Be(order);
        }
    }
}
