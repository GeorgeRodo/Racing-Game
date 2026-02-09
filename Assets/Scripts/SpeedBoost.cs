using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [Header("Animation Settings")]
    public float rotationSpeed = 180f;        // Degrees per second
    public float floatAmplitude = 0.5f;       // How high it bobs up and down
    public float floatFrequency = 1f;         // How fast it bobs
    
    [Header("Boost Settings")]
    public float boostDuration = 2f;          // How long the boost lasts
    public bool respawns = true;              // Does it come back?
    public float respawnTime = 5f;            // Time until respawn
    
    [Header("Visual Feedback")]
    public GameObject visualObject;           // The mesh that spins
    public ParticleSystem collectEffect;      // Optional particle effect
    
    private Vector3 startPosition;
    private bool isCollected = false;
    private float respawnTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        
        // If no visual object assigned, use self
        if (visualObject == null)
        {
            visualObject = gameObject;
        }
    }

    void Update()
    {
        // Handle respawn timer
        if (isCollected && respawns)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                Respawn();
            }
            return; // Don't animate while collected
        }
        
        // Float up and down - calculate the Y offset
        float yOffset = Mathf.Sin(Time.time * floatFrequency * Mathf.PI * 2) * floatAmplitude;
        
        // Apply position with float offset
        transform.position = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);
        
        // Rotate continuously around Y axis (this happens AFTER position is set)
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        // Skip if already collected
        if (isCollected) return;
        
        // Try to find CustomVehicleController first
        CustomVehicleController customVehicle = other.GetComponent<CustomVehicleController>();
        if (customVehicle != null)
        {
            CollectBoost(customVehicle);
            return;
        }
        
        // Fallback: Try to find regular VehicleController
        VehicleController vehicle = other.GetComponent<VehicleController>();
        if (vehicle != null)
        {
            CollectBoostLegacy(vehicle);
            return;
        }
    }

    void CollectBoost(CustomVehicleController vehicle)
    {
        isCollected = true;
        
        // Activate the vehicle's boost state
        vehicle.ActivateBoost(boostDuration);
        
        // Visual feedback
        PlayCollectEffects();
        
        // Reset timer if it respawns
        if (respawns)
        {
            respawnTimer = 0f;
        }
        else
        {
            // If it doesn't respawn, destroy it
            Destroy(gameObject, 0.5f);
        }
    }

    // Legacy method for old VehicleController (direct force application)
    void CollectBoostLegacy(VehicleController vehicle)
    {
        isCollected = true;
        
        Rigidbody rb = vehicle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(vehicle.transform.forward * 15f, ForceMode.VelocityChange);
        }
        
        PlayCollectEffects();
        
        if (respawns)
        {
            respawnTimer = 0f;
        }
        else
        {
            Destroy(gameObject, 0.5f);
        }
    }

    void PlayCollectEffects()
    {
        // Visual feedback
        if (collectEffect != null)
        {
            collectEffect.Play();
        }
        
        // Hide the visual
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
        else
        {
            // If no visual object, hide the whole thing
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.enabled = false;
            }
        }
    }

    void Respawn()
    {
        isCollected = false;
        
        // Show the visual again
        if (visualObject != null)
        {
            visualObject.SetActive(true);
        }
        else
        {
            // If no visual object, show the renderer
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.enabled = true;
            }
        }
    }
    
    // Optional: Draw gizmo in editor to see placement
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 gizmoPos = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(gizmoPos, 1f);
        Gizmos.DrawLine(gizmoPos + Vector3.up * floatAmplitude, gizmoPos - Vector3.up * floatAmplitude);
    }
}