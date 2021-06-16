using Groover.BL.Handlers.Requirements;
using Groover.BL.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Handlers
{
    public class IsUserAuthorizationHandler : AuthorizationHandler<IsUserRequirement, UserDTO>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsUserRequirement requirement, UserDTO userDTO)
        {
            if (context.User.HasClaim(c => c.Type == ClaimTypes.NameIdentifier &&
                               c.Value == userDTO.Id.ToString()))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
