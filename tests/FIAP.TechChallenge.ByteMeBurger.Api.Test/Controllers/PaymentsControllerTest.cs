using FIAP.TechChallenge.ByteMeBurger.Api.Controllers;
using FIAP.TechChallenge.ByteMeBurger.Api.Model.Payment;
using FIAP.TechChallenge.ByteMeBurger.Controllers;
using FIAP.TechChallenge.ByteMeBurger.Controllers.Contracts;
using FIAP.TechChallenge.ByteMeBurger.Controllers.Dto;
using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;
using FIAP.TechChallenge.ByteMeBurger.Domain.Interfaces;
using FIAP.TechChallenge.ByteMeBurger.Domain.ValueObjects;
using FluentAssertions;
using FluentAssertions.Execution;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using DomainPaymentType = FIAP.TechChallenge.ByteMeBurger.Domain.ValueObjects.PaymentType;
using PaymentType = FIAP.TechChallenge.ByteMeBurger.Api.Model.Payment.PaymentType;

namespace FIAP.TechChallenge.ByteMeBurger.Api.Test.Controllers;

[TestSubject(typeof(PaymentsController))]
public class PaymentsControllerTest
{
    private readonly Mock<IPaymentService> _serviceMock;
    private readonly PaymentsController _target;

    public PaymentsControllerTest()
    {
        _serviceMock = new Mock<IPaymentService>();
        _target = new PaymentsController(_serviceMock.Object, Mock.Of<ILogger<PaymentsController>>());
    }

    [Fact]
    public async void Create_Success()
    {
        // Arrange
        var paymentId = new PaymentId(Guid.NewGuid());
        var payment = new PaymentDto(paymentId.Value, "qrcode", PaymentStatusDto.Pending);
        _serviceMock.Setup(p => p.CreateOrderPaymentAsync(It.IsAny<CreateOrderPaymentRequestDto>()))
            .ReturnsAsync(payment);
        var paymentRequest = new CreatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            PaymentType = PaymentType.Test
        };

        // Act
        var response = await _target.Create(paymentRequest, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            response.Result.Should().BeOfType<CreatedResult>();
            var paymentViewModel = response.Result.As<CreatedResult>().Value.As<PaymentDto>();

            paymentViewModel.Id.Should().Be(paymentId.Value);
            paymentViewModel.QrCode.Should().Be(payment.QrCode);
        }
    }

    [Fact]
    public async void GetStatus_Success()
    {
        // Arrange
        var paymentId = new PaymentId(Guid.NewGuid());
        var payment = new PaymentDto(paymentId.Value, "qrcode", PaymentStatusDto.Approved);
        _serviceMock.Setup(p => p.GetPaymentAsync(It.IsAny<Guid>()))
            .ReturnsAsync(payment);

        // Act
        var response = await _target.GetStatus(paymentId.Value, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            response.Result.Should().BeOfType<OkObjectResult>();
            var status = response.Result.As<OkObjectResult>().Value.As<PaymentStatusDto>();

            status.Should().Be(PaymentStatusDto.Approved);
        }
    }
}
