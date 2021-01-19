using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Ellersoft.SignalR.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HubAttribute : Attribute
    {
        public const string BASE_PATH = "hubs";

        private static IEnumerable<Type> GetAllHubs() =>
            typeof(HubAttribute).Assembly.GetTypes()
                .Where(x => x.GetCustomAttribute(typeof(HubAttribute)) != null);

        private static string ConvertClassNameForRoute(MemberInfo t) =>
            t.Name.EndsWith("Hub") ? t.Name.Substring(0, t.Name.Length - 3) : t.Name;

        public static string GetRoute(string basePath, Type hub) =>
            System.IO.Path.Combine("/", basePath, ConvertClassNameForRoute(hub)).Replace("\\", "/");

        public static void RegisterEndpoints(IEndpointRouteBuilder endpoints, string baseHubPath = null)
        {
            // There are two "MapHub" functions, we need the first one
            var mi = GetMapHubMethod();
            foreach (var hub in GetAllHubs())
            {
                InvokeMapHubMethod(mi, hub, endpoints, baseHubPath ?? BASE_PATH);
            }
        }

        private static string GetRoute(Type hub) =>
            (string)hub.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .First(x => x.Name == nameof(GetRoute) && x.IsGenericMethod)
                .MakeGenericMethod(hub)
                .Invoke(null, null);

        private static void InvokeMapHubMethod(MethodInfo mi, Type hub, IEndpointRouteBuilder endpoints, string basePath) =>
            mi.MakeGenericMethod(hub).Invoke(null, new object[] {endpoints, basePath, GetRoute(hub)});

        private static MethodInfo GetMapHubMethod() =>
            typeof(HubEndpointRouteBuilderExtensions)
                .GetMethods()
                .First(x =>
                    x.Name == nameof(HubEndpointRouteBuilderExtensions.MapHub)
                    && x.IsGenericMethod);
    }
}