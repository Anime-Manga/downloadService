using Cesxhin.AnimeManga.Application.Consumers;
using Cesxhin.AnimeManga.Application.CronJob;
using Cesxhin.AnimeManga.Application.Generic;
using Cesxhin.AnimeManga.Application.Proxy;
using Cesxhin.AnimeManga.Application.Schema;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using Quartz;
using System;

namespace Cesxhin.AnimeManga.DownloadService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SchemaControl.Check();
            ProxyManagement.InitProxy();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //rabbit
                    services.AddMassTransit(
                    x =>
                    {
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(
                                Environment.GetEnvironmentVariable("ADDRESS_RABBIT") ?? "localhost",
                                "/",
                                credentials =>
                                {
                                    credentials.Username(Environment.GetEnvironmentVariable("USERNAME_RABBIT") ?? "guest");
                                    credentials.Password(Environment.GetEnvironmentVariable("PASSWORD_RABBIT") ?? "guest");
                                });

                            cfg.ReceiveEndpoint("download-video", e =>
                            {
                                e.Consumer<DownloadVideoConsumer>(cc =>
                                {
                                    string limit = Environment.GetEnvironmentVariable("LIMIT_CONSUMER_RABBIT") ?? "3";

                                    cc.UseConcurrentMessageLimit(int.Parse(limit));
                                });
                            });

                            cfg.ReceiveEndpoint("download-book", e =>
                            {
                                e.Consumer<DownloadBookConsumer>(cc =>
                                {
                                    string limit = Environment.GetEnvironmentVariable("LIMIT_CONSUMER_RABBIT") ?? "3";

                                    cc.UseConcurrentMessageLimit(int.Parse(limit));
                                });
                            });

                            cfg.ReceiveEndpoint("delete-video", e =>
                            {
                                e.Consumer<DeleteVideoConsumer>(cc =>
                                {
                                    cc.UseConcurrentMessageLimit(1);
                                });
                            });

                            cfg.ReceiveEndpoint("delete-book", e =>
                            {
                                e.Consumer<DeleteBookConsumer>(cc =>
                                {
                                    cc.UseConcurrentMessageLimit(1);
                                });
                            });

                        });
                    });

                    //setup nlog
                    var level = Environment.GetEnvironmentVariable("LOG_LEVEL").ToLower() ?? "info";
                    LogLevel logLevel = NLogManager.GetLevel(level);
                    NLogManager.Configure(logLevel);

                    //cronjob for check health
                    services.AddQuartz(q =>
                    {
                        q.UseMicrosoftDependencyInjectionJobFactory();
                        q.ScheduleJob<HealthJob>(trigger => trigger
                            .StartNow()
                            .WithDailyTimeIntervalSchedule(x => x.WithIntervalInSeconds(60)), job => job.WithIdentity("download"));
                    });
                    services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

                    services.AddHostedService<Worker>();
                });
    }
}
