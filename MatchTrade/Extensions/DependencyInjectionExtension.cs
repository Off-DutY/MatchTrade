using System;
using System.Reflection;
using Autofac;
using Data.MatchTrade.Factories;
using Data.MatchTrade.Logic;
using MatchTrade.Factory;
using MatchTrade.Services;
using Microsoft.Extensions.Configuration;

namespace MatchTrade.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static void AddMatchTradeDependencyInjection(this ContainerBuilder builder, IConfiguration configuration)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            var registrationBuilder = builder.RegisterAssemblyTypes(executingAssembly)
                    .Where(r =>
                            r.Name.EndsWith("Repo", StringComparison.OrdinalIgnoreCase)
                         || r.Name.EndsWith("Logic", StringComparison.OrdinalIgnoreCase)
                         || r.Name.EndsWith("Service", StringComparison.OrdinalIgnoreCase));
            registrationBuilder.AsImplementedInterfaces();

            builder.RegisterType<MatchStrategyFactory>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<MemberOrderMatchService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<MmPartnerOrderMatchService>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<MySqlOrderStorageService>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<MatchTradeContextFactory>().AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<AwakeDbContextLogic>().AsImplementedInterfaces();
            builder.RegisterType<MatchTradeTimeService>().AsImplementedInterfaces().SingleInstance();
        }
    }
}