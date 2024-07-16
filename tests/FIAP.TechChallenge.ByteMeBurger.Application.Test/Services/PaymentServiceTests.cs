// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using AutoFixture;
using FIAP.TechChallenge.ByteMeBurger.Application.Controllers;
using FIAP.TechChallenge.ByteMeBurger.Application.UseCases.Orders;
using FIAP.TechChallenge.ByteMeBurger.Application.UseCases.Payment;
using FIAP.TechChallenge.ByteMeBurger.Domain.Interfaces;

namespace FIAP.TechChallenge.ByteMeBurger.Application.Test.Services;

[TestSubject(typeof(PaymentService))]
public class PaymentServiceTests
{
    private readonly Mock<ICreatePaymentUseCase> _mockCreatePaymentUseCase;
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly Mock<IUpdatePaymentStatusUseCase> _mockUpdatePaymentStatusUseCase;
    private readonly Mock<IPaymentGateway> _mockPaymentGateway;
    private readonly Mock<IUpdateOrderPaymentUseCase> _mockUpdateOrderPaymentUseCase;
    private readonly PaymentService _target;

    public PaymentServiceTests()
    {
        _mockCreatePaymentUseCase = new Mock<ICreatePaymentUseCase>();
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockUpdatePaymentStatusUseCase = new Mock<IUpdatePaymentStatusUseCase>();
        _mockPaymentGateway = new Mock<IPaymentGateway>();
        _mockUpdateOrderPaymentUseCase = new Mock<IUpdateOrderPaymentUseCase>();
        Mock<IPaymentGatewayFactoryMethod> paymentGatewayFactory = new();

        paymentGatewayFactory.Setup(g => g.Create(It.IsAny<PaymentType>()))
            .Returns(_mockPaymentGateway.Object);

        _target = new PaymentService(_mockCreatePaymentUseCase.Object, _mockUpdatePaymentStatusUseCase.Object,
            _mockPaymentRepository.Object, _mockUpdateOrderPaymentUseCase.Object, paymentGatewayFactory.Object);
    }

    [Fact]
    public async Task CreateOrderPaymentAsync_Success()
    {
        // Arrange
        _mockCreatePaymentUseCase.Setup(p => p.Execute(It.IsAny<Guid>(), It.IsAny<PaymentType>()))
            .ReturnsAsync(new Fixture().Create<Payment>());
        var createOrderPaymentRequestDto = new CreateOrderPaymentRequestDto(Guid.NewGuid(), PaymentType.Test);

        // Act
        var result = await _target.CreateOrderPaymentAsync(createOrderPaymentRequestDto);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            _mockPaymentRepository.Verify(r => r.SaveAsync(It.IsAny<Payment>()), Times.Once);
        }
    }

    [Fact]
    public async Task GetPaymentAsync_Success()
    {
        // Arrange
        var expectedPayment = new Fixture().Create<Payment>();
        _mockPaymentRepository.Setup(p => p.GetPaymentAsync(It.IsAny<PaymentId>()))
            .ReturnsAsync(expectedPayment)
            .Verifiable();

        // Act
        var result = await _target.GetPaymentAsync(expectedPayment.Id);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().Be(expectedPayment);
            _mockPaymentRepository.Verify();
        }
    }

    [Fact]
    public async Task SyncPaymentStatusWithGatewayAsync_Success()
    {
        // Arrange
        var expectedPayment = new Fixture().Create<Payment>();
        _mockPaymentRepository.Setup(p => p.GetPaymentAsync(It.IsAny<string>(), It.IsAny<PaymentType>()))
            .ReturnsAsync(expectedPayment)
            .Verifiable();

        _mockPaymentGateway.Setup(p => p.GetPaymentStatusAsync(expectedPayment.ExternalReference))
            .ReturnsAsync(PaymentStatus.Approved)
            .Verifiable();

        _mockUpdatePaymentStatusUseCase.Setup(p => p.Execute(expectedPayment, PaymentStatus.Approved))
            .Returns(Task.FromResult(true))
            .Verifiable();

        // Act
        var result = await _target.SyncPaymentStatusWithGatewayAsync(expectedPayment.ExternalReference, PaymentType.MercadoPago);

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeTrue();
            _mockPaymentRepository.Verify();
            _mockPaymentGateway.Verify();
            _mockUpdatePaymentStatusUseCase.Verify();
        }
    }
}
