using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Handles movement and input handling for the player.
/// Based on a character controller made by Sebastian Lague:
/// <see cref="https://github.com/SebLague/2DPlatformer-Tutorial"/> 
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Player_CATALYST : MonoBehaviour
{
    enum PlayerState
    {
        Idle,
        Walk,
        Jump,
        Damaged,
        Deposit
    }

    [Header("Movement")]
    public float jumpHeight = 4;
    public float timeToJumpApex = 0.4f;
    public float moveSpeedGround = 6;
    public float moveSpeedAir = 0;
    
    [Header("Double Jump")]
    public bool allowDoubleJump = true;
    public float doubleJumpHeight = 3;
    public float maxForwardForce = 8f;
    public float forwardForcePerSecond = 8f;
    
    [Header("Ground Slam")]
    public float slamForce = 15f;
    public float slamRadius = 3f;
    public int slamDamage = 2;
    public float screenShakeIntensity = 0.5f;
    public float screenShakeDuration = 0.3f;
    
    [Header("Health System")]
    public int maxHealth = 9;
    public float invincibilityDuration = 1.5f;
    public float knockbackDuration = 0.3f;
    
    [Header("Visual Feedback")]
    public float flashDuration = 0.1f;

    // The numeric ID of the global shader property used to control the
    // visual contents of the flask
    static readonly int flaskLayerColorId;

    static Player_CATALYST()
    {
        flaskLayerColorId = Shader.PropertyToID("_LayerColors");
    }

    // Movement variables
    float gravity;
    float jumpVelocity;
    float doubleJumpVelocity;
    Vector2 velocity;
    Vector2 moveDirection;
    bool flipped;
    bool hasDoubleJumped = false;
    Vector3 originalScale;
    bool lastFlippedState;
    bool isHoldingDoubleJump = false;
    float doubleJumpHoldTime = 0f;
    MovementController_CATALYST controller;
    Animator animator;
    
    // Health and damage variables
    private int currentHealth;
    private bool isInvincible = false;
    private bool isKnockedBack = false;
    private Vector2 knockbackVelocity;
    private float invincibilityTimer;
    private float knockbackTimer;
    private SpriteRenderer spriteRenderer;
    
    // Powerup system
    private int pounceCharges = 0;
    private bool isPoweredUp = false;
    private float powerupTimer = 0f;
    private bool isHoldingSlam = false;

    PlayerState playerState;
    DropletType[] flaskStorage;

    void SetState(PlayerState state)
    {
        playerState = state;
    }

    void Start()
    {
        controller = GetComponent<MovementController_CATALYST>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Store original scale to preserve it during flipping
        originalScale = transform.localScale;
        lastFlippedState = flipped;

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        doubleJumpVelocity = Mathf.Sqrt(2 * doubleJumpHeight * Mathf.Abs(gravity));
        SetState(PlayerState.Idle);

        // Initialize health
        currentHealth = maxHealth;

        flaskStorage = new DropletType[10];
        UpdateFlaskSprite();
    }

    void Update()
    {
        // Handle double jump with forward force (leap)
        if (isHoldingDoubleJump)
        {
            // Only add forward force if player is actually moving (left or right)
            if (moveDirection.x != 0)
            {
                //gradually add forward velocity while holding
                float forwardDirection = moveDirection.x > 0 ? 1f : -1f;
                
                //add forward force gradually, but cap at maxForwardForce
                float currentForwardVelocity = Mathf.Abs(velocity.x);
                if (currentForwardVelocity < maxForwardForce)
                {
                    float additionalForce = forwardForcePerSecond * Time.deltaTime;
                    velocity.x += additionalForce * forwardDirection;
                }
            }
        }
        
        // Handle powerup timer
        if (isPoweredUp)
        {
            powerupTimer -= Time.deltaTime;
            if (powerupTimer <= 0)
            {
                isPoweredUp = false;
                pounceCharges = 0;
                // Stop glowing
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.white;
            }
        }
    }

    void OnInteract(InputValue inputValue)
    {
        if (!MinigameManager.IsReady()) return;
        if (!CanPlayerAct()) return;

        if (inputValue.isPressed)
        {
            // Button pressed down
            if (controller.collisions.below)
            {
                //normal jump when on ground
                velocity.y = jumpVelocity;
                hasDoubleJumped = false; //reset double jump when touching ground
                SetState(PlayerState.Jump);
            }
            else if (allowDoubleJump && !hasDoubleJumped && velocity.y < 0)
            {
                //start the double jump with immediate upward boost
                velocity.y = doubleJumpVelocity;
                hasDoubleJumped = true;

                isHoldingDoubleJump = true;
            }
        }
        else
        {
            //if the spacebar is released: stop adding forward force
            if (isHoldingDoubleJump)
            {
                isHoldingDoubleJump = false;
            }
        }
    }
    

    void OnMove(InputValue inputValue)
    {
        if (!MinigameManager.IsReady()) return;
        moveDirection = inputValue.Get<Vector2>();
        if (moveDirection.x != 0)
        {
            flipped = moveDirection.x > 0;
        }
    }
    
    void OnGroundSlam(InputValue inputValue)
    {
        if (!MinigameManager.IsReady()) return;
        if (!CanPlayerAct()) return;
        
        if (inputValue.isPressed)
        {
            // Start ground slam
            if (isPoweredUp && pounceCharges > 0 && controller.collisions.below)
            {
                isHoldingSlam = true;
                Debug.Log("Charging ground slam...");
            }
        }
        else
        {
            // Release ground slam
            if (isHoldingSlam)
            {
                PerformGroundSlam();
                isHoldingSlam = false;
            }
        }
    }

    bool CanPlayerAct()
    {
        return !(playerState == PlayerState.Damaged || playerState == PlayerState.Deposit);
    }

    void UpdateFlipped()
    {
        // Only update scale if flip state has changed
        if (flipped != lastFlippedState)
        {
            if (flipped)
            {
                transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            }
            lastFlippedState = flipped;
        }
    }

    void FixedUpdate()
    {
        if (!MinigameManager.IsReady()) return;

        if (playerState == PlayerState.Walk || (moveSpeedAir > 0 && playerState == PlayerState.Jump))
        {
            UpdateFlipped();
            velocity.x = moveDirection.x * (controller.collisions.below ? moveSpeedGround : moveSpeedAir);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        if (controller.collisions.below)
        {
            if (playerState == PlayerState.Jump)
            {
                SetState(PlayerState.Idle);
                velocity.x = 0;
            }
            if (CanPlayerAct()) SetState(moveDirection.x != 0 ? PlayerState.Walk : PlayerState.Idle);
        }

        UpdateDamageSystem();
    }
    
    void UpdateDamageSystem()
    {
        // Handle invincibility timer
        if (isInvincible)
        {
            invincibilityTimer -= Time.fixedDeltaTime;
            
            // Flash effect during invincibility
            if (spriteRenderer != null)
            {
                float flashTime = Mathf.Sin(Time.time * 20f); // Fast flashing
                spriteRenderer.color = new Color(1f, 1f, 1f, flashTime > 0 ? 1f : 0.5f);
            }

            if (invincibilityTimer < 1)
            {
                // allow movement again
                SetState(PlayerState.Idle);
            }
            else
            {
                velocity.x /= 1.3f;
                velocity.y /= 1.3f;
            }
            
            if (invincibilityTimer <= 0)
                {
                    isInvincible = false;
                    // Reset sprite color
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = Color.white;
                    }
                }
        }
        
        // Handle knockback timer
        if (isKnockedBack)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            
            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
                knockbackVelocity = Vector2.zero;
            }
        }
    }
    
    /// <summary>
    /// Damages the player with knockback and give player some mercy with invincibility
    /// </summary>
    /// <param name="damage">Amount of damage to deal</param>
    /// <param name="knockbackForce">Force of the knockback</param>
    /// <param name="knockbackDirection">Direction of knockback (normalized)</param>
    public void TakeDamage(int damage, float knockbackForce, Vector2 knockbackDirection)
    {
        // Don't take damage if invincible
        if (isInvincible)
            return;

        // Apply damage
        currentHealth -= damage;
        SetState(PlayerState.Damaged);
        Debug.Log($"Player took {damage} damage! Health: {currentHealth}/{maxHealth}");

        // Activate invincibility
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // Apply knockback
        knockbackVelocity = knockbackDirection.normalized * knockbackForce;
        isKnockedBack = true;
        knockbackTimer = knockbackDuration;

        // Override current velocity with knockback
        velocity = knockbackVelocity;


        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Debug.Log("Player died!");
        MinigameManager.SetStateToFailure();
        MinigameManager.EndGame();
    }

    void UpdateFlaskSprite()
    {
        Vector4[] flaskColors = new Vector4[10];
        for (int i = 0; i < flaskColors.Length; i++)
        {
            Debug.Log(flaskStorage[i].getColor());
            flaskColors[i] = flaskStorage[i].getColor();
        }
        
        Shader.SetGlobalVectorArray(flaskLayerColorId, flaskColors);
    }

    public bool AddToFlask(DropletType dropletType)
    {
        for (int i = 0; i < flaskStorage.Length; i++)
        {
            if (flaskStorage[i] == DropletType.None)
            {
                flaskStorage[i] = dropletType;
                UpdateFlaskSprite();
                return true;
            }
        }
        return false; // flask is full
    }
    
    void PerformGroundSlam()
    {
        if (pounceCharges <= 0) return;
        
        pounceCharges--;
        Debug.Log($"Ground slam! Charges remaining: {pounceCharges}");
        
        // Screen shake effect
        Camera.main.GetComponent<CameraShake>()?.Shake(screenShakeIntensity, screenShakeDuration);
        
        // Find and damage nearby enemies
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, slamRadius);
        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                Enemy_CATALYST enemyScript = enemy.GetComponent<Enemy_CATALYST>();
                if (enemyScript != null)
                {
                    // Push enemy away
                    Vector2 direction = (enemy.transform.position - transform.position).normalized;
                    enemy.GetComponent<Rigidbody2D>()?.AddForce(direction * slamForce, ForceMode2D.Impulse);
                    
                    Debug.Log($"Enemy hit by ground slam!");
                }
            }
        }
        
        // Visual effect - make player glow briefly
        if (spriteRenderer != null)
        {
            StartCoroutine(GlowEffect());
        }
    }
    
    System.Collections.IEnumerator GlowEffect()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = originalColor;
    }
    
    public void CollectPowerup(int charges, float duration)
    {
        pounceCharges += charges;
        isPoweredUp = true;
        powerupTimer = duration;
        
        // Make player glow
        if (spriteRenderer != null)
            spriteRenderer.color = Color.cyan;
            
        Debug.Log($"Powerup collected! Ground slam charges: {pounceCharges}");
    }
    
    // Public getters for UI or other systems
    public int GetCurrentHealth() { return currentHealth; }
    public int GetMaxHealth() { return maxHealth; }
    public bool IsInvincible() { return isInvincible; }
    public int GetPounceCharges() { return pounceCharges; }
    public bool IsPoweredUp() { return isPoweredUp; }
}
