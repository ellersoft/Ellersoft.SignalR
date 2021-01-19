# Ellersoft.SignalR

This project is a wrapper over the Microsoft.SignalR implementation which simplifies the process of using SignalR.

To get started:

1. Install this package (Ellersoft.SignalR.Core), and open the `Ellersoft.SignalR.Core` namespace.
   
2. Add the following configuration section to the root of your configuration file, or into any subsection you like:
    
         "JWT": {
           "Audience": "<your hostname, such as 'localhost'>",
           "Issuer": "<your hostname, such as 'localhost'>",
           "Key": "<a sequence of ASCII characters used to construct JWT token keys>"
         }
   
    - **Audience**: this should be the hostname of the audience of your JWT token. Most of the time this will be the hostname of your application.
    - **Issuer**: this should be the hostname of the issuer of your JWT token. Most of the time this will be the hostname of your application.
    - **Key**: this should be a relatively long (for example, 32 characters or more) sequence of characters which are used as the secret portion of the JWT signing process.
    
3. Add the following to your `ConfigureServices` method, before any services are registered:

         services.AddEllersoftSignalR(configuration);

    - **configuration**: The configuration root for the Ellersoft SignalR Options. For example, if your `Jwt` section were at the root of your `appSettings.json`, you would pass the root configuration object.
    
4. Add the following to your `ConfigureServices` method, in your `AddAuthentication` pipeline call (typically at the end of the authentication chain):

        .AddEllersoftJwt(configuration, baseHubPath, additionalManipulation)

    - **configuration**: The configuration root for the Ellersoft SignalR Options. For example, if your `Jwt` section were at the root of your `appSettings.json`, you would pass the root configuration object.
    - **baseHubPath**: The base path to the Hub route. Defaults to `HubAttribute.BASE_PATH`.
    - **additionalManipulation**: Any additional changes to make to the JWT configuration. Defaults to null.
    
    If you are manually configuration a JWT token, you can also use `options.ConfigureJwt(configuration, baseHubPath)` in your `Action<JwtBearerOptions>` delegate, which will also configure the proper JWT components.
    
5. Add the following to your `ConfigureServices` method, during route registration:

        endpoints.MapEllersoftSignalR();
   
6. To begin using SignalR, create a class that implements `Ellersoft.SignalR.Core.BaseHub` and decorate it with the `Ellersoft.SignalR.Core.HubAttribute` and `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` attributes. An example hub might look like the following:
   
        [Hub]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public class ChatHub : BaseHub
        {
            public static class ClientMethods
            {
                public const string RECEIVE_MESSAGE = "ReceiveMessage";
                public const string SEND_MESSAGE = nameof(SendMessage);
            }

            public ChatHub(IConfiguration configuration) : base(configuration) { }

            private Guid GetCurrentUserId() => Guid.Parse(NameUserIdProvider.GetUserId(Context.User));

            private async Task SendGlobalMessage(string message) =>
                await Clients.All.SendAsync(
                    ClientMethods.RECEIVE_MESSAGE,
                    MessageTypes.Global,
                    DateTime.UtcNow,
                    message);

            private async Task SendSystemMessage(string message) =>
                await Clients.Caller.SendAsync(
                    ClientMethods.RECEIVE_MESSAGE,
                    MessageTypes.System,
                    DateTime.UtcNow,
                    message);

            public enum MessageTypes
            {
                Global = 0,
                Direct = 1,
                System = 2
            }
        }

