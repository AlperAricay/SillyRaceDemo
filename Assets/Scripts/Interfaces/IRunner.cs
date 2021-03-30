using JetBrains.Annotations;
using UnityEngine;

namespace Interfaces
{
    public interface IRunner
    {
        bool IsStanding { get; set; }
        int CurrentCheckpointIndex { get; set; }
        string Username { get; }
        bool HasFinished { get; set; }
        Transform RunnerTransform { get; }
        [UsedImplicitly]
        void HandleRagdoll(bool newValue);
        void HandleRagdoll(float impulseForce, Vector3 impulsePosition);
        void Jump(float jumpPower);
        void Respawn();
        void Finish();
    }
}
