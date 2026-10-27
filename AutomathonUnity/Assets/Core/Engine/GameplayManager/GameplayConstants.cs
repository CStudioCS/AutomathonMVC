namespace Automathon
{
    public static class GameplayConstants
    {
        public const int FRAMERATE = 60;
        public const int MAX_PLAYERS = 2;

        /// <summary>Duration of a simulation frame, in seconds.</summary>
        public static readonly Fixed DELTA_TIME = Fixed.Ratio(1, FRAMERATE);

        /// <summary>Number of simulation frames needed to cover the given duration (rounded to the nearest frame).</summary>
        public static int SecondsToFrames(Fixed seconds) => (seconds * FRAMERATE).RoundToInt();

        /// <summary>Duration in seconds of the given number of simulation frames.</summary>
        public static Fixed FramesToSeconds(int frames) => Fixed.Ratio(frames, FRAMERATE);
    }
}
