using System;
using Interfaces;
using UnityEngine;

public class Jumper : MonoBehaviour, IInteractable
{
    [SerializeField] private float jumpPower = 5f;
    public void Interact(IRunner runner, PlayerController.CollisionType collisionType)
    {
        if (collisionType == PlayerController.CollisionType.Enter) runner.Jump(jumpPower);
    }
}