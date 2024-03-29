﻿using System;
using Autofac;
using Serilog;
using WarhammerLauncherTool.Commands;
using WarhammerLauncherTool.Commands.Implementations.File_related.BackupFile;
using WarhammerLauncherTool.Commands.Implementations.File_related.FindLauncherDataPath;
using WarhammerLauncherTool.Commands.Implementations.File_related.SaveModsToFile;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFile;
using WarhammerLauncherTool.Commands.Implementations.File_related.SelectFolder;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModFromStream;
using WarhammerLauncherTool.Commands.Implementations.Mod_related.GetModsForGame;
using WarhammerLauncherTool.Views;

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

        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(ICommandAsync<>));

        builder.RegisterAssemblyTypes(assemblies)
            .AsClosedTypesOf(typeof(ICommandAsync<,>));

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
                context.Resolve<ISelectFile>(),
                context.Resolve<IBackupFile>(),
                context.Resolve<ISelectFolder>(),
                context.Resolve<ISaveModsToFile>(),
                context.Resolve<IGetModsForGame>(),
                context.Resolve<IGetModFromStream>(),
                context.Resolve<IFindLauncherDataPath>()
            )).SingleInstance();

        return builder.Build();
    }
}
