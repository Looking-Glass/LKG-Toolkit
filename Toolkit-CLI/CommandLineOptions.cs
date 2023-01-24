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
    }

    public class CommandLineOptions
    {
        [Option('t', "task", Default = CLI_Task.listen, HelpText = "Task to perform")]
        public CLI_Task task { get; set; }

        [Option('o', "orchestration", Default = "default", HelpText = "Sets the orchestration to connect to")]
        public string orchestrationName { get; set; }

        [Option('a', "address", Default = "localhost", HelpText = "Sets the address to connect to bridge through")]
        public string address { get; set; }


        [Usage(ApplicationAlias = "Toolkit_CLI")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("\nListen for Bridge events", new CommandLineOptions { task = CLI_Task.listen });
            }
        }
    }
}
