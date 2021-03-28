using System.Collections;

namespace Interfaces
{
    public interface IInteractable
    {
        void Interact(IRunner runner, bool entered);
    }
}