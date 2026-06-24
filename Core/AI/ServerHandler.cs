using Automathon.Engine;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Automathon.AI
{
    public static class ServerHandler
    {
        private static string tcpAddress = DEFAULT_TCP_ADRESS;
        private const string DEFAULT_TCP_ADRESS = "tcp://localhost:5555";
        private static RequestSocket gameSocket;
        public static void StartServer(string tcpAddress = DEFAULT_TCP_ADRESS)
        {
            ServerHandler.tcpAddress = tcpAddress;

            try
            {
                gameSocket = new RequestSocket();
                gameSocket.Connect(tcpAddress);
                gameSocket.Options.Linger = TimeSpan.Zero;
            }
            catch (AddressAlreadyInUseException)
            {
                Debug.Log("TCP Adress is already in use, probaby Unity not killing the process (tiz ok !)");
            }

            Debug.Log("Server started on port 5555");
        }

        public static bool GetAIResponse(out string responseAction)
        {
            float[] state = GameplayManager.GetState();

            Debug.Log("Try send");
            gameSocket.SendFrame(JsonConvert.SerializeObject(state));

            Debug.Log("Sent");

            if (gameSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(500), out string response))
            {
                // Handle the AI response
                Debug.Log("Responded");
                Debug.Log(response);
                responseAction = response;
                return true;
            }

            Debug.Log("Didn't respond");

            gameSocket.Options.Linger = TimeSpan.Zero;
            gameSocket.Disconnect(tcpAddress);
            gameSocket.Dispose();

            gameSocket = new RequestSocket();
            gameSocket.Connect(tcpAddress);
            responseAction = "";
            return false;
        }

        public static void StopServer()
        {
            if (gameSocket == null)
            {
                Debug.LogError("Attempted to stop Server even though it hasn't been started");
                return;
            }

            gameSocket.Options.Linger = TimeSpan.Zero;

            try { gameSocket.Unbind(tcpAddress); }
            catch (KeyNotFoundException) { }
            catch (EndpointNotFoundException) { }

            gameSocket.Dispose();
            gameSocket = null;
            Debug.Log("Servor successfully shudown");
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
