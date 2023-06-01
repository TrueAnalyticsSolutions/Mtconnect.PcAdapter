# Mtconnect.PcAdapter
This is a simple, interactive demo of a MTConnect Adapter using the [TAMS Adapter SDK](https://github.com/TrueAnalyticsSolutions/Mtconnect.Adapter).

## How to demo
 - Run the Terminal project (console application)
 - Accept the security warning. This is because of the TcpAdapter implementation opening a connection on `localhost:7878`, depending on your local App.config.
 - Move your mouse around and switch windows to see the SHDR commands in the console window.
 - If properly connected to the [reference C++ Agent](https://github.com/mtconnect/cppagent), you should see changes reflected in the MTConnect Current response

## Important features
This demo showcases the implementation of both `IAdapterSource` and `IAdapterDataModel` from the [TAMS Adapter SDK](https://github.com/TrueAnalyticsSolutions/Mtconnect.Adapter).

Within the `PCAdapter` project:

The `PCAdapterSource` implements `IAdapterSource`. This class implements a simple timer, which upon tick, will reference windows handles for cursor position and window titles. At the end of the tick method, changes to the model are published to the underlying Adapter SDK for publishing thru TCP.

The `PCModel.cs` implements `IAdapterDataModel`. This file contains multiple classes that describe the MTConnect component model and implements the explicit MTConnect observation Types/Sub-Types.
