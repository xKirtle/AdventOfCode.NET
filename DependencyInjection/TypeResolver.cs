using Spectre.Console.Cli;

namespace AdventOfCode.NET.DependencyInjection;

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver, IDisposable
{
    public object? Resolve(Type? type) {
        return type == null ? null : provider.GetService(type);
    }

    public void Dispose() {
        if (provider is IDisposable disposable)
            disposable.Dispose();
    }
}