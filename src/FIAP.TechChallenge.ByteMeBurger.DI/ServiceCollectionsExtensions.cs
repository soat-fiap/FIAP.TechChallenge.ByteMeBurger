﻿using System.Diagnostics.CodeAnalysis;
using FIAP.TechChallenge.ByteMeBurger.Application;
using FIAP.TechChallenge.ByteMeBurger.Controllers;
using FIAP.TechChallenge.ByteMeBurger.Domain.Interfaces;
using FIAP.TechChallenge.ByteMeBurger.FakePayment.Gateway;
using FIAP.TechChallenge.ByteMeBurger.MercadoPago.Gateway;
using FIAP.TechChallenge.ByteMeBurger.Persistence;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.TechChallenge.ByteMeBurger.DI;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionsExtensions
{
    public static void IoCSetup(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.ConfigurePersistenceApp(configuration);
        ConfigurePaymentGateway(serviceCollection);
        ConfigHybridCache(serviceCollection, configuration);
        serviceCollection.AddUseCases();
        serviceCollection.AddControllers();
    }

    private static void ConfigHybridCache(IServiceCollection services, IConfiguration configuration)
    {
        var hybridCacheSettings = configuration.GetSection("HybridCache")
            .Get<HybridCacheEntryOptions>();
        services.AddHybridCache(options =>
            options.DefaultEntryOptions = new HybridCacheEntryOptions()
            {
                Expiration = hybridCacheSettings.Expiration,
                LocalCacheExpiration = hybridCacheSettings.LocalCacheExpiration,
                Flags = hybridCacheSettings.Flags,
            }
        );
    }

    private static void ConfigurePaymentGateway(IServiceCollection services)
    {
        services.AddMercadoPagoGateway();
        services.AddFakePaymentGateway();
        services.AddScoped<IPaymentGatewayFactoryMethod, PaymentGatewayFactory>();
    }
}
