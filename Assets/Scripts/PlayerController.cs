using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]
[RequireComponent (typeof (Animator))]
public class PlayerController : MonoBehaviour, IRunner
{
    public static PlayerController Instance;
    
    #region InterfaceProperties

    public bool IsStanding { get; set; }
    public int CurrentCheckpointIndex { get; set; }
    public Transform RunnerTransform { get; private set; }
    public bool HasFinished { get; set; }
    public string Username { get; private set; }

    #endregion
    
    public enum CollisionType
    {
        Enter,
        Stay,
        Exit
    }

    [SerializeField] private float speed = 10f, groundCheckDistance = 0.4f, ragdollToStandTime = 2f;

    private float _ragdollTime;
    private bool _inControl, _isRagdoll, _isGrounded;
    private FloatingJoystick _joystick;
    private Rigidbody _rb;
    private CapsuleCollider _capsuleCollider;
    private Animator _anim;
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int Grounded = Animator.StringToHash("IsGrounded");
    private List<Rigidbody> _rigidbodiesList = new List<Rigidbody>();
    private List<Collider> _rigidbodyCollidersList = new List<Collider>();
    private static readonly int Stand = Animator.StringToHash("Stand");

    private void Awake()
    {
        Instance = this;
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _anim = GetComponent<Animator>();
        RunnerTransform = transform;
        Username = "You";
        CurrentCheckpointIndex = 0;
        
        var rigidBodies=GetComponentsInChildren(typeof(Rigidbody));
        for (var i = 1; i < rigidBodies.Length; i++)
        {
            var component = rigidBodies[i];
            if (component is Rigidbody rb)
            {
                _rigidbodiesList.Add(rb);
                if (rb.TryGetComponent<Collider>(out var coll)) _rigidbodyCollidersList.Add(coll);
            }
        }
    }

    private void Start()
    {
        GameManager.Instance.CurrentRunners.Add(this);
        HasFinished = false;
        _joystick = FloatingJoystick.Instance;
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
        transform.position = GameManager.Instance.GetSpawnPoint(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleRagdoll(true);
            transform.GetChild(2).GetChild(0).GetComponent<Rigidbody>().AddForce((-transform.forward + Vector3.down).normalized * 50, ForceMode.VelocityChange);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            HandleRagdoll(false);
        }
        _isGrounded = IsGrounded();
        if (!_isRagdoll)
        {
            Rotate();
            _anim.SetBool(Grounded, _isGrounded);
        }
    }

    private void FixedUpdate()
    {
        if (_inControl && !_isRagdoll && !IsStanding && _isGrounded)
        {
            Move();   
        }
        else if (_isRagdoll)
        {
            //transform.position = hipsTransform.position;
        }
    }

    private void Rotate()
    {
        if (_joystick.Direction != Vector2.zero)
        {
            var lookDirection = new Vector3(_joystick.Horizontal, 0, _joystick.Vertical);
            var lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = lookRotation;
        }
    }

    private void Move()
    {
        if (_joystick.Direction != Vector2.zero)
        {
            var movementVector = transform.forward * speed;
            var velocity = _rb.velocity;
            var requiredVelocity = movementVector - velocity;
            requiredVelocity.y = 0;
            _rb.AddForce(requiredVelocity, ForceMode.VelocityChange);

            _anim.SetBool(IsRunning, true);
        }
        else
        {
            //Stop XZ movement manually
            var velocity = _rb.velocity;
            velocity.x = 0;
            velocity.z = 0;
            _rb.velocity = velocity;

            _anim.SetBool(IsRunning, false);
        }
    }

    private bool IsGrounded()
    {
        var lowerSphere = transform.position + transform.up * (_capsuleCollider.radius + Physics.defaultContactOffset);
        var groundCheck = Physics.SphereCast(lowerSphere, _capsuleCollider.radius - Physics.defaultContactOffset,
            -transform.up, out _, groundCheckDistance, LayerMask.GetMask("Platform","Obstacle"));
        return groundCheck;
    }

    #region Collision / Trigger Methods

    private void OnCollisionEnter(Collision other)
        {
            var isInteractable = other.gameObject.TryGetComponent<IInteractable>(out var obj);
            var isImpulsingObstacle = other.gameObject.TryGetComponent<IImpulse>(out var impulsingObj);
            if (isInteractable)
            {
                obj.Interact(this, CollisionType.Enter);
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
                obj.Interact(this, CollisionType.Stay);
            }
        }
        private void OnCollisionExit(Collision other)
        {
            var isInteractable = other.gameObject.TryGetComponent<IInteractable>(out var obj);
            if (isInteractable)
            {
                obj.Interact(this, CollisionType.Exit);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            var isInteractable = other.gameObject.TryGetComponent<IInteractable>(out var obj);
            if (isInteractable)
            {
                obj.Interact(this, CollisionType.Enter);
            }
        }

    #endregion
    
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

    public void HandleRagdoll(float impulseForce, Vector3 impulsePosition)
    {
        if (_isRagdoll) return;
        GameManager.Instance.thirdPersonCamera.m_Follow = _rigidbodiesList[0].transform;
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
    
    
}