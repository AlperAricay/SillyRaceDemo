using System;
using System.Collections;
using Interfaces;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]
[RequireComponent (typeof (Animator))]
public class PlayerController : MonoBehaviour, IRunner
{
    public static PlayerController Instance;
    
    #region InterfaceProperties

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

    [SerializeField] private float speed = 10f, groundCheckDistance = 0.4f;

    private bool _inControl, _isRagdoll, _isGrounded;
    private FloatingJoystick _joystick;
    private Rigidbody _rb;
    private CapsuleCollider _capsuleCollider;
    private Animator _anim;
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int Grounded = Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        Instance = this;
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _anim = GetComponent<Animator>();
        RunnerTransform = transform;
        Username = "You";
    }

    private void Start()
    {
        GameManager.Instance.CurrentRunners.Add(this);
        HasFinished = false;
        _joystick = FloatingJoystick.Instance;
        _inControl = true;
        HandleRagdoll(false);
        transform.position = GameManager.Instance.GetSpawnPoint();
    }

    private void Update()
    {
        _isGrounded = IsGrounded();
        if (_isRagdoll)
        {
            
        }
        else
        {
            Rotate();
            _anim.SetBool(Grounded, _isGrounded);
        }
    }

    private void FixedUpdate()
    {
        if (_inControl && !_isRagdoll)
        {
            Move();   
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
        if (_inControl && _isGrounded)
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
    }

    private bool IsGrounded()
    {
        var lowerSphere = transform.position + transform.up * (_capsuleCollider.radius + Physics.defaultContactOffset);
        var groundCheck = Physics.SphereCast(lowerSphere, _capsuleCollider.radius - Physics.defaultContactOffset,
            -transform.up, out var hit, groundCheckDistance, LayerMask.GetMask("Platform","Obstacle"));
        return groundCheck;
    }

    private void OnCollisionEnter(Collision other)
    {
        var isInteractable = other.gameObject.TryGetComponent<IInteractable>(out var obj);
        if (isInteractable)
        {
            obj.Interact(this, CollisionType.Enter);
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

    // private void OnDrawGizmos()
    // {
    //     Gizmos.DrawWireSphere(transform.position + transform.up * (_capsuleCollider.radius + Physics.defaultContactOffset) + -transform.up * groundCheckDistance,_capsuleCollider.radius - Physics.defaultContactOffset);
    // }
    
    #region InterfaceMethods

    public void HandleRagdoll(bool newValue)
    {
        if (_isRagdoll == newValue) return;
        _inControl = !newValue;
        _isRagdoll = newValue;
        _rb.isKinematic = newValue;
        _capsuleCollider.enabled = !newValue;
        _anim.enabled = !newValue;
        var rigidBodies=GetComponentsInChildren(typeof(Rigidbody));
        
        foreach (var component in rigidBodies)
        {
            if (component is Rigidbody rb) rb.isKinematic = newValue;
        }
    }

    public void Jump(float jumpPower)
    {
        var jumpForce = (transform.forward + Vector3.up * 1.5f).normalized * jumpPower;
        _rb.AddForce(jumpForce, ForceMode.VelocityChange);
    }

    public void Respawn()
    {
        _inControl = true;
        HandleRagdoll(false);
        transform.position = GameManager.Instance.GetSpawnPoint();
    }

    public void Finish()
    {
        HasFinished = true;
    }

    #endregion
}
