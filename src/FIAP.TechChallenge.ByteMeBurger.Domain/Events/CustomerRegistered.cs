// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using FIAP.TechChallenge.ByteMeBurger.Domain.Entities;

namespace FIAP.TechChallenge.ByteMeBurger.Domain.Events;

/// <summary>
/// Customer registered event
/// </summary>
/// <param name="payload"></param>
public class CustomerRegistered(Customer payload) : DomainEvent<Customer>(payload);
