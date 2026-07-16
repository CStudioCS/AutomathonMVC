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

        [SerializeField] private ParticleSystem dashBurstParticleSystem;
        [SerializeField] private ParticleSystem dashFlame;

        [SerializeField] private HealthBarView healthBar;

        [SerializeField] private VisualEffect miniExplosion;
        [SerializeField] private SpriteRenderer[] sprites;
        [SerializeField] private Material tankGreenMaterial; // TankTwoTone material for team Green
        [SerializeField] private Material tankRedMaterial;   // TankTwoTone material for team Red

        private CameraShaker cameraShaker;

        // Yellow dash gradient (#ffe040). fadeAlpha=true tapers the tail to transparent.
        private static Gradient BuildDashGradient(bool fadeAlpha)
        {
            var g = new Gradient();
            GradientAlphaKey[] alpha = fadeAlpha
                ? new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                : new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };
            Color32 yellow = new Color32(0xff, 0xe0, 0x40, 0xff);
            g.SetKeys(
                new[] { new GradientColorKey(yellow, 0f), new GradientColorKey(yellow, 1f) },
                alpha);
            return g;
        }

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

            // Two-tone skin recolour via a per-team material asset (TankGreen/TankRed .mat).
            // Applies to the tank body + turret (the `sprites` set).
            Material teamMat = entity.Team == Tank.TeamType.Green ? tankGreenMaterial : tankRedMaterial;
            foreach (SpriteRenderer spriteRenderer in sprites)
            {
                spriteRenderer.sharedMaterial = teamMat;
                spriteRenderer.color = Color.white;       // shader ignores RGB; alpha stays for the dash fade
            }

            // The black tread marks came from the TrackMarks decal spawner (now drawn into the
            // mud layer instead), NOT the trail renderers — so only disable TrackMarks and keep
            // the normal/dash TrailRenderers, which are the flame-like tank trails.
            foreach (TrackMarks tm in GetComponentsInChildren<TrackMarks>(true))
                tm.enabled = false;

            // Yellow dash effects (#ffe040): flame particles + dash trails.
            foreach (ParticleSystem ps in new[] { dashFlame, dashBurstParticleSystem })
            {
                if (ps == null) continue;
                ParticleSystem.MainModule main = ps.main;
                main.startColor = new ParticleSystem.MinMaxGradient(BuildDashGradient(false)) { mode = ParticleSystemGradientMode.RandomColor };
                ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
                if (col.enabled) col.color = new ParticleSystem.MinMaxGradient(BuildDashGradient(true)); // override any baked-in over-lifetime tint
            }
            foreach (TrailRenderer tr in new[] { dashLeftTrailRenderer, dashRightTrailRenderer })
                if (tr != null) tr.colorGradient = BuildDashGradient(true);

            // The two driving trails behind the tank -> #ccff00 (fades out along their length).
            Gradient lime = new Gradient();
            lime.SetKeys(
                new[] { new GradientColorKey(new Color32(204, 255, 0, 255), 0f), new GradientColorKey(new Color32(204, 255, 0, 255), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            foreach (TrailRenderer tr in new[] { normalLeftTrailRenderer, normalRightTrailRenderer })
                if (tr != null) tr.colorGradient = lime;
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

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
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
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
