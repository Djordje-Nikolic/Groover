using Groover.DB.MySqlDb;
using Groover.DB.MySqlDb.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Groover.BL.CustomTokenProviders;
using Groover.BL.Utils;
using Groover.BL.Helpers;

namespace Groover.BL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityDatabase(this IServiceCollection services, string connectionString)
        {

            services.AddDbContextPool<GrooverDbContext>((serviceProvider, options) =>
                        options.UseMySql(connectionString, 
                        ServerVersion.AutoDetect(connectionString), 
                        options => options.MigrationsAssembly("Groover.DB")));

            services.AddIdentity<User, Role>()
                    .AddEntityFrameworkStores<GrooverDbContext>()
                    .AddDefaultTokenProviders()
                    .AddTokenProvider<EmailConfirmationTokenProvider<User>>(Constants.EmailConfirmationTokenProvider)
                    .AddTokenProvider<GroupInviteTokenProvider<User>>(Constants.GroupInviteTokenProvider);

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
               opt.TokenLifespan = TimeSpan.FromHours(2));

            services.Configure<GroupInviteTokenProviderOptions>(opt =>
               opt.TokenLifespan = TimeSpan.FromDays(3));

            services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
               opt.TokenLifespan = TimeSpan.FromDays(7));

            services.AddScoped<ITokenProviderAccessor<User>, TokenProviderAccessor<User>>();

            services.Configure<IdentityOptions>(options => IdentityOptionsCustomizer.Customize(options));
            services.Configure<PasswordHasherOptions>(options => { });

            return services;
        }
    }
}
