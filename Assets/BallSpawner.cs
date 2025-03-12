using UnityEngine;
using System.Collections.Generic;

public class BallSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableBall
    {
        public GameObject ballPrefab;
        public float spawnWeight = 1f; // Higher weight = more likely to spawn
    }

    // List of balls that can be randomly spawned
    public List<SpawnableBall> spawnableBalls = new List<SpawnableBall>();
    
    // Movement speed for horizontal control
    public float moveSpeed = 10f;
    
    // Spawn position Y coordinate
    public float spawnHeight = 18f;
    
    // Left and right boundaries
    public float leftBoundary = -8f;
    public float rightBoundary = 8f;
    
    // Current active ball
    private GameObject activeBall;
    private Rigidbody2D activeRigidbody;
    private bool isHolding = true;
    
    // Start is called before the first frame update
    void Start()
    {
        SpawnNewBall();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Only control the ball if we're holding one
        if (isHolding && activeBall != null)
        {
            // Get horizontal input
            float horizontalInput = Input.GetAxis("Horizontal");
            
            // Calculate new position
            Vector3 newPosition = activeBall.transform.position;
            newPosition.x += horizontalInput * moveSpeed * Time.deltaTime;
            
            // Clamp position within boundaries
            newPosition.x = Mathf.Clamp(newPosition.x, leftBoundary, rightBoundary);
            
            // Update position
            activeBall.transform.position = newPosition;
            
            // Drop ball when space is pressed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DropBall();
            }
        }
        
        // If we dropped the ball and need a new one, or the ball was destroyed
        if (!isHolding && (activeBall == null || activeRigidbody.linearVelocity.magnitude < 0.1f))
        {
            // Wait a brief moment before spawning a new ball
            Invoke("SpawnNewBall", 0.5f);
            isHolding = true;
        }
    }
    
    void SpawnNewBall()
    {
        // Select a random ball based on weights
        GameObject selectedPrefab = SelectRandomBall();
        
        // Spawn the ball at the top center of the screen
        Vector3 spawnPosition = new Vector3(0, spawnHeight, 0);
        activeBall = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        
        // Get and configure the Rigidbody2D
        activeRigidbody = activeBall.GetComponent<Rigidbody2D>();
        
        if (activeRigidbody != null)
        {
            // Freeze the ball in place until dropped
            activeRigidbody.gravityScale = 0;
            activeRigidbody.linearVelocity = Vector2.zero;
            activeRigidbody.angularVelocity = 0;
        }
        
        isHolding = true;
    }
    
    void DropBall()
    {
        if (activeRigidbody != null)
        {
            // Restore gravity and let physics take over
            activeRigidbody.gravityScale = 1;
            isHolding = false;
        }
    }
    
    GameObject SelectRandomBall()
    {
        // Calculate total weight
        float totalWeight = 0;
        foreach (SpawnableBall ball in spawnableBalls)
        {
            totalWeight += ball.spawnWeight;
        }
        
        // Select a random value within the total weight
        float randomValue = Random.Range(0, totalWeight);
        
        // Find which ball corresponds to this random value
        float weightSum = 0;
        foreach (SpawnableBall ball in spawnableBalls)
        {
            weightSum += ball.spawnWeight;
            if (randomValue <= weightSum)
            {
                return ball.ballPrefab;
            }
        }
        
        // Fallback - return the first ball if something went wrong
        return spawnableBalls[0].ballPrefab;
    }
}