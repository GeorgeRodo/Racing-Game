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
    public float spawnHeightOffset = 1f;      
    public float spawnDistanceBehind = 5f;    

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
    
    private Transform lastValidCheckpoint;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        vehicleController = GetComponent<CustomVehicleController>();
        
        if (trackCheckPoints != null)
        {
            trackCheckPoints.OnPlayerCorrectCheckpoint += TrackCheckPoints_OnPlayerCorrectCheckpoint;
        }
        else
        {
            Debug.LogWarning("[PlayerRespawn] TrackCheckPoints reference not set! Respawn will use default spawn point.");
        }
        
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
        if (trackCheckPoints != null)
        {
            trackCheckPoints.OnPlayerCorrectCheckpoint -= TrackCheckPoints_OnPlayerCorrectCheckpoint;
        }
    }

    private void TrackCheckPoints_OnPlayerCorrectCheckpoint(object sender, System.EventArgs e)
    {
        int lastCheckpointIndex = trackCheckPoints.GetNextCheckpointIndex() - 1;
        
        if (lastCheckpointIndex < 0)
        {
            lastCheckpointIndex = trackCheckPoints.GetTotalCheckpoints() - 1;
        }
        
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
        if (other.CompareTag("Water") && !isRespawning)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[PlayerRespawn] Hit water! Respawning to: {lastValidCheckpoint.name}");
            }
            
            // Disable vehicle controller to stop driving
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
        
        if (cameraFollow != null)
        {
            cameraFollow.FreezeCamera();
        }

        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            SetFadeAlpha(alpha);
            yield return null;
        }

        Vector3 spawnPosition = lastValidCheckpoint.position 
            - (lastValidCheckpoint.forward * spawnDistanceBehind) 
            + (Vector3.up * spawnHeightOffset);
        
        transform.position = spawnPosition;
        transform.rotation = lastValidCheckpoint.rotation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        Physics.SyncTransforms();

        yield return new WaitForSeconds(respawnDelay);

        // Re-enable vehicle controller
        if (vehicleController != null) 
        {
            vehicleController.enabled = true;
        }
        
        if (cameraFollow != null)
        {
            cameraFollow.UnfreezeCamera();
        }

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
    
    public void ManualRespawn()
    {
        if (!isRespawning)
        {
            StartCoroutine(RespawnSequence());
        }
    }
    
    public Transform GetCurrentRespawnPoint()
    {
        return lastValidCheckpoint;
    }
}