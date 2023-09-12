using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour, IDamageInterface
{
    public float moveForceWalking = 20f;
    public float moveForceFalling = 5f;
    public float jumpImpulse = 5f;
    public float jumpCooldown = 0.1f;

    public float standUpSpringWalking = 5f;
    public float standUpSpringFalling = 0.1f;
    public float dragDefault = 0.2f;
    public float dragLava = 10f;
    public float angularDragDefault = 0.1f;
    public float angularDragLava = 1f;
    public float angularDragDead = 0.05f;
    public float turnSpeed = 360f;
    
    public float minFallDamageSpeed = 0.1f;
    public float maxFallDamageSpeed = 0.5f;
    public float damageAnimLength = 0.5f;

    public AudioClip jumpAudio;
    public AudioClip landAudio;
    public float landAudioDelay = 0.5f;
    public AudioClip hurtAudio;
    public float hurtAudioDelay = 0.5f;
    public AudioClip deathAudio;

    public new Rigidbody rigidbody;
    public Transform mesh;
    public Animator animator;
    public Renderer meshRenderer;

    private bool _enableInput = true;
    // Normal directions of the currently touched surfaces
    private List<Vector3> _groundNormals;
    // Ground normal samples accumulated throughout the frame
    private List<Vector3> _newGroundNormals;
    // Last time the character jumped
    private float _lastJumpTime;
    private float _health = 1f;
    private Vector2 _moveInput;
    private bool _wantsToJump;
    // Whether the character should currently face right
    private bool _faceRight = true;
    private float _currentRot;
    // Number of overlaps we have with lava objects, usually only 1 or 0
    private int _lavaOverlaps;
    private float _lastLandAudioTime;
    private float _lastHurtAudioTime;
    private float _damageAnimStartTime;
    
    private static readonly int MoveSpeedParam = Animator.StringToHash("MoveSpeed");
    private static readonly int JumpParam = Animator.StringToHash("Jump");
    private static readonly int IsGroundedParam = Animator.StringToHash("IsGrounded");
    private static readonly int DeadParam = Animator.StringToHash("IsDead");
    private static readonly int DamageParam = Shader.PropertyToID("_Damage");

    private void Awake()
    {
        // Update the rigid body initially
        UpdateRigidBody();
    }

    private void Start()
    {
        GameManager.Instance.AddCharacter(this);
    }

    private void OnDisable()
    {
        GameManager.Instance.RemoveCharacter(this);
    }

    private void Update()
    {
        if (_enableInput)
        {
            _moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            _wantsToJump |= Input.GetButtonDown("Jump");
            
            // Debug jump to spawn point
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.T))
            {
                var debugSpawns = FindObjectsOfType<DebugSpawn>();
                
                var maxTime = 0f;
                var minY = float.MaxValue;
                DebugSpawn bestSpawn = null;
                foreach (var spawn in debugSpawns)
                {
                    var timeSinceSpawn = spawn.GetTimeSinceLastSpawn();
                    if (timeSinceSpawn > maxTime || Math.Abs(timeSinceSpawn - maxTime) < Mathf.Epsilon && spawn.transform.position.y < minY)
                    {
                        maxTime = timeSinceSpawn;
                        minY = spawn.transform.position.y;
                        bestSpawn = spawn;
                    }
                }

                if (bestSpawn)
                {
                    transform.position = bestSpawn.transform.position;
                    bestSpawn.OnSpawned();
                }
            }
        }
        
        // Update the animation
        animator.SetFloat(MoveSpeedParam, Mathf.Abs(_moveInput.x));
    }
    
    private void FixedUpdate()
    {
        // Update the grounded state
        UpdateGround();

        // Don't update movement while dead
        if (IsDead())
        {
            return;
        }

        var isGrounded = IsGrounded();
        
        // Add torque to make the character stand upright
        var upVector = transform.up;
        var angle = Mathf.Acos(upVector.y);
        if (angle > 0.01f)
        {
            var axis = Vector3.Cross(upVector, Vector3.up);
            var spring = isGrounded ? standUpSpringWalking : standUpSpringFalling;
            var torque = axis * (Mathf.Clamp(angle, 0f, Mathf.PI * 0.125f) * spring);
            rigidbody.AddTorque(torque);
        }
        
        // Process movement force
        var moveForce = isGrounded ? moveForceWalking : moveForceFalling;
        var moveDir = ProjectVectorOnGround(Vector3.right * Mathf.Sign(_moveInput.x));
        // Reduce force when walking up
        var slopeScale = Mathf.Clamp01(1f - moveDir.y);
        var force = moveDir * (Mathf.Abs(_moveInput.x) * moveForce * slopeScale);
        // Add vertical movement while in lava
        if (IsInLava())
        {
            force += Vector3.up * (_moveInput.y * moveForce);
        }
        rigidbody.AddForce(force);
        
        // Check if we want to jump
        if (_wantsToJump)
        {
            // Reset the flag regardless if we actually jumped
            _wantsToJump = false;
            
            if (CanJump())
            {
                // Jump into the direction the player is pressing
                var jumpDir = ProjectVectorOnGround((Vector3.up + Vector3.right * _moveInput.x).normalized);
                var impulse = jumpDir * jumpImpulse;
                rigidbody.AddForce(impulse, ForceMode.Impulse);
                _lastJumpTime = Time.time;
                
                // Trigger jump animation and sound
                animator.SetTrigger(JumpParam);
                AudioManager.Instance.PlayAudio(jumpAudio);
            }
        }
        
        // Update the target face direction
        if (Mathf.Abs(_moveInput.x) > 0.05f)
        {
            _faceRight = _moveInput.x > 0f;
        }
        
        // Rotate the mesh towards the desired direction
        var targetRot = _faceRight ? 0f : 180f;
        var deltaRot = targetRot - _currentRot;
        if (Mathf.Abs(deltaRot) > 0.001f)
        {
            _currentRot = Mathf.Lerp(_currentRot, targetRot, turnSpeed * Time.deltaTime);
            mesh.localRotation = Quaternion.Euler(0f, _currentRot, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Lava"))
        {
            ++_lavaOverlaps;
            UpdateRigidBody();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Lava"))
        {
            --_lavaOverlaps;
            UpdateRigidBody();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        OnAnyCollisionEnterOrStay(collision);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        OnAnyCollisionEnterOrStay(collision);
        
        // Apply fall damage
        var impactSpeed = collision.relativeVelocity.magnitude;
        var fallDamage = Mathf.Clamp((impactSpeed - minFallDamageSpeed) / (maxFallDamageSpeed - minFallDamageSpeed), 0f, 1f);
        if (fallDamage > 0f)
        {
            ApplyDamage(fallDamage);
        }
    }

    private void OnAnyCollisionEnterOrStay(Collision collision)
    {
        _newGroundNormals ??= new List<Vector3>();

        // Add the contact to the ground normals
        foreach (var contact in collision.contacts)
        {
            _newGroundNormals.Add(contact.normal);
        }
    }

    public void SetEnableInput(bool enable)
    {
        _enableInput = enable;
    }

    private bool CanJump()
    {
        return (IsGrounded() || IsInLava()) && Time.time - _lastJumpTime > jumpCooldown;
    }

    private bool IsGrounded()
    {
        return _groundNormals is { Count: > 0 };
    }
    
    private void UpdateGround()
    {
        var oldGrounded = IsGrounded();
        var newGrounded = _newGroundNormals is { Count: > 0 };
        _groundNormals = newGrounded ? new List<Vector3>(_newGroundNormals) : null;

        if (newGrounded != oldGrounded)
        {
            animator.SetBool(IsGroundedParam, newGrounded);
            
            // Play the landing sound
            if (newGrounded && Time.time - _lastLandAudioTime > landAudioDelay)
            {
                AudioManager.Instance.PlayAudio(landAudio);
            }
            
            UpdateRigidBody();
        }

        // Reset for next frame
        _newGroundNormals?.Clear();
    }

    private Vector3 ProjectVectorOnGround(Vector3 vec)
    {
        if (_groundNormals != null)
        {
            foreach (var normal in _groundNormals)
            {
                var dot = Vector3.Dot(normal, vec);
                if (dot < 0f)
                {
                    vec = Vector3.ProjectOnPlane(vec, normal).normalized;
                }
            }
        }

        return vec;
    }

    private void UpdateRigidBody()
    {
        var isInLava = IsInLava();
        rigidbody.drag = isInLava ? dragLava : dragDefault;
        rigidbody.angularDrag = isInLava ? angularDragLava : IsDead() ? angularDragDead : angularDragDefault;
        rigidbody.useGravity = !isInLava;
    }

    private bool IsInLava()
    {
        return _lavaOverlaps > 0;
    }
    
    private  bool IsAlive()
    {
        return _health > 0f;
    }

    private bool IsDead()
    {
        return !IsAlive();
    }

   private void OnDied()
   {
       animator.SetBool(DeadParam, IsDead());
       UpdateRigidBody();
       
       // Notify the game manager
       GameManager.Instance.OnPlayerDied();
   }
   
    public void ApplyDamage(float damageAmount)
    {
        AddHealth(-damageAmount);
    }

    private IEnumerator DamagedAnimCoroutine()
    {
        while (Time.time - _damageAnimStartTime < damageAnimLength)
        {
            var alpha = Mathf.Clamp01(1f - (Time.time - _damageAnimStartTime) / damageAnimLength);
            meshRenderer.material.SetFloat(DamageParam, alpha);
            yield return null;
        }
    }

    public float GetHealth()
    {
        return _health;
    }

    // Can be used to add or subtract health
    public float AddHealth(float amount)
    {
        if (IsAlive())
        {
            var oldHealth = _health;
            _health = Mathf.Clamp01(_health + amount);
            // Make sure the player dies when they are really close to zero
            if (_health < 0.01f)
            {
                _health = 0f;
            }

            var newDead = IsDead();

            if (amount < 0f)
            {
                // Start the anim coroutine
                _damageAnimStartTime = Time.time;
                StartCoroutine(DamagedAnimCoroutine());
                
                // Play the hurt sound periodically and the death sound on death
                if (newDead || Time.time - _lastHurtAudioTime > hurtAudioDelay)
                {
                    _lastHurtAudioTime = Time.time;
                    AudioManager.Instance.PlayAudio(newDead ? deathAudio : hurtAudio);
                }
            }

            if (newDead)
            {
                OnDied();
            }

            return _health - oldHealth;
        }

        return 0f;
    }
}
