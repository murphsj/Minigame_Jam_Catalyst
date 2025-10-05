using UnityEngine;

public class Powerup_CATALYST : MonoBehaviour
{
    [Header("Powerup Settings")]
    public int pounceCharges = 3;
    public float duration = 10f;
    public Color powerupColor = Color.cyan;
    
    [Header("Floating Animation")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.3f;
    
    private Vector3 startPosition;
    private float rotationSpeed = 30f; 
    
    void Start()
    {

        transform.localScale = new Vector3(0.15f, 0.15f, 1f);
        GetComponent<SpriteRenderer>().color = powerupColor;
        startPosition = transform.position;
        
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        // Force the correct settings regardless of prefab
        boxCollider.isTrigger = true;
        boxCollider.size = new Vector2(3f, 3f); // Make collider much bigger for easier collision
        boxCollider.enabled = true;
        
        // Fix layer issue - move to same layer as player
        gameObject.layer = 6; // Same layer as player
        
        StartCoroutine(FloatAnimation());
        StartCoroutine(SlowRotate());
    }
    
    System.Collections.IEnumerator FloatAnimation()
    {
        while (true)
        {
            // up and down floating
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }
    }
    
    System.Collections.IEnumerator SlowRotate()
    {
        while (true)
        {
            // slower rotation
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }
    
    void Update()
    {
        // if player is in range, collect powerup, and destroy powerup
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < 1f) // If player is within 1 unit
            {
                Player_CATALYST playerScript = player.GetComponent<Player_CATALYST>();
                if (playerScript != null)
                {
                    playerScript.CollectPowerup(pounceCharges, duration);
                    Destroy(gameObject);
                }
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player_CATALYST player = other.GetComponent<Player_CATALYST>();
            if (player != null)
            {
                player.CollectPowerup(pounceCharges, duration);
                Destroy(gameObject);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw collider bounds in Scene view for debugging
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, collider.size);
        }
    }
}
