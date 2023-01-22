using FileStorage.Cli;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddSingleton<IApiClient, ApiClient>()
    .AddSingleton<CommandExecutor>();

var provider = services.BuildServiceProvider();

var executor = provider.GetService<CommandExecutor>();

await executor!.ExecuteAsync(args);