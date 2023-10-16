using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolkitAPI.Bridge;

namespace ToolKitCLI.Samples
{
    internal class PlayRGBDItem
    {
        public static void Run(CommandLineOptions args)
        {
            // Create BridgeConnectionHTTP instance.
            // Make sure to use the using pattern or properly dispose of the BridgeConnectionHTTP object
            using BridgeConnectionHTTP b = new BridgeConnectionHTTP(args.address);

            // Connect to bridge
            bool connectionStatus = b.Connect();
            if (connectionStatus)
            {
                Console.WriteLine("Connected to bridge");

                // Enter the named Orchestration
                // This is similar to a session but multiple
                // clients can connect to the same instance, receive
                // the same events and control the same state
                if (!b.TryEnterOrchestration(args.orchestrationName))
                {
                    Console.WriteLine("Failed to enter orchestration");
                    return;
                }

                if (!b.TrySubscribeToEvents())
                {
                    Console.WriteLine("Failed to subscribe to events");
                    return;
                }

                if (!b.TryUpdateDevices())
                {
                    Console.WriteLine("Failed to update devices");
                    return;
                }

                Random rng = new Random();
                Playlist p = new Playlist("default_" + rng.Next(0, 10000), args.loopPlaylist);
                p.AddRGBDItem(args.inputFile, args.rows, args.cols, args.aspect,
                    1.0f,    //depthiness
                    0.9f,   //depth_cutoff
                    -0.04f,  //focus
                    2,       //depth_loc right
                    5f,    //cam_dist
                    30,      //fov
                    1f);   //zoom 

                if (!b.TryPlayPlaylist(p, args.head))
                {
                    Console.WriteLine("Failed to play playlist");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Failed to connect to bridge, ensure bridge is running");
                return;
            }

            Console.WriteLine("Listening for events, press any key to stop.");
            Console.ReadKey();
        }
    }
}
