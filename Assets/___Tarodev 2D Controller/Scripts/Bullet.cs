using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TarodevController
{
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float _bulletLifetime = 5f;  // Time before the bullet is destroyed
        [SerializeField] private ParticleSystem _impactEffect;  // Optional: Particle effect when the bullet hits a target

        [SerializeField] private float _impactEffectDuration = 1f; // Duration of the impact particle effect

        private Rigidbody2D _rb;

        private CircleCollider2D _collider;
        private SpriteRenderer _spriteRenderer; // Reference to the bullet's sprite renderer
        private bool _isDestroyed = false; // Flag to prevent multiple destruction triggers

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>(); // Get the collider
            _spriteRenderer = GetComponent<SpriteRenderer>(); // Get the sprite renderer
            _rb.collisionDetectionMode = (CollisionDetectionMode2D)CollisionDetectionMode.Continuous;
            
            StartCoroutine(DestroyBullet(gameObject));
            
        }



        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Prevent multiple destruction triggers
            if (_isDestroyed) return;

            // Check if the bullet hits an object, ignoring the player or other bullets
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bullet"))
            {
                // Maybe bounce the bullets off the player or other bullets
            }

            if (collision.gameObject.CompareTag("Player") )
            {
                return; // Ignore collisions with the player or other bullets
            }

            // Check if the collider is of type BoxCollider2D or TilemapCollider2D
            if (collision.collider is BoxCollider2D || collision.collider is TilemapCollider2D || collision.gameObject.CompareTag("Bullet"))
            {
                // Hide the bullet sprite before playing the particle effect
                _spriteRenderer.enabled = false;
                _collider.enabled = false;

                // Play the impact particle effect (if any)
                if (_impactEffect != null)
                {
                    _impactEffect.transform.position = transform.position;
                    _impactEffect.Play();
                }

                // Set the destroyed flag to prevent multiple triggers
                _isDestroyed = true;

            }

            // Destroy the bullet after the set lifetime (if it doesn't already collide)
            //StartCoroutine(DestroyBulletAfterEffect());
        }

        private IEnumerator DestroyBullet(GameObject gameObject)
        {
            // Wait for the lifetime to complete
            yield return new WaitForSeconds(_bulletLifetime);

            // Destroy the bullet game object
            Destroy(gameObject);
        }

        private IEnumerator DestroyBulletAfterEffect()
        {
            // Wait for the particle effect to finish playing or for the specified duration
            yield return new WaitForSeconds(_impactEffectDuration);

            // Destroy the bullet game object
            Destroy(gameObject);
        }

        private void OnBecameInvisible()
        {
            // Optionally, destroy the bullet when it goes off-screen (e.g., off-screen)
            //StartCoroutine(DestroyBullet(gameObject));
        }
    }
}
