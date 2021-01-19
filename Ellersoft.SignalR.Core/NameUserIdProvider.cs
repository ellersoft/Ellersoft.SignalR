using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Ellersoft.SignalR.Core
{
    public class NameUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection) =>
            GetUserId(connection.User);

        public static string GetUserId(ClaimsPrincipal user) =>
            user?.Claims
                ?.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)
                ?.Value
                ?.ToLower();
    }
}
