using UnityEngine;

namespace Automathon.Game
{
    // Drives the Automathon/NeonZapper shader. Replaces NeonOutline: no runtime GameObject,
    // no LineRenderer, no LateUpdate -- the SDF lives in the quad's local space, so the
    // visual follows any transform (move/rotate/parent scale) for free. Shields no longer
    // need a `follow` flag.
    //
    // Put this on the child GameObject that holds the SpriteRenderer (never on the view root,
    // since it drives localScale). The sprite must be a plain white square imported with
    // Mesh Type = Full Rect, and the SpriteRenderer must be in Simple draw mode.
    [RequireComponent(typeof(SpriteRenderer))]
    [ExecuteAlways]
    public class NeonZapper : MonoBehaviour
    {
        [SerializeField] private float glowPadding = 0.5f; // world units of quad bleed for the halo

        private const int ImpactSlots = 4;   // must match IMPACT_SLOTS in the shader

        private static readonly int SizeId = Shader.PropertyToID("_Size");
        private static readonly int ImpactsId = Shader.PropertyToID("_Impacts");
        private static readonly int PadId = Shader.PropertyToID("_Pad");
        private static readonly int PhaseId = Shader.PropertyToID("_Phase");

        private SpriteRenderer sr;
        private MaterialPropertyBlock mpb;
        private Vector2 size = Vector2.one;
        private readonly Vector4[] impacts = new Vector4[ImpactSlots];
        private int nextSlot;

        private void Awake()
        {
            // y far in the past + z = 0 so idle slots contribute exactly nothing.
            for (int i = 0; i < ImpactSlots; i++) impacts[i] = new Vector4(0f, -1000f, 0f, 0f);
        }

        // t = the Engine's OnHit parameter, 0..1000 along the wall's long axis
        // (0 = the -right/-up end in local space, 1000 = the other). Integer in, so the
        // Engine stays float-free; the View is the only place a division happens.
        public void Impact(int t, int power = 1000)
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (mpb == null) mpb = new MaterialPropertyBlock();

            float s = (t * 0.001f - 0.5f) * Mathf.Max(size.x, size.y);   // -> [-E, +E] world units

            impacts[nextSlot] = new Vector4(s, Time.timeSinceLevelLoad, power * 0.001f, 0f);
            nextSlot = (nextSlot + 1) % ImpactSlots;

            sr.GetPropertyBlock(mpb);
            mpb.SetVectorArray(ImpactsId, impacts);
            sr.SetPropertyBlock(mpb);
        }

        private bool sized;

        public void SetSize(Vector2 worldSize) { size = worldSize; sized = true; Apply(); }
        private void OnEnable() { if (sized) Apply(); }

        private void Apply()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (sr.sprite == null) return;
            if (mpb == null) mpb = new MaterialPropertyBlock();

            // Quad is padded so the halo isn't clipped; the SDF still uses the true wall size.
            Vector2 quad = size + 2f * glowPadding * Vector2.one;
            Vector2 unit = sr.sprite.bounds.size; // sprite world size at scale 1 (PPU-independent)
            transform.localScale = new Vector3(quad.x / unit.x, quad.y / unit.y, 1f);

            sr.GetPropertyBlock(mpb);
            mpb.SetVector(SizeId, new Vector4(size.x, size.y, 0f, 0f));
            mpb.SetFloat(PadId, glowPadding);
            mpb.SetFloat(PhaseId, Phase(transform.position));
            mpb.SetVectorArray(ImpactsId, impacts);
            sr.SetPropertyBlock(mpb);
        }

        // Deterministic per-position hash so neighbouring walls don't pulse in lockstep.
        private static float Phase(Vector3 p)
        {
            float h = Mathf.Sin(p.x * 12.9898f + p.y * 78.233f) * 43758.5453f;
            return (h - Mathf.Floor(h)) * 6.2831853f;
        }
    }
}