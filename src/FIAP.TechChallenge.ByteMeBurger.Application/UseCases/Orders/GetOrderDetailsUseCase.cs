using Bmb.Domain.Core.Entities;
using Bmb.Domain.Core.Interfaces;
using Bmb.Orders.Domain.Contracts;
using Bmb.Orders.Domain.Entities;

namespace FIAP.TechChallenge.ByteMeBurger.Application.UseCases.Orders;

public class GetOrderDetailsUseCase(IOrderRepository repository, ICustomerRepository customerRepository)
    : IGetOrderDetailsUseCase
{
    public async Task<Order?> Execute(Guid id)
    {
        var order = await GetOrder(id);
        if (order is null) return null;

        if (order.Customer is not null)
            order.SetCustomer(await GetCustomer(order.Customer.Id)!);

        return order;
    }

    private async Task<Order?> GetOrder(Guid id) => id == Guid.Empty ? null : await repository.GetAsync(id);

    private Task<Customer?> GetCustomer(Guid id) => customerRepository.FindByIdAsync(id);
}
