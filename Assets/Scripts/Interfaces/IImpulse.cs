using UnityEngine;

namespace Interfaces
{
    public interface IImpulse
    {
        void Impulse(IRunner runner, Collision collision);
    }
}