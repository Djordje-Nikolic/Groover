using Groover.BL.Handlers.Requirements;
using Groover.BL.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Handlers
{
    public class GroupAuthorizationHandler : AuthorizationHandler<GroupRoleRequirement, GroupDTO>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupRoleRequirement requirement, GroupDTO groupDTO)
        {
            if (context.User.HasClaim(c => c.Type == requirement.RoleClaimType &&
                                           c.Value == groupDTO.Id.ToString()))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
