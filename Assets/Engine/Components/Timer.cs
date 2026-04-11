using System;

namespace Automathon.Engine
{
    public class Timer : Component
    {
        public readonly int MaxValueMillis;
        public int ValueMillis;
        public Func<bool> PausedFunc = () => false;

        private bool destroyOnComplete;

        public Action OnComplete;
        public Action<Timer> UpdateAction;

        /// <summary>
        /// Timer ticks down from the given time to 0, then calls OnComplete. UpdateAction is called every update while the timer is still ticking.
        /// </summary>
        /// <param name="timerValueMilliseconds"></param>
        /// <param name="UpdateAction"></param>
        /// <param name="OnComplete"></param>
        public Timer(int timerValueMilliseconds, Action<Timer> UpdateAction = null, Action OnComplete = null)
        {
            this.MaxValueMillis = timerValueMilliseconds;
            ValueMillis = timerValueMilliseconds;
            this.OnComplete = OnComplete;
            this.UpdateAction = UpdateAction;
        }

        public override void Update()
        {
            if (ValueMillis > 0)
            {
                ValueMillis -= GameplayConstants.DeltatimeMillis;
                if (ValueMillis <= 0)
                {
                    ValueMillis = 0;
                    OnComplete?.Invoke();
                    if (destroyOnComplete)
                        ParentEntity.RemoveComponent(this);
                }
                else
                    UpdateAction?.Invoke(this);
            }
        }

        public void End()
        {
            OnComplete?.Invoke();
            ParentEntity.RemoveComponent(this);
        }
    }
}