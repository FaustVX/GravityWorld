using System;

namespace test1
{
    public static class Using
    {
        public interface IDisposable : System.IDisposable
        {
            System.IDisposable Start();
        }

        public static Using.IDisposable Create(Action start, Action end)
            => new UsingMethod(start, end);

        private readonly struct UsingMethod : Using.IDisposable
        {
            public Action End { get; }
            public Action Start { get; }

            public UsingMethod(Action start, Action end)
            {
                Start = start;
                End = end;
            }

            System.IDisposable Using.IDisposable.Start()
            {
                this.Start();
                return this;
            }

            public void Dispose()
            {
                End();
            }
        }
    }
}
