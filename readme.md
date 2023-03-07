# Looking Glass Toolkit

Looking Glass Toolkit is an open source set of tools for interacting with the Looking Glass Bridge API.

> :warning: **NOTE**: This is ALPHA software and as such should not be used in any critical applications. 
> If you find any bugs or issues, please open an issue on this github repo.

Toolkit consists of 3 tools:

* Toolkit-API  
* Toolkit-CLI
* LKG-Toolkit

Toolkit-API is an C# library consisting of a client for accessing the bridge API.

Toolkit-CLI is a C# CLI application for working with bridge it:

* Supports Windows, MacOS, and Linux
* Checks display state
* Monitors display state changes
* Plays sets of quilts and quilt videos
* Synchronizes sets of quilts and quilt videos

LKG-Toolkit is a GUI version of Toolkit CLI for MacOS and Windows.

> :warning: **NOTE**: This project is still a WIP some features may not be included currently. LKG-Toolkit may be behind Toolkit CLI, which may not implement all of the Bridge API.

# How to build:

Prerequisites:
1. [Looking Glass Bridge](https://lookingglassfactory.com/software-downloads)
2. [.Net 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## Windows and Mac:

1. Install Visual Studio 2022 ([Windows](https://visualstudio.microsoft.com/vs/community/) or [MacOS](https://visualstudio.microsoft.com/vs/mac/)).
2. Make sure to include the .Net Multi-platform App UI development workload
3. Clone this repo and open in Visual Studio

If you would rather use VS code or some other editor follow the linux instructions below.

## Linux:
```sh
# download LKG-Toolkit repo
git clone https://github.com/Looking-Glass/LKG-Toolkit.git
cd LKG-Toolkit          
```

```sh
# setup, install project, and build
dotnet workload restore # Installs necessary .net tools
dotnet restore          # Installs nuget dependencies
dotnet build            # Builds all the projects
```

```sh
# Run the CLI
cd Toolkit-CLI
dotnet run
```


# CLI Examples

## Help
```sh
> Toolkit-CLI.exe --help

Toolkit-CLI 1.0.0
Copyright (C) 2023 Toolkit-CLI
USAGE:

Listen for Bridge events:
  Toolkit_CLI

  -t, --task             (Default: listen) Task to perform

  -o, --orchestration    (Default: default) Sets the orchestration to connect to

  -a, --address          (Default: localhost) Sets the address to connect to bridge through

  --help                 Display this help screen.

  --version              Display version information.
```

## Listening for events

```sh
> Toolkit-CLI.exe -t listen

Connecting to: ws://localhost:9724/event_source
Connected to bridge
Listening for events, press any key to stop.
```

## List connected devices

```sh
> Toolkit-CLI.exe -t list

Connecting to: ws://localhost:9724/event_source
Connected to bridge
Display Type: portrait
Display Serial: LKG-P03273
Display Loc: [3840, 0]
Calibration Version: 3.0
```

## Playback quilt

```sh
> Toolkit-CLI.exe -t play -i https://s3.amazonaws.com/lkg-blocks/legacy/715/source.png -c 7 -r 11 -v 77 --loop --aspect 0.75

Connecting to: ws://localhost:9724/event_source
Connected to bridge
Listening for events, press any key to stop.
```

# API Examples

## Listening for events

This is the exact code executed by: ```Toolkit-CLI.exe -t listen```

```csharp
// Create BridgeConnectionHTTP instance with the default IP address and ports
// Make sure to use the using pattern or properly dispose of the BridgeConnectionHTTP object
using BridgeConnectionHTTP b = new BridgeConnectionHTTP();

// Connect to bridge
bool connectionStatus = b.Connect();
if (connectionStatus)
{
    Console.WriteLine("Connected to bridge");
    b.AddListener("", (e) => { Console.WriteLine(e); });

    // Enter the default Orchestration
    // This is similar to a session but multiple
    // clients can connect to the same one
    if (!b.TryEnterOrchestration())
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
}
else
{
    Console.WriteLine("Failed to connect to bridge, ensure bridge is running");
    return;
}

Console.WriteLine("Listening for events, press any key to stop.");
Console.ReadKey();
```

## List Devices

This is the exact code executed by: ```Toolkit-CLI.exe -t list```

```csharp
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

    if (b.TryUpdateDevices())
    {
        List<Display> displays = b.GetLKGDisplays();
        foreach (Display display in displays)
        {
            Console.WriteLine(display.getInfoString());
        }
    }
    else
    {
        Console.WriteLine("Failed to update devices");
        return;
    }
}
else
{
    Console.WriteLine("Failed to connect to bridge, ensure bridge is running");
    return;
}
```

## Playback Quilt

This is the exact code executed by: ```Toolkit-CLI.exe -t play -i https://s3.amazonaws.com/lkg-blocks/legacy/715/source.png -c 7 -r 11 -v 77 --loop --aspect 0.75```

```csharp
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

    Playlist p = new Playlist("default", args.loopPlaylist);
    p.AddItem(args.inputFile, args.rows, args.cols, args.aspect, args.viewCount);

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
```