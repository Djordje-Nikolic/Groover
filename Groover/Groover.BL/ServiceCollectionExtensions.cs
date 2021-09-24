using Groover.IdentityDB.MySqlDb;
using Groover.IdentityDB.MySqlDb.Entities;
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
using Microsoft.Extensions.Configuration;
using Groover.ChatDB;
using Groover.ChatDB.Interfaces;
using Microsoft.Extensions.Logging;

namespace Groover.BL
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddIdentityDatabase(this IServiceCollection services, string connectionString)
        {

            services.AddDbContextPool<GrooverDbContext>((serviceProvider, options) =>
                        options.UseMySql(connectionString, 
                        ServerVersion.AutoDetect(connectionString), 
                        options => options.MigrationsAssembly("Groover.IdentityDB")));

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

        public static IServiceCollection AddChatDatabase(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            //Init config
            var chatDbConfiguration = configurationSection
                .Get<IChatDbConfiguration>();
            services.AddSingleton(chatDbConfiguration);

            //Init cluster factory
            services.AddSingleton<IChatDbClusterFactory, ChatDbClusterFactory>(factory =>
            {
                var clusterFactory = new ChatDbClusterFactory();
                clusterFactory.AddLoggerProvider(factory.GetService<ILoggerProvider>());
                return clusterFactory;
            });

            //Init cluster
            services.AddSingleton<IChatDbCluster>(factory =>
            {
                var clusterFactory = factory.GetRequiredService<IChatDbClusterFactory>();
                return clusterFactory.CreateInstance(factory.GetRequiredService<IChatDbConfiguration>());
            });

            //Init session and repositories
            services.AddSingleton<IGroupChatSession, GroupChatSession>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<ITrackRepository, TrackRepository>();

            return services;
        }
    }
}
