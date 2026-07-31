using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mvp.Cli.Bootstrap;
using Mvp.Cli.Commands;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddMvpServices();

using var host = builder.Build();
var rootCommand = host.Services.GetRequiredService<RootCommandFactory>().Create();
var configuration = new InvocationConfiguration
{
    EnableDefaultExceptionHandler = false,
};

return await rootCommand.Parse(args).InvokeAsync(configuration);
