using System;

namespace Automathon.Engine.Physics
{
    // Max 32 calques par pitwei si qq en mets 33 pour troll nsm ça fait exploser ton pc jspr
    public enum CollisionLayer
    {
        Default = 0,
        Tank = 1,
        Bullet = 2,
        Missile = 3,
        Wall = 4,
        Dash = 5,
        Shield = 6,
    }

    // Matrice de collision symétrique, stockée en (32 int de chacun 32 bits ça tombe bien).
    // Pour voir si le mask i et j collisionnent, on fait (mask[i] & (1 << j)) != 0 (le bit j du int i)
    public static class LayerMatrix
    {
        private static int[] mask;
        public static void Initialize()
        {
            mask = Init();
            SetCollision(CollisionLayer.Dash, CollisionLayer.Bullet, false);
            SetCollision(CollisionLayer.Dash, CollisionLayer.Missile, false);
            SetCollision(CollisionLayer.Dash, CollisionLayer.Shield, false);
        }

        private static int[] Init()
        {
            int[] m = new int[32];
            for (int i = 0; i < m.Length; i++)
                m[i] = ~0; // tout collisionne avec tout par défaut
            return m;
        }

        public static void SetCollision(CollisionLayer a, CollisionLayer b, bool shouldCollide)
        {
            int bitA = 1 << (int)a;
            int bitB = 1 << (int)b;
            if (shouldCollide)
            {
                mask[(int)a] |= bitB;
                mask[(int)b] |= bitA;
            }
            else
            {
                mask[(int)a] &= ~bitB;
                mask[(int)b] &= ~bitA;
            }
        }

        public static bool GetCollision(CollisionLayer a, CollisionLayer b)
            => (mask[(int)a] & (1 << (int)b)) != 0;

        //En vrai pas sûr que ça serve mais je le mets parce que c'est simple à implémenter
        public static void SetCollidesOnlyWith(CollisionLayer layer, params CollisionLayer[] others)
        {
            foreach (CollisionLayer other in (CollisionLayer[])Enum.GetValues(typeof(CollisionLayer)))
                SetCollision(layer, other, false);
            foreach (CollisionLayer other in others)
                SetCollision(layer, other, true);
        }
    }
}