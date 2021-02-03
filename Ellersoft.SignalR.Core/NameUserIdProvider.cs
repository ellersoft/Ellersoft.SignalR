using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Ellersoft.SignalR.Core
{
    /// <summary>
    /// Represents a <see cref="IUserIdProvider"/> which uses a 'name' claim to establish the User ID.
    /// </summary>
    public class NameUserIdProvider : IUserIdProvider
    {
        /// <summary>
        /// The type of the claim with the user ID.
        /// </summary>
        public string UserIdClaimName { get; }

        /// <summary>
        /// Returns the User ID for the given <see cref="HubConnectionContext"/>.
        /// </summary>
        /// <param name="connection">The <see cref="HubConnectionContext"/> of the user to locate.</param>
        /// <returns>The value of the <see cref="UserIdClaimName"/> claim.</returns>
        public virtual string GetUserId(HubConnectionContext connection) =>
            GetUserId(connection.User);

        /// <summary>
        /// Returns the User ID for the given <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> of the user to query.</param>
        /// <returns>The value of the <see cref="UserIdClaimName"/> claim.</returns>
        public string GetUserId(ClaimsPrincipal user) =>
            user?.Claims
                ?.FirstOrDefault(x => x.Type == UserIdClaimName)
                ?.Value
                ?.ToLower();

        /// <summary>
        /// Creates a new instance of the <see cref="NameUserIdProvider"/> with the specified <see cref="UserIdClaimName"/>.
        /// </summary>
        /// <param name="userIdClaimName">The type of the claim with the User ID. Defaults to ClaimTypes.Name.</param>
        public NameUserIdProvider(string userIdClaimName = ClaimTypes.Name)
        {
            UserIdClaimName = userIdClaimName;
        }
    }
}
