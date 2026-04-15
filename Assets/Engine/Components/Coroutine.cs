using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Automathon.Engine
{
    //Copied over from Fiourp with various fixes
    public class Coroutine : Component
    {
        public IEnumerator Enumerator;
        private float waitTimer;
        private Func<bool> pausedUntil;

        private Stack<IEnumerator> stack;

        public Coroutine(IEnumerator enumerator)
        {
            Enumerator = enumerator;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumerators">The Enumerators in order of execution</param>
        public Coroutine(params IEnumerator[] enumerators)
        {
            stack = new Stack<IEnumerator>(enumerators.Reverse());
            Enumerator = stack.Pop();
        }

        public Coroutine(Stack<IEnumerator> stack)
        {
            this.stack = stack;
            Enumerator = stack.Pop();
        }

        private struct WaitForMilliseconds
        {
            public int TimeMilliseconds;
            public WaitForMilliseconds(int milliseconds) { TimeMilliseconds = milliseconds; }
        }

        private struct PausedUntil
        {
            public Func<bool> Until;
            public PausedUntil(Func<bool> pausedUntil) { Until = pausedUntil; }
        }

        public override void Update()
        {
            if (pausedUntil != null && !pausedUntil())
                return;
            else
                pausedUntil = null;

            if (waitTimer > 0)
                waitTimer--;
            else if (Enumerator.MoveNext()) //executing the coroutine and handling different yield returns
            {
                if (Enumerator.Current == null)
                    waitTimer = 0;
                else if (Enumerator.Current is int returnedInt)
                    waitTimer = returnedInt;
                else if (Enumerator.Current is WaitForMilliseconds wait)
                    waitTimer = wait.TimeMilliseconds / GameplayConstants.FRAMERATE;
                else if (Enumerator.Current is PausedUntil paused)
                {
                    pausedUntil = paused.Until;
                    waitTimer = 0;
                }
            }
            else if (stack != null && stack.Count > 0)
                Enumerator = stack.Pop();
            else if (ParentEntity != null)
                ParentEntity.RemoveComponent(this);
            else
                Enumerator = null;
        }

        //Some util that's nice when building stacks of IEnumerators
        public static IEnumerator WaitFrames(int frames)
        {
            yield return frames;
        }

        public static IEnumerator WaitFramesThen(int frames, Action action)
        {
            yield return frames;
            action();
        }

        public static IEnumerator WaitMilliseconds(int milliseconds)
        {
            yield return new WaitForMilliseconds(milliseconds);
        }

        public static IEnumerator WaitMilisecondsThen(int milliSeconds, Action action)
        {
            yield return new WaitForMilliseconds(milliSeconds);
            action();
        }

        public static IEnumerator WaitUntil(Func<bool> Until)
        {
            yield return new PausedUntil(Until);
        }
    }
}