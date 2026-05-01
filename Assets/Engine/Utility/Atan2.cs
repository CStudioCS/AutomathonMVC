namespace Automathon
{
    /// <summary>
    /// Fonctions trigonométriques déterministes calculées uniquement à l'aide
    /// d'arithmétique entière (algorithme CORDIC).
    /// </summary>
    public static class Atan2Int
    {
        // ---------------------------------------------------------------------
        // Constantes du calcul interne en virgule fixe Q30
        // ---------------------------------------------------------------------
        // Toutes les valeurs angulaires intermédiaires sont exprimées en
        // radians multipliés par 2^30. Le résultat final est ensuite converti
        // en "milliradians" (radians * 1000) renvoyés sous forme d'int.
        private const int FRACTIONAL_BITS = 30;

        // pi/2 exprimé en virgule fixe Q30 : round(pi/2 * 2^30)
        private const long HALF_PI_FIXED = 1686629713L;

        // pi/2 * 1000 arrondi à l'entier le plus proche : la valeur que
        // doit renvoyer la fonction lorsque l'on est exactement aux bornes.
        // pi/2 ≈ 1.5707963267948966 -> * 1000 ≈ 1570.796... -> 1571
        private const int HALF_PI_TIMES_1000 = 1571;

        // Nombre d'itérations CORDIC.
        private const int CORDIC_ITERATIONS = 30;

        // Table des valeurs atan(2^-i) en virgule fixe Q30, pré-calculées.
        // Aucun calcul flottant n'est effectué à l'exécution.
        private static readonly long[] AtanTable = new long[]
        {
            843314857L, // atan(2^0)  = atan(1)
            497837829L, // atan(2^-1) = atan(0.5)
            263043837L, // atan(2^-2)
            133525159L, // atan(2^-3)
             67021687L, // atan(2^-4)
             33543516L, // atan(2^-5)
             16775851L, // atan(2^-6)
              8388437L, // atan(2^-7)
              4194283L, // atan(2^-8)
              2097149L, // atan(2^-9)
              1048576L, // atan(2^-10)
               524288L, // atan(2^-11)
               262144L, // atan(2^-12)
               131072L, // atan(2^-13)
                65536L, // atan(2^-14)
                32768L, // atan(2^-15)
                16384L, // atan(2^-16)
                 8192L, // atan(2^-17)
                 4096L, // atan(2^-18)
                 2048L, // atan(2^-19)
                 1024L, // atan(2^-20)
                  512L, // atan(2^-21)
                  256L, // atan(2^-22)
                  128L, // atan(2^-23)
                   64L, // atan(2^-24)
                   32L, // atan(2^-25)
                   16L, // atan(2^-26)
                    8L, // atan(2^-27)
                    4L, // atan(2^-28)
                    2L, // atan(2^-29)
                    1L  // atan(2^-30)
        };

        /// <summary>
        /// Calcule atan(Y / X) et renvoie le résultat sous forme d'entier
        /// égal à l'angle en radians multiplié par 1000 et arrondi.
        /// </summary>
        /// <param name="X">Composante X.</param>
        /// <param name="Y">Composante Y.</param>
        /// <returns>
        /// Un entier correspondant à atan(Y/X) * 1000 (arrondi à l'entier le
        /// plus proche) avec la convention :
        ///   - X = 0 et Y > 0  ->  +1571   (correspond à  +pi/2 * 1000)
        ///   - X = 0 et Y &lt; 0  ->  -1571   (correspond à  -pi/2 * 1000)
        ///   - X = 0 et Y = 0  ->     0
        /// La valeur retournée est toujours dans l'intervalle [-1571, +1571],
        /// quelle que soit le signe de X (la fonction calcule atan(Y/X), pas
        /// l'angle complet du vecteur).
        /// </returns>
        public static int Atan2(int X, int Y)
        {
            // ---------- Cas limites pour X = 0 ----------
            if (X == 0)
            {
                if (Y > 0) return HALF_PI_TIMES_1000;
                if (Y < 0) return -HALF_PI_TIMES_1000;
                return 0;
            }

            // ---------- Réduction au demi-plan droit (X > 0) ----------
            // On veut atan(Y/X), une valeur dans ]-pi/2, +pi/2[ : on peut donc
            // toujours négativer X et Y simultanément si X < 0, car
            // atan((-Y)/(-X)) = atan(Y/X). Aucune correction post-CORDIC.
            long x = X;
            long y = Y;
            if (x < 0)
            {
                x = -x;
                y = -y;
            }

            // ---------- Mise à l'échelle pour exploiter toute la précision ----------
            // CORDIC fonctionne d'autant mieux que |x| et |y| sont grands. On
            // amplifie autant que possible sans risquer de débordement (le gain
            // CORDIC est ~1.647 sur 30 itérations, donc on garde de la marge).
            long absY = y < 0 ? -y : y;
            long maxAbs = x > absY ? x : absY;
            while (maxAbs < (1L << 28) && maxAbs > 0)
            {
                x <<= 1;
                y <<= 1;
                maxAbs <<= 1;
            }
            while (maxAbs > (1L << 60))
            {
                x >>= 1;
                y >>= 1;
                maxAbs >>= 1;
            }

            // ---------- Boucle CORDIC en mode vectoring ----------
            // À chaque itération i, on fait tourner le vecteur (x, y) d'un
            // angle ±atan(2^-i) de manière à ramener y vers 0. L'angle accumulé
            // est alors égal à l'angle initial du vecteur (dans ]-pi/2; pi/2[).
            long angleQ30 = 0;
            for (int i = 0; i < CORDIC_ITERATIONS; i++)
            {
                long xNew, yNew;
                if (y >= 0)
                {
                    xNew = x + (y >> i);
                    yNew = y - (x >> i);
                    angleQ30 += AtanTable[i];
                }
                else
                {
                    xNew = x - (y >> i);
                    yNew = y + (x >> i);
                    angleQ30 -= AtanTable[i];
                }
                x = xNew;
                y = yNew;
            }

            // ---------- Conversion Q30 -> entier (radians * 1000) ----------
            // result = round(angleQ30 * 1000 / 2^30)
            // On reste en arithmétique entière. Pour arrondir au plus proche
            // sans flottant, on ajoute (signé) la moitié du diviseur avant la
            // division entière.
            long divisor = 1L << FRACTIONAL_BITS;          // 2^30
            long scaled = angleQ30 * 1000L;
            long halfDiv = divisor >> 1;                   // 2^29
            long result;
            if (scaled >= 0)
                result = (scaled + halfDiv) >> FRACTIONAL_BITS;
            else
                result = -(((-scaled) + halfDiv) >> FRACTIONAL_BITS);

            // Borne dure pour garantir qu'on ne dépasse jamais ±1571 même en
            // cas d'erreur d'arrondi de dernier rang.
            if (result > HALF_PI_TIMES_1000) result = HALF_PI_TIMES_1000;
            if (result < -HALF_PI_TIMES_1000) result = -HALF_PI_TIMES_1000;

            return (int)result;
        }
    }
}
