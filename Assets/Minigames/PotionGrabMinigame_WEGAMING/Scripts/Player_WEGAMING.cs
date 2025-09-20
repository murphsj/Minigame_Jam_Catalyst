using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles movement and input handling for the player.
/// Based on a character controller made by Sebastian Lague:
/// <see cref="https://github.com/SebLague/2DPlatformer-Tutorial"/> 
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Player_WEGAMING : MonoBehaviour
{
    public float jumpHeight = 4;
    public float timeToJumpApex = 0.4f;
    public float moveSpeedGround = 6;
    public float moveSpeedAir = 0;

    float gravity;
    float jumpVelocity;

    Vector2 velocity;
    Vector2 moveDirection;

    MovementController_WEGAMING controller;

    void Start()
    {
        controller = GetComponent<MovementController_WEGAMING>();

        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void OnInteract(InputValue inputValue)
    {
        if (!MinigameManager.IsReady()) return;

        if (controller.collisions.below)
        {
            velocity.y = jumpVelocity;
        }
    }

    void OnMove(InputValue inputValue)
    {
        if (!MinigameManager.IsReady()) return;
        moveDirection = inputValue.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (!MinigameManager.IsReady()) return;

        if (controller.collisions.below || moveSpeedAir > 0)
        {
            velocity.x = moveDirection.x * (controller.collisions.below ? moveSpeedGround : moveSpeedAir);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
    }


}
