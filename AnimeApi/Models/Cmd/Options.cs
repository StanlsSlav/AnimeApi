using CommandLine;

namespace AnimeApi.Models.Cmd
{
    internal class Options
    {
        [Option('d', "database", Required = false, HelpText = "Database to access")]
        public string Database { get; set; }

        [Option('c', "collection", Required = false, HelpText = "Collection to use")]
        public string Collection { get; set; }
    }
}