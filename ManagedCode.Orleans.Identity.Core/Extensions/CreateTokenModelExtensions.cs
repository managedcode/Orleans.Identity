using System;
using ManagedCode.Orleans.Identity.Core.Models;

namespace ManagedCode.Orleans.Identity.Core.Extensions;

public static class CreateTokenModelExtensions
{
    public static bool IsModelValid(this CreateTokenModel createTokenModel)
    {
        return createTokenModel.Lifetime != TimeSpan.Zero && !string.IsNullOrWhiteSpace(createTokenModel.Value);
    }
}