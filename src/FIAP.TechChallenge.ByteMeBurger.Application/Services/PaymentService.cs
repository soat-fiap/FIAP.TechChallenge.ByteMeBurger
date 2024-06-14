// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using FIAP.TechChallenge.ByteMeBurger.Application.UseCases.Payment;
using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;
using FIAP.TechChallenge.ByteMeBurger.Domain.Interfaces;

namespace FIAP.TechChallenge.ByteMeBurger.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly ICreatePaymentUseCase _createOrderUseCase;
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(ICreatePaymentUseCase createOrderUseCase,
        IPaymentRepository paymentRepository)
    {
        _createOrderUseCase = createOrderUseCase;
        _paymentRepository = paymentRepository;
    }

    public async Task<Payment> CreateOrderPaymentAsync(Guid orderId)
    {
        var payment = await _createOrderUseCase.Execute(orderId);

        await _paymentRepository.SaveAsync(payment);
        return payment;
    }
}