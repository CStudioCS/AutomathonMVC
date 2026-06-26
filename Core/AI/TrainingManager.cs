using Automathon.Engine;
using Automathon.Game;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;

namespace Automathon.AI
{
    public class TrainingManager
    {
        private AIInputProvider selfInputProvider;
        private AIInputProvider enemyInputProvider;

        private ResponseSocket responseSocket;
        private string tcpAddress;

        public TrainingManager(string tcpAddress)
        {
            GameplayManager.Initialize();

            this.tcpAddress = tcpAddress;
            responseSocket = new ResponseSocket();
            responseSocket.Bind(tcpAddress);
            responseSocket.Options.Linger = TimeSpan.Zero;

            selfInputProvider = new AIInputProvider();
            enemyInputProvider = new AIInputProvider();

            GameplayManager.Reset(selfInputProvider, enemyInputProvider);

            Console.WriteLine("GameBridge initialized and waiting for AI messages...");
        }

        public bool Step()
        {
            string response = responseSocket.ReceiveFrameString();

            AIMessage msg = JsonConvert.DeserializeObject<AIMessage>(response);

            if (msg.Reset)
                GameplayManager.Reset(selfInputProvider, enemyInputProvider);
            else if (!msg.DoneWithTraining)
            {
                selfInputProvider.UpdateFromAction(msg.SelfAction);
                enemyInputProvider.UpdateFromAction(msg.EnemyAction);
                GameplayManager.Update();
            }

            GameState state = GameplayManager.GetState(selfInputProvider);
            string stringState = JsonConvert.SerializeObject(state);
            responseSocket.SendFrame(stringState);

            return msg.DoneWithTraining;
        }

        public void Dispose()
        {
            responseSocket.Options.Linger = TimeSpan.Zero;
            responseSocket.Unbind(tcpAddress);
            responseSocket.Dispose();
            responseSocket = null;

            GameplayManager.Dispose();
        }
    }

}