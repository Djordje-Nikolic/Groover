using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.Handlers.Requirements
{
    public class GroupRoleRequirement : IAuthorizationRequirement
    {
        public string RoleClaimType { get; }

        public GroupRoleRequirement(string roleClaimType)
        {
            RoleClaimType = roleClaimType;
        }
    }
}
