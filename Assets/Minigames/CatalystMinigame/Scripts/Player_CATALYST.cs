using System;
using System.Collections;
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
    
    [Header("Plunge Attack")]
    public float minPlungeHeight = 1f;
    public float maxPlungeHeight = 5f;
    public float chargeRate = 2f;
    public float plungeRadius = 4f;
    public int plungeDamage = 3;
    
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
    private float plungeCharge = 0f;
    private bool isPowerupInvincible = false; // Separate from damage invincibility
    

    PlayerState playerState;
    DropletType[] flaskStorage;

    void SetState(PlayerState state)
    {
        playerState = state;
    }

    void Start()
    {
        // Get components
        controller = GetComponent<MovementController_CATALYST>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Additional check after a short delay
        StartCoroutine(DelayedControllerCheck());

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
    
    IEnumerator DelayedControllerCheck()
    {
        yield return new WaitForSeconds(0.1f);
        if (controller == null)
        {
            controller = GetComponent<MovementController_CATALYST>();
        }
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
        
        // Update plunge charge while holding E
        if (isHoldingSlam)
        {
            plungeCharge += chargeRate * Time.deltaTime;
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
        
        // Make powered-up players invincible (separate from damage invincibility)
        if (isPoweredUp)
        {
            isPowerupInvincible = true;
        }
        else
        {
            isPowerupInvincible = false;
        }
    }

    void OnInteract(InputValue inputValue)
    {
        if (!MinigameManager.IsReady()) return;
        if (!CanPlayerAct()) return;

        if (inputValue.isPressed)
        {
            // Button pressed down
            if (controller != null && controller.collisions.below)
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
        if (!MinigameManager.IsReady() || !CanPlayerAct()) return;
        
        if (inputValue.isPressed)
        {
            // First E press - start charging
            if (isPoweredUp && pounceCharges > 0 && controller != null && controller.collisions.below)
            {
                if (!isHoldingSlam)
                {
                    isHoldingSlam = true;
                    plungeCharge = 0f;
                }
                else
                {
                    // Second E press - execute plunge
                    PerformPlungeAttack();
                    isHoldingSlam = false;
                }
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

        // Try to get controller if we don't have it yet
        if (controller == null)
        {
            controller = GetComponent<MovementController_CATALYST>();
        }
        
        // Apply movement and gravity
        if (controller != null)
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            // Fallback: apply basic movement without collision detection
            transform.Translate(velocity * Time.deltaTime);
        }

        if (controller != null && (controller.collisions.above || controller.collisions.below))
        {
            velocity.y = 0;
        }

        if (controller != null && controller.collisions.below)
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
        // Don't take damage if invincible (either from damage or powerup)
        if (isInvincible || isPowerupInvincible)
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
    
    void PerformPlungeAttack()
    {
        if (pounceCharges <= 0) return;
        
        pounceCharges--;
        
        // Calculate jump height based on charge time
        float jumpHeight = Mathf.Lerp(minPlungeHeight, maxPlungeHeight, Mathf.Clamp01(plungeCharge));
        
        // Vertical jump
        velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(gravity));
        SetState(PlayerState.Jump);
        
        // Screen shake
        Camera.main.GetComponent<CameraShake>()?.Shake(screenShakeIntensity, screenShakeDuration);
        
        // Visual effect
        if (spriteRenderer != null)
        {
            StartCoroutine(GlowEffect());
        }
        
        // Start drill down effect after reaching peak
        StartCoroutine(DrillDownEffect());
    }
    
    IEnumerator DrillDownEffect()
    {
        // Wait for player to reach peak of jump
        yield return new WaitUntil(() => velocity.y <= 0);
        
        // Wait for player to land on ground
        yield return new WaitUntil(() => controller != null && controller.collisions.below);
        
        
        // Kill all enemies in radius
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, plungeRadius);
        
        foreach (Collider2D enemy in enemies)
        {
            Enemy_CATALYST enemyScript = enemy.GetComponent<Enemy_CATALYST>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(plungeDamage);
            }
        }
        
        // Screen shake for impact
        Camera.main.GetComponent<CameraShake>()?.Shake(screenShakeIntensity * 1.5f, screenShakeDuration);
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

