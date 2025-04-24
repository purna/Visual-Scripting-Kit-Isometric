using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// Updated Animator to handle isometric animations with 8-directional movement and shooting.
    /// </summary>
    public class IsometricPlayerAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _anim;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private IsometricCharacterRenderer _isometricRenderer;

        [Header("Settings")]
        [SerializeField, Range(1f, 3f)] private float _maxIdleSpeed = 2;

        [Header("Particles")]
        [SerializeField] private ParticleSystem _jumpParticles;
        [SerializeField] private ParticleSystem _launchParticles;
        [SerializeField] private ParticleSystem _moveParticles;
        [SerializeField] private ParticleSystem _landParticles;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip[] _footsteps;

        private AudioSource _source;
        private IIsometricPlayerController _player;
        private bool _grounded;
        private Vector2 _lastDirection;

        private ParticleSystem.MinMaxGradient _currentGradient;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _player = GetComponentInParent<IIsometricPlayerController>();
        }

        private void OnEnable()
        {
            _player.ShootAction += OnShoot;

            _moveParticles.Play();
        }

        private void OnDisable()
        {
            _player.ShootAction -= OnShoot;

            _moveParticles.Stop();
        }

        private void Update()
        {
            if (_player == null) return;

            DetectGroundColor();
            HandleIdleSpeed();
            HandleIsometricMovement();
        }

        private void HandleIdleSpeed()
        {
            // Update the idle animation speed based on movement input strength
            var inputStrength = _player.FrameInput.magnitude;
            _anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));

            _moveParticles.transform.localScale = Vector3.MoveTowards(
                _moveParticles.transform.localScale,
                Vector3.one * inputStrength,
                2 * Time.deltaTime
            );
        }

        private void HandleIsometricMovement()
        {
            // Use FrameInput to determine direction and pass it to the IsometricCharacterRenderer
            Vector2 input = _player.FrameInput;

            if (input.magnitude > 0.01f)
            {
                _lastDirection = input; // Update the last direction when there's input
                _isometricRenderer.SetDirection(input);
                _anim.SetBool("IsMoving", true);
            }
            else
            {
                _isometricRenderer.SetDirection(Vector2.zero); // Use static animations when idle
                _anim.SetBool("IsMoving", false);
            }
        }

        private void OnShoot()
        {
            SetColor(_jumpParticles);
            SetColor(_launchParticles);
            _jumpParticles.Play();
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
    }
}
