using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

public class OpponentController : MonoBehaviour, IRunner
{
    public bool IsStanding { get; set; }
    public int CurrentCheckpointIndex { get; set; }
    public string Username { get; private set; }
    public Transform RunnerTransform { get; private set; }
    public bool HasFinished { get; set; }

    [SerializeField] private float speed = 10f, groundCheckDistance = 0.4f, ragdollToStandTime = 2f;
    
    private float _ragdollTime;
    private bool _inControl, _isRagdoll, _isGrounded;
    private Rigidbody _rb;
    private CapsuleCollider _capsuleCollider;
    private Animator _anim;
    private List<Rigidbody> _rigidbodiesList = new List<Rigidbody>();
    private List<Collider> _rigidbodyCollidersList = new List<Collider>();
    private PlayerController _playerControllerInstance;
    
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int Grounded = Animator.StringToHash("IsGrounded");
    private static readonly int Stand = Animator.StringToHash("Stand");
    private static readonly int Paint = Animator.StringToHash("Paint");
    
    private void Awake()
    {
        RunnerTransform = transform;
        Username = "Bot " + Random.Range(1, 9999);
        CurrentCheckpointIndex = 0;
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _anim = GetComponent<Animator>();
        CurrentCheckpointIndex = 0;
    }

    private void Start()
    {
        GameManager.Instance.CurrentRunners.Add(this);
        transform.position = GameManager.Instance.GetSpawnPoint(this);
        _playerControllerInstance = PlayerController.Instance;
        HasFinished = false;
        _inControl = true;
        IsStanding = false;
        foreach (var rb in _rigidbodiesList)
        {
            rb.isKinematic = true;
        }
        foreach (var coll in _rigidbodyCollidersList)
        {
            coll.enabled = false;
        }
    }

    private void Update()
    {
        switch (_playerControllerInstance.currentPhase)
        {
            case PlayerController.GameplayPhases.StartingPhase:
                break;
            case PlayerController.GameplayPhases.RacingPhase:
                _isGrounded = IsGrounded();
                if (!_isRagdoll) _anim.SetBool(Grounded, _isGrounded);
                break;
            case PlayerController.GameplayPhases.PaintingPhase:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private bool IsGrounded()
    {
        var lowerSphere = transform.position + transform.up * (_capsuleCollider.radius + Physics.defaultContactOffset);
        var groundCheck = Physics.SphereCast(lowerSphere, _capsuleCollider.radius - Physics.defaultContactOffset,
            -transform.up, out _, groundCheckDistance, LayerMask.GetMask("Platform","Obstacle"));
        return groundCheck;
    }
    
    private IEnumerator RagdollToAnimHandler()
    {
        while (Time.time - _ragdollTime < ragdollToStandTime)
        {
            if (!_isRagdoll) yield break;
            yield return null;
        }
        var isGrounded = false;
        while (!isGrounded)
        {
            if (!_isRagdoll) yield break;
            var lowerSphere = _anim.GetBoneTransform(HumanBodyBones.Hips).position + transform.up * (_capsuleCollider.radius + Physics.defaultContactOffset);
            isGrounded = Physics.SphereCast(lowerSphere, _capsuleCollider.radius - Physics.defaultContactOffset,
                Vector3.down, out var hit, groundCheckDistance, LayerMask.GetMask("Platform"));
            yield return null;
        }
        HandleRagdoll(false);
    }

    #region InterfaceMethods

    public void HandleRagdoll(bool newValue)
        {
            if (_isRagdoll == newValue) return;

            if (!newValue)
            {
                GameManager.Instance.thirdPersonCamera.m_Follow = transform;
                _anim.SetTrigger(Stand);
                //move transform close to hip
                var lowerSphere = _anim.GetBoneTransform(HumanBodyBones.Hips).position + transform.up * (_capsuleCollider.radius + Physics.defaultContactOffset);
                Physics.SphereCast(lowerSphere, _capsuleCollider.radius - Physics.defaultContactOffset,
                    Vector3.down, out var hit, groundCheckDistance, LayerMask.GetMask("Platform"));
                transform.position = hit.point;
            }
        
            _inControl = !newValue;
            _isRagdoll = newValue;
            _anim.enabled = !newValue;
            _rb.isKinematic = newValue;
            _capsuleCollider.enabled = !newValue;

            foreach (var rb in _rigidbodiesList)
            {
                rb.isKinematic = !newValue;
            }
            foreach (var coll in _rigidbodyCollidersList)
            {
                coll.enabled = newValue;
            }
            _ragdollTime = Time.time;
        }
    
        public void HandleRagdoll(float impulseForce, Vector3 impulsePosition)
        {
            if (_isRagdoll) return;
            _inControl = false;
            _isRagdoll = true;
            IsStanding = true;
            _anim.enabled = false;
            _rb.isKinematic = true;
            _capsuleCollider.enabled = false;

            foreach (var rb in _rigidbodiesList)
            {
                rb.isKinematic = false;
                rb.AddExplosionForce(impulseForce, impulsePosition, 5f, 0f, ForceMode.Impulse);
            }
            foreach (var coll in _rigidbodyCollidersList)
            {
                coll.enabled = true;
            }
            _ragdollTime = Time.time;
            StartCoroutine(RagdollToAnimHandler());
        }
    
        public void Jump(float jumpPower)
        {
            var jumpForce = (transform.forward + Vector3.up * 1.5f).normalized * jumpPower;
            _rb.AddForce(jumpForce, ForceMode.VelocityChange);
        }
    
        public void Respawn()
        {
            _anim.ResetTrigger(Stand);
            _anim.SetBool(IsRunning, false);
            _inControl = true;
            _isRagdoll = false;
            IsStanding = false;
            _anim.enabled = true;
            _rb.isKinematic = false;
            _capsuleCollider.enabled = true;
            foreach (var rb in _rigidbodiesList)
            {
                rb.isKinematic = true;
            }
            foreach (var coll in _rigidbodyCollidersList)
            {
                coll.enabled = false;
            }
            transform.position = GameManager.Instance.GetSpawnPoint(this);
        }
    
        public void Finish()
        {
            HasFinished = true;
        }

    #endregion

    #region Collision / Trigger Methods
    
    private void OnCollisionEnter(Collision other)
    {
        var isInteractable = other.gameObject.TryGetComponent<IInteractable>(out var obj);
        var isImpulsingObstacle = other.gameObject.TryGetComponent<IImpulse>(out var impulsingObj);
        if (isInteractable)
        {
            obj.Interact(this, PlayerController.CollisionType.Enter);
        }
        else if (isImpulsingObstacle)
        {
            impulsingObj.Impulse(this, other);
        }
    }
    private void OnCollisionStay(Collision other)
    {
        var isInteractable = other.gameObject.TryGetComponent<IInteractable>(out var obj);
        if (isInteractable)
        {
            obj.Interact(this, PlayerController.CollisionType.Stay);
        }
    }
    private void OnCollisionExit(Collision other)
    {
        var isInteractable = other.gameObject.TryGetComponent<IInteractable>(out var obj);
        if (isInteractable)
        {
            obj.Interact(this, PlayerController.CollisionType.Exit);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        var isInteractable = other.gameObject.TryGetComponent<IInteractable>(out var obj);
        if (isInteractable)
        {
            obj.Interact(this, PlayerController.CollisionType.Enter);
        }
    }

    #endregion

    public void Move(Vector3 calculatedVelocity)
    {
        if (!_inControl || _isRagdoll || IsStanding || !_isGrounded) return;
        _rb.AddForce(calculatedVelocity, ForceMode.VelocityChange);
        _anim.SetBool(IsRunning, true);
    }
}
