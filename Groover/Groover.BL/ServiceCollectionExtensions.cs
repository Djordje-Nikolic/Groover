using Groover.BL.CustomTokenProviders;
using Groover.BL.Helpers;
using Groover.BL.Utils;
using Groover.ChatDB;
using Groover.ChatDB.Interfaces;
using Groover.IdentityDB.MySqlDb;
using Groover.IdentityDB.MySqlDb.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

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
            ChatDbConfiguration chatDbConfiguration = configurationSection
                .Get<ChatDbConfiguration>();
            services.AddSingleton<IChatDbConfiguration>(chatDbConfiguration);

            //Init cluster factory
            services.AddSingleton<IChatDbClusterFactory, ChatDbClusterFactory>(serviceProvider =>
            {
                var clusterFactory = new ChatDbClusterFactory();
                var loggerProvider = serviceProvider.GetService<ILoggerProvider>();
                clusterFactory.AddLoggerProvider(loggerProvider);
                return clusterFactory;
            });

            //Init cluster
            services.AddSingleton<IChatDbCluster>(serviceProvider =>
            {
                var clusterFactory = serviceProvider.GetRequiredService<IChatDbClusterFactory>();
                var chatConfig = serviceProvider.GetRequiredService<IChatDbConfiguration>();
                return clusterFactory.CreateInstance(chatConfig);
            });

            //Init session and repositories
            services.AddSingleton<IGroupChatSession, GroupChatSession>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<ITrackRepository, TrackRepository>();
            services.AddScoped<IGroupChatRepository, GroupChatRepository>(serviceProvider =>
            {
                var trackRepository = serviceProvider.GetRequiredService<ITrackRepository>();
                var messageRepository = serviceProvider.GetRequiredService<IMessageRepository>();
                return new GroupChatRepository(trackRepository, messageRepository);
            });

            return services;
        }
    }
}
