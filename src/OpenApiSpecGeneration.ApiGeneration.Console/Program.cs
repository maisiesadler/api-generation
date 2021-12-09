using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace OpenApiSpecGeneration.Console
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var services = BuildServiceCollection();

            var app = new CommandApp(new TypeRegistrar(services));
            app.Configure(config =>
            {
                config.AddCommand<GenerateOpenApiSpec>("generate");
            });

            await app.RunAsync(args);
        }

        private static IServiceCollection BuildServiceCollection()
        {
            var services = new ServiceCollection();
            return services;
        }
    }
}
