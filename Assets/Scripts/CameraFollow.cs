using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
    
    [Header("Boost Post-Processing")]
    [Tooltip("Assign your Global Volume here")]
    public Volume postProcessVolume;
    public float boostMotionBlur = 0.5f;        // Motion blur intensity during boost
    public float boostLensDistortion = -0.3f;   // Lens distortion during boost (negative = barrel)
    public float boostEffectSpeed = 5f;         // How fast effects transition
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
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
    
    // Post-processing effect references (only for boost effects)
    private MotionBlur motionBlurEffect;
    private LensDistortion lensDistortionEffect;
    
    // Track current boost effect values
    private float currentMotionBlurIntensity = 0f;
    private float currentLensDistortion = 0f;
    private bool effectsInitialized = false;

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
        
        // Initialize boost-specific post-processing effects
        InitializeBoostEffects();
    }
    
    void InitializeBoostEffects()
    {
        // If no volume assigned, try to find one
        if (postProcessVolume == null)
        {
            postProcessVolume = FindFirstObjectByType<Volume>();
            
            if (postProcessVolume != null && showDebugInfo)
            {
                Debug.Log($"<color=cyan>[Camera]</color> Auto-found Volume: {postProcessVolume.name}");
            }
        }
        
        // If we still don't have a volume, we're done
        if (postProcessVolume == null)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("<color=yellow>[Camera]</color> No Post Processing Volume found! Boost visual effects disabled.");
            }
            return;
        }
        
        // Make sure volume has a profile
        if (postProcessVolume.profile == null)
        {
            Debug.LogError("<color=red>[Camera]</color> Volume has no profile assigned!");
            return;
        }
        
        // Try to get boost-specific effects from the volume profile
        bool hasMotionBlur = postProcessVolume.profile.TryGet(out motionBlurEffect);
        bool hasLensDistortion = postProcessVolume.profile.TryGet(out lensDistortionEffect);
        
        // Initialize Motion Blur (if it exists)
        if (hasMotionBlur)
        {
            // We'll control the intensity dynamically
            motionBlurEffect.intensity.overrideState = true;
            currentMotionBlurIntensity = 0f; // Start at 0, only activate during boost
            motionBlurEffect.intensity.value = 0f;
            
            if (showDebugInfo) 
                Debug.Log("<color=green>[Camera]</color> Motion Blur ready for boost");
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("<color=yellow>[Camera]</color> Motion Blur not found in Volume Profile. Add it for boost blur effect!");
        }
        
        // Initialize Lens Distortion (if it exists)
        if (hasLensDistortion)
        {
            // We'll control the intensity dynamically
            lensDistortionEffect.intensity.overrideState = true;
            currentLensDistortion = 0f; // Start at 0, only activate during boost
            lensDistortionEffect.intensity.value = 0f;
            
            if (showDebugInfo) 
                Debug.Log("<color=green>[Camera]</color> Lens Distortion ready for boost");
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("<color=yellow>[Camera]</color> Lens Distortion not found in Volume Profile. Add it for boost distortion effect!");
        }
        
        effectsInitialized = hasMotionBlur || hasLensDistortion;
        
        if (!effectsInitialized && showDebugInfo)
        {
            Debug.LogWarning("<color=yellow>[Camera]</color> No boost effects found! Add Motion Blur and/or Lens Distortion to your Volume Profile.");
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
        
        // Handle boost camera zoom and effects
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
        // Smoothly adjust boost modifiers
        if (isBoosting)
        {
            // Zoom back and increase FOV during boost
            boostDistanceModifier = Mathf.Lerp(boostDistanceModifier, boostZoomDistance - maxDistance, boostZoomSpeed * Time.deltaTime);
            boostFOVModifier = Mathf.Lerp(boostFOVModifier, boostFOVIncrease, boostZoomSpeed * Time.deltaTime);
            
            // Apply boost visual effects (blur + distortion)
            if (effectsInitialized)
            {
                ApplyBoostVisualEffects(true);
            }
        }
        else
        {
            // Return to normal slowly
            boostDistanceModifier = Mathf.Lerp(boostDistanceModifier, 0f, boostReturnSpeed * Time.deltaTime);
            boostFOVModifier = Mathf.Lerp(boostFOVModifier, 0f, boostReturnSpeed * Time.deltaTime);
            
            // Return post-processing to normal (0)
            if (effectsInitialized)
            {
                ApplyBoostVisualEffects(false);
            }
        }
    }
    
    void ApplyBoostVisualEffects(bool isBoosting)
    {
        float speed = boostEffectSpeed * Time.deltaTime;
        
        // Motion Blur - only active during boost
        if (motionBlurEffect != null)
        {
            float targetBlur = isBoosting ? boostMotionBlur : 0f;
            currentMotionBlurIntensity = Mathf.Lerp(currentMotionBlurIntensity, targetBlur, speed);
            motionBlurEffect.intensity.value = currentMotionBlurIntensity;
        }
        
        // Lens Distortion - only active during boost
        if (lensDistortionEffect != null)
        {
            float targetDistortion = isBoosting ? boostLensDistortion : 0f;
            currentLensDistortion = Mathf.Lerp(currentLensDistortion, targetDistortion, speed);
            lensDistortionEffect.intensity.value = currentLensDistortion;
        }
    }
    
    // Helper method to manually reinitialize effects (useful if volume changes)
    [ContextMenu("Reinitialize Boost Effects")]
    public void ReinitializeBoostEffects()
    {
        effectsInitialized = false;
        InitializeBoostEffects();
    }
}