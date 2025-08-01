﻿using System.Net;
using Example;
using ClickHouse.Facades;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = CreateHostBuilder(args).Build();
var serviceProvider = host.Services;

var contextFactory = serviceProvider.GetRequiredService<IClickHouseContextFactory<ExampleContext>>();
await using var context = await contextFactory.CreateContextAsync();

await context.Orders.InsertRandomOrders();
var topExpensesUser = await context.Orders.GetTopExpensesUser();

Console.WriteLine(
	$"Top expenses user Id: {topExpensesUser!.UserId}. With total expenses: {topExpensesUser.Expenses:F2}.");

return 0;


static IHostBuilder CreateHostBuilder(string[] args) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureAppConfiguration((context, builder) =>
		{
			var path = Path.Combine(context.HostingEnvironment.ContentRootPath, "appsettings.json");
			builder.AddJsonFile(path, false, true);
		})
		.ConfigureLogging(logging =>
		{
			logging.ClearProviders();
			logging.AddConsole();
			logging.SetMinimumLevel(LogLevel.Debug);
		})
		.ConfigureServices((_, services) =>
		{
			services.AddOptions<ClickHouseConfig>()
				.BindConfiguration(nameof(ClickHouseConfig));

			services.AddSingleton<QueryLogger>();

			services.AddClickHouseContext<ExampleContext, ExampleContextFactory>(
				builder => builder
					.AddFacade<OrdersFacade>());

			services.AddHttpClient("ch-https")
				.ConfigureHttpClient((_, httpClient) =>
				{
					httpClient.Timeout = TimeSpan.FromSeconds(60);
				})
				.SetHandlerLifetime(Timeout.InfiniteTimeSpan)
				.ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
				{
					AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
					MaxConnectionsPerServer = 1,
					ServerCertificateCustomValidationCallback = (_, cert, _, _) =>
						cert is not null && cert.Issuer.Contains("CN=example_user"),
				});
		});
