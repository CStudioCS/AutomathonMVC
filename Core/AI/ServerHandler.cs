using Automathon.Engine;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;

namespace Automathon.AI
{
    public static class ServerHandler
    {
        private static string tcpAddress = DEFAULT_TCP_ADRESS;
        private const string DEFAULT_TCP_ADRESS = "tcp://*:5555";
        private static RequestSocket gameSocket;
        public static void StartServer(string tcpAddress = DEFAULT_TCP_ADRESS)
        {
            ServerHandler.tcpAddress = tcpAddress;

            gameSocket = new RequestSocket();
            gameSocket.Bind(tcpAddress);
            Debug.Log("Server started on port 5555");
        }

        public static bool GetAIResponse(out string responseAction)
        {
            float[] state = GameplayManager.GetState();

            gameSocket.SendFrame(JsonConvert.SerializeObject(state));

            if (gameSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(500), out string response))
            {
                // Handle the AI response
                Debug.Log(response);
                responseAction = response;
                return true;
            }

            gameSocket.Options.Linger = TimeSpan.Zero;
            gameSocket.Unbind(tcpAddress);
            gameSocket.Dispose();

            gameSocket = new RequestSocket();
            gameSocket.Bind(tcpAddress);
            responseAction = "";
            return false;
        }

        public static void StopServer()
        {
            if (gameSocket != null)
            {
                gameSocket.Options.Linger = TimeSpan.Zero; // add this
                //gameSocket.Unbind(tcpAddress);
                gameSocket.Dispose();
                gameSocket = null;
                Debug.Log("Servor successfully shudown");
            }
            else
                Debug.LogError("Attempted to stop Server even though it hasn't been started");

        }

        public static void Dispose()
        {
            if (gameSocket != null)
                StopServer();

            NetMQConfig.Cleanup(false);
            //Debug.Log("Cleanup");
        }
    }
}
