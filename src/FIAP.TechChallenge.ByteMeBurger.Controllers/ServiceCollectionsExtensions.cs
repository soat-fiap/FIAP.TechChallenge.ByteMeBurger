﻿using System.Diagnostics.CodeAnalysis;
using FIAP.TechChallenge.ByteMeBurger.Application.DomainServices;
using FIAP.TechChallenge.ByteMeBurger.Controllers.Contracts;
using Bmb.Domain.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.TechChallenge.ByteMeBurger.Controllers;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionsExtensions
{
    public static void AddControllers(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>()
            .AddScoped<IProductService, ProductService>()
            .AddScoped<IOrderService, OrderService>()
            .AddScoped<IOrderTrackingCodeService, OrderTrackingCodeService>()
            .AddScoped<IPaymentService, PaymentService>();
    }
}
