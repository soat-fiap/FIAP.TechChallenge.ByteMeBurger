// Copyright (c) 2024, Italo Pessoa (https://github.com/italopessoa)
// All rights reserved.
//
// This source code is licensed under the BSD-style license found in the
// LICENSE file in the root directory of this source tree.

using FIAP.TechChallenge.ByteMeBurger.Api.Auth;
using FIAP.TechChallenge.ByteMeBurger.MercadoPago.Gateway.Security;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;

namespace FIAP.TechChallenge.ByteMeBurger.Api.Test.Auth;

[TestSubject(typeof(MercadoPagoMessageAuthorizationFilter))]
public class MercadoPagoMessageAuthorizationFilterTests
{
    private readonly Mock<IMercadoPagoHmacSignatureValidator> _validatorMock = new();
    private readonly Mock<ILogger<MercadoPagoMessageAuthorizationFilter>> _loggerMock = new();
    private readonly ActionContext _actionContext;

    public MercadoPagoMessageAuthorizationFilterTests()
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Headers =
                {
                    ["x-signature"] = "signature",
                    ["x-request-id"] = "requestId",
                },
                Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    { "data_id", "dataId" }
                })
            },
        };

        _actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }

    [Fact]
    public void OnAuthorization_SuccessAuthorized()
    {
        // Arrange
        var reasonToFailMock = string.Empty;
        var context = new AuthorizationFilterContext(_actionContext, new List<IFilterMetadata>());
        _validatorMock.Setup(v => v.TryToValidate(It.IsAny<AuthorizationFilterContext>(), out reasonToFailMock))
            .Returns(true);

        // Act
        var filter = new MercadoPagoMessageAuthorizationFilter(_loggerMock.Object, _validatorMock.Object);
        filter.OnAuthorization(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void OnAuthorization_Unauthorized()
    {
        // Arrange
        var reasonToFailMock = "unauthorized";
        var context = new AuthorizationFilterContext(_actionContext, new List<IFilterMetadata>());
        _validatorMock.Setup(v => v.TryToValidate(It.IsAny<AuthorizationFilterContext>(), out reasonToFailMock))
            .Returns(false);

        // Act
        var filter = new MercadoPagoMessageAuthorizationFilter(_loggerMock.Object, _validatorMock.Object);
        filter.OnAuthorization(context);

        // Assert
        context.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public void OnAuthorization_ExceptionUnauthorized()
    {
        // Arrange
        var reasonToFailMock = string.Empty;
        var context = new AuthorizationFilterContext(_actionContext, new List<IFilterMetadata>());
        _validatorMock.Setup(v => v.TryToValidate(It.IsAny<AuthorizationFilterContext>(), out reasonToFailMock))
            .Throws<ArgumentNullException>();

        // Act
        var filter = new MercadoPagoMessageAuthorizationFilter(_loggerMock.Object, _validatorMock.Object);
        filter.OnAuthorization(context);

        // Assert
        context.Result.Should().BeOfType<UnauthorizedResult>();
    }
}
