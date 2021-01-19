using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Ellersoft.SignalR.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Adds the default Ellersoft SignalR <see cref="JwtMiddleware"/> and <see cref="NameUserIdProvider"/> to the ASP.NET Core Service Collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for registration.</param>
        /// <param name="configuration">The configuration root for the Ellersoft SignalR Options.</param>
        public static void AddEllersoftSignalR(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(provider => new JwtMiddleware(configuration));
            services.AddSingleton<IUserIdProvider, NameUserIdProvider>();
        }

        /// <summary>
        /// Adds the default Ellersoft SignalR <see cref="JwtMiddleware"/> bearer token to the ASP.NET Core Authentication Pipeline.
        /// </summary>
        /// <param name="builder">The <see cref="AuthenticationBuilder"/> to add JWT Authentication to.</param>
        /// <param name="configuration">The configuration root for the Ellersoft SignalR Options.</param>
        /// <param name="baseHubPath">The base path to the Hub route. Defaults to <see cref="HubAttribute.BASE_PATH"/>.</param>
        /// <param name="additionalConfiguration">Any additional JWT options for other services.</param>
        public static void AddEllersoftJwt(
            this AuthenticationBuilder builder,
            IConfiguration configuration,
            string baseHubPath = null,
            Action<JwtBearerOptions> additionalConfiguration = null) =>
            builder.AddJwtBearer(options =>
            {
                ConfigureJwt(options, configuration, baseHubPath);
                additionalConfiguration?.Invoke(options);
            });

        /// <summary>
        /// Configured the default Ellersoft SignalR <see cref="JwtMiddleware"/> options.
        /// </summary>
        /// <param name="options">The <see cref="JwtBearerOptions"/> to add JWT Authentication to.</param>
        /// <param name="configuration">The configuration root for the Ellersoft SignalR Options.</param>
        /// <param name="baseHubPath">The base path to the Hub route. Defaults to <see cref="HubAttribute.BASE_PATH"/>.</param>
        public static void ConfigureJwt(
            JwtBearerOptions options,
            IConfiguration configuration,
            string baseHubPath = null)
        {
            // We have to hook the OnMessageReceived event in order to
            // allow the JWT authentication handler to read the access
            // token from the query string when a WebSocket or 
            // Server-Sent Events request comes in.

            // Sending the access token in the query string is required due to
            // a limitation in Browser APIs. We restrict it to only calls to the
            // SignalR hub in this code.
            // See https://docs.microsoft.com/aspnet/core/signalr/security#access-token-logging
            // for more information about security considerations when using
            // the query string to transmit the access token.
            options.RequireHttpsMetadata = false;
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                    JwtMiddleware.GetTokenFromQueryString(context, baseHubPath ?? $"/{HubAttribute.BASE_PATH}")
            };

            options.TokenValidationParameters =
                new JwtMiddleware(configuration).GetValidationParameters(new TokenValidationParameters());
        }

        /// <summary>
        /// Maps hubs tagged by the <see cref="HubAttribute"/>.
        /// </summary>
        /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to map into.</param>
        public static void MapEllersoftSignalR(this IEndpointRouteBuilder endpoints) =>
            HubAttribute.RegisterEndpoints(endpoints);
    }
}