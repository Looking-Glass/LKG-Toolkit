using CommandLine;
using ToolkitAPI.Bridge;
using ToolKitCLI.Samples;

namespace ToolKitCLI
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
                case CLI_Task.hide:
                    HideWindow.Run(args);
                    break;
                case CLI_Task.list:
                    ListDevices.Run(args);
                    break;
                case CLI_Task.play:
                    PlayRGBDItem.Run(args);
                    //PlayQuiltItem.Run(args);
                    break;
                case CLI_Task.quiltify_RGBD:
                    QuiltifyRGBDItem.Run(args);
                    break;
                case CLI_Task.playlist:
                    break;
            }

        }
    }
}