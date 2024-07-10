// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using Microsoft.Build.Framework;

namespace FIAP.TechChallenge.ByteMeBurger.Api.Model.Payment;

/// <summary>
/// Create payment request.
/// </summary>
public class CreatePaymentRequest
{
    /// <summary>
    /// Order id.
    /// </summary>
    [Required]
    public Guid OrderId { get; set; }

    /// <summary>
    /// Payment type
    /// </summary>
    [Required]
    public PaymentType PaymentType { get; set; }
}

public enum PaymentType
{
    Test = 0,
    MercadoPago = 1
}
