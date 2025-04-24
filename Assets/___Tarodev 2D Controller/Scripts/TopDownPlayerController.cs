using System;
using UnityEngine;

namespace TarodevController
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class TopDownPlayerController : MonoBehaviour, ITopDownPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        [SerializeField] private GameObject _bulletPrefab;  // Bullet prefab to be shot
        [SerializeField] private Transform _shootPoint;     // Point from where the bullet will spawn
        [SerializeField] private GameObject _shootParticleEffect; // Particle effect to show when firing (jump effect)
        private GameManager2D gameManager;

        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private TopDownFrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        private TopDownPlayerAnimator topDownPlayerAnimator;

        private Vector2 _facingDirection = Vector2.up; // Default facing direction (up)

        // Dash variables
        private bool _isDashing;
        private float _dashTime;
        private float _lastDashTime;
        private const float DashDuration = 0.2f; // Duration of the dash
        private const float DashCooldown = 1f; // Time before you can dash again
        private Vector2 _lastMoveDirection = Vector2.right; // Default to right direction


        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action ShootAction;  // Event for shooting

        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            if (_shootPoint == null)
            {
                _shootPoint = transform;  // Default to the player's position if no shoot point is assigned
            }
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
            // Shooting with space key (or you can change it to another input if needed)
            _frameInput = new TopDownFrameInput
            {
                ShootDown = Input.GetKeyDown(KeyCode.Space),  // Space key to shoot
                ShootHeld = Input.GetKey(KeyCode.Space),
                Move = new Vector2(0, 0), // Initialize Move vector
                DashDown = Input.GetButtonDown("Dash") || Input.GetKeyDown(KeyCode.LeftShift) // Assuming LeftShift is used for dash

            };

            // W (Move Up) and S (Move Down)
            float verticalInput = 0;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) verticalInput = 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) verticalInput = -1;

            // A (Move Left) and D (Move Right)
            float horizontalInput = 0;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1;

            // Set both horizontal and vertical inputs to Move vector
            _frameInput.Move.x = horizontalInput;
            _frameInput.Move.y = verticalInput;

            if (_stats.SnapInput)
            {
                // Snap input for both axes (horizontal and vertical)
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            // Update player facing direction based on movement
            if (_frameInput.Move != Vector2.zero)
            {
                _facingDirection = _frameInput.Move.normalized;
            }

            if (_frameInput.ShootDown)
            {
                ShootAction?.Invoke(); // Trigger shoot action
            }
            // Store the last movement direction if the player is moving horizontally
            if (_frameInput.Move.x != 0)
            {
                _lastMoveDirection = new Vector2(_frameInput.Move.x, 0).normalized;
            }
            // Store the last movement direction if the player is moving vertically
            if (_frameInput.Move.y != 0)
            {
                _lastMoveDirection = new Vector2(0, _frameInput.Move.y).normalized;
            }
        }

        private void FixedUpdate()
        {
            CheckCollisions();
            HandleShoot();
            HandleDash();
            HandleDirection();
            ApplyMovement();
        }

        #region Dash
       private void HandleDash()
        {
            // Check if Dash is triggered and the cooldown has passed
            if (_frameInput.DashDown && !_isDashing && _time - _lastDashTime > _stats.DashCooldown)
            {
                _isDashing = true;

                // If the player is stationary, use the last movement direction
                if (_frameInput.Move == Vector2.zero)
                {
                    // If the player is stationary, dash in the direction of the last movement
                    if (_lastMoveDirection.x != 0)
                    {
                        // Dash horizontally (left or right)
                        _frameVelocity = new Vector2(Mathf.Sign(_lastMoveDirection.x), 0) * _stats.MaxSpeed * _stats.DashMultiplier;
                    }
                    else if (_lastMoveDirection.y != 0)
                    {
                        // Dash vertically (up or down)
                        _frameVelocity = new Vector2(0, Mathf.Sign(_lastMoveDirection.y)) * _stats.MaxSpeed * _stats.DashMultiplier;
                    }
                }
                else
                {
                    // Update the last movement direction to the current movement input
                    _lastMoveDirection = _frameInput.Move.normalized;

                    // Apply dash speed in the direction of the last move
                    if (_frameInput.Move.x != 0)
                    {
                        // Dash horizontally (left or right)
                        _frameVelocity = new Vector2(Mathf.Sign(_frameInput.Move.x), 0) * _stats.MaxSpeed * _stats.DashMultiplier;
                    }
                    else if (_frameInput.Move.y != 0)
                    {
                        // Dash vertically (up or down)
                        _frameVelocity = new Vector2(0, Mathf.Sign(_frameInput.Move.y)) * _stats.MaxSpeed * _stats.DashMultiplier;
                    }
                }

                _dashTime = DashDuration; // Set dash duration
                _lastDashTime = _time; // Record the time of the dash
            }

            // If we are dashing, apply the dash logic
            if (_isDashing)
            {
                // Apply dash velocity (only in the direction set above)
                _rb.velocity = _frameVelocity;

                _dashTime -= Time.fixedDeltaTime;

                // End dash after the duration
                if (_dashTime <= 0)
                {
                    _isDashing = false;
                    _dashTime = 0;
                }
            }
        }





    #endregion

        #region Collisions

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling (Note: You can modify these for top-down behavior if you need to check for obstacles)
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Hit a Ceiling
            // (For top-down games, you may not need ceiling detection, so you can remove this if unnecessary)
            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion

        #region Shooting (formerly Jumping)

        private void HandleShoot()
        {
            if (_frameInput.ShootDown)
            {
                FireBullet();
                PlayJumpParticleEffect(); // Show the jump particle effect on fire
            }
        }

private void FireBullet()
{
    // Ensure there is a bullet prefab to instantiate
    if (_bulletPrefab != null)
    {
        // Number of bullets to fire in the spread
        //int bulletCount = 10;  // Adjust this to control how many bullets you want to fire in the spray

        // Spread angle in degrees (how wide the spread is)
        //float spreadAngle = 30f;  // You can adjust the spread angle

        // Fire bullets in a spread pattern
        for (int i = 0; i < _stats.bulletCount; i++)
        {
            // Calculate a random angle within the spread range
            float angleOffset = UnityEngine.Random.Range(-_stats.spreadAngle / 2f, _stats.spreadAngle / 2f);

            // Create the bullet
            GameObject bullet = Instantiate(_bulletPrefab, _shootPoint.position, Quaternion.identity);

            // Apply the spread to the bullet's direction
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                // Apply the facing direction with an additional angle offset
                Vector2 directionWithSpread = Quaternion.Euler(0, 0, angleOffset) * _facingDirection;
                bulletRb.velocity = directionWithSpread * _stats.BulletSpeed;
            }

            // Disable collision between bullet and player
            Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), _col, true);
        }
    }
}


        private void PlayJumpParticleEffect()
        {
            if (_shootParticleEffect != null)
            {
               _shootParticleEffect.GetComponent<ParticleSystem>().Play();
             }
        }

        #endregion

        #region Horizontal and Vertical Movement

       private void HandleDirection()
        {
            // Horizontal movement (left/right)
            if (_frameInput.Move.x == 0)
            {
                // Deceleration when no movement input is given
                var deceleration = _stats.GroundDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                // Acceleration when movement input is given
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }

            // Vertical movement (up/down)
            if (_frameInput.Move.y == 0)
            {
                // Deceleration when no vertical input is given
                var deceleration = _stats.GroundDeceleration;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, 0, deceleration * Time.fixedDeltaTime);
            }
            else
            {
                // Acceleration when vertical input is given
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, _frameInput.Move.y * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }



        #endregion

        private void ApplyMovement() => _rb.velocity = _frameVelocity;

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

    public struct TopDownFrameInput
    {
        public bool ShootDown;
        public bool ShootHeld;
        public bool DashDown;  // Add DashDown input
        public Vector2 Move;
    }

    public interface ITopDownPlayerController
    {
        public event Action ShootAction;  // Trigger action for shooting
        public Vector2 FrameInput { get; }
    }
}
