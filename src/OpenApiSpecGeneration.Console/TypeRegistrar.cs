using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace OpenApiSpecGeneration.Console
{
    internal class TypeRegistrar : ITypeRegistrar
    {
        private readonly IServiceCollection _services;

        public TypeRegistrar(IServiceCollection services)
        {
            _services = services;
        }

        public ITypeResolver Build()
        {
            return new TypeResolver(_services.BuildServiceProvider());
        }
        public void Register(Type service, Type implementation)
        {
            _services.AddTransient(service, implementation);
            // System.Console.WriteLine($"Register {service.Name} - {implementation.Name}");
        }
        public void RegisterInstance(Type service, object implementation)
        {
            _services.AddSingleton(service, implementation);
            // System.Console.WriteLine($"RegisterInstance {service.Name}");
        }
        public void RegisterLazy(Type service, Func<object> factory)
        {
            _services.AddTransient(service, sp => factory);
            // System.Console.WriteLine($"RegisterLazy {service.Name}");
        }
    }

    internal class TypeResolver : ITypeResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public TypeResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public object? Resolve(Type? type)
        {
            if (type == null) return null;
            var x = _serviceProvider.GetService(type);
            // System.Console.WriteLine($"Resolving {type.Name}, got {x == null}");
            return x;
        }
    }
}
