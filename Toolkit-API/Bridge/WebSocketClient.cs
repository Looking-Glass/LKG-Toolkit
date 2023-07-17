using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Toolkit_API.Bridge
{
    internal class BridgeWebSocketClient : IDisposable
    {
        private WebSocket? WS;
        private Action<string> messageReceivedCallback;
        public BridgeWebSocketClient(Action<string> messageReceivedCallback)
        {
            this.messageReceivedCallback = messageReceivedCallback; 
        }

        public bool TryConnect(string url)
        {
            WS = new WebSocket(url);

            WS.OnMessage += (sender, e) =>
            {
                messageReceivedCallback(e.Data);
            };

            WS.Connect();

            return WS.IsAlive;
        }

        private void WS_OnError(object? sender, WebSocketSharp.ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            WS?.Close();
        }

        public bool TrySendMessage(string message)
        {
            try
            {
                WS.Send(message);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool Connected()
        {
            if(WS is null) return false;
            return WS.IsAlive;
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}
