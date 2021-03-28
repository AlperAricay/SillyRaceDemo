using UnityEngine;

namespace Interfaces
{
    public interface IRunner
    {
        Transform RunnerTransform { get; }
        void HandleRagdoll(bool newValue);
    }
}
