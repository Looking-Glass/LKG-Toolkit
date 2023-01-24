using CommandLine;
using Toolkit_API.Bridge;
using ToolKit_CLI.Samples;

namespace ToolKit_CLI
{

    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunOptions);
        }

        private static void RunOptions(CommandLineOptions args)
        {
            switch (args.task)
            {
                case CLI_Task.listen:
                    ListenForEvents.Run(args);
                    break;
                case CLI_Task.list:
                    ListDevices.Run(args);
                    break;
                case CLI_Task.play:
                    PlayItem.Run(args);
                    break;
                case CLI_Task.playlist:
                    break;
            }

        }
    }
}