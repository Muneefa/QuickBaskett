using System.Security.Claims;
using QuickBasket.Application.Common;

namespace QuickBasket.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue("sub"), out var id) ? id : throw new UnauthorizedAppException("Invalid token.");
}
