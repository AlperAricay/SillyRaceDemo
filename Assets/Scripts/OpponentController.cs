using System;
using System.Collections;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class OpponentController : MonoBehaviour, IRunner
{
    public bool IsStanding { get; set; }
    public int CurrentCheckpointIndex { get; set; }
    public string Username { get; private set; }
    public Transform RunnerTransform { get; private set; }
    public bool HasFinished { get; set; }

    public Vector3 calculatedVelocity;

    [SerializeField] private float speed = 10f, groundCheckDistance = 0.4f, ragdollToStandTime = 2f;

    private float _ragdollTime, _jumpTime;
    private bool _inControl, _isRagdoll, _isGrounded;
    private Rigidbody _rb;
    private CapsuleCollider _capsuleCollider;
    private Animator _anim;
    private List<Rigidbody> _rigidbodiesList = new List<Rigidbody>();
    private List<Collider> _rigidbodyCollidersList = new List<Collider>();
    private PlayerController _playerControllerInstance;
    private NavMeshPath _path;
    private int _runnerID;

    private static readonly int IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int Grounded = Animator.StringToHash("IsGrounded");
    private static readonly int Stand = Animator.StringToHash("Stand");

    private void Awake()
    {
        RunnerTransform = transform;
        Username = "Bot ";
        CurrentCheckpointIndex = 0;
        _rb = GetComponent<Rigidbody>();
        _rb.sleepThreshold = 0.0f;
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _anim = GetComponent<Animator>();
        CurrentCheckpointIndex = 0;
        calculatedVelocity = Vector3.zero;
        _path = new NavMeshPath();
        _jumpTime = -10;
        
        var rigidBodies=GetComponentsInChildren(typeof(Rigidbody));
        for (var i = 1; i < rigidBodies.Length; i++)
        {
            var component = rigidBodies[i];
            if (!(component is Rigidbody rb)) continue;
            _rigidbodiesList.Add(rb);
            if (rb.TryGetComponent<Collider>(out var coll)) _rigidbodyCollidersList.Add(coll);
        }
    }

    private void Start()
    {
        _runnerID = GameManager.Instance.GetRunnerID();
        Username += _runnerID;
        GameManager.Instance.CurrentRunners.Add(this);
        //transform.position = GameManager.Instance.GetSpawnPoint(this);
        transform.position = GameManager.Instance.GetSpawnPoint(this, _runnerID);
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
                if (!_isRagdoll)
                {
                    Rotate();
                    _anim.SetBool(Grounded, _isGrounded);
                }

                break;
            case PlayerController.GameplayPhases.PaintingPhase:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void FixedUpdate()
    {
        if (_inControl && !_isRagdoll && !IsStanding && _isGrounded && !HasFinished && Time.time - _jumpTime >= 0.5f)
            Move();
        else
            _anim.SetBool(IsRunning, false);
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
            _jumpTime = Time.time;
            var jumpForce = (transform.forward + Vector3.up * 1.5f).normalized * jumpPower;
            _rb.AddForce(jumpForce, ForceMode.VelocityChange);
            Debug.Log("Opponent jump");
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
            transform.rotation = Quaternion.identity;
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

    private void Move()
    {
        //if (!_inControl || _isRagdoll || IsStanding || !_isGrounded || HasFinished) return;
        var targetPos = GameManager.Instance.checkpoints.Count <= CurrentCheckpointIndex + 1
            ? GameManager.Instance.checkpoints[CurrentCheckpointIndex].spawnPoints[_runnerID].transform.position
            : GameManager.Instance.checkpoints[CurrentCheckpointIndex + 1].spawnPoints[_runnerID].transform.position;

        if (NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, _path))
        {
            for (int i = 0; i < _path.corners.Length - 1; i++)
                Debug.DrawLine(_path.corners[i], _path.corners[i + 1], Color.red);

            if (_path.corners.Length > 1)
            {
                var dir = (_path.corners[1] - transform.position).normalized;
                //dir.y = 0;
                //MANIPULATE DIR IF THEY ARE ABOUT TO COLLIDE
                if (Physics.BoxCast(
                    new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z),
                    Vector3.one * .75f, dir, Quaternion.identity, 1.5f, LayerMask.GetMask("Bot", "Player")))
                {
                    //for loop with many angles, break if found a dir without collision
                    var currX = -2;
                    for (int i = 0; i < 5; i++)
                    {
                        var possiblePosition = transform.position;
                        possiblePosition.z += 1.5f;
                        possiblePosition.x += currX;
                        var possibleDir = (possiblePosition - transform.position).normalized;
                        if (!Physics.BoxCast(
                            new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z),
                            Vector3.one * .75f, possibleDir, Quaternion.identity, 1.5f,
                            LayerMask.GetMask("Bot", "Player", "Obstacle", "Walls"), QueryTriggerInteraction.Collide))
                        {
                            //check if this cast is out of bounds
                            if (Physics.CheckBox(possiblePosition, Vector3.one, Quaternion.identity,
                                LayerMask.GetMask("Platform")))
                            {
                                //use this cast's direction
                                dir = possibleDir;
                                break;
                            }
                        }

                        currX++;
                    }
                }

                var velocity = _rb.velocity;
                calculatedVelocity = dir * speed;
                var requiredVelocity = calculatedVelocity - velocity;
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
    
    /*private void Move()
    {
        //if (!_inControl || _isRagdoll || IsStanding || !_isGrounded || HasFinished) return;
        var targetPos = GameManager.Instance.checkpoints.Count <= CurrentCheckpointIndex + 1
            ? GameManager.Instance.checkpoints[CurrentCheckpointIndex].spawnPoints[_runnerID].transform.position
            : GameManager.Instance.checkpoints[CurrentCheckpointIndex + 1].spawnPoints[_runnerID].transform.position;
        
        NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, _path);

        for (int i = 0; i < _path.corners.Length - 1; i++)
            Debug.DrawLine(_path.corners[i], _path.corners[i + 1], Color.red);

        if (_path.corners.Length > 1)
        {
            var dir = (_path.corners[1] - transform.position).normalized;
            //dir.y = 0;
            //MANIPULATE DIR IF THEY ARE ABOUT TO COLLIDE
            if (Physics.BoxCast(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z),
                Vector3.one * .75f, dir, Quaternion.identity, 1.5f, LayerMask.GetMask("Bot", "Player")))
            {
                //for loop with many angles, break if found a dir without collision
                var currX = -2;
                for (int i = 0; i < 5; i++)
                {
                    var possiblePosition = transform.position;
                    possiblePosition.z += 1.5f;
                    possiblePosition.x += currX;
                    var possibleDir = (possiblePosition - transform.position).normalized;
                    if (!Physics.BoxCast(new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z),
                        Vector3.one * .75f, possibleDir, Quaternion.identity, 1.5f,
                        LayerMask.GetMask("Bot", "Player", "Obstacle", "Walls"), QueryTriggerInteraction.Collide))
                    {
                        //check if this cast is out of bounds
                        if (Physics.CheckBox(possiblePosition, Vector3.one, Quaternion.identity, LayerMask.GetMask("Platform")))
                        {
                            //use this cast's direction
                            dir = possibleDir;
                            break;
                        }
                    }
                    currX++;
                }
            }
            var velocity = _rb.velocity;
            calculatedVelocity = dir * speed;
            var requiredVelocity = calculatedVelocity - velocity;
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
    }*/
    
    private void Rotate()
    {
        if (_path.corners.Length <= 1 || Time.time - _jumpTime < 0.5f || !_isGrounded) return;
        var lookDirection = (_path.corners[1] - transform.position).normalized;
        //lookDirection.y = transform.position.y;
        
        var lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        
        var step = 180 * Time.deltaTime;
        lookRotation = Quaternion.RotateTowards(transform.rotation, lookRotation, step);
        transform.rotation = lookRotation;
    }
}
