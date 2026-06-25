using System.Collections.Generic;

namespace Automathon.Game
{
    public class GameState
    {
        public bool Done;
        public List<State> States;
    }

    public class State
    {
        public string Type => this.GetType().Name;
        public Vector2Int Position;
    }
}
