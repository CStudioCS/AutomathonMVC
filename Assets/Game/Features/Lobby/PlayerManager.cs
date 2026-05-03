using Automathon.Engine;
using Automathon.Game.Input;
using Automathon.Game.TankSystem;
using System;
using System.Collections.Generic;

namespace Automathon.Game.Lobby.MultiTankManagement
{
    public static class PlayerManager
    {
        public static Func<IInputProvider, Tank> PlayerJoined; //this is called by the view
        public static Action<IInputProvider> PlayerLeft;
        private static List<Tank> tanks;

        public static void Initialize()
        {
            tanks = new();

            PlayerJoined += OnPlayerJoin;
            PlayerLeft += OnPlayerLeft;
        }

        private static Tank OnPlayerJoin(IInputProvider inputProvider)
        {
            Tank tank = new Tank(Vector2Int.Zero, inputProvider);
            GameplayManager.Instantiate(tank);
            tanks.Add(tank);

            if (tanks.Count == 2)
                GameplayManager.ChangeState(GameplayManager.GameState.Game);

            return tank;
        }

        private static void OnPlayerLeft(IInputProvider inputProvider)
        {
            foreach (Tank tank in tanks)
            {
                if (tank.InputProvider == inputProvider)
                {
                    GameplayManager.Destroy(tank);
                    tanks.Remove(tank);
                    break;
                }
            }
        }

        public static void Dispose()
        {
            /*for (int i = tanks.Count - 1; i >= 0; i--)
                OnPlayerLeft(tanks[i].InputProvider);*/

            PlayerJoined -= OnPlayerJoin;
            PlayerLeft -= OnPlayerLeft;

            tanks.Clear();
        }
    }
}
