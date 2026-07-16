using Assets.Game.View;
using Automathon.Game.Input;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace Automathon.Game
{
    public class TankView : EntityView<Tank>
    {
        [SerializeField] private float bulletShakingIntensity;
        [SerializeField] private float bulletShakingDuration;

        [SerializeField] private float bulletCameraShakingIntensity;
        [SerializeField] private float bulletCameraShakingDuration;

        [SerializeField] private float DashShakingIntensity;
        [SerializeField] private float DashCameraShakingIntensity;
        [SerializeField] private AudioSource DashA;
        [SerializeField] private TrailRenderer normalLeftTrailRenderer;
        [SerializeField] private TrailRenderer normalRightTrailRenderer;
        [SerializeField] private TrailRenderer dashLeftTrailRenderer;
        [SerializeField] private TrailRenderer dashRightTrailRenderer;
        public bool IsDashing { get; private set; } = false;

        [SerializeField] private Transform turret;
        [SerializeField] private Transform turretTip;
        [SerializeField] private Transform body;
        [SerializeField] private float bodyTurnTime = 0.1f; // characteristic time (s) for the body's smooth turn toward the move direction

        [SerializeField] private ParticleSystem dashBurstParticleSystem;
        [SerializeField] private ParticleSystem dashFlame;

        [SerializeField] private HealthBarView healthBar;

        [SerializeField] private VisualEffect miniExplosion;
        [SerializeField] private SpriteRenderer[] sprites;          // dimmed during the dash fade (SetAlpha)
        [SerializeField] private SpriteRenderer[] teamColorSprites; // recoloured with this player's team colour
        [SerializeField] private Color player1Color = new Color32(0x00, 0x9b, 0x00, 0xff); // #009b00 team Green (P1)
        [SerializeField] private Color player2Color = new Color32(0xff, 0x66, 0x00, 0xff); // #ff6600 team Red (P2)

        private CameraShaker cameraShaker;
        private Quaternion smoothedBodyRotation;
        private bool bodyRotationInitialized;

        public override void Initialize(Tank entity)
        {
            base.Initialize(entity);

            healthBar.Bind(() => (float)Entity.Health.CurrentHealth / Entity.Health.MaxHealth);

            //Entity.BulletAbility.AbilityActivated += OnShooting;
            //Entity.GrenadeAbility.AbilityActivated += OnGrenadeAbility;
            Entity.MachineGunAbility.MachineGunFired += () => SoundManager.instance.PlaySound("MachineGun");
            Entity.MachineGunAbility.BulletShot += OnMachineGunAbilityBulletShot;
            Entity.DashAbility.AbilityActivated += OnDashAbility;
            Entity.MissileAbility.AbilityActivated += OnMissileAbility;

            if (entity.InputProvider is PlayerInputProvider p)
                p.Setup(this);

            // Team colour: tint the serialized subset of sprites with this player's team colour.
            Color teamColor = entity.Team == Tank.TeamType.Green ? player1Color : player2Color;
            foreach (SpriteRenderer spriteRenderer in teamColorSprites)
                if (spriteRenderer != null) spriteRenderer.color = teamColor;

            // Dash flame/burst particles and the dash + driving trail renderers are coloured on
            // the prefab components (authored in the editor).
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            // View-only smoothing: base.LateUpdate snaps the root to the entity's move-direction
            // rotation; ease the body toward it instead. Shortest-arc via Slerp, frame-rate-
            // independent exponential approach (time constant = bodyTurnTime). First frame snaps so
            // the body spawns already facing its direction.
            if (body != null)
            {
                Quaternion target = transform.rotation;
                if (!bodyRotationInitialized)
                {
                    smoothedBodyRotation = target;
                    bodyRotationInitialized = true;
                }
                else
                {
                    float k = bodyTurnTime > 0f ? 1f - Mathf.Exp(-Time.deltaTime / bodyTurnTime) : 1f;
                    smoothedBodyRotation = Quaternion.Slerp(smoothedBodyRotation, target, k);
                }
                body.rotation = smoothedBodyRotation;
            }

            turret.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(Entity.LastMilliDirection.Y, Entity.LastMilliDirection.X));
        }

        private void OnMachineGunAbilityBulletShot()
        {
            if (!cameraShaker)
            {
                cameraShaker = Camera.main.GetComponent<CameraShaker>();
            }

            StartCoroutine(Shaker.Shake(turret, bulletShakingDuration, bulletShakingIntensity));
            cameraShaker.CameraShake(bulletShakingDuration, bulletCameraShakingIntensity);

            EmitShootingMiniExplosion();
        }

        public void SetAlpha(float alpha)
        {
            foreach (SpriteRenderer sr in sprites)
            {
                if (sr == null) continue;
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        IEnumerator Dash(int dashDurationMili)
        {
            float dashDuration = (float)dashDurationMili / 1000f;

            SetAlpha(0.5f);

            dashFlame.Play();
            float timer = -0.10f;

            SetDashTrail();

            while (timer < dashDuration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            dashFlame.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            SetAlpha(1f);
            SetNormalTrail();
            IsDashing = false;
        }

        private void SetDashTrail()
        {
            normalLeftTrailRenderer.emitting = false;
            normalRightTrailRenderer.emitting = false;
            dashLeftTrailRenderer.Clear();
            dashRightTrailRenderer.Clear();
            dashLeftTrailRenderer.emitting = true;
            dashRightTrailRenderer.emitting = true;
        }

        private void SetNormalTrail()
        {
            normalLeftTrailRenderer.emitting = true;
            normalRightTrailRenderer.emitting = true;
            dashLeftTrailRenderer.emitting = false;
            dashRightTrailRenderer.emitting = false;
        }

        private void OnDashAbility()
        {
            SoundManager.instance.PlaySound("Dash");
            IsDashing = true;
            if (!cameraShaker)
            {
                cameraShaker = Camera.main.GetComponent<CameraShaker>();
            }
            StartCoroutine(Shaker.Translate(turret, new Vector2(0, -1), DashAbility.DASH_DURATION_MILLIS, DashShakingIntensity));
            cameraShaker.CameraTranslate(body.right, DashAbility.DASH_DURATION_MILLIS, DashCameraShakingIntensity);

            //dashBurstParticleSystem.Play();  imo pas besoin de burst initiale mais bon c'est implémenté quoi
            StartCoroutine(Dash(DashAbility.DASH_DURATION_MILLIS));
        }

        private void OnGrenadeAbility()
        {
            //No shake when shooting a grenade imo
            //StartCoroutine(Shaker.Shake(turret, grenadeShakingDuration, grenadeShakingIntensity));
        }

        private void OnMissileAbility()
        {
            EmitShootingMiniExplosion();
        }

        protected override void OnDestroy()
        {
            UnSub();
            base.OnDestroy();
        }

        protected override void OnControllerDestroyed()
        {
            UnSub();
            base.OnControllerDestroyed();
        }

        private void EmitShootingMiniExplosion()
        {
            VisualEffect miniExplosionVFX = Instantiate(miniExplosion, turretTip.position, Quaternion.identity);
            miniExplosionVFX.SetVector2("InitVelocity", Entity.Rigidbody.Velocity.ToVector2Scaled());
            float rot = turret.rotation.eulerAngles.z * Mathf.Deg2Rad;
            miniExplosionVFX.SetVector2("Direction", new Vector2(Mathf.Cos(rot), Mathf.Sin(rot)));
        }

        private void UnSub()
        {
            //Entity.BulletAbility.AbilityActivated -= OnShooting;
            //Entity.GrenadeAbility.AbilityActivated -= OnGrenadeAbility;
            Entity.MachineGunAbility.AbilityActivated -= OnMachineGunAbilityBulletShot;
            Entity.MissileAbility.AbilityActivated -= OnMissileAbility;
            Entity.DashAbility.AbilityActivated -= OnDashAbility;
        }
    }
}
