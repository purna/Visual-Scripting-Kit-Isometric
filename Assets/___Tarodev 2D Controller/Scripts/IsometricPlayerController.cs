using System;
using UnityEngine;

namespace TarodevController // Renamed class namespace
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class IsometricPlayerController : MonoBehaviour, IIsometricPlayerController
    {
        [SerializeField] private ScriptableStats _stats;
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private Transform _shootPoint;
        [SerializeField] private GameObject _shootParticleEffect;
        private GameManager2D gameManager;

        private Rigidbody2D _rb;
        private CircleCollider2D _col;
        private IsometricFrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        private Vector2 _facingDirection = Vector2.up;

     // Dash variables
        private bool _isDashing;
        private float _dashTime;
        private float _lastDashTime;
        private const float DashDuration = 0.2f; // Duration of the dash
        private const float DashCooldown = 1f; // Time before you can dash again


        private Vector2 _lastMoveDirection = new Vector2(1, 1); // Default diagonal

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action ShootAction;

        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponentInChildren<CircleCollider2D>();
            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

            if (_shootPoint == null)
            {
                _shootPoint = transform;
            }
        }

        private void Start()
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager2D>();
        }

        private void Update()
        {
                Debug.Log($"Player Scale: {transform.localScale}");

                _time += Time.deltaTime;
                GatherInput();
        
        }


        private void GatherInput()
        {
            _frameInput = new IsometricFrameInput
            {
                ShootDown = Input.GetKeyDown(KeyCode.Space),
                ShootHeld = Input.GetKey(KeyCode.Space),
                Move = new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")
                ),
                DashDown = Input.GetButtonDown("Dash") || Input.GetKeyDown(KeyCode.LeftShift)
            };

            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            if (_frameInput.Move != Vector2.zero)
            {
                _facingDirection = _frameInput.Move.normalized;
            }

            if (_frameInput.ShootDown)
            {
                ShootAction?.Invoke();
            }

            if (_frameInput.Move != Vector2.zero)
            {
                _lastMoveDirection = _frameInput.Move.normalized;
            }
        }

        private void FixedUpdate()
        {
            HandleDash();
            HandleShoot();
            ApplyMovement();
        }

        private void HandleDash()
        {
            if (_frameInput.DashDown && !_isDashing && _time - _lastDashTime > _stats.DashCooldown)
            {
                _isDashing = true;

                Vector2 dashDirection = _frameInput.Move == Vector2.zero ? _lastMoveDirection : _frameInput.Move.normalized;
                _frameVelocity = dashDirection * _stats.MaxSpeed * _stats.DashMultiplier;

                _dashTime = DashDuration;
                _lastDashTime = _time;
            }

            if (_isDashing)
            {
                _rb.velocity = _frameVelocity;
                _dashTime -= Time.fixedDeltaTime;

                if (_dashTime <= 0)
                {
                    _isDashing = false;
                    _dashTime = 0;
                }
            }
        }

        private void HandleShoot()
        {
            if (_frameInput.ShootDown)
            {
                FireBullet();
                PlayShootParticleEffect();
            }
        }

        private void FireBullet()
        {
            if (_bulletPrefab != null)
            {
                for (int i = 0; i < _stats.bulletCount; i++)
                {
                    float angleOffset = UnityEngine.Random.Range(-_stats.spreadAngle / 2f, _stats.spreadAngle / 2f);
                    GameObject bullet = Instantiate(_bulletPrefab, _shootPoint.position, Quaternion.identity);

                    Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                    if (bulletRb != null)
                    {
                        Vector2 directionWithSpread = Quaternion.Euler(0, 0, angleOffset) * _facingDirection;
                        bulletRb.velocity = directionWithSpread * _stats.BulletSpeed;
                    }

                    Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), _col, true);
                }
            }
        }

        private void PlayShootParticleEffect()
        {
            if (_shootParticleEffect != null)
            {
                _shootParticleEffect.GetComponent<ParticleSystem>().Play();
            }
        }

private void ApplyMovement()
{
    // Map movement to cardinal directions in isometric space
    Vector2 isometricMove = Vector2.zero;

    if (_frameInput.Move.x > 0 && _frameInput.Move.y == 0) // Right (East)
        isometricMove = new Vector2(1, 0);
    else if (_frameInput.Move.x < 0 && _frameInput.Move.y == 0) // Left (West)
        isometricMove = new Vector2(-1, 0);
    else if (_frameInput.Move.x == 0 && _frameInput.Move.y > 0) // Up (North)
        isometricMove = new Vector2(0, 1);
    else if (_frameInput.Move.x == 0 && _frameInput.Move.y < 0) // Down (South)
        isometricMove = new Vector2(0, -1);
    else if (_frameInput.Move.x < 0 && _frameInput.Move.y > 0) // Left + Up (Northwest)
        isometricMove = new Vector2(-2, 1).normalized;
    else if (_frameInput.Move.x > 0 && _frameInput.Move.y > 0) // Right + Up (Northeast)
        isometricMove = new Vector2(2, 1).normalized;
    else if (_frameInput.Move.x < 0 && _frameInput.Move.y < 0) // Left + Down (Southwest)
        isometricMove = new Vector2(-2, -1).normalized;
    else if (_frameInput.Move.x > 0 && _frameInput.Move.y < 0) // Right + Down (Southeast)
        isometricMove = new Vector2(2, -1).normalized;

    // Scale movement by max speed and apply it to velocity
    _frameVelocity = isometricMove * _stats.MaxSpeed;

    // Smoothly adjust velocity if not dashing
    if (!_isDashing)
    {
        _rb.velocity = Vector2.MoveTowards(
            _rb.velocity,
            _frameVelocity,
            _stats.Acceleration * Time.fixedDeltaTime
        );
    }
}


        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                _stats.deathState = true;
            }
            else
            {
                _stats.deathState = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Coin"))
            {
                gameManager.coinsCounter += 1;
                Destroy(other.gameObject);
            }
        }
    }

    public struct IsometricFrameInput
    {
        public bool ShootDown;
        public bool ShootHeld;
        public bool DashDown;
        public Vector2 Move;
    }

    public interface IIsometricPlayerController
    {
        public event Action ShootAction;
        public Vector2 FrameInput { get; }
    }
}
