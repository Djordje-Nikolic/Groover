using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groover.BL.CustomTokenProviders
{
    public class GroupInviteTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public GroupInviteTokenProvider(IDataProtectionProvider dataProtectionProvider, 
            IOptions<GroupInviteTokenProviderOptions> options, 
            ILogger<DataProtectorTokenProvider<TUser>> logger) : base(dataProtectionProvider, options, logger)
        {
        }
    }

    public class GroupInviteTokenProviderOptions : DataProtectionTokenProviderOptions
    {
    }
}
