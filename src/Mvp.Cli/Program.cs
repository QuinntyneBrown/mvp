using Mvp.Cli.Commands;
using Mvp.Cli.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddMvpServices())
    .Build();

var rootCommand = new RootCommand("mvp - CLI tool for creating full-stack MVP .NET and Angular solutions");

var newCommand = NewCommand.Create(host.Services);
rootCommand.AddCommand(newCommand);

return await rootCommand.InvokeAsync(args);
