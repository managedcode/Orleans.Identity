using System;
using ManagedCode.Orleans.Identity.Models;

namespace ManagedCode.Orleans.Identity.Extensions;

public static class CreateTokenModelExtensions
{
    public static bool IsModelValid(this CreateTokenModel createTokenModel)
    {
        return createTokenModel.Lifetime != TimeSpan.Zero;
    }
}