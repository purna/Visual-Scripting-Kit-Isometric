using System;
using UnityEngine;

public class IsometricPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 3f;
    public float acceleration = 10f;
    public float deceleration = 10f;

    [Header("Dash Settings")]
    public float dashMultiplier = 2f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float bulletSpeed = 10f;
    public int bulletCount = 1; // Number of bullets in spread
    public float spreadAngle = 30f; // Spread angle of bullets

    [Header("Effects")]
    public GameObject shootParticleEffect;

    private Rigidbody2D rbody;
    private IsometricCharacterRenderer isoRenderer;
    private Vector2 inputVector;
    private Vector2 movementVector;
    private Vector2 facingDirection = Vector2.up; // Default facing direction

    private bool isDashing;
    private float dashTimer;
    private float lastDashTime;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();

        if (shootPoint == null)
        {
            shootPoint = transform; // Default to player position if no shoot point is assigned
        }
    }

    private void Update()
    {
        GatherInput();
        HandleShooting();
    }

    private void FixedUpdate()
    {
        HandleDash();
        ApplyMovement();
    }

    /// <summary>
    /// Gather player input for movement, dashing, and shooting.
    /// </summary>
    private void GatherInput()
    {
        // Movement input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1f);

        // Update facing direction if there is movement
        if (inputVector != Vector2.zero)
        {
            facingDirection = inputVector.normalized;
        }

        // Start a dash if Dash button (Left Shift) is pressed
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown)
        {
            StartDash();
        }
    }

    /// <summary>
    /// Handles movement physics for the player.
    /// </summary>
    private void ApplyMovement()
    {
        if (isDashing) return; // Skip movement during dashing

        // Smooth movement using acceleration and deceleration
        if (inputVector.magnitude > 0)
        {
            movementVector = Vector2.MoveTowards(movementVector, inputVector * movementSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            movementVector = Vector2.MoveTowards(movementVector, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }

        // Apply movement to the Rigidbody
        Vector2 newPosition = rbody.position + movementVector * Time.fixedDeltaTime;
        rbody.MovePosition(newPosition);

        // Update the isometric character renderer
        if (isoRenderer != null)
        {
            isoRenderer.SetDirection(movementVector);
        }
    }

    /// <summary>
    /// Handles the dashing logic.
    /// </summary>
    private void HandleDash()
    {
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }
    }

    /// <summary>
    /// Starts a dash in the current movement direction.
    /// </summary>
    private void StartDash()
    {
        if (inputVector == Vector2.zero) return; // No dash if no input

        isDashing = true;
        dashTimer = dashDuration;
        lastDashTime = Time.time;

        // Apply dash velocity
        movementVector = inputVector.normalized * movementSpeed * dashMultiplier;
    }

    /// <summary>
    /// Handles shooting logic.
    /// </summary>
    private void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireBullet();
            PlayShootParticleEffect();
        }
    }

    /// <summary>
    /// Fires a bullet or bullets from the shoot point.
    /// </summary>
    private void FireBullet()
    {
        if (bulletPrefab == null) return;

        for (int i = 0; i < bulletCount; i++)
        {
            // Calculate spread angle
            float angleOffset = UnityEngine.Random.Range(-spreadAngle / 2f, spreadAngle / 2f);

            // Spawn the bullet
            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

            // Apply velocity to the bullet
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                Vector2 directionWithSpread = Quaternion.Euler(0, 0, angleOffset) * facingDirection;
                bulletRb.linearVelocity = directionWithSpread * bulletSpeed;
            }
        }
    }

    /// <summary>
    /// Plays the shoot particle effect.
    /// </summary>
    private void PlayShootParticleEffect()
    {
        if (shootParticleEffect != null)
        {
            shootParticleEffect.GetComponent<ParticleSystem>().Play();
        }
    }
}
