﻿using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace AdventOfCode.NET.DependencyInjection;

public sealed class TypeRegistrar(IServiceCollection builder) : ITypeRegistrar
{
    public ITypeResolver Build() {
        return new TypeResolver(builder.BuildServiceProvider());
    }

    public void Register(Type service, Type implementation) {
        builder.AddSingleton(service, implementation);
    }

    public void RegisterInstance(Type service, object implementation) {
        builder.AddSingleton(service, implementation);
    }

    public void RegisterLazy(Type service, Func<object> factory) {
        ArgumentNullException.ThrowIfNull(factory);
        builder.AddSingleton(service, _ => factory());
    }
}