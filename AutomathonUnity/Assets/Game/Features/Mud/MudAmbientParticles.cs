using UnityEngine;

namespace Automathon.Game.View
{
    // Ambient glowing pixel-particles over the mud: they wander/jiggle and drift with the mud's
    // flow field (MudLayerController.SampleFlow). Authored on the MudLayer object; reads the arena
    // bounds from the sibling MudLayerController. Rendered as tiny additive quads -> bloom glow.
    [RequireComponent(typeof(MudLayerController))]
    public class MudAmbientParticles : MonoBehaviour
    {
        [Header("Particles")]
        [SerializeField] private int count = 260;
        [SerializeField] private float size = 0.05f;          // world units
        [SerializeField] private float wanderSpeed = 0.5f;    // gentle self-motion
        [SerializeField] private float wanderFreq = 0.4f;
        [SerializeField] private float flowScale = 22f;       // how strongly they ride the mud flow
        [SerializeField] private float renderZ = -1f;         // in front of gameplay
        [SerializeField] private Color particleColor = new Color32(0, 204, 255, 255); // #00ccff
        [SerializeField] private Material particleMaterial;   // additive glow material asset

        private Vector2 arenaMin;
        private Vector2 arenaSize;
        private Vector2[] pos;
        private Vector2[] seed;
        private float[] baseBright;
        private Color[] baseColor;
        private Mesh mesh;
        private Vector3[] verts;
        private Color[] colors;
        private GameObject rendererGo;

        private void Start()
        {
            MudLayerController mud = GetComponent<MudLayerController>();
            Vector2 center = mud != null ? mud.ArenaCenterWorld : Vector2.zero;
            arenaSize = mud != null ? mud.ArenaSizeWorld : new Vector2(32f, 18f);
            arenaMin = center - arenaSize * 0.5f;

            pos = new Vector2[count];
            seed = new Vector2[count];
            baseBright = new float[count];
            baseColor = new Color[count];
            for (int i = 0; i < count; i++)
            {
                pos[i] = arenaMin + new Vector2(Random.value * arenaSize.x, Random.value * arenaSize.y);
                seed[i] = new Vector2(Random.value * 100f, Random.value * 100f);
                baseBright[i] = Random.Range(0.4f, 1f);
                baseColor[i] = particleColor.linear;
            }

            verts = new Vector3[count * 4];
            colors = new Color[count * 4];
            int[] tris = new int[count * 6];
            for (int i = 0; i < count; i++)
            {
                int v = i * 4, t = i * 6;
                tris[t] = v; tris[t + 1] = v + 1; tris[t + 2] = v + 2;
                tris[t + 3] = v + 2; tris[t + 4] = v + 1; tris[t + 5] = v + 3;
            }

            mesh = new Mesh { name = "AmbientParticles" };
            mesh.MarkDynamic();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.bounds = new Bounds(new Vector3(center.x, center.y, renderZ),
                                     new Vector3(arenaSize.x + 2f, arenaSize.y + 2f, 2f));

            rendererGo = new GameObject("AmbientParticlesRenderer");
            rendererGo.transform.position = Vector3.zero;   // mesh verts are already world-space
            rendererGo.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = rendererGo.AddComponent<MeshRenderer>();
            mr.sharedMaterial = particleMaterial;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }

        private void LateUpdate()
        {
            if (pos == null) return;

            float dt = Time.deltaTime;
            float t = Time.time;

            for (int i = 0; i < count; i++)
            {
                Vector2 wander = new Vector2(
                    Mathf.PerlinNoise(seed[i].x, t * wanderFreq) - 0.5f,
                    Mathf.PerlinNoise(seed[i].y, t * wanderFreq + 13.7f) - 0.5f) * (2f * wanderSpeed);

                Vector2 flow = MudLayerController.SampleFlow(pos[i]) * flowScale;
                pos[i] += (wander + flow) * dt;

                if (pos[i].x < arenaMin.x) pos[i].x += arenaSize.x;
                else if (pos[i].x > arenaMin.x + arenaSize.x) pos[i].x -= arenaSize.x;
                if (pos[i].y < arenaMin.y) pos[i].y += arenaSize.y;
                else if (pos[i].y > arenaMin.y + arenaSize.y) pos[i].y -= arenaSize.y;

                float b = baseBright[i] * (0.6f + 0.4f * Mathf.Sin(t * 3f + seed[i].x));
                Color c = baseColor[i] * b;
                c.a = 1f;

                int v = i * 4;
                float s = size;
                float x = pos[i].x, y = pos[i].y;
                verts[v]     = new Vector3(x - s, y - s, renderZ);
                verts[v + 1] = new Vector3(x + s, y - s, renderZ);
                verts[v + 2] = new Vector3(x - s, y + s, renderZ);
                verts[v + 3] = new Vector3(x + s, y + s, renderZ);
                colors[v] = colors[v + 1] = colors[v + 2] = colors[v + 3] = c;
            }

            mesh.vertices = verts;
            mesh.colors = colors;
        }

        private void OnDestroy()
        {
            if (rendererGo != null) Destroy(rendererGo);
            if (mesh != null) Destroy(mesh);
        }
    }
}
