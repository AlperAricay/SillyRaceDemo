namespace Interfaces
{
    public interface IInteractable
    {
        void Interact(IRunner runner, PlayerController.CollisionType collisionType);
    }
}