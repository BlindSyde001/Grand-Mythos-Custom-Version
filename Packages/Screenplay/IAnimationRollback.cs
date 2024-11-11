using System;

namespace Screenplay
{
    public interface IAnimationRollback : IDisposable
    {
        void Rollback();
    }
}
