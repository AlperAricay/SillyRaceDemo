using System;
using Interfaces;
using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]
[RequireComponent (typeof (Animator))]
public class PlayerController : MonoBehaviour, IRunner
{
    public Transform RunnerTransform { get; private set; }

    [SerializeField] private float speed = 10f, jumpPower = 2f, groundCheckDistance = 0.15f;

    private bool _inControl, _isRagdoll, _isGrounded;
    private FloatingJoystick _joystick;
    private Rigidbody _rb;
    private CapsuleCollider _capsuleCollider;
    private Animator _anim;
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int Grounded = Animator.StringToHash("IsGrounded");

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _anim = GetComponent<Animator>();
        RunnerTransform = transform;
    }

    private void Start()
    {
        _joystick = FloatingJoystick.Instance;
        _inControl = true;
        HandleRagdoll(false);
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
            obj.Interact(this, true);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        var isInteractable = other.gameObject.TryGetComponent<IInteractable>(out var obj);
        if (isInteractable)
        {
            obj.Interact(this, false);
        }
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.DrawWireSphere(transform.position + transform.up * (_capsuleCollider.radius + Physics.defaultContactOffset) + -transform.up * groundCheckDistance,_capsuleCollider.radius - Physics.defaultContactOffset);
    // }
}
