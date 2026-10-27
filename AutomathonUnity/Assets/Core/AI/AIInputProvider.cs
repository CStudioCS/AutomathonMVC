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
            lastAction.MovingDirection = FixedVector2.Right;
            lastAction.AimingDirection = FixedVector2.Right;

            this.handleCallingAI = tcpAddress != null;
            this.tcpAddress = tcpAddress;

            if (handleCallingAI)
            {
                requestSocket = new RequestSocket();
                requestSocket.Options.Linger = TimeSpan.Zero;
                requestSocket.Connect(tcpAddress);
            }
        }

        public bool TestPing()
        {
            requestSocket.SendFrame("Ping");
            if (requestSocket.TryReceiveFrameString(TimeSpan.FromMilliseconds(500), out string response))
                return response == "Pong";

            // A REQ socket that didn't get its reply can't send anymore: reset it so TestPing can be retried
            Reconnect();
            return false;
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

                if (msg.SelfAction != null)
                    UpdateFromAction(msg.SelfAction);
                return;
            }

            Debug.LogError("Timeout, the AI took too long to respond. Make sure to launch the Python AI Server !");

            Reconnect();
        }

        private void Reconnect()
        {
            requestSocket.Options.Linger = TimeSpan.Zero;
            requestSocket.Disconnect(tcpAddress);
            requestSocket.Dispose();

            requestSocket = new RequestSocket();
            requestSocket.Connect(tcpAddress);
        }

        public void UpdateFromAction(AIAction action)
        {
            //Attention : Data du user donc programmer defensivement pour eviter les erreurs
            if (action == null)
                return;

            // Fixed values saturate instead of overflowing, and normalization works on the whole range,
            // so any user value ends up as a valid direction.
            if (action.MovingDirection.LengthSquaredLong() > FixedLong.One)
                action.MovingDirection = action.MovingDirection.Normalized();

            action.AimingDirection = action.AimingDirection.Normalized();
            if (action.AimingDirection == FixedVector2.Zero)
                action.AimingDirection = lastAction.AimingDirection;

            lastAction = action;
        }

        public void EndLife()
        {
            base.OnDestroyed();

            if (!handleCallingAI)
                return;

            requestSocket.Options.Linger = TimeSpan.Zero;
            requestSocket.Disconnect(tcpAddress);
            requestSocket.Dispose();
            requestSocket = null;
        }

        public override FixedVector2 GetAimingDir()
            => lastAction.AimingDirection;

        public override FixedVector2 GetMovementDir()
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
