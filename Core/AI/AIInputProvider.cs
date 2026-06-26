using Automathon.Engine;
using Automathon.Game;
using Automathon.Game.Input;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System;

namespace Automathon.AI
{
    public class AIInputProvider : InputProvider
    {
        private AIAction lastAction;
        private bool handleCallingAI;
        private string tcpAddress;
        private RequestSocket requestSocket;

        public AIInputProvider() : this(null) { }

        /// <summary>
        /// Initializes a new instance of the AIInputProvider class. Set TCP address to null if you want to handle the AI through NETMQ yourself, otherwise set it to the address of the AI server you want to connect to.
        /// </summary>
        public AIInputProvider(string tcpAddress = null)
        {
            lastAction = new AIAction();
            lastAction.MovingDirection = new Vector2Int(1000, 0);
            lastAction.AimingDirection = new Vector2Int(1000, 0);

            this.handleCallingAI = tcpAddress != null;
            this.tcpAddress = tcpAddress;

            if (handleCallingAI)
            {
                requestSocket = new RequestSocket();
                requestSocket.Connect(tcpAddress);
                requestSocket.Options.Linger = TimeSpan.Zero;
            }
        }

        public override void Update()
        {
            base.Update();

            if (!handleCallingAI)
                return;

            GameState state = GameplayManager.GetState(this);

            string stringState = JsonConvert.SerializeObject(state);

            requestSocket.SendFrame(stringState);

            if (requestSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(500), out string response))
            {
                AIMessage msg;
                try
                {
                    msg = JsonConvert.DeserializeObject<AIMessage>(response);
                }
                catch
                {
                    Debug.LogError("Count not parse JSON sent from python");
                    return;
                }

                if (msg.Reset)
                {
                    Debug.LogError("The AI requested a reset of the game, but it is not in training mode.");
                    return;
                }

                if (msg.DoneWithTraining)
                {
                    Debug.LogError("The AI requested to be done with training, but it is not in training mode.");
                    return;
                }

                UpdateFromAction(msg.SelfAction);
                return;
            }

            Debug.LogError("Timeout, the AI took too long to respond. Make sure to launch the Python AI Server !");

            requestSocket.Options.Linger = TimeSpan.Zero;
            requestSocket.Disconnect(tcpAddress);
            requestSocket.Dispose();

            requestSocket = new RequestSocket();
            requestSocket.Connect(tcpAddress);
        }

        public void UpdateFromAction(AIAction action)
        {
            void NormalizeAndCheckDir(ref Vector2Int dir, bool allowLower)
            {
                //prevent int overflow
                if (dir.X > 1000 * 1000)
                    dir.X = 1000;
                if (dir.Y > 1000 * 1000)
                    dir.Y = 1000;

                if (dir.LengthSquared() > 1000000 || !allowLower)
                    dir.NormalizeAtScale(1000);
            }

            NormalizeAndCheckDir(ref action.MovingDirection, true);
            NormalizeAndCheckDir(ref action.AimingDirection, false);

            //Attention : Data du user donc programmer defensivement pour eviter les erreurs
            if (action.AimingDirection == Vector2Int.Zero)
                action.AimingDirection = lastAction.AimingDirection;

            lastAction = action;
        }

        public override void OnDestroyed()
        {
            base.OnDestroyed();

            if (!handleCallingAI)
                return;

            requestSocket.Options.Linger = TimeSpan.Zero;
            requestSocket.Disconnect(tcpAddress);
            requestSocket.Dispose();
            requestSocket = null;
        }

        public override Vector2Int GetMilliAimingDir()
            => lastAction.AimingDirection;

        public override Vector2Int GetMilliMovementDir()
            => lastAction.MovingDirection;

        public override bool ShouldDash()
            => lastAction.Dash;

        public override bool ShouldMissile()
            => lastAction.Missile;

        public override bool ShouldShield()
            => lastAction.Shield;

        public override bool ShouldShoot()
            => lastAction.MachineGun;
    }

}
