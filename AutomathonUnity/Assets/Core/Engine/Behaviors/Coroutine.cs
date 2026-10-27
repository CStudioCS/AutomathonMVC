using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Automathon.Engine
{
    //Copied over from Fiourp with various fixes
    public class Coroutine : Behavior
    {
        public IEnumerator Enumerator;
        private int waitTimer;
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

        private struct WaitForSeconds
        {
            public Fixed Seconds;
            public WaitForSeconds(Fixed seconds) { Seconds = seconds; }
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
                else if (Enumerator.Current is WaitForSeconds wait)
                    waitTimer = GameplayConstants.SecondsToFrames(wait.Seconds);
                else if (Enumerator.Current is PausedUntil paused)
                {
                    pausedUntil = paused.Until;
                    waitTimer = 0;
                }
            }
            else if (stack != null && stack.Count > 0)
                Enumerator = stack.Pop();
            else if (ParentEntity != null)
                ParentEntity.RemoveBehavior(this);
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

        public static IEnumerator WaitSeconds(Fixed seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        public static IEnumerator WaitSecondsThen(Fixed seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }

        public static IEnumerator WaitUntil(Func<bool> Until)
        {
            yield return new PausedUntil(Until);
        }
    }
}