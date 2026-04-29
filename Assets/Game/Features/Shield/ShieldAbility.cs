using Automathon.Engine;
using Automathon.Engine.Physics;
using Automathon.Game.AbilitySystem;
using Automathon.Game.TankSystem;
using System;

namespace Automathon.Game.ShieldSystem
{
    public class ShieldAbility : Ability
    {
        private const int SHIELD_COOLDOWN_MILLIS = 3000;
        private const int spawnDistanceFromTank = 2000;
        public event Action<Shield> OnShieldActivated;
        public Tank tank { get; private set; }
        private GameplayManager gameplayManager;

        public ShieldAbility(Tank tank, Func<bool> shouldActivateParam, GameplayManager gameplayManager) : base(cooldown: SHIELD_COOLDOWN_MILLIS, shouldActivate: shouldActivateParam)
        {
            this.tank = tank;
            this.gameplayManager = gameplayManager;
        }

        protected override void Activate()
        {
            Debug.Log("Shield activated!");
            Vector2Int position = tank.Position + tank.lastMilliDirection * spawnDistanceFromTank / 1000;
            int rotationMilliRad = ((BoxCollider)tank.Rigidbody.Collider).RotationMillirad + 1571;
            Shield shieldEntity = new Shield(position, rotationMilliRad);
            OnShieldActivated?.Invoke(shieldEntity);
            gameplayManager.Instantiate(shieldEntity);
        }
    }
}