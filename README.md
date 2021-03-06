# Ellersoft.SignalR

This project is a wrapper over the Microsoft SignalR implementation which simplifies the process of using SignalR.

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

       services.AddEllersoftSignalR(configuration, userIdProvider);

   - **configuration**: The configuration root for the Ellersoft SignalR Options. For example, if your `Jwt` section were at the root of your `appSettings.json`, you would pass the root configuration object.
   - **userIdProvider**: The `IUserIdProvider` to associate with the SignalR instance. You can create an instance of the `NameUserIdProvider` for an easy start, or create your own `IUserIdProvider`. If excluded, the `NameUserIdProvider` will be used with a `ClaimTypes.NameIdentifier` search pattern.
   
4. Add the following to your `ConfigureServices` method, in your `AddAuthentication` pipeline call (typically at the end of the authentication chain):

       .AddEllersoftJwt(configuration, baseHubPath, additionalManipulation)

   - **configuration**: The configuration root for the Ellersoft SignalR Options. For example, if your `Jwt` section were at the root of your `appSettings.json`, you would pass the root configuration object.
   - **baseHubPath**: The base path to the Hub route. Defaults to `HubAttribute.BASE_PATH`.
   - **additionalManipulation**: Any additional changes to make to the JWT configuration. Defaults to null.
    
   If you are manually configuration a JWT token, you can also use `options.ConfigureJwt(configuration, baseHubPath)` in your `Action<JwtBearerOptions>` delegate, which will also configure the proper JWT components.
    
5. Note, at the end of your `ConfigureServices` method you should add SignalR like normal:

       services.AddSignalR();

6. Add the following to your `Configure` method, during route registration:

       endpoints.MapEllersoftSignalR();
   
7. To begin using SignalR, create a class that implements `Ellersoft.SignalR.Core.BaseHub` and decorate it with the `Ellersoft.SignalR.Core.HubAttribute` and `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` attributes. An example hub might look like the following:
   
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

## Minimal Implementation

A relatively minimal implementation might look like the following:

    public void ConfigureServices(IServiceCollection services)
    {
        // Add Ellersoft SignalR models
        services.AddEllersoftSignalR(Configuration, new NameUserIdProvider(ClaimTypes.Name));
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => { options.ExpireTimeSpan = TimeSpan.FromDays(30); })
            .AddEllersoftJwt(Configuration); // Add Ellersoft SignalR JWT Configuration
       
        services.AddControllersWithViews();
      
        services.AddSignalR(); // Add standard Microsoft SignalR services
    }
      
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var cookieOptions = new CookiePolicyOptions()
        {
            MinimumSameSitePolicy = SameSiteMode.Strict
        };
        app.UseCookiePolicy(cookieOptions);
    
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
      
        app.UseHttpsRedirection();
        app.UseStaticFiles();
    
        app.UseRouting();
      
        app.UseAuthentication();
        app.UseAuthorization();
      
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            endpoints.MapEllersoftSignalR(); // Map all routes tagged with `HubAttribute` and that are `BaseHub` or `BaseHub<T>`
        });
    }
