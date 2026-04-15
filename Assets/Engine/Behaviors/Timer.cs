using System;

namespace Automathon.Engine
{
    public class Timer : Behavior
    {
        public readonly int MaxValueFrames;
        public int ValueFrames;
        public Func<bool> PausedFunc = () => false;

        private Action OnComplete;
        private Action<Timer> UpdateAction;

        /// <summary>
        /// Timer ticks down from the given time to 0, then calls OnComplete. UpdateAction is called every update while the timer is still ticking.
        /// </summary>
        /// <param name="timerValueMilliseconds"></param>
        /// <param name="UpdateAction"></param>
        /// <param name="OnComplete"></param>
        public Timer(int timerValueMilliseconds, Action<Timer> UpdateAction = null, Action OnComplete = null)
        {
            this.MaxValueFrames = timerValueMilliseconds / GameplayConstants.FRAMERATE;
            ValueFrames = MaxValueFrames;
            this.OnComplete = OnComplete;
            this.UpdateAction = UpdateAction;
        }

        public override void Update()
        {
            if (ValueFrames > 0)
            {
                ValueFrames--;
                if (ValueFrames <= 0)
                {
                    ValueFrames = 0;
                    OnComplete?.Invoke();
                    ParentEntity.RemoveBehavior(this);
                }
                else
                    UpdateAction?.Invoke(this);
            }
        }

        //by calling this we can end the timer without triggering onComplete using RemoveComponent
        public void End()
        {
            OnComplete?.Invoke();
            ParentEntity.RemoveBehavior(this);
        }
    }
}