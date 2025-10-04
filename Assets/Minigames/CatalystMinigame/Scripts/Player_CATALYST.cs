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
    bool isHoldingDoubleJump = false;
    float doubleJumpHoldTime = 0f;
    MovementController_CATALYST controller;
    
    // Health and damage variables
    private int currentHealth;
    private bool isInvincible = false;
    private bool isKnockedBack = false;
    private Vector2 knockbackVelocity;
    private float invincibilityTimer;
    private float knockbackTimer;
    private SpriteRenderer spriteRenderer;

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

    bool CanPlayerAct()
    {
        return !(playerState == PlayerState.Damaged || playerState == PlayerState.Deposit);
    }

    void UpdateFlipped()
    {
        if (flipped)
        {
            gameObject.transform.localScale = new Vector2(1, gameObject.transform.localScale.y);
        }
        else
        {
            gameObject.transform.localScale = new Vector2(-1, gameObject.transform.localScale.y);
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
    
    // Public getters for UI or other systems
    public int GetCurrentHealth() { return currentHealth; }
    public int GetMaxHealth() { return maxHealth; }
    public bool IsInvincible() { return isInvincible; }
}
