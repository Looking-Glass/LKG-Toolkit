using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolKit_CLI
{
    public enum CLI_Task
    {
        listen,
        list,
        play,
        playlist,
        build_playlist,
        quiltify_RGBD,
    }

    public class CommandLineOptions
    {
        [Option('t', "task", Default = CLI_Task.list, HelpText = "Task to perform")]
        public CLI_Task task { get; set; }

        [Option('o', "orchestration", Default = "default", HelpText = "Sets the orchestration to connect to")]
        public string orchestrationName { get; set; }

        [Option('a', "address", Default = "localhost", HelpText = "Sets the address to connect to bridge through")]
        public string address { get; set; }

        [Option('i', "input", HelpText = "Input URI")]
        public string inputFile { get; set; }

        [Option("out", HelpText = "Output Path")]
        public string outputFile { get; set; }

        [Option('h', "head", Default = -1, HelpText = "Selects LKG display")]
        public int head { get; set; }

        [Option('r', "rows", Default = 1, HelpText = "Quilt row count")]
        public int rows { get; set; }

        [Option('c', "cols", Default = 1, HelpText = "Quilt column count")]
        public int cols { get; set; }

        [Option('q', "aspect", Default = 1f, HelpText = "Quilt aspect ratio")]
        public float aspect { get; set; }

        [Option('v', "view_count", Default = 1, HelpText = "Quilt view count")]
        public int viewCount { get; set; }

        [Option('l', "loop", Default = false, HelpText = "Controls if the playback loops")]
        public bool loopPlaylist { get; set; }


        [Usage(ApplicationAlias = "Toolkit-CLI.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("\nListen for Bridge events", new CommandLineOptions { task = CLI_Task.listen });
                yield return new Example("\nList connected displays", new CommandLineOptions { task = CLI_Task.list });
                yield return new Example("\nPlay Quilt", new CommandLineOptions { task = CLI_Task.play, inputFile = "https://s3.amazonaws.com/lkg-blocks/legacy/715/source.png", rows = 11, cols = 7, aspect = 0.75f, viewCount = 77 , loopPlaylist = true});
                yield return new Example("\nPlay Playlist of Quilts", new CommandLineOptions { task = CLI_Task.playlist, inputFile = "playlist.json", rows = 6, cols = 8, aspect = 1.77f, viewCount = 45 });
                yield return new Example("\nBuild Playlist of Quilts", new CommandLineOptions { task = CLI_Task.build_playlist, inputFile = "playlist.json"});
            }
        }
    }
}
