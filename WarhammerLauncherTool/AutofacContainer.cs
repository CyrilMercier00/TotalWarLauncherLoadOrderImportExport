﻿using System;
using Autofac;
using Serilog;
using WarhammerLauncherTool.Commands;
using WarhammerLauncherTool.Commands.Implementations.File_related.FindLauncherDataPath;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFile;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFolder;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModFromStream;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsFromLauncherData;

namespace WarhammerLauncherTool;

public static class AutofacContainer
{
    public static IContainer Initialize()
    {
        var builder = new ContainerBuilder();

        // Commands
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(ICommand<>));

        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(ICommand<,>));

        // Serilog
        builder.Register<ILogger>((_, _) =>
            new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/log-{Date}.txt", retainedFileCountLimit: 5)
                .CreateLogger()
        ).SingleInstance();

        // Views
        builder.Register<MainWindow>(
            context => new MainWindow(
                context.Resolve<ILogger>(),
                context.Resolve<ISelectFile>(),
                context.Resolve<ISelectFolder>(),
                context.Resolve<IGetModFromStream>(),
                context.Resolve<IFindLauncherDataPath>(),
                context.Resolve<IGetModsFromLauncherData>())
        ).SingleInstance();

        return builder.Build();
    }
}