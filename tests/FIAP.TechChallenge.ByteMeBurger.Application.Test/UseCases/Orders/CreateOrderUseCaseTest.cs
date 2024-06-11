// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using AutoFixture.Xunit2;
using FIAP.TechChallenge.ByteMeBurger.Application.UseCases.Orders;
using FIAP.TechChallenge.ByteMeBurger.Domain.Base;
using FIAP.TechChallenge.ByteMeBurger.Domain.Interfaces;

namespace FIAP.TechChallenge.ByteMeBurger.Application.Test.UseCases.Orders;

[TestSubject(typeof(CreateOrderUseCase))]
public class CreateOrderUseCaseTest
{
    private readonly Mock<ICustomerRepository> _customerRepository;
    private readonly Mock<IProductRepository> _productRepository;
    private readonly ICreateOrderUseCase _useCase;
    private readonly Cpf _validCpf = new("863.917.790-23");

    public CreateOrderUseCaseTest()
    {
        _customerRepository = new Mock<ICustomerRepository>();
        _productRepository = new Mock<IProductRepository>();
        _useCase = new CreateOrderUseCase(_productRepository.Object,
            _customerRepository.Object);
    }

    [Theory]
    [AutoData]
    public async Task Checkout_Success(SelectedProduct selectedProduct)
    {
        // Arrange
        var product = new Product(selectedProduct.ProductId, "product", "description", ProductCategory.Drink, 10, []);
        var expectedCustomer = new Customer(Guid.NewGuid(), _validCpf, "customer", "customer@email.com");
        var expectedOrder = new Order(expectedCustomer);
        expectedOrder.AddOrderItem(selectedProduct.ProductId, product.Name, product.Price, selectedProduct.Quantity);

        expectedOrder.Create();

        _customerRepository.Setup(r => r.FindByCpfAsync(
                It.Is<string>(cpf => cpf == _validCpf.Value)))
            .ReturnsAsync(expectedCustomer);

        _productRepository.Setup(r => r.FindByIdAsync(
                It.IsAny<Guid>()))
            .ReturnsAsync(product);

        // Act
        var result = await _useCase.Execute(_validCpf, [selectedProduct]);

        // Assert
        using (new AssertionScope())
        {
            _customerRepository.Verify(m => m.FindByCpfAsync(
                It.IsAny<string>()), Times.Once);

            _productRepository.Verify(m => m.FindByIdAsync(
                It.IsAny<Guid>()), Times.AtLeastOnce);
        }
    }

    [Theory]
    [InlineAutoData]
    public async Task Checkout_CustomerNotFound_Error(List<SelectedProduct> selectedProducts)
    {
        // Arrange
        var expectedCustomer = new Customer(Guid.NewGuid(), _validCpf, "customer", "customer@email.com");
        var expectedOrder = new Order(expectedCustomer);
        selectedProducts.ForEach(i => { expectedOrder.AddOrderItem(i.ProductId, "productName", 1, i.Quantity); });
        expectedOrder.Create();

        _customerRepository.Setup(r => r.FindByCpfAsync(
                It.Is<string>(cpf => cpf == _validCpf.Value)))
            .ReturnsAsync(default(Customer));

        // Act
        var func = async () => await _useCase.Execute(_validCpf, selectedProducts);

        // Assert
        using (new AssertionScope())
        {
            (await func.Should().ThrowExactlyAsync<EntityNotFoundException>())
                .And
                .Message
                .Should()
                .Be("Customer not found.");

            // _orderRepository.Verify(m => m.CreateAsync(
            //     It.IsAny<Order>()), Times.Never);

            _customerRepository.Verify(m => m.FindByCpfAsync(
                It.IsAny<string>()), Times.Once);

            _productRepository.Verify(m => m.FindByIdAsync(
                It.IsAny<Guid>()), Times.Never);
        }
    }

    [Theory]
    [InlineAutoData]
    public async Task Checkout_ProductNotFound_Error(List<SelectedProduct> selectedProducts)
    {
        // Arrange
        var expectedCustomer = new Customer(Guid.NewGuid(), _validCpf, "customer", "customer@email.com");
        var expectedOrder = new Order(expectedCustomer);
        selectedProducts.ForEach(i => { expectedOrder.AddOrderItem(i.ProductId, "productName", 2, i.Quantity); });
        expectedOrder.Create();

        _customerRepository.Setup(r => r.FindByCpfAsync(
                It.Is<string>(cpf => cpf == _validCpf.Value)))
            .ReturnsAsync(expectedCustomer);

        _productRepository.Setup(r => r.FindByIdAsync(
                It.IsAny<Guid>()))
            .ReturnsAsync(default(Product));

        // Act
        var func = async () => await _useCase.Execute(_validCpf, selectedProducts);

        // Assert
        using (new AssertionScope())
        {
            (await func.Should().ThrowExactlyAsync<EntityNotFoundException>())
                .And
                .Message
                .Should()
                .Be($"Product '{selectedProducts.First().ProductId}' not found.");

            // _orderRepository.Verify(m => m.CreateAsync(
            //     It.IsAny<Order>()), Times.Never);

            _customerRepository.Verify(m => m.FindByCpfAsync(
                It.IsAny<string>()), Times.Once);

            _productRepository.Verify(m => m.FindByIdAsync(
                It.IsAny<Guid>()), Times.Once);
        }
    }
}
