using FIAP.TechChallenge.ByteMeBurger.Domain.Base;
using FIAP.TechChallenge.ByteMeBurger.Domain.Events;
using FIAP.TechChallenge.ByteMeBurger.Domain.Interfaces;
using FIAP.TechChallenge.ByteMeBurger.Domain.ValueObjects;

namespace FIAP.TechChallenge.ByteMeBurger.Application.UseCases.Payment;

public class CreatePaymentUseCase : ICreatePaymentUseCase
{
    private readonly IPaymentGatewayFactoryMethod _paymentGatewayFactory;
    private readonly IOrderRepository _orderRepository;

    public CreatePaymentUseCase(IPaymentGatewayFactoryMethod paymentGatewayFactory, IOrderRepository orderRepository)
    {
        _paymentGatewayFactory = paymentGatewayFactory;
        _orderRepository = orderRepository;
    }

    public async Task<Domain.Entities.Payment?> Execute(Guid orderId, PaymentType paymentType)
    {
        var order = await _orderRepository.GetAsync(orderId);

        if (order is null)
            throw new EntityNotFoundException("Order not found.");

        if (order.PaymentId is not null)
            throw new DomainException("There's already a Payment for the order.");

        var paymentGateway = _paymentGatewayFactory.Create(paymentType);
        var payment = await paymentGateway.CreatePaymentAsync(order);
        DomainEventTrigger.RaisePaymentCreated(new PaymentCreated(payment));
        return payment;
    }
}
