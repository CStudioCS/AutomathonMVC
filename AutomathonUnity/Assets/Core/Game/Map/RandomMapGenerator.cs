using Automathon.Engine;
using Automathon.Engine.Physics;
using System;
using System.Collections.Generic;

namespace Automathon.Game.MapSystem
{
    /// <summary>
    /// Génère une arène de "murs de couverture" : panneaux fins orientés aléatoirement,
    /// placés par échantillonnage Poisson-disk, en symétrie centrale (équité 1v1 : chaque
    /// mur a son image en -p) et avec connectivité A↔B garantie (re-tirage déterministe).
    ///
    /// Déterminisme : le PRNG (mulberry32) et le Poisson-disk sont portés bit-à-bit depuis
    /// l'outil d'aperçu, donc une même seed => même carte, sur n'importe quelle plateforme.
    /// Seul le test de connectivité utilise de la trigo flottante (cos/sin) ; il ne sert qu'à
    /// accepter/re-tirer, pas à la physique. Pour du bit-exact cross-plateforme absolu,
    /// remplace-le par du fixed-point.
    ///
    /// GOLDEN REFERENCE (seed=12345, density=1) — voir VerifyReferenceLayout() :
    ///   mulberry32(12345) 5 premiers : 0.979728267761, 0.3067522645, 0.484205421526,
    ///                                  0.817934412509, 0.509428369347
    ///   nombre de murs (avec miroirs) : 26 ; connecté : true
    ///   1er mur : cx=-1621 cy=-2435 len=4130 thick=298 deg=108
    /// </summary>
    public class RandomMapGenerator
    {
        // ---------- Paramètres carte ----------
        private const int MapLength = 27000;
        private const int MapHeight = 15000;
        private const int HX = MapLength / 2;   // 13500
        private const int HY = MapHeight / 2;   // 7500

        private static readonly Vector2Int SpawnA = new Vector2Int(-6750, 0);
        private static readonly Vector2Int SpawnB = new Vector2Int(6750, 0);

        // ---------- Paramètres génération ----------
        private const int BorderThickness = 270;   // épaisseur des murs invisibles de bord
        private const int SpawnClear = 1900;       // rayon dégagé autour du spawn
        private const int WallLenMin = 1900, WallLenMax = 4500;
        private const int WallThickMin = 240, WallThickMax = 380;
        private const double BaseMinDist = 2600.0; // distance min Poisson à density=1
        private const int MaxAttempts = 16;        // re-tirages si non connexe

        private Rng rng;

        private struct WallSpec
        {
            public double X, Y, Len, Thick, Angle; // Angle en radians
        }

        // ======================= Points d'entrée =======================

        /// <summary>Génère une carte avec une seed aléatoire.</summary>
        public static void GenerateRandomMap()
            => GenerateRandomMap(Environment.TickCount);

        /// <summary>Génère une carte reproductible à partir d'une seed (et densité optionnelle).</summary>
        public static void GenerateRandomMap(int seed, double density = 0.7)
        {
            RandomMapGenerator gen = new RandomMapGenerator();
            List<Entity> walls = gen.Build(seed, density);
            MapGenerator.InstantiateMap(new Map("random_map", walls));
        }

        // ======================= Pipeline =======================

        private List<Entity> Build(int seed, double density)
        {
            List<WallSpec> specs = null;
            for (int attempt = 0; attempt < MaxAttempts; attempt++)
            {
                // attempt 0 => seed exacte (parité avec l'aperçu) ; sinon décalage déterministe
                int s = unchecked(seed + attempt * (int)0x9E3779B9);
                specs = GenerateSpecs(s, density);
                if (IsConnected(specs))
                    break;
            }
            return Assemble(specs);
        }

        private List<WallSpec> GenerateSpecs(int seed, double density)
        {
            rng = new Rng(seed);
            double minDist = BaseMinDist / density;

            List<(double x, double y)> pts =
                Poisson(-HX + 1200, -HY + 1200, -1400, HY - 1200, minDist);

            List<WallSpec> specs = new List<WallSpec>();
            foreach (var p in pts)
            {
                // dégagement du spawn AVANT tout tirage (préserve la parité RNG)
                double dx = p.x - SpawnA.X, dy = p.y - SpawnA.Y;
                if (dx * dx + dy * dy < (double)SpawnClear * SpawnClear)
                    continue;

                double len = rng.Range(WallLenMin, WallLenMax);
                double thick = rng.Range(WallThickMin, WallThickMax);
                double angle = rng.Range(0, Math.PI);
                specs.Add(new WallSpec { X = p.x, Y = p.y, Len = len, Thick = thick, Angle = angle });
            }

            // symétrie centrale : chaque mur + son image en -p (même taille, même angle)
            int n = specs.Count;
            for (int i = 0; i < n; i++)
            {
                WallSpec b = specs[i];
                specs.Add(new WallSpec { X = -b.X, Y = -b.Y, Len = b.Len, Thick = b.Thick, Angle = b.Angle });
            }
            return specs;
        }

        private List<Entity> Assemble(List<WallSpec> specs)
        {
            List<Entity> list = new List<Entity>();

            foreach (WallSpec s in specs)
            {
                Vector2Int pos = new Vector2Int(RoundJS(s.X), RoundJS(s.Y));
                Vector2Int size = new Vector2Int(RoundJS(s.Len), RoundJS(s.Thick));
                // NB: angle en degrés. Adapte cette conversion si ton Wall attend
                //     des radians ou du fixed-point.
                int mrad = RoundJS(s.Angle * 1000.0);   // [0, π] rad → [0, 3142] mrad
                list.Add(new Wall(pos, size, mrad));
            }

            // murs invisibles de bord (comme dans ta version)
            list.Add(new InvisibleWall(new Vector2Int(0, HY), new Vector2Int(MapLength, BorderThickness), 0));
            list.Add(new InvisibleWall(new Vector2Int(0, -HY), new Vector2Int(MapLength, BorderThickness), 0));
            list.Add(new InvisibleWall(new Vector2Int(-HX, 0), new Vector2Int(BorderThickness, MapHeight), 0));
            list.Add(new InvisibleWall(new Vector2Int(HX, 0), new Vector2Int(BorderThickness, MapHeight), 0));

            return list;
        }

        // ======================= Connectivité (grille grossière) =======================

        private bool IsConnected(List<WallSpec> specs)
        {
            int gw = 120;
            int gh = RoundJS(120.0 * MapHeight / MapLength); // 67
            double cw = (double)MapLength / gw, ch = (double)MapHeight / gh;

            bool[] free = new bool[gw * gh];
            for (int j = 0; j < gh; j++)
                for (int i = 0; i < gw; i++)
                {
                    double wx = -HX + (i + 0.5) * cw;
                    double wy = -HY + (j + 0.5) * ch;
                    free[j * gw + i] = !Blocked(wx, wy, specs);
                }

            int Cell(Vector2Int v, out int cy)
            {
                int cx = Clamp((int)((v.X + HX) / cw), 0, gw - 1);
                cy = Clamp((int)((v.Y + HY) / ch), 0, gh - 1);
                return cx;
            }

            int ax = Cell(SpawnA, out int ay);
            int bx = Cell(SpawnB, out int by);
            if (!free[ay * gw + ax] || !free[by * gw + bx]) return false;

            bool[] seen = new bool[gw * gh];
            Stack<int> st = new Stack<int>();
            st.Push(ay * gw + ax); seen[ay * gw + ax] = true;
            int[] dx4 = { 1, -1, 0, 0 }, dy4 = { 0, 0, 1, -1 };
            while (st.Count > 0)
            {
                int cur = st.Pop();
                int x = cur % gw, y = cur / gw;
                if (x == bx && y == by) return true;
                for (int k = 0; k < 4; k++)
                {
                    int nx = x + dx4[k], ny = y + dy4[k];
                    if (nx < 0 || ny < 0 || nx >= gw || ny >= gh) continue;
                    int ni = ny * gw + nx;
                    if (free[ni] && !seen[ni]) { seen[ni] = true; st.Push(ni); }
                }
            }
            return false;
        }

        private static bool Blocked(double wx, double wy, List<WallSpec> specs)
        {
            foreach (WallSpec s in specs)
            {
                double dx = wx - s.X, dy = wy - s.Y;
                double c = Math.Cos(-s.Angle), sn = Math.Sin(-s.Angle);
                double lx = dx * c - dy * sn;
                double ly = dx * sn + dy * c;
                if (Math.Abs(lx) <= s.Len / 2 && Math.Abs(ly) <= s.Thick / 2) return true;
            }
            return false;
        }

        // ======================= Poisson-disk (Bridson, porté du JS) =======================

        private List<(double x, double y)> Poisson(double x0, double y0, double x1, double y1, double r, int k = 18)
        {
            double cell = r / Math.Sqrt(2);
            double W = x1 - x0, H = y1 - y0;
            int gw = Math.Max(1, (int)Math.Ceiling(W / cell));
            int gh = Math.Max(1, (int)Math.Ceiling(H / cell));
            int[] grid = new int[gw * gh];
            for (int i = 0; i < grid.Length; i++) grid[i] = -1;

            List<(double x, double y)> pts = new List<(double, double)>();
            List<int> active = new List<int>();

            (int, int) Gxy(double px, double py)
            {
                int cx = Math.Min(gw - 1, (int)((px - x0) / cell));
                int cy = Math.Min(gh - 1, (int)((py - y0) / cell));
                return (cx, cy);
            }
            bool Fits(double px, double py)
            {
                var (cx, cy) = Gxy(px, py);
                for (int iy = Math.Max(0, cy - 2); iy <= Math.Min(gh - 1, cy + 2); iy++)
                    for (int ix = Math.Max(0, cx - 2); ix <= Math.Min(gw - 1, cx + 2); ix++)
                    {
                        int id = grid[iy * gw + ix];
                        if (id >= 0)
                        {
                            double qx = pts[id].x, qy = pts[id].y;
                            if ((qx - px) * (qx - px) + (qy - py) * (qy - py) < r * r) return false;
                        }
                    }
                return true;
            }

            double fx = x0 + rng.NextDouble() * W, fy = y0 + rng.NextDouble() * H;
            pts.Add((fx, fy)); active.Add(0);
            var (g0x, g0y) = Gxy(fx, fy); grid[g0y * gw + g0x] = 0;

            while (active.Count > 0)
            {
                int ai = (int)(rng.NextDouble() * active.Count);
                var p = pts[active[ai]];
                bool placed = false;
                for (int t = 0; t < k; t++)
                {
                    double ang = rng.NextDouble() * Math.PI * 2;
                    double rad = r * (1 + rng.NextDouble());
                    double nx = p.x + Math.Cos(ang) * rad;
                    double ny = p.y + Math.Sin(ang) * rad;
                    if (nx < x0 || nx > x1 || ny < y0 || ny > y1) continue;
                    if (Fits(nx, ny))
                    {
                        int id = pts.Count;
                        pts.Add((nx, ny)); active.Add(id);
                        var (gx, gy) = Gxy(nx, ny); grid[gy * gw + gx] = id;
                        placed = true; break;
                    }
                }
                if (!placed) active.RemoveAt(ai);
            }
            return pts;
        }

        // ======================= Utilitaires =======================

        // Arrondi identique à JS Math.round (moitié vers +∞), pour la parité avec l'aperçu.
        private static int RoundJS(double v) => (int)Math.Floor(v + 0.5);
        private static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);

        /// <summary>
        /// PRNG déterministe mulberry32 (porté bit-à-bit du JS). Même seed => même séquence,
        /// indépendamment de la plateforme (contrairement à System.Random).
        /// </summary>
        private sealed class Rng
        {
            private uint state;
            public Rng(int seed) { state = (uint)seed; }

            public double NextDouble()
            {
                unchecked
                {
                    state += 0x6D2B79F5u;
                    uint t = state;
                    t = (t ^ (t >> 15)) * (1u | state);
                    t = (t + ((t ^ (t >> 7)) * (61u | t))) ^ t;
                    return (t ^ (t >> 14)) / 4294967296.0;
                }
            }

            public double Range(double a, double b) => a + (b - a) * NextDouble();
        }

        // ======================= Self-test (parité avec l'aperçu) =======================

        /// <summary>
        /// Vérifie que le portage reproduit la golden reference. À appeler depuis un test
        /// (ex. NUnit) ou un menu éditeur. Retourne true si tout matche.
        /// </summary>
        public static bool VerifyReferenceLayout()
        {
            // 1) PRNG
            Rng r = new Rng(12345);
            double[] expected = { 0.979728267761, 0.3067522645, 0.484205421526, 0.817934412509, 0.509428369347 };
            for (int i = 0; i < expected.Length; i++)
                if (Math.Abs(r.NextDouble() - expected[i]) > 1e-9) return false;

            // 2) Layout (attempt 0 => seed exacte)
            RandomMapGenerator gen = new RandomMapGenerator();
            List<WallSpec> specs = gen.GenerateSpecs(12345, 1.0);
            if (specs.Count != 26) return false;
            if (!gen.IsConnected(specs)) return false;

            WallSpec f = specs[0];
            if (RoundJS(f.X) != -1621 || RoundJS(f.Y) != -2435) return false;
            if (RoundJS(f.Len) != 4130 || RoundJS(f.Thick) != 298) return false;
            if (RoundJS(f.Angle * 1000.0) != 1877) return false;   // 1.876728 rad → 1877 mrad

            return true;
        }
    }
}