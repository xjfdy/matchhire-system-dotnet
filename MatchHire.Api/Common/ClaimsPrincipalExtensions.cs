using System.Security.Claims;

namespace MatchHire.Api.Common;

public static class ClaimsPrincipalExtensions
{
    // read the user ID from the verified JWT 
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirst("sub")!.Value);
}