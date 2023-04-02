using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using SmartAc.Infrastructure.Repositories;

namespace SmartAc.API.Identity;

public class ValidTokenAuthorizationHandler : AuthorizationHandler<ValidTokenRequirement>
{
    private readonly SmartAcContext _smartAcContext;
    public ValidTokenAuthorizationHandler(SmartAcContext smartAcContext)
    {
        _smartAcContext = smartAcContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ValidTokenRequirement requirement)
    {
        var tokenId = context.User.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;
        var deviceSerialNumber = context.User.Identity?.Name;

        var isTokenValid = await _smartAcContext.DeviceRegistrations.AnyAsync(
            registration => registration.DeviceSerialNumber == deviceSerialNumber
                            && registration.TokenId == tokenId
                            && registration.Active);

        if (isTokenValid)
        {
            context.Succeed(requirement);
        }
    }
}