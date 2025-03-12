using System.Collections.Generic;
using UnityEngine;

public class BallCombination : MonoBehaviour
{
    [System.Serializable]
    public class BallType
    {
        public string tagName;
        public GameObject prefab;
        public int pointValue; // Point value for creating this ball
    }

    // List of ball types in order of evolution
    public List<BallType> ballEvolutionChain = new List<BallType>();
    
    // Set of currently combining ball pairs to prevent duplicate combinations
    private static HashSet<int> combiningPairs = new HashSet<int>();

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string thisTag = gameObject.tag;
        string otherTag = collision.gameObject.tag;

        // Only process if both objects have the same tag
        if (thisTag == otherTag)
        {
            // Create a unique identifier for this pair of objects to prevent duplicate processing
            int instanceID1 = gameObject.GetInstanceID();
            int instanceID2 = collision.gameObject.GetInstanceID();
            int pairID = instanceID1 < instanceID2 ? 
                         instanceID1 * 100000 + instanceID2 : 
                         instanceID2 * 100000 + instanceID1;
            
            // Check if this pair is already being processed
            if (combiningPairs.Contains(pairID))
                return;
                
            // Find the current ball's index in the evolution chain
            int currentIndex = FindBallIndexByTag(thisTag);
            
            // Check if this ball type can evolve (not the last in the chain)
            if (currentIndex >= 0 && currentIndex < ballEvolutionChain.Count - 1)
            {
                // Add this pair to the set of combining pairs
                combiningPairs.Add(pairID);
                
                // Get the next ball in the evolution chain
                BallType nextBall = ballEvolutionChain[currentIndex + 1];

                // Get the positions of both balls
                Vector3 averagePosition = (transform.position + collision.transform.position) / 2f;
                
                // Get relevant physics properties to transfer
                float averageVelocityX = 0;
                float averageVelocityY = 0;
                
                Rigidbody2D thisRigidbody = GetComponent<Rigidbody2D>();
                Rigidbody2D otherRigidbody = collision.gameObject.GetComponent<Rigidbody2D>();
                
                if (thisRigidbody != null && otherRigidbody != null)
                {
                    averageVelocityX = (thisRigidbody.linearVelocity.x + otherRigidbody.linearVelocity.x) / 2f;
                    averageVelocityY = (thisRigidbody.linearVelocity.y + otherRigidbody.linearVelocity.y) / 2f;
                }

                // Store the GameObject references before destroying
                GameObject thisObj = gameObject;
                GameObject otherObj = collision.gameObject;

                // Use delayed destruction to allow other collision events to process
                // We use zero delay, but this forces execution to happen after the current frame's physics
                StartCoroutine(DelayedCombine(thisObj, otherObj, nextBall.prefab, averagePosition, 
                                             new Vector2(averageVelocityX, averageVelocityY), pairID, nextBall.pointValue));
            }
        }
    }

    private System.Collections.IEnumerator DelayedCombine(GameObject obj1, GameObject obj2, 
                                                        GameObject newPrefab, Vector3 position, 
                                                        Vector2 velocity, int pairID, int pointValue)
    {
        // Wait until the end of the frame to ensure all collision events are processed
        yield return new WaitForEndOfFrame();
        
        // Make sure both objects still exist
        if (obj1 != null && obj2 != null)
        {
            // Instantiate the new ball
            GameObject newBall = Instantiate(newPrefab, position, Quaternion.identity);


            // Set the combined ball flag
            BallPhysicsEnabler physicsEnabler = newBall.GetComponent<BallPhysicsEnabler>();
            if (physicsEnabler != null)
            {
                physicsEnabler.isCombinedBall = true;
                physicsEnabler.EnablePhysics();
            }
            
            // Add points to the score
            if (ScoreMan.instance != null)
            {
                ScoreMan.instance.AddScore(pointValue);
            }
            
            // Destroy the original balls
            Destroy(obj1);
            Destroy(obj2);
        }
        
        // Remove this pair from the set after a slight delay to avoid race conditions
        yield return new WaitForSeconds(0.1f);
        combiningPairs.Remove(pairID);
    }

    // New coroutine to ensure gravity is applied
    private System.Collections.IEnumerator EnsureGravity(GameObject ball)
    {
        if (ball == null) yield break;

        // Wait for a few frames to let any initial setup happen
        for (int i = 0; i < 5; i++)
        {
            yield return null;

            // Try to enforce gravity scale after each frame
            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            BallPhysicsEnabler enabler = ball.GetComponent<BallPhysicsEnabler>();

            if (rb != null)
            {
                rb.gravityScale = 1f;

                // Additionally, directly modify the script's private field if we can
                if (enabler != null)
                {
                    enabler.EnablePhysics();
                }
            }
        }

        // One more check after half a second
        yield return new WaitForSeconds(0.5f);

        if (ball != null)
        {
            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 1f;
            }
        }
    }

    private int FindBallIndexByTag(string tag)
    {
        for (int i = 0; i < ballEvolutionChain.Count; i++)
        {
            if (ballEvolutionChain[i].tagName == tag)
            {
                return i;
            }
        }
        return -1; // Not found
    }
}