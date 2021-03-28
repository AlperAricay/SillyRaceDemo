using System;
using Interfaces;
using UnityEngine;

public class Jumper : MonoBehaviour, IInteractable
{
    [SerializeField] private float jumpPower = 5f;
    public void Interact(IRunner runner, PlayerController.CollisionType collisionType)
    {
        switch (collisionType)
        {
            case PlayerController.CollisionType.Enter:
                runner.Jump(jumpPower);
                break;
            case PlayerController.CollisionType.Stay:
                break;
            case PlayerController.CollisionType.Exit:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(collisionType), collisionType, null);
        }
    }
}