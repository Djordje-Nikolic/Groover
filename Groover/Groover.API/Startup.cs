using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AutoMapper;
using Newtonsoft.Json;
using Groover.BL;
using Groover.BL.Handlers.Requirements;
using Microsoft.AspNetCore.Authorization;
using Groover.BL.Handlers;
using Groover.BL.Models;
using Groover.BL.Services.Interfaces;
using Groover.BL.Services;
using Groover.BL.Helpers;
using Groover.API.Hubs;
using System.Threading.Tasks;
using Groover.API.Services;
using Groover.API.Services.Interfaces;
using Groover.BL.Models.Chat;

namespace Groover.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            string identityConnectionString = Configuration.GetConnectionString("grooverMySql");
            //string identityConnectionString = Configuration.GetConnectionString("grooverMySqlPC");
            services.AddIdentityDatabase(identityConnectionString);
            AutoMigrator.ApplyMigrations(identityConnectionString);

            IConfigurationSection chatDbConfigSection = Configuration.GetSection("ChatDbConfiguration");
            services.AddChatDatabase(chatDbConfigSection);

            AddJwt(services);

            AddEmailService(services);
            AddImageProcessing(services);
            AddAudioProcessing(services);

            services.AddSignalR(options =>
            {
                //Maybe remove after development
                options.EnableDetailedErrors = true;
            });
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IGroupChatService, GroupChatService>();
            services.AddScoped<INotificationService, NotificationService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Groover.API", Version = "v1" });
            });

            ConfigureAuthorization(services);
            services.AddAutoMapper(typeof(Groover.API.Models.AutoMapperProfile), typeof(Groover.BL.Models.AutoMapperProfile));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Groover.API v1"));
            }

            app.UseMiddleware(typeof(Utils.ErrorHandlingMiddleware));
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<GroupChatHub>("/hubs/groupchat");
            });
        }

        private IServiceCollection AddJwt(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateLifetime = true,
                        ValidateAudience = true,
                        RequireExpirationTime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecretKey"]))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs/groupchat"))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                    options.Validate();
                });

            return services;
        }

        private IServiceCollection AddEmailService(IServiceCollection services)
        {
            var emailConfig = Configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>();
            services.AddSingleton(emailConfig);
            services.AddScoped<IEmailSender, EmailSender>();

            return services;
        }

        private IServiceCollection AddImageProcessing(IServiceCollection services)
        {
            var avatarImageConfig = Configuration
                .GetSection("AvatarImageConfiguration")
                .Get<AvatarImageConfiguration>();
            services.AddSingleton(avatarImageConfig);
            services.AddSingleton<IAvatarImageProcessor, AvatarImageProcessor>();

            var chatImageConfig = Configuration
                .GetSection("ChatImageConfiguration")
                .Get<ChatImageConfiguration>();
            services.AddSingleton(chatImageConfig);
            services.AddSingleton<IChatImageProcessor, ChatImageProcessor>();

            return services;
        }

        private IServiceCollection AddAudioProcessing(IServiceCollection services)
        {
            var audioConfig = Configuration
                .GetSection("AudioConfiguration")
                .Get<AudioConfiguration>();
            services.AddSingleton(audioConfig);
            services.AddSingleton<ITrackProcessor, TrackProcessor>();

            return services;
        }

        private IServiceCollection ConfigureAuthorization(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                //options.AddPolicy("GroupAdmin", policy =>
                //    policy.Requirements.Add(new GroupRoleRequirement(GroupClaimTypeConstants.Admin)));
                //options.AddPolicy("GroupMember", policy =>
                //    policy.Requirements.Add(new GroupRoleRequirement(GroupClaimTypeConstants.Member)));
                //options.AddPolicy("IsUser", policy =>
                //    policy.Requirements.Add(new IsUserRequirement()));
            });

            services.AddSingleton<IAuthorizationHandler, GroupAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, IsUserAuthorizationHandler>();

            return services;
        }
    }
}
