using UnityEngine;

namespace Interfaces
{
    public interface IRunner
    {
        string Username { get; }
        bool HasFinished { get; set; }
        Transform RunnerTransform { get; }
        void HandleRagdoll(bool newValue);
        void Jump(float jumpPower);
        void Respawn();
        void Finish();
    }
}
