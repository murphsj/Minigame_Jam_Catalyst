using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class Droplet_CATACLYST : MonoBehaviour
{
    public DropletType dropletType;
    public SpriteAtlas dropletSpriteSheet;

    new BoxCollider2D collider;
    new SpriteRenderer renderer;
    Animator animator;
    float moveSpeed;
    float destroyAtTime;

    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        renderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        animator.Play(dropletType.getStateName());
    }

    public void Initialize(DropletType dropletType, float moveSpeed, float lifetime)
    {
        this.dropletType = dropletType;
        this.moveSpeed = moveSpeed;
        destroyAtTime = Time.time + lifetime;
    }

    void Update()
    {
        if (Time.time > destroyAtTime)
        {
            Destroy(gameObject);
        }
        transform.Translate(new Vector2(0, -Time.deltaTime * moveSpeed));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Droplet hit: " + other.name);
        
        // Try to find Player_CATALYST in the collider's parent hierarchy
        Player_CATALYST player = other.GetComponentInParent<Player_CATALYST>();
        
        // If not found in parent, try the collider itself
        if (player == null)
        {
            player = other.GetComponent<Player_CATALYST>();
        }
        
        if (player != null)
        {
            Debug.Log("Player found, trying to add to flask: " + dropletType);
            if (player.AddToFlask(dropletType)) 
            {
                Debug.Log("Droplet collected successfully!");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Flask is full!");
            }
        }
        else
        {
            Debug.Log("No Player_CATALYST found - droplet hit: " + other.name);
        }
    }
    
    
}
