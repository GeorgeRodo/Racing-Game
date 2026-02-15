using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0, 3, -8);
    
    [Header("Smoothing")]
    public float positionSmoothing = 5f;
    public float rotationSmoothing = 3f;
    
    [Header("Mouse Look")]
    public float mouseSensitivity = 3f;
    public float returnSpeed = 5f;
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 60f;
    
    [Header("Speed-Based Zoom")]
    public float minDistance = 8f;      
    public float maxDistance = 14f;
    public float speedForMaxZoom = 25f;
    public float zoomSmoothing = 4f;
    
    [Header("Boost Camera Effects")]
    public float boostZoomDistance = 18f;     // Extra zoom back during boost
    public float boostZoomSpeed = 8f;         // How fast camera zooms back
    public float boostReturnSpeed = 3f;       // How fast camera returns to normal
    public float boostFOVIncrease = 10f;      // Extra FOV during boost
    
    [Header("Dynamic Camera")]
    public float maxSpeedFOV = 75f;
    public float normalFOV = 60f;
    
    [Header("Boost Visual Effects")]
    public UnityEngine.Rendering.Volume postProcessVolume;  // URP Volume
    public float boostBloomIntensity = 8f;      // Extra bloom during boost
    public float boostSaturation = 10f;          // Extra color saturation
    public float boostVignetteIntensity = 0.2f;  // Extra vignette darkness
    public float boostEffectSpeed = 5f;          // How fast effects transition
    
    private Camera cam;
    private Rigidbody targetRb;
    private CustomVehicleController vehicleController;
    private Vector3 velocity = Vector3.zero;
    
    private float currentHorizontalAngle = 0f;
    private float currentVerticalAngle = 0f;
    private Vector3 currentOffset;
    private float currentDistance;
    
    // Boost state
    private float boostDistanceModifier = 0f;
    private float boostFOVModifier = 0f;
    
    // Water sinking effect
    private bool isFrozen = false;
    private Vector3 frozenPosition;
    private Quaternion frozenRotation;
    
    // Post-processing effect references
    private UnityEngine.Rendering.Universal.Bloom bloomEffect;
    private UnityEngine.Rendering.Universal.ColorAdjustments colorAdjustments;
    private UnityEngine.Rendering.Universal.Vignette vignetteEffect;
    
    // Base values for post-processing (to return to after boost)
    private float baseBloomIntensity = 0f;
    private float baseSaturation = 0f;
    private float baseVignetteIntensity = 0f;
    private bool hasStoredBaseValues = false;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target != null)
        {
            targetRb = target.GetComponent<Rigidbody>();
            vehicleController = target.GetComponent<CustomVehicleController>();
        }
        currentOffset = offset;
        currentDistance = offset.magnitude;
        
        // Get post-processing effects if volume is assigned
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGet(out bloomEffect);
            postProcessVolume.profile.TryGet(out colorAdjustments);
            postProcessVolume.profile.TryGet(out vignetteEffect);
        }
    }
    
    /// <summary>
    /// Freezes the camera at its current position (for water sinking effect)
    /// </summary>
    public void FreezeCamera()
    {
        isFrozen = true;
        frozenPosition = transform.position;
        frozenRotation = transform.rotation;
    }
    
    /// <summary>
    /// Unfreezes the camera to resume following the target
    /// </summary>
    public void UnfreezeCamera()
    {
        isFrozen = false;
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        // If camera is frozen (car sinking in water), don't update position
        if (isFrozen)
        {
            transform.position = frozenPosition;
            transform.rotation = frozenRotation;
            return;
        }

        float speed = targetRb != null ? targetRb.linearVelocity.magnitude : 0f;
        bool isBoosting = vehicleController != null && vehicleController.IsBoosting();
        
        // Handle boost camera zoom
        UpdateBoostCamera(isBoosting);
        
        // Calculate base distance from speed
        float speedFactor = Mathf.Clamp01(speed / speedForMaxZoom);
        float targetDistance = Mathf.Lerp(minDistance, maxDistance, speedFactor);
        
        // Add boost distance modifier
        targetDistance += boostDistanceModifier;
        
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSmoothing * Time.deltaTime);
        
        Vector3 offsetDirection = offset.normalized;
        Vector3 baseOffset = offsetDirection * currentDistance;

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            currentHorizontalAngle += mouseX;
            currentVerticalAngle -= mouseY;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
            
            Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);
            currentOffset = rotation * baseOffset;
        }
        else
        {
            currentHorizontalAngle = Mathf.Lerp(currentHorizontalAngle, 0f, returnSpeed * Time.deltaTime);
            currentVerticalAngle = Mathf.Lerp(currentVerticalAngle, 0f, returnSpeed * Time.deltaTime);
            
            Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);
            currentOffset = rotation * baseOffset;
        }

        Vector3 desiredPosition = target.position + target.TransformDirection(currentOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1f / positionSmoothing);
        
        Vector3 lookDirection = target.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing * Time.deltaTime);
        
        // Dynamic FOV with boost modifier
        if (targetRb != null && cam != null)
        {
            float baseFOV = Mathf.Lerp(normalFOV, maxSpeedFOV, speed / 25f);
            float targetFOV = baseFOV + boostFOVModifier;
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * 3f);
        }
    }

    void UpdateBoostCamera(bool isBoosting)
    {
        // Store base values on first boost
        if (isBoosting && !hasStoredBaseValues)
        {
            StoreBasePostProcessingValues();
            hasStoredBaseValues = true;
        }
        
        // Smoothly adjust boost modifiers
        if (isBoosting)
        {
            // Zoom back and increase FOV during boost
            boostDistanceModifier = Mathf.Lerp(boostDistanceModifier, boostZoomDistance - maxDistance, boostZoomSpeed * Time.deltaTime);
            boostFOVModifier = Mathf.Lerp(boostFOVModifier, boostFOVIncrease, boostZoomSpeed * Time.deltaTime);
            
            // Apply boost visual effects
            ApplyBoostVisualEffects(true);
        }
        else
        {
            // Return to normal slowly
            boostDistanceModifier = Mathf.Lerp(boostDistanceModifier, 0f, boostReturnSpeed * Time.deltaTime);
            boostFOVModifier = Mathf.Lerp(boostFOVModifier, 0f, boostReturnSpeed * Time.deltaTime);
            
            // Return post-processing to normal
            ApplyBoostVisualEffects(false);
        }
    }
    
    void StoreBasePostProcessingValues()
    {
        if (bloomEffect != null && bloomEffect.intensity.overrideState)
        {
            baseBloomIntensity = bloomEffect.intensity.value;
        }
        
        if (colorAdjustments != null && colorAdjustments.saturation.overrideState)
        {
            baseSaturation = colorAdjustments.saturation.value;
        }
        
        if (vignetteEffect != null && vignetteEffect.intensity.overrideState)
        {
            baseVignetteIntensity = vignetteEffect.intensity.value;
        }
    }
    
    void ApplyBoostVisualEffects(bool isBoosting)
    {
        if (postProcessVolume == null) return;
        
        float speed = boostEffectSpeed * Time.deltaTime;
        
        // Bloom effect (intense glow during boost)
        if (bloomEffect != null)
        {
            float targetBloom = isBoosting ? baseBloomIntensity + boostBloomIntensity : baseBloomIntensity;
            bloomEffect.intensity.value = Mathf.Lerp(bloomEffect.intensity.value, targetBloom, speed);
        }
        
        // Saturation (more vibrant colors during boost)
        if (colorAdjustments != null)
        {
            float targetSaturation = isBoosting ? baseSaturation + boostSaturation : baseSaturation;
            colorAdjustments.saturation.value = Mathf.Lerp(colorAdjustments.saturation.value, targetSaturation, speed);
        }
        
        // Vignette (darker edges for tunnel vision effect)
        if (vignetteEffect != null)
        {
            float targetVignette = isBoosting ? baseVignetteIntensity + boostVignetteIntensity : baseVignetteIntensity;
            vignetteEffect.intensity.value = Mathf.Lerp(vignetteEffect.intensity.value, targetVignette, speed);
        }
    }
}