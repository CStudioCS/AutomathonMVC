using System.Collections.Generic;
using UnityEngine;

namespace Automathon.Game.View
{
    /// <summary>
    /// Cell-shaded mud ground over a lightweight viscous fluid. A velocity field (injected by
    /// tanks + explosions, damped) advects three "dye" buffers whose CHANNELS are per-type
    /// intensities (not blended colour):
    ///   slow: R=trail, G=explosion | fast: G=shield | push: R=bullet, G=missile.
    /// Each buffer fades/blurs/advects the same as before. The ground shader picks the max
    /// intensity per pixel and shows that type's TRUE colour (else base mud), smoothstep-AA'd.
    ///
    /// SETUP: one MudLayerController on a GameObject (already in Prototyping). Shaders load via
    /// Shader.Find; buffers + ground quad self-create. TankView disables the black track marks;
    /// MissileView pushes explosions via AddExplosionMark.
    ///
    /// NOTE: tuning values are CONSTANTS for now; convert to [SerializeField] before pushing.
    /// </summary>
    public class MudLayerController : MonoBehaviour
    {
        private const int MaxCircle = 128;
        private const int MaxPush = 8;
        private const int MaxBlast = 8;

        private const int Resolution = 512; // render-texture size (compile-time)

        [Header("Arena")]
        [SerializeField] private Vector2 ArenaCenter = Vector2.zero;
        [SerializeField] private Vector2 ArenaSize = new Vector2(32f, 18f);
        [SerializeField] private float GroundZ = 20f;
        [SerializeField] private float MoveForFullTrace = 0.1f;
        [SerializeField] private float DepositBehind = 0.4f;

        // Read by the sibling MudAmbientParticles component.
        public Vector2 ArenaCenterWorld => ArenaCenter;
        public Vector2 ArenaSizeWorld => ArenaSize;

        [Header("Fluid velocity field (world units/frame)")]
        [SerializeField] private float VelDamping = 0.83f;
        [SerializeField] private float VelDiffuse = 0.18f;
        [SerializeField] private float VelBlurSize = 2f;
        [SerializeField] private float AdvectScaleSlow = 1.05f;
        [SerializeField] private float AdvectScaleFast = 0.05f;
        [SerializeField] private float AdvectScalePush = 1.05f;
        [SerializeField] private float PushRadius = 1.8f;
        [SerializeField] private float TankVelInject = 0.14f;
        [SerializeField] private float BlastRadius = 4f;
        [SerializeField] private float BlastSpeed = 0.45f;

        [Header("Fade / blur per buffer")]
        [SerializeField] private float FadeSlow = 0.985f;
        [SerializeField] private float BlurSizeSlow = 2.5f;
        [SerializeField] private float BlurRateSlow = 0.1f;
        [SerializeField] private float FadeFast = 0.998f;
        [SerializeField] private float BlurSizeFast = 2f;
        [SerializeField] private float BlurRateFast = 0.02f;
        [SerializeField] private float FadePush = 0.97f;
        [SerializeField] private float BlurSizePush = 2f;
        [SerializeField] private float BlurRatePush = 0.3f;

        [Header("Tank trail")]
        [SerializeField] private float TrailRadius = 0.45f;
        [SerializeField] private float TrailStrength = 0.4f;

        [Header("Bullet marks")]
        [SerializeField] private float BulletRadius = 0.1f;
        [SerializeField] private float BulletStrength = 0.3f;
        [SerializeField] private float BulletStampSpacing = 0.07f;
        [SerializeField] private int BulletMaxSteps = 12;

        [Header("Missile marks")]
        [SerializeField] private float MissileRadius = 0.4f;
        [SerializeField] private float MissileStrength = 0.4f;
        [SerializeField] private float MissileStampSpacing = 0.08f;
        [SerializeField] private int MissileMaxSteps = 12;

        [Header("Shield marks")]
        [SerializeField] private float ShieldRadius = 0.25f;
        [SerializeField] private float ShieldStrength = 0.5f;
        [SerializeField] private float ShieldMoveThreshold = 0.02f;

        [Header("Explosion marks")]
        [SerializeField] private float ExplosionStrength = 1f;
        [SerializeField] private float ExplosionRadiusScale = 3f;

        // Ground-shader palette (Cyberpunk Neon City). Mud base ramps through the 4 blues by
        // noise; a tank trace lightens it toward Mud3. Marks are the darker purples.
        [Header("Palette")]
        [SerializeField] private Color Mud0 = new Color32(13, 34, 64, 255);      // #0d2240 darkest
        [SerializeField] private Color Mud1 = new Color32(10, 58, 92, 255);      // #0a3a5c
        [SerializeField] private Color Mud2 = new Color32(16, 80, 128, 255);     // #105080
        [SerializeField] private Color Mud3 = new Color32(24, 112, 168, 255);    // #1870a8 brightest
        [SerializeField] private Color TrailColor = new Color32(24, 112, 168, 255); // #1870a8 (lightens the mud)
        [SerializeField] private Color BulletColor = new Color32(18, 16, 30, 255);  // #12101e
        [SerializeField] private Color MissileColor = new Color32(28, 24, 48, 255); // #1c1830
        [SerializeField] private Color ExplosionColor = new Color32(10, 10, 20, 255);// #0a0a14
        [SerializeField] private Color ShieldColor = new Color32(42, 36, 69, 255);  // #2a2445

        [Header("Stain / warp (domain-warped noise + fluid drag)")]
        [SerializeField] private float StainWorldSize = 4f;
        [SerializeField] private float StainWarp = 0.8f;
        [SerializeField] private float WarpAdvect = 1f;
        [SerializeField] private float WarpGain = 1.15f;
        [SerializeField] private float WarpDamp = 0.97f;
        [SerializeField] private float WarpMax = 12f;
        [SerializeField] private float WarpDisplace = 1.6f;

        private sealed class TankMudState
        {
            public Vector3 LastCenter;
        }

        private struct PendingMark
        {
            public Vector2 Pos;
            public float Radius;
            public float Strength;
        }

        private static MudLayerController instance;

        private RenderTexture slowA, slowB, fastA, fastB, pushA, pushB, velA, velB;
        private RenderTexture warpA, warpB;
        private Material updateMat;
        private Material velMat;
        private Material groundMat;
        private Material warpMat;

        private readonly Vector4[] slowCirclePos = new Vector4[MaxCircle];
        private readonly Vector4[] slowCircleColor = new Vector4[MaxCircle];
        private int slowCircleCount;
        private readonly Vector4[] fastCirclePos = new Vector4[MaxCircle];
        private readonly Vector4[] fastCircleColor = new Vector4[MaxCircle];
        private int fastCircleCount;
        private readonly Vector4[] pushCirclePos = new Vector4[MaxCircle];
        private readonly Vector4[] pushCircleColor = new Vector4[MaxCircle];
        private int pushCircleCount;
        private readonly Vector4[] pusherPos = new Vector4[MaxPush];
        private readonly Vector4[] pusherVel = new Vector4[MaxPush];
        private int pushCount;
        private readonly Vector4[] blastPos = new Vector4[MaxBlast];
        private int blastCount;

        private readonly Dictionary<TankView, TankMudState> tankStates = new();
        private readonly Dictionary<ShieldView, Vector3> shieldLastPos = new();
        private readonly Dictionary<BulletView, Vector3> bulletLastPos = new();
        private readonly Dictionary<MissileView, Vector3> missileLastPos = new();
        private readonly List<PendingMark> pending = new();
        private readonly List<TankView> staleTankKeys = new();
        private readonly List<ShieldView> staleShieldKeys = new();
        private readonly List<BulletView> staleBulletKeys = new();
        private readonly List<MissileView> staleMissileKeys = new();

        private static readonly int MudBufferSlowId = Shader.PropertyToID("_MudBufferSlow");
        private static readonly int MudBufferFastId = Shader.PropertyToID("_MudBufferFast");
        private static readonly int MudBufferPushId = Shader.PropertyToID("_MudBufferPush");
        private static readonly int Mud0Id = Shader.PropertyToID("_Mud0");
        private static readonly int Mud1Id = Shader.PropertyToID("_Mud1");
        private static readonly int Mud2Id = Shader.PropertyToID("_Mud2");
        private static readonly int Mud3Id = Shader.PropertyToID("_Mud3");
        private static readonly int StainWarpId = Shader.PropertyToID("_StainWarp");
        private static readonly int NoiseScaleId = Shader.PropertyToID("_NoiseScale");
        private static readonly int WarpBufferId = Shader.PropertyToID("_WarpBuffer");
        private static readonly int WarpAdvectId = Shader.PropertyToID("_WarpAdvect");
        private static readonly int WarpGainId = Shader.PropertyToID("_WarpGain");
        private static readonly int WarpDampId = Shader.PropertyToID("_WarpDamp");
        private static readonly int WarpMaxId = Shader.PropertyToID("_WarpMax");
        private static readonly int WarpDisplaceId = Shader.PropertyToID("_WarpDisplace");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");
        private static readonly int BlurSizeId = Shader.PropertyToID("_BlurSize");
        private static readonly int BlurRateId = Shader.PropertyToID("_BlurRate");
        private static readonly int ArenaSizeId = Shader.PropertyToID("_ArenaSize");
        private static readonly int VelBufferId = Shader.PropertyToID("_VelBuffer");
        private static readonly int AdvectScaleId = Shader.PropertyToID("_AdvectScale");
        private static readonly int CircleCountId = Shader.PropertyToID("_CircleCount");
        private static readonly int CirclePosId = Shader.PropertyToID("_CirclePos");
        private static readonly int CircleColorId = Shader.PropertyToID("_CircleColor");
        private static readonly int VelDampingId = Shader.PropertyToID("_VelDamping");
        private static readonly int VelDiffuseId = Shader.PropertyToID("_VelDiffuse");
        private static readonly int VelBlurSizeId = Shader.PropertyToID("_VelBlurSize");
        private static readonly int PushCountId = Shader.PropertyToID("_PushCount");
        private static readonly int PusherPosId = Shader.PropertyToID("_PusherPos");
        private static readonly int PusherVelId = Shader.PropertyToID("_PusherVel");
        private static readonly int BlastCountId = Shader.PropertyToID("_BlastCount");
        private static readonly int BlastPosId = Shader.PropertyToID("_BlastPos");

        public static void AddExplosionMark(Vector2 worldPos, float worldRadius)
        {
            if (instance == null) return;
            instance.pending.Add(new PendingMark
            {
                Pos = worldPos,
                Radius = worldRadius * instance.ExplosionRadiusScale,
                Strength = instance.ExplosionStrength
            });
        }

        private void Awake()
        {
            instance = this;

            Shader updateShader = Shader.Find("Hidden/Automathon/MudBufferUpdate");
            Shader velShader = Shader.Find("Hidden/Automathon/MudVelocityUpdate");
            Shader warpShader = Shader.Find("Hidden/Automathon/MudWarpUpdate");
            Shader groundShader = Shader.Find("Automathon/MudGround");
            if (updateShader == null || velShader == null || warpShader == null || groundShader == null)
            {
                UnityEngine.Debug.LogError("[MudLayer] Missing shaders. Disabling.");
                enabled = false;
                return;
            }

            updateMat = new Material(updateShader);
            velMat = new Material(velShader);
            warpMat = new Material(warpShader);
            groundMat = new Material(groundShader);

            slowA = CreateBuffer(RenderTextureFormat.ARGBHalf);
            slowB = CreateBuffer(RenderTextureFormat.ARGBHalf);
            fastA = CreateBuffer(RenderTextureFormat.ARGBHalf);
            fastB = CreateBuffer(RenderTextureFormat.ARGBHalf);
            pushA = CreateBuffer(RenderTextureFormat.ARGBHalf);
            pushB = CreateBuffer(RenderTextureFormat.ARGBHalf);
            velA = CreateBuffer(RenderTextureFormat.RGHalf);
            velB = CreateBuffer(RenderTextureFormat.RGHalf);
            warpA = CreateBuffer(RenderTextureFormat.RGHalf);
            warpB = CreateBuffer(RenderTextureFormat.RGHalf);

            CreateGroundQuad();
            groundMat.SetVector(Mud0Id, LinRGB(Mud0));
            groundMat.SetVector(Mud1Id, LinRGB(Mud1));
            groundMat.SetVector(Mud2Id, LinRGB(Mud2));
            groundMat.SetVector(Mud3Id, LinRGB(Mud3));
            groundMat.SetFloat(StainWarpId, StainWarp);
            groundMat.SetVector(NoiseScaleId, new Vector2(ArenaSize.x / StainWorldSize, ArenaSize.y / StainWorldSize));
            groundMat.SetVector(ArenaSizeId, ArenaSize);
            groundMat.SetFloat(WarpDisplaceId, WarpDisplace);
        }

        private RenderTexture CreateBuffer(RenderTextureFormat format)
        {
            var rt = new RenderTexture(Resolution, Resolution, 0, format)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "MudBuffer"
            };
            rt.Create();

            RenderTexture prev = RenderTexture.active;
            Graphics.SetRenderTarget(rt);
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = prev;
            return rt;
        }

        private void CreateGroundQuad()
        {
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "MudGroundQuad";
            quad.transform.SetParent(transform, false);

            Collider col = quad.GetComponent<Collider>();
            if (col != null) Destroy(col);

            quad.transform.position = new Vector3(ArenaCenter.x, ArenaCenter.y, GroundZ);
            quad.transform.localScale = new Vector3(ArenaSize.x, ArenaSize.y, 1f);

            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = groundMat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            // Ground backdrop: render on the "Behind" sorting layer so everything on Default
            // (tank body, walls, and the order -1 tank trails) draws over it.
            renderer.sortingLayerName = "Behind";
            renderer.sortingOrder = 0;
        }

        private void LateUpdate()
        {
            if (updateMat == null) return;

            GatherStamps();

            // 1) Velocity field.
            velMat.SetVector(ArenaSizeId, ArenaSize);
            velMat.SetFloat(VelDampingId, VelDamping);
            velMat.SetFloat(VelDiffuseId, VelDiffuse);
            velMat.SetFloat(VelBlurSizeId, VelBlurSize);
            velMat.SetInt(PushCountId, pushCount);
            velMat.SetVectorArray(PusherPosId, pusherPos);
            velMat.SetVectorArray(PusherVelId, pusherVel);
            velMat.SetInt(BlastCountId, blastCount);
            velMat.SetVectorArray(BlastPosId, blastPos);
            Graphics.Blit(velA, velB, velMat);
            (velA, velB) = (velB, velA);

            // 1b) Accumulated displacement field that drags the base stain texture.
            warpMat.SetVector(ArenaSizeId, ArenaSize);
            warpMat.SetTexture(VelBufferId, velA);
            warpMat.SetFloat(WarpAdvectId, WarpAdvect);
            warpMat.SetFloat(WarpGainId, WarpGain);
            warpMat.SetFloat(WarpDampId, WarpDamp);
            warpMat.SetFloat(WarpMaxId, WarpMax);
            Graphics.Blit(warpA, warpB, warpMat);
            (warpA, warpB) = (warpB, warpA);
            groundMat.SetTexture(WarpBufferId, warpA);

            // 2) Dye buffers (advected by the field), fade/blur/stamp.
            updateMat.SetVector(ArenaSizeId, ArenaSize);
            updateMat.SetTexture(VelBufferId, velA);

            BlitColour(ref slowA, ref slowB, FadeSlow, BlurSizeSlow, BlurRateSlow, AdvectScaleSlow,
                slowCircleCount, slowCirclePos, slowCircleColor);
            BlitColour(ref fastA, ref fastB, FadeFast, BlurSizeFast, BlurRateFast, AdvectScaleFast,
                fastCircleCount, fastCirclePos, fastCircleColor);
            BlitColour(ref pushA, ref pushB, FadePush, BlurSizePush, BlurRatePush, AdvectScalePush,
                pushCircleCount, pushCirclePos, pushCircleColor);

            groundMat.SetTexture(MudBufferSlowId, slowA);
            groundMat.SetTexture(MudBufferFastId, fastA);
            groundMat.SetTexture(MudBufferPushId, pushA);
        }

        private void BlitColour(ref RenderTexture a, ref RenderTexture b, float fade, float blurSize,
            float blurRate, float advectScale, int circleCount, Vector4[] circlePos, Vector4[] circleColor)
        {
            updateMat.SetFloat(FadeId, fade);
            updateMat.SetFloat(BlurSizeId, blurSize);
            updateMat.SetFloat(BlurRateId, blurRate);
            updateMat.SetFloat(AdvectScaleId, advectScale);
            updateMat.SetInt(CircleCountId, circleCount);
            updateMat.SetVectorArray(CirclePosId, circlePos);
            updateMat.SetVectorArray(CircleColorId, circleColor);
            Graphics.Blit(a, b, updateMat);
            (a, b) = (b, a);
        }

        private void GatherStamps()
        {
            slowCircleCount = 0;
            fastCircleCount = 0;
            pushCircleCount = 0;
            pushCount = 0;
            blastCount = 0;
            Vector2 min = ArenaCenter - ArenaSize * 0.5f;

            GatherTankMarks(min);
            GatherBulletMarks(min);
            GatherMissileMarks(min);
            GatherShieldMarks(min);

            for (int i = 0; i < pending.Count; i++)
            {
                AddPushCircle(pending[i].Pos, pending[i].Radius, ExplosionColor, pending[i].Strength, min);
                AddBlast(pending[i].Pos, min);
            }
            pending.Clear();
        }

        private void GatherTankMarks(Vector2 min)
        {
            TankView[] tanks = FindObjectsByType<TankView>(FindObjectsSortMode.None);
            for (int i = 0; i < tanks.Length; i++)
            {
                TankView tank = tanks[i];
                Vector3 p3 = tank.transform.position;
                Vector2 p = new Vector2(p3.x, p3.y);

                if (!tankStates.TryGetValue(tank, out TankMudState st))
                {
                    st = new TankMudState { LastCenter = p3 };
                    tankStates[tank] = st;
                    continue;
                }

                Vector2 delta = p - new Vector2(st.LastCenter.x, st.LastCenter.y);
                float moved = delta.magnitude;
                st.LastCenter = p3;

                float strength = TrailStrength * Mathf.Clamp01(moved / Mathf.Max(MoveForFullTrace, 1e-4f));
                if (strength <= 0.0001f || moved <= 1e-5f) continue;

                Vector2 dir = delta / moved;
                Vector2 basePos = p - dir * DepositBehind;

                AddPusher(p, delta, min);
                AddSlowCircle(basePos, TrailRadius, TrailColor, strength, min);
            }

            PruneTankStates();
        }

        private void GatherBulletMarks(Vector2 min)
        {
            BulletView[] bullets = FindObjectsByType<BulletView>(FindObjectsSortMode.None);
            for (int i = 0; i < bullets.Length; i++)
            {
                BulletView bullet = bullets[i];
                Vector3 p3 = bullet.transform.position;
                Vector2 cur = new Vector2(p3.x, p3.y);

                if (bulletLastPos.TryGetValue(bullet, out Vector3 last3))
                {
                    Vector2 last = new Vector2(last3.x, last3.y);
                    float segLen = Vector2.Distance(cur, last);
                    int steps = Mathf.Clamp(Mathf.CeilToInt(segLen / BulletStampSpacing), 1, BulletMaxSteps);
                    for (int s = 1; s <= steps; s++)
                        AddPushCircle(Vector2.Lerp(last, cur, (float)s / steps), BulletRadius, BulletColor, BulletStrength, min);
                }
                else
                {
                    AddPushCircle(cur, BulletRadius, BulletColor, BulletStrength, min);
                }

                bulletLastPos[bullet] = p3;
            }

            PruneBullets();
        }

        private void GatherMissileMarks(Vector2 min)
        {
            MissileView[] missiles = FindObjectsByType<MissileView>(FindObjectsSortMode.None);
            for (int i = 0; i < missiles.Length; i++)
            {
                MissileView missile = missiles[i];
                Vector3 p3 = missile.transform.position;
                Vector2 cur = new Vector2(p3.x, p3.y);

                if (missileLastPos.TryGetValue(missile, out Vector3 last3))
                {
                    Vector2 last = new Vector2(last3.x, last3.y);
                    float segLen = Vector2.Distance(cur, last);
                    int steps = Mathf.Clamp(Mathf.CeilToInt(segLen / MissileStampSpacing), 1, MissileMaxSteps);
                    for (int s = 1; s <= steps; s++)
                        AddPushCircle(Vector2.Lerp(last, cur, (float)s / steps), MissileRadius, MissileColor, MissileStrength, min);
                }
                else
                {
                    AddPushCircle(cur, MissileRadius, MissileColor, MissileStrength, min);
                }

                missileLastPos[missile] = p3;
            }

            PruneMissiles();
        }

        private void GatherShieldMarks(Vector2 min)
        {
            ShieldView[] shields = FindObjectsByType<ShieldView>(FindObjectsSortMode.None);
            for (int i = 0; i < shields.Length; i++)
            {
                ShieldView shield = shields[i];
                Vector3 p = shield.transform.position;
                if (shieldLastPos.TryGetValue(shield, out Vector3 last)
                    && (p - last).sqrMagnitude >= ShieldMoveThreshold * ShieldMoveThreshold)
                {
                    AddFastCircle(new Vector2(p.x, p.y), ShieldRadius, ShieldColor, ShieldStrength, min);
                }
                shieldLastPos[shield] = p;
            }

            PruneShields();
        }

        private void AddSlowCircle(Vector2 world, float radius, Color color, float strength, Vector2 min)
        {
            if (slowCircleCount >= MaxCircle) return;
            Vector2 uv = Uv(world, min);
            slowCirclePos[slowCircleCount] = new Vector4(uv.x, uv.y, radius, strength);
            slowCircleColor[slowCircleCount] = LinRGB(color);
            slowCircleCount++;
        }

        private void AddFastCircle(Vector2 world, float radius, Color color, float strength, Vector2 min)
        {
            if (fastCircleCount >= MaxCircle) return;
            Vector2 uv = Uv(world, min);
            fastCirclePos[fastCircleCount] = new Vector4(uv.x, uv.y, radius, strength);
            fastCircleColor[fastCircleCount] = LinRGB(color);
            fastCircleCount++;
        }

        private void AddPushCircle(Vector2 world, float radius, Color color, float strength, Vector2 min)
        {
            if (pushCircleCount >= MaxCircle) return;
            Vector2 uv = Uv(world, min);
            pushCirclePos[pushCircleCount] = new Vector4(uv.x, uv.y, radius, strength);
            pushCircleColor[pushCircleCount] = LinRGB(color);
            pushCircleCount++;
        }

        private void AddPusher(Vector2 world, Vector2 worldDelta, Vector2 min)
        {
            if (pushCount >= MaxPush) return;
            Vector2 uv = Uv(world, min);
            pusherPos[pushCount] = new Vector4(uv.x, uv.y, PushRadius, 0f);
            pusherVel[pushCount] = new Vector4(worldDelta.x * TankVelInject, worldDelta.y * TankVelInject, 0f, 0f);
            pushCount++;
        }

        private void AddBlast(Vector2 world, Vector2 min)
        {
            if (blastCount >= MaxBlast) return;
            Vector2 uv = Uv(world, min);
            blastPos[blastCount] = new Vector4(uv.x, uv.y, BlastRadius, BlastSpeed);
            blastCount++;
        }

        private Vector2 Uv(Vector2 world, Vector2 min)
            => new Vector2((world.x - min.x) / ArenaSize.x, (world.y - min.y) / ArenaSize.y);

        // Project is Linear color space; the custom mud shader outputs colours directly, so
        // convert sRGB -> linear before handing them over (Unity doesn't do it for SetColor/
        // SetVectorArray into custom shaders).
        private static Vector4 LinRGB(Color c)
        {
            Color l = c.linear;
            return new Vector4(l.r, l.g, l.b, l.a);
        }

        // World-space flow at a point, from the same pushers/blasts that drive the fluid field.
        // Used by the ambient particles so they drift with the mud's flow.
        public static Vector2 SampleFlow(Vector2 worldPos)
            => instance != null ? instance.SampleFlowInternal(worldPos) : Vector2.zero;

        private Vector2 SampleFlowInternal(Vector2 worldPos)
        {
            const float reach = 3f; // particles feel moving objects over a wider area than the mud itself
            Vector2 min = ArenaCenter - ArenaSize * 0.5f;
            Vector2 flow = Vector2.zero;

            for (int i = 0; i < pushCount; i++)
            {
                Vector2 pw = min + Vector2.Scale(new Vector2(pusherPos[i].x, pusherPos[i].y), ArenaSize);
                float f = Mathf.Clamp01(1f - Vector2.Distance(worldPos, pw) / Mathf.Max(pusherPos[i].z * reach, 1e-4f));
                flow += new Vector2(pusherVel[i].x, pusherVel[i].y) * f;
            }
            for (int i = 0; i < blastCount; i++)
            {
                Vector2 bw = min + Vector2.Scale(new Vector2(blastPos[i].x, blastPos[i].y), ArenaSize);
                Vector2 dir = worldPos - bw;
                float d = dir.magnitude;
                float f = Mathf.Clamp01(1f - d / Mathf.Max(blastPos[i].z * reach, 1e-4f));
                if (d > 1e-4f) flow += (dir / d) * blastPos[i].w * f;
            }
            return flow;
        }

        private void PruneTankStates()
        {
            staleTankKeys.Clear();
            foreach (KeyValuePair<TankView, TankMudState> kv in tankStates)
                if (kv.Key == null) staleTankKeys.Add(kv.Key);
            for (int i = 0; i < staleTankKeys.Count; i++)
                tankStates.Remove(staleTankKeys[i]);
        }

        private void PruneShields()
        {
            staleShieldKeys.Clear();
            foreach (KeyValuePair<ShieldView, Vector3> kv in shieldLastPos)
                if (kv.Key == null) staleShieldKeys.Add(kv.Key);
            for (int i = 0; i < staleShieldKeys.Count; i++)
                shieldLastPos.Remove(staleShieldKeys[i]);
        }

        private void PruneBullets()
        {
            staleBulletKeys.Clear();
            foreach (KeyValuePair<BulletView, Vector3> kv in bulletLastPos)
                if (kv.Key == null) staleBulletKeys.Add(kv.Key);
            for (int i = 0; i < staleBulletKeys.Count; i++)
                bulletLastPos.Remove(staleBulletKeys[i]);
        }

        private void PruneMissiles()
        {
            staleMissileKeys.Clear();
            foreach (KeyValuePair<MissileView, Vector3> kv in missileLastPos)
                if (kv.Key == null) staleMissileKeys.Add(kv.Key);
            for (int i = 0; i < staleMissileKeys.Count; i++)
                missileLastPos.Remove(staleMissileKeys[i]);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
            if (slowA != null) slowA.Release();
            if (slowB != null) slowB.Release();
            if (fastA != null) fastA.Release();
            if (fastB != null) fastB.Release();
            if (pushA != null) pushA.Release();
            if (pushB != null) pushB.Release();
            if (velA != null) velA.Release();
            if (velB != null) velB.Release();
            if (warpA != null) warpA.Release();
            if (warpB != null) warpB.Release();
            if (updateMat != null) Destroy(updateMat);
            if (velMat != null) Destroy(velMat);
            if (warpMat != null) Destroy(warpMat);
            if (groundMat != null) Destroy(groundMat);
        }
    }
}
