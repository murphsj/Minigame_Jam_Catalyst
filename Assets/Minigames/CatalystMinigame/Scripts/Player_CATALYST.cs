using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Actions")]
    public LayerMask cauldronRaycastMask;
    public GameObject flask;
    public ScoreCounter_CATALYST scoreCounter;

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
    private ParticleSystem dropletParticleSys;
    

    PlayerState playerState;
    DropletType_CATALYST[] flaskStorage;

    void SetState(PlayerState state)
    {
        playerState = state;
    }

    void Start()
    {
        controller = GetComponent<MovementController_CATALYST>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        dropletParticleSys = GetComponent<ParticleSystem>();

        // Store original scale to preserve it during flipping
        originalScale = transform.localScale;
        lastFlippedState = !flipped;

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        doubleJumpVelocity = Mathf.Sqrt(2 * doubleJumpHeight * Mathf.Abs(gravity));
        SetState(PlayerState.Idle);

        // Initialize health
        currentHealth = maxHealth;

        flaskStorage = new DropletType_CATALYST[10];
        UpdateFlaskSprite();
    }

    void Update()
    {
        //Debug.Log(animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
        // Handle double jump with forward force (leap)
        if (CanPlayerAct() && isHoldingDoubleJump)
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
        //Animates the player
        //Changes animation parameters based on playerState enum
        if (playerState == PlayerState.Walk) //starts walking anim
        {
            animator.SetBool("walking", true);
            animator.SetBool("idling", false);
            animator.SetBool("jumping", false);
            animator.ResetTrigger("pouring");
            animator.ResetTrigger("double jumping");
            animator.ResetTrigger("hurting");
        }
        else if (playerState == PlayerState.Idle) //starts idle anim
        {
            animator.SetBool("idling", true);
            animator.SetBool("jumping", false);
            animator.SetBool("walking", false);
            animator.ResetTrigger("pouring");
            animator.ResetTrigger("double jumping");
            animator.ResetTrigger("hurting");
        }
        else if (playerState == PlayerState.Jump) //starts jumping anim
        {
            animator.SetBool("jumping", true);
            animator.SetBool("idling", false);
            animator.SetBool("walking", false);
            if (hasDoubleJumped)
            {
                animator.SetTrigger("double jumping");
                animator.SetBool("jumping", false);
            }
        }
        else if (playerState == PlayerState.Deposit) //starts pouring anim
        {
            //pouring is a trigger, meaning that it automatically goes to
            //whatever state the statemachine has the pouring animation transition to based on the other parameters
            animator.SetTrigger("pouring");
            animator.SetBool("idling", false);
            animator.SetBool("jumping", false);
            animator.SetBool("walking", false);
        }
        else if (playerState == PlayerState.Damaged) //starts taking damage anim
        {
            animator.SetTrigger("hurting");
            animator.SetBool("idling", false);
            animator.SetBool("jumping", false);
            animator.SetBool("walking", false);
            animator.ResetTrigger("pouring");
            animator.ResetTrigger("double jumping");
        }
        //animator.SetFloat("xVel", Math.Abs(velocity.x));
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
        moveDirection = inputValue.Get<Vector2>().normalized;
        if (moveDirection.x != 0)
        {
            flipped = moveDirection.x > 0;
        }
        if (controller.collisions.below && CanPlayerAct() && moveDirection.y < 0)
        {
            TryDeposit();
        }
    }

    void TryDeposit()
    {
        // Can't deposit if the flask is empty
        if (flaskStorage[0] == DropletType_CATALYST.None) return;
        // Raycast down from the center of the cat to check if we're below the cauldron
        RaycastHit2D hit = Physics2D.Raycast(flask.GetComponent<SpriteRenderer>().bounds.center, Vector2.down,
            300, cauldronRaycastMask);

        //Debug.DrawRay(spriteRenderer.bounds.center, Vector2.down * 300, Color.red);
        //Debug.Break();

        if (hit)
        {
            velocity.x = 0;
            SetState(PlayerState.Deposit);
            flask.SetActive(false);
            InvokeRepeating("ScoreDroplet", 0.05f, 0.05f);
            Invoke("EndDeposit", 0.5f);
        }
    }

    /// <summary>
    /// Removes the topmost droplet from the beaker and scores it
    /// </summary>
    void ScoreDroplet()
    {
        DropletType_CATALYST topColor = DropletType_CATALYST.None;
        int totalSize = -1;
        for (int i = 0; i < flaskStorage.Length; i++)
        {
            if (flaskStorage[i] != DropletType_CATALYST.None)
            {
                topColor = flaskStorage[i];
                totalSize++;
            }
            else
            {
                continue;
            }
        }

        if (topColor == DropletType_CATALYST.None) return;

        flaskStorage[totalSize] = DropletType_CATALYST.None;

        var emitParams = new ParticleSystem.EmitParams();
        Vector4 color = topColor.getColor();
        emitParams.startColor = Util_CATACLYST.color32FromFloat4(color);

        dropletParticleSys.Emit(emitParams, 1);

        scoreCounter.AddDroplet(topColor);
    }

    void EndDeposit()
    {
        CancelInvoke("ScoreDroplet");
        flask.SetActive(true);
        for (int i = 0; i < flaskStorage.Length; i++)
        {
            flaskStorage[i] = DropletType_CATALYST.None;
        }
        UpdateFlaskSprite();
        SetState(PlayerState.Idle);
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
        //Debug.Log(playerState);
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

            if (invincibilityTimer < 1 && playerState == PlayerState.Damaged)
            {
                velocity = Vector2.zero;
                // allow movement again
                SetState(PlayerState.Idle);
            }
            else
            {
                velocity.x /= 1.3f;
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
        // Player is invincible while depositing since it doesn't last very long
        // It would be quite frustrating to get hit while pouring
        if (playerState == PlayerState.Deposit)
            return;

        // Apply damage
        
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage! Health: {currentHealth}/{maxHealth}");
        

        SetState(PlayerState.Damaged);

        var emitParams = new ParticleSystem.EmitParams();


        for (int i = 0; i < flaskStorage.Length; i++)
        {
            if (flaskStorage[i] != DropletType_CATALYST.None)
            {
                DropletType_CATALYST thisDrop = flaskStorage[i];
                flaskStorage[i] = DropletType_CATALYST.None;
                Vector4 color = thisDrop.getColor();
                emitParams.startColor = Util_CATACLYST.color32FromFloat4(color);
                emitParams.velocity = new Vector2(Random.Range(-10, 10), Random.Range(-5, 10));

                dropletParticleSys.Emit(emitParams, 1);
            }
        }

        UpdateFlaskSprite();

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

    public bool AddToFlask(DropletType_CATALYST dropletType)
    {
        for (int i = 0; i < flaskStorage.Length; i++)
        {
            if (flaskStorage[i] == DropletType_CATALYST.None)
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
