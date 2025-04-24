using System;
using UnityEngine;

namespace TarodevController
{
 
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        private GameManager2D gameManager;

        private float _fallTime;
        private bool _falling;

        // Dash variables
        private bool _isDashing;
        private float _dashTime;
        private float _lastDashTime;
        private const float DashDuration = 0.2f; // Duration of the dash
        private const float DashCooldown = 1f; // Cooldown between dashes
        private Vector2 _lastMoveDirection = Vector2.right; // Default to right direction

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

      

        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void Start()
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager2D>();
        }

        private void Update()
        {
            _time += Time.deltaTime;
            GatherInput();
        }

        private void GatherInput()
        {
            _frameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                DashDown = Input.GetButtonDown("Dash") || Input.GetKeyDown(KeyCode.LeftShift) // Assuming LeftShift is used for dash

            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
            // Store the last movement direction if the player is moving horizontally
            if (_frameInput.Move.x != 0)
            {
                _lastMoveDirection = new Vector2(_frameInput.Move.x, 0).normalized;
            }
        }

        private void FixedUpdate()
        {
            CheckCollisions();
            HandleJump();
            HandleFallState();
            HandleDash();
            HandleDirection();
            HandleGravity();
            ApplyMovement();
        }
    

     #region Fall Detection

        private void HandleFallState()
        {
            if (!_grounded && _rb.velocity.y < 0) // Player is in the air and falling
            {
                if (!_falling)
                {
                    _falling = true;
                    _fallTime = 0; // Start counting fall time
                }

                _fallTime += Time.fixedDeltaTime;

                if (_fallTime > 1f) // Check if falling for more than 1 seconds
                {
                    _stats.deathState = true;
                }
            }
            else if (_grounded) // Reset fall state when grounded
            {
                _falling = false;
                _fallTime = 0;
            }
        }

        #endregion

         #region Dash

    private void HandleDash()
    {
        if (_frameInput.DashDown && !_isDashing && _time - _lastDashTime > _stats.DashCooldown)
        {
            _isDashing = true;
            _frameVelocity = _lastMoveDirection * _stats.MaxSpeed * _stats.DashMultiplier;
            _dashTime = DashDuration;
            _lastDashTime = _time; // Record the time of the dash
        }

        if (_isDashing)
        {
            // Apply dash multiplier to speed
            _frameVelocity.x = Mathf.Sign(_frameVelocity.x) * _stats.MaxSpeed * _stats.DashMultiplier;
            _dashTime -= Time.fixedDeltaTime;

            // End dash after the duration
            if (_dashTime >= DashDuration) {
                _isDashing = false;
                _dashTime = 0;
            }
            if (_dashTime <= 0)
            {
                _isDashing = false;
                _dashTime = 0;
            }
        }
    }

    #endregion


        #region Collisions
        
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion


        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;


        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;


        private bool _hasDoubleJumped; // Tracks whether the double jump has been used

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) 
                _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) 
                return;

            if (_grounded || CanUseCoyote)
            {
                _hasDoubleJumped = false; // Reset double jump when grounded
                ExecuteJump(_stats.JumpPower);
            }
            else if (!_hasDoubleJumped)
            {
                // Perform double jump
                _hasDoubleJumped = true;
                ExecuteJump(_stats.JumpPower * _stats.DoubleJumpMultiplier); // Reduce power for the second jump (adjust the 0.7f as needed)
            }

            _jumpToConsume = false;
        }


        private void ExecuteJump(float jumpPower)
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = jumpPower; // Use the passed jump power
            Jumped?.Invoke();
        }



        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else
            {
                var inAirGravity = _stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        private void ApplyMovement() => _rb.velocity = _frameVelocity;

         private void Dash()
        {
            _isDashing = true;
            _frameVelocity = _lastMoveDirection * _stats.MaxSpeed * _stats.DashMultiplier;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif

        // Move the collision methods **inside** the PlayerController class
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.tag == "Enemy")
            {
                _stats.deathState = true; // Say to GameManager that player is dead
            }
            else
            {
                _stats.deathState = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag == "Coin")
            {
                gameManager.coinsCounter += 1;
                Destroy(other.gameObject);
            }
        }
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public bool DashDown;  // Add DashDown input

        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;
        public Vector2 FrameInput { get; }

       
    }
}
