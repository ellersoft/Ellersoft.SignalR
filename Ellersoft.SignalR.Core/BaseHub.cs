using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace Ellersoft.SignalR.Core
{
    /// <inheritdoc cref="Hub"/>
    public abstract class BaseHub : Hub
    {
        protected IConfiguration Configuration { get; }

        protected BaseHub(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static string GetRoute<THub>(string basePath = null) where THub : BaseHub =>
            HubAttribute.GetRoute(basePath ?? HubAttribute.BASE_PATH, typeof(THub));
    }
    
    /// <inheritdoc cref="Hub{T}"/>
    public abstract class BaseHub<T> : Hub<T>
        where T : class
    {
        protected IConfiguration Configuration { get; }

        protected BaseHub(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static string GetRoute<THub>(string basePath = null) where THub : BaseHub<T> =>
            HubAttribute.GetRoute(basePath ?? HubAttribute.BASE_PATH, typeof(THub));
    }
}
