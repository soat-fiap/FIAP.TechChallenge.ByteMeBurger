using AutoFixture;
using FIAP.TechChallenge.ByteMeBurger.Application.UseCases.Orders;
using FIAP.TechChallenge.ByteMeBurger.Application.UseCases.Payment;
using Bmb.Domain.Core.Interfaces;

namespace FIAP.TechChallenge.ByteMeBurger.Application.Test.UseCases.Payment;

[TestSubject(typeof(UpdatePaymentStatusUseCase))]
public class UpdatePaymentStatusUseCaseTest
{
    private readonly Mock<IUpdateOrderStatusUseCase> _mockUpdateOrderStatusUseCase;
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly UpdatePaymentStatusUseCase _target;

    public UpdatePaymentStatusUseCaseTest()
    {
        _mockUpdateOrderStatusUseCase = new Mock<IUpdateOrderStatusUseCase>();
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _target = new UpdatePaymentStatusUseCase(_mockUpdateOrderStatusUseCase.Object, _mockPaymentRepository.Object);
    }

    [Fact]
    public async void Execute_UpdatePaymentAndOrderStatus_Success()
    {
        // Arrange
        var payment = new Fixture().Create<Bmb.Domain.Core.Entities.Payment>();
        var newStatus = PaymentStatus.Approved;

        _mockPaymentRepository.Setup(p =>
                p.UpdatePaymentStatusAsync(It.Is<Bmb.Domain.Core.Entities.Payment>(x => x.Status == newStatus)))
            .ReturnsAsync(true)
            .Verifiable();

        // Act
        var result = await _target.Execute(payment, newStatus);

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeTrue();
            _mockUpdateOrderStatusUseCase.Verify(uc => uc.Execute(It.IsAny<Guid>(), It.IsAny<OrderStatus>()),
                Times.Once);
        }
    }
}
