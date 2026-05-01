using Automathon.Engine;
using Automathon.Engine.Utility;
using Automathon.Game.AbilitySystem;
using Automathon.Game.TankSystem;
using System;

namespace Automathon.Game.ShieldSystem
{
    public class ShieldAbility : Ability
    {
        private const int SHIELD_COOLDOWN_MILLIS = 3000;
        private const int SPAWN_DISTANCE_FROM_TANK = 2000;
        public event Action<Shield> ShieldActivated;
        public Tank Tank { get; private set; }
        private GameplayManager gameplayManager;

        public ShieldAbility(Tank tank, Func<bool> shouldActivateParam, GameplayManager gameplayManager) : base(cooldown: SHIELD_COOLDOWN_MILLIS, shouldActivate: shouldActivateParam)
        {
            this.Tank = tank;
            this.gameplayManager = gameplayManager;
        }

        protected override void Activate()
        {
            Debug.Log("Shield activated!");
            Vector2Int position = Tank.Position + Tank.LastMilliDirection * SPAWN_DISTANCE_FROM_TANK / 1000;
            int rotationMilliRad = Tank.BoxCollider.RotationMillirad + IntMath.PI / 2;
            Shield shieldEntity = new Shield(position, rotationMilliRad);
            ShieldActivated?.Invoke(shieldEntity);
            gameplayManager.Instantiate(shieldEntity);
        }
    }
}