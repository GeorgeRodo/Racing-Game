using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Checkpoint System")]
    [Tooltip("Reference to the TrackCheckPoints system")]
    public TrackCheckPoints trackCheckPoints;
    
    [Header("Respawn Settings")]
    [Tooltip("Starting position if no checkpoint has been passed yet")]
    public Transform defaultSpawnPoint;
    public float respawnDelay = 0.5f;
    public float spawnHeightOffset = 1f;      // Height above checkpoint
    public float spawnDistanceBehind = 5f;    // Distance behind checkpoint

    [Header("UI Fade Settings")]
    public Image fadeImage; 
    public float fadeSpeed = 2f;

    [Header("Audio Settings")]
    public AudioSource sfxSource; 
    public AudioClip waterBubbleClip;
    
    [Header("Camera Reference")]
    public CameraFollow cameraFollow;
    
    [Header("Debug")]
    public bool showDebugInfo = false;

    private Rigidbody rb;
    private CustomVehicleController vehicleController;
    private bool isRespawning = false;
    
    // Track the last valid checkpoint
    private Transform lastValidCheckpoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        vehicleController = GetComponent<CustomVehicleController>();
        
        // Subscribe to checkpoint events
        if (trackCheckPoints != null)
        {
            trackCheckPoints.OnPlayerCorrectCheckpoint += TrackCheckPoints_OnPlayerCorrectCheckpoint;
        }
        else
        {
            Debug.LogWarning("[PlayerRespawn] TrackCheckPoints reference not set! Respawn will use default spawn point.");
        }
        
        // Set initial spawn point
        if (defaultSpawnPoint == null)
        {
            Debug.LogWarning("[PlayerRespawn] No default spawn point set! Using current position.");
            lastValidCheckpoint = transform;
        }
        else
        {
            lastValidCheckpoint = defaultSpawnPoint;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (trackCheckPoints != null)
        {
            trackCheckPoints.OnPlayerCorrectCheckpoint -= TrackCheckPoints_OnPlayerCorrectCheckpoint;
        }
    }

    private void TrackCheckPoints_OnPlayerCorrectCheckpoint(object sender, System.EventArgs e)
    {
        // Get the checkpoint that was just passed
        int lastCheckpointIndex = trackCheckPoints.GetNextCheckpointIndex() - 1;
        
        // Handle wrap-around (if we just completed a lap, go to last checkpoint)
        if (lastCheckpointIndex < 0)
        {
            lastCheckpointIndex = trackCheckPoints.GetTotalCheckpoints() - 1;
        }
        
        // Get the checkpoint transform from the TrackCheckPoints parent
        if (lastCheckpointIndex >= 0 && lastCheckpointIndex < trackCheckPoints.transform.childCount)
        {
            Transform checkpointTransform = trackCheckPoints.transform.GetChild(lastCheckpointIndex);
            lastValidCheckpoint = checkpointTransform;
            
            if (showDebugInfo)
            {
                Debug.Log($"[PlayerRespawn] Updated last checkpoint to: {checkpointTransform.name}");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only handle water collision, checkpoints are handled by the event system
        if (other.CompareTag("Water") && !isRespawning)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[PlayerRespawn] Hit water! Respawning to: {lastValidCheckpoint.name}");
            }
            
            // Immediately disable vehicle controller to stop driving
            if (vehicleController != null)
            {
                vehicleController.enabled = false;
            }
            
            // Stop all movement
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            if (sfxSource != null && waterBubbleClip != null)
                sfxSource.PlayOneShot(waterBubbleClip);

            StartCoroutine(RespawnSequence());
        }
    }

    IEnumerator RespawnSequence()
    {
        isRespawning = true;
        
        // Freeze camera to watch car sink
        if (cameraFollow != null)
        {
            cameraFollow.FreezeCamera();
        }

        // Fade to black
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            SetFadeAlpha(alpha);
            yield return null;
        }

        // Calculate spawn position behind the last checkpoint
        Vector3 spawnPosition = lastValidCheckpoint.position 
            - (lastValidCheckpoint.forward * spawnDistanceBehind) 
            + (Vector3.up * spawnHeightOffset);
        
        // Teleport vehicle
        transform.position = spawnPosition;
        transform.rotation = lastValidCheckpoint.rotation;

        // Ensure physics are reset
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Force physics update
        Physics.SyncTransforms();

        yield return new WaitForSeconds(respawnDelay);

        // Re-enable vehicle controller
        if (vehicleController != null) 
        {
            vehicleController.enabled = true;
        }
        
        // Unfreeze camera
        if (cameraFollow != null)
        {
            cameraFollow.UnfreezeCamera();
        }

        // Fade back in
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            SetFadeAlpha(alpha);
            yield return null;
        }

        isRespawning = false;
        
        if (showDebugInfo)
        {
            Debug.Log("[PlayerRespawn] Respawn complete!");
        }
    }

    private void SetFadeAlpha(float a)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = a;
            fadeImage.color = c;
        }
    }
    
    // Manual respawn method (can be called from code or UI button)
    public void ManualRespawn()
    {
        if (!isRespawning)
        {
            StartCoroutine(RespawnSequence());
        }
    }
    
    // Get current respawn point (for debugging or other systems)
    public Transform GetCurrentRespawnPoint()
    {
        return lastValidCheckpoint;
    }
}