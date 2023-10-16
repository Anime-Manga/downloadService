using Cesxhin.AnimeManga.Application.Consumers;
using Cesxhin.AnimeManga.Domain.DTO;
using Cesxhin.AnimeManga.Modules.Generic;
using Cesxhin.AnimeManga.Modules.Schema;
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

                            if ((Environment.GetEnvironmentVariable("ENABLE_VIDEO") ?? "true") == "true")
                            {
                                cfg.ReceiveEndpoint("download-video", e =>
                                {
                                    e.EnablePriority(255);
                                    e.Consumer<DownloadVideoConsumer>(cc =>
                                    {
                                        string limit = Environment.GetEnvironmentVariable("LIMIT_CONSUMER_RABBIT") ?? "3";

                                        cc.UseConcurrentMessageLimit(int.Parse(limit));
                                        cc.Message<EpisodeDTO>(m => m.UseDelayedRedelivery(Retry.Interval(10, TimeSpan.FromSeconds(10))));
                                    });
                                });

                                cfg.ReceiveEndpoint("delete-video", e =>
                                {
                                    e.Consumer<DeleteVideoConsumer>(cc =>
                                    {
                                        cc.UseConcurrentMessageLimit(1);
                                    });
                                });
                            }

                            if ((Environment.GetEnvironmentVariable("ENABLE_BOOK") ?? "true") == "true")
                            {
                                cfg.ReceiveEndpoint("download-book", e =>
                                {
                                    e.EnablePriority(255);
                                    e.Consumer<DownloadBookConsumer>(cc =>
                                    {
                                        string limit = Environment.GetEnvironmentVariable("LIMIT_CONSUMER_RABBIT") ?? "3";

                                        cc.UseConcurrentMessageLimit(int.Parse(limit));
                                        cc.Message<ChapterDTO>(m => m.UseDelayedRedelivery(Retry.Interval(10, TimeSpan.FromSeconds(10))));
                                    });
                                });

                                cfg.ReceiveEndpoint("delete-book", e =>
                                {
                                    e.Consumer<DeleteBookConsumer>(cc =>
                                    {
                                        cc.UseConcurrentMessageLimit(1);
                                    });
                                });
                            }
                        });
                    });

                    //setup nlog
                    var level = Environment.GetEnvironmentVariable("LOG_LEVEL").ToLower() ?? "info";
                    LogLevel logLevel = NLogManager.GetLevel(level);
                    NLogManager.Configure(logLevel);

                    services.AddHostedService<Worker>();
                });
    }
}
