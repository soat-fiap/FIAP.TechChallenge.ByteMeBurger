// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using FIAP.TechChallenge.ByteMeBurger.MercadoPago.Gateway.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FIAP.TechChallenge.ByteMeBurger.Api.Auth;

/// <summary>
/// Validate Mercado Pago Webhook message
/// </summary>
public class MercadoPagoMessageAuthorizationFilter : IAuthorizationFilter
{
    private readonly ILogger<MercadoPagoMessageAuthorizationFilter> _logger;
    private readonly IMercadoPagoHmacSignatureValidator _mercadoPagoHmacSignatureValidator;

    /// <summary>
    /// Initilize class
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mercadoPagoHmacSignatureValidator"></param>
    public MercadoPagoMessageAuthorizationFilter(ILogger<MercadoPagoMessageAuthorizationFilter> logger,
        IMercadoPagoHmacSignatureValidator mercadoPagoHmacSignatureValidator)
    {
        _logger = logger;
        _mercadoPagoHmacSignatureValidator = mercadoPagoHmacSignatureValidator;
    }

    /// <summary>
    /// Validate Mercado Pago Webhook message
    /// </summary>
    /// <param name="context"></param>
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            if (_mercadoPagoHmacSignatureValidator.TryToValidate(context, out var reasonToFail))
            {
                return;
            }

            context.Result = new UnauthorizedResult();
            _logger.LogWarning(reasonToFail);
        }
        catch (Exception e)
        {
            _logger.LogError("Webhook message not authorized. Error {@Error}", e);
            context.Result = new UnauthorizedResult();
        }
    }
}
