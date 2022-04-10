using Microsoft.Extensions.DependencyInjection;
using OpenApiSpecGeneration.Console.Commands;
using OpenApiSpecGeneration.Console.Commands.Helpers;
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
                config.AddCommand<GenerateOpenApiSpecWithMockImplementation>("generate-mock");
            });

            await app.RunAsync(args);
        }

        private static IServiceCollection BuildServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddSingleton<GetOpenApiSpecFile>();
            services.AddSingleton<WriteToFile>();
            services.AddSingleton<GenerateFromOpenApiSpec>();
            return services;
        }
    }
}
