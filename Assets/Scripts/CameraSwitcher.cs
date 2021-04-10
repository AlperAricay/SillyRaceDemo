using System;
using Interfaces;
using UnityEngine;

public class CameraSwitcher : MonoBehaviour, IInteractable
{
    [SerializeField] private CameraType camType; 
    
    private IRunner _player;
    
    private enum CameraType
    {
        Default,
        Upwards
    }

    private void Start()
    {
        _player = PlayerController.Instance;
    }

    public void Interact(IRunner runner, PlayerController.CollisionType collisionType)
    {
        if (runner != _player) return;
        switch (camType)
        {
            case CameraType.Default:
                GameManager.Instance.thirdPersonCamera.m_Priority = 5;
                GameManager.Instance.thirdPersonCameraUpwards.m_Priority = 2;
                break;
            case CameraType.Upwards:
                GameManager.Instance.thirdPersonCamera.m_Priority = 2;
                GameManager.Instance.thirdPersonCameraUpwards.m_Priority = 5;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
