using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [Header("Animation Settings")]
    public float rotationSpeed = 180f;        
    public float floatAmplitude = 0.5f;       
    public float floatFrequency = 1f;         
    
    [Header("Boost Settings")]
    public float boostDuration = 2f;          
    public bool respawns = true;              
    public float respawnTime = 5f;            
    
    [Header("Visual Feedback")]
    public GameObject visualObject;           
    public ParticleSystem collectEffect;      
    
    private Vector3 startPosition;
    private bool isCollected = false;
    private float respawnTimer = 0f;

    void Start()
    {
        startPosition = transform.position;
        
        if (visualObject == null)
        {
            visualObject = gameObject;
        }
    }

    void Update()
    {
        if (isCollected && respawns)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                Respawn();
            }
            return; 
        }
        
        // Float up and down
        float yOffset = Mathf.Sin(Time.time * floatFrequency * Mathf.PI * 2) * floatAmplitude;
        
        transform.position = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);
        
        // Rotate continuously around Y 
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        
        CustomVehicleController customVehicle = other.GetComponent<CustomVehicleController>();
        if (customVehicle != null)
        {
            CollectBoost(customVehicle);
            return;
        }
        
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
        
        vehicle.ActivateBoost(boostDuration);
        
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
        if (collectEffect != null)
        {
            collectEffect.Play();
        }
        
        if (visualObject != null)
        {
            visualObject.SetActive(false);
        }
        else
        {
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
        
        if (visualObject != null)
        {
            visualObject.SetActive(true);
        }
        else
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.enabled = true;
            }
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 gizmoPos = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(gizmoPos, 1f);
        Gizmos.DrawLine(gizmoPos + Vector3.up * floatAmplitude, gizmoPos - Vector3.up * floatAmplitude);
    }
}