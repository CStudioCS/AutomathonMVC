namespace Automathon.AI
{
    public struct AIMessage
    {
        public bool Reset;
        public bool DoneWithTraining;
        public AIAction SelfAction;
        public AIAction EnemyAction;
    }
}
