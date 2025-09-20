using UnityEngine;

/// <summary>
/// Handles movement and collision detection for the player.
/// Based on a character controller made by Sebastian Lague:
/// <see cref="https://github.com/SebLague/2DPlatformer-Tutorial"/> 
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class MovementController_WEGAMING : MonoBehaviour
{
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomleft, bottomRight;
    }

    const float skinWidth = 0.15f;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D collider;
    RaycastOrigins raycastOrigins;


    void Start()
    {
        collider = GetComponent<BoxCollider2D>();

        UpdateRaycastOrigins();
        CalculateRaySpacing();
    }

    /// <summary>
    /// Moves the controller in the specified direction.
    /// </summary>
    /// <param name="velocity">The distance to move</param>
    public void Move(Vector2 velocity)
    {

    }

    void HorizontalCollision(ref Vector2 velocity)
    {
        
    }

    void VerticalCollision(ref Vector2 velocity)
    {

    }

    /// <summary>
    /// Recalculates raycast origins. Should be called if the controller size is updated
    /// </summary>
    void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.max.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    /// <summary>
    /// Calculates the spacing between raycasts based on ray count. Should be called if ray count is updated
    /// </summary>
    void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.x / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.y / (verticalRayCount - 1);
    }
}
