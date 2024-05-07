using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;

namespace FIAP.TechChallenge.ByteMeBurger.Domain.Events;

/// <summary>
/// Payment confirmed event
/// </summary>
/// <param name="payload"></param>
public class OrderPaymentConfirmed(Order payload) : DomainEvent<Order>(payload);