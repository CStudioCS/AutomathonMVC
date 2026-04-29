using Automathon.Engine;
using Automathon.Game.Input;
using Automathon.Game.TankSystem;
using System.Collections.Generic;

namespace Automathon.Game.Lobby.MultiTankManagement
{
    public class MultiTankManager
    {
        List<Tank> tanks = new();
        public static MultiTankManager Instance { get; private set; }
        public void Start()
        {
            if (Instance != null)
                return;
            Instance = this;
        }

        public Tank CreateTank(PlayerInputProvider playerInputProvider)
        {
            Tank tank = new Tank(Vector2Int.Zero, playerInputProvider);
            GameplayManager.Instance.Instantiate(tank);
            tanks.Add(tank);
            return tank;
        }

        public void OnPlayerLeft(Tank tank)
        {
            GameplayManager.Instance.Destroy(tank);
            tanks.Remove(tank);
        }

        public bool IsGameReady()
        {
            if (tanks.Count < 2)
                return false;

            foreach (Tank tank in tanks)
            {
                if (!tank.IsReady)
                    return false;
            }
            return true;
        }
    }
}
