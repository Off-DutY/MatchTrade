using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using MatchTrade.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MatchTrade
{
    public class Program
    {
        private static string _environmentName;

        public static void Main(string[] args)
        {
            Console.WriteLine("The number of processors on this computer is {0}.",
            Environment.ProcessorCount);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                    .ConfigureServices((context, services) =>
                    {
                        _environmentName = context.HostingEnvironment.EnvironmentName;
                        services.AddMemoryCache();
                        services.AddHttpContextAccessor();
                        services.AddMvcCore().AddNewtonsoftJson();
                        services.AddHttpClient();
                    })
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureContainer<ContainerBuilder>((context, builder) =>
                    {
                        builder.AddMatchTradeDependencyInjection(context.Configuration);
                    });
        }
    }
}