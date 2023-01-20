using Toolkit_API.Bridge;

namespace ToolKit_CLI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                using BridgeConnectionHTTP b = new BridgeConnectionHTTP();
                if(b.Connect())
                {
                    Console.WriteLine("Connected to bridge");
                    if(!b.TryEnterOrchestration())
                    {
                        return;
                    }

                    if(!b.TrySubscribeToEvents() )
                    {
                        return;
                    }

                    if(!b.TryUpdateDevices())
                    {
                        return;
                    }
                }

                Console.WriteLine("Listening for events, press any key to stop.");

                Console.ReadKey();
            }
            else
            {
                // TODO args parser
            }
        }
    }
}