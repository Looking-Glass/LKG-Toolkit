using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LookingGlass.Toolkit.Bridge;
using WebSocketSharp;

namespace LookingGlass.Toolkit.CLI.Samples
{
    internal class QuiltifyRGBDItem
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
                p.AddRGBDItem(args.inputFile, args.rows, args.cols, args.aspect, args.Depthiness, 0.9f, args.Focus, args.DepthLoc, 5f, 30, "", args.Zoom); 

                if (b.TryPlayPlaylist(p, args.head))
                {
                    Thread.Sleep(2500);

                    string filename = Environment.CurrentDirectory + $"\\output_qs{args.cols}x{args.rows}a{args.aspect}.png";

                    if (!args.outputFile.IsNullOrEmpty())
                    {
                        filename = args.outputFile;
                    }

                    // When an RGBD is on screen we can readback the quilt being displayed on screen to save to disk
                    b.TrySaveout("QUILT_VIEW", filename);
                }
                else
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
        }
    }
}
