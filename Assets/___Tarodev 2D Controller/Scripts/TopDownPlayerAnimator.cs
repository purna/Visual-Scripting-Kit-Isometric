using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// Updated Animator to handle horizontal, vertical movement and shooting.
    /// </summary>
    public class TopDownPlayerAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _anim;
        [SerializeField] private SpriteRenderer _sprite;

        [Header("Settings")]
        [SerializeField, Range(1f, 3f)] private float _maxIdleSpeed = 2;
        [SerializeField] private float _maxTilt = 5;
        [SerializeField] private float _tiltSpeed = 20;

        [Header("Particles")]
        [SerializeField] private ParticleSystem _jumpParticles;
        [SerializeField] private ParticleSystem _launchParticles;
        [SerializeField] private ParticleSystem _moveParticles;
        [SerializeField] private ParticleSystem _landParticles;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] _footsteps;

        private AudioSource _source;
        private ITopDownPlayerController _player;
        private bool _grounded;
        private ParticleSystem.MinMaxGradient _currentGradient;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _player = GetComponentInParent<ITopDownPlayerController>();
        }

       public void OnEnable()
        {
            _player.ShootAction += OnShoot;

            _moveParticles.Play();
        }

        public void OnDisable()
        {
            _player.ShootAction -= OnShoot;

            _moveParticles.Stop();
        }

        private void Update()
        {
            if (_player == null) return;

            DetectGroundColor();
            HandleSpriteFlip();
            HandleIdleSpeed();
            HandleCharacterTilt();
            HandleMovementAnimation();
        }

        private void HandleSpriteFlip()
        {
            if (_player.FrameInput.x != 0)
                _sprite.flipX = _player.FrameInput.x < 0;
        }

        private void HandleIdleSpeed()
        {
            // Update the idle animation speed based on horizontal movement input strength
            var inputStrengthX = Mathf.Abs(_player.FrameInput.x);
            var inputStrengthY = Mathf.Abs(_player.FrameInput.y);

            if (inputStrengthY == 0) {
                _anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrengthX));

                    _moveParticles.transform.localScale = Vector3.MoveTowards(
                    _moveParticles.transform.localScale,
                    Vector3.one * inputStrengthX,
                    2 * Time.deltaTime
                );
            }

            if (inputStrengthX == 0) {
               _anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrengthY));

                    _moveParticles.transform.localScale = Vector3.MoveTowards(
                    _moveParticles.transform.localScale,
                    Vector3.one * inputStrengthY,
                    2 * Time.deltaTime
                );
            }
        }



        private void HandleCharacterTilt()
        {
            var runningTilt = _grounded ? Quaternion.Euler(0, 0, _maxTilt * _player.FrameInput.x) : Quaternion.identity;
            _anim.transform.up = Vector3.RotateTowards(_anim.transform.up, runningTilt * Vector2.up, _tiltSpeed * Time.deltaTime, 0f);
        }

        private void HandleMovementAnimation()
        {
            // Set animator parameters for horizontal and vertical movement
            _anim.SetFloat("Horizontal", _player.FrameInput.x);
            _anim.SetFloat("Vertical", _player.FrameInput.y);

            // If moving in either axis, ensure the movement animation is playing
            if (_player.FrameInput.x != 0 || _player.FrameInput.y != 0)
            {
                _anim.SetBool("IsMoving", true);
            }
            else
            {
                _anim.SetBool("IsMoving", false);
            }
        }

          private void OnShoot()
        {
                SetColor(_jumpParticles);
                SetColor(_launchParticles);
                _jumpParticles.Play();

        }

        private void OnJumped()
        {
            _anim.SetTrigger(JumpKey);
            _anim.ResetTrigger(GroundedKey);

            if (_grounded) // Avoid coyote
            {
                SetColor(_jumpParticles);
                SetColor(_launchParticles);
                _jumpParticles.Play();
            }
        }

        private void OnGroundedChanged(bool grounded, float impact)
        {
            _grounded = grounded;

            if (grounded)
            {
                DetectGroundColor();
                SetColor(_landParticles);

                _anim.SetTrigger(GroundedKey);
                _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
                _moveParticles.Play();

                _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
                _landParticles.Play();
            }
            else
            {
                _moveParticles.Stop();
            }
        }

        private void DetectGroundColor()
        {
            var hit = Physics2D.Raycast(transform.position, Vector3.down, 2);

            if (!hit || hit.collider.isTrigger || !hit.transform.TryGetComponent(out SpriteRenderer r)) return;

            var color = r.color;
            _currentGradient = new ParticleSystem.MinMaxGradient(color * 0.9f, color * 1.2f);
            SetColor(_moveParticles);
        }

        private void SetColor(ParticleSystem ps)
        {
            var main = ps.main;
            main.startColor = _currentGradient;
        }

        private static readonly int GroundedKey = Animator.StringToHash("Grounded");
        private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
        private static readonly int JumpKey = Animator.StringToHash("Jump");
    }
}
