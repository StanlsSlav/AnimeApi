#pragma warning disable 1591

using System;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace AnimeAPI
{
    internal class Options
    {
        [Option('d', "database", Required = false, HelpText = "Database to access")]
        public string Database { get; set; }

        [Option('c', "collection", Required = false, HelpText = "Collection to use")]
        public string Collection { get; set; }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    var dbVariable
                        = Environment.GetEnvironmentVariable(nameof(Options.Database),
                            EnvironmentVariableTarget.User);

                    var colVariable
                        = Environment.GetEnvironmentVariable(nameof(Options.Collection),
                            EnvironmentVariableTarget.User);

                    Environment.SetEnvironmentVariable(nameof(Options.Database), o.Database ?? dbVariable);
                    Environment.SetEnvironmentVariable(nameof(Options.Collection), o.Collection ?? colVariable);

                    if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(nameof(Options.Database))) ||
                        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(nameof(Options.Collection))))
                    {
                        throw new Exception(
                            "Either 'Database' or 'Collection' was not set through env vars or flags");
                    }
                });

            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}