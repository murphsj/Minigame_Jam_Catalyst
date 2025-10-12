using UnityEngine;

public class Enemy_CATALYST : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float leftBoundary = -16f;
    public float rightBoundary = 11f;
    
    bool movingRight = true;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    public void Initialize(bool movingRight)
    {
        this.movingRight = movingRight;
        GetComponent<SpriteRenderer>().flipX = movingRight;
    }
    
    void Update()
    {
        if (rb != null)
        {
            float currentXVelocity = movingRight ? moveSpeed : -moveSpeed;
            rb.linearVelocity = new Vector2(currentXVelocity, rb.linearVelocity.y);
        }
        
        if (transform.position.x >= rightBoundary && movingRight)
        {
            Destroy(gameObject);
        }
        else if (transform.position.x <= leftBoundary && !movingRight)
        {
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Player_CATALYST player = other.GetComponent<Player_CATALYST>();
        if (player != null)
        {
            Vector2 knockbackDir = movingRight ? Vector2.right : Vector2.left;
            player.TakeDamage(1, 20f, knockbackDir);
        }
    }
}
