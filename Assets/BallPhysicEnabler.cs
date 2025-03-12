using UnityEngine;

public class BallPhysicsEnabler : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool physicsEnabled = false;
    
    // Flag to indicate this ball was created by combination
    public bool isCombinedBall = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // If this is a combined ball (set by BallCombination script), 
        // automatically enable physics
        if (isCombinedBall && rb != null)
        {
            EnablePhysics();
        }
        // Otherwise start with physics disabled (controlled by the spawner)
        else if (rb != null)
        {
            rb.gravityScale = 0;
        }
    }
    
    void Update()
    {
        // Safety check to ensure gravity stays enabled for combined balls
        if (isCombinedBall && rb != null && rb.gravityScale != 1)
        {
            rb.gravityScale = 1;
        }
    }
    
    // This gets called by the BallSpawner when the space key is pressed
    public void EnablePhysics()
    {
        if (rb != null)
        {
            rb.gravityScale = 1;
            physicsEnabled = true;
        }
    }
}