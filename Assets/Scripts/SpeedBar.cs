using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CustomVehicleController vehicleController;
    [SerializeField] private Image speedBarFill;  // The UI Image that will fill
    [SerializeField] private TextMeshProUGUI speedText;  // Optional speed text display
    
    [Header("Speed Settings")]
    [Tooltip("Will automatically use vehicle's max speed if not set")]
    public float maxDisplaySpeed = 0f;  // Leave at 0 to use vehicle's max speed
    
    [Header("Visual Settings")]
    public float fillSmoothing = 8f;  // How quickly the bar catches up to actual speed
    
    [Header("Color Gradient")]
    [Tooltip("Color at 0% speed")]
    public Color lowSpeedColor = new Color(0.2f, 0.8f, 0.2f);  // Green
    
    [Tooltip("Color at 50% speed")]
    public Color midSpeedColor = new Color(0.8f, 0.8f, 0.2f);  // Yellow
    
    [Tooltip("Color at 100% speed")]
    public Color highSpeedColor = new Color(0.8f, 0.2f, 0.2f); // Red
    
    [Tooltip("Color during boost")]
    public Color boostColor = new Color(0.3f, 0.6f, 1f);       // Bright blue
    
    [Header("Boost Visual")]
    public bool pulseOnBoost = true;
    public float boostPulseSpeed = 10f;
    public float boostPulseAmount = 0.15f;
    
    [Header("Speed Text (Optional)")]
    [Tooltip("Show speed as text (e.g., '125 km/h')")]
    public bool showSpeedText = true;
    [Tooltip("Number format: 0 = no decimals, 0.0 = one decimal")]
    public string speedFormat = "0";  // "0" for integers, "0.0" for one decimal
    public string speedUnit = " km/h";  // Text after the number
    
    private Rigidbody rb;
    private float currentFillAmount = 0f;
    private float actualMaxSpeed;

    void Start()
    {
        // Get Rigidbody from vehicle
        if (vehicleController != null)
        {
            rb = vehicleController.GetComponent<Rigidbody>();
            
            // Use vehicle's max speed if we didn't set a custom one
            if (maxDisplaySpeed <= 0f)
            {
                actualMaxSpeed = vehicleController.maxSpeed;
                
                // Account for boost if vehicle can boost
                actualMaxSpeed *= vehicleController.boostMaxSpeedMultiplier;
            }
            else
            {
                actualMaxSpeed = maxDisplaySpeed;
            }
        }
        else
        {
            // Fallback: try to find vehicle with Player tag
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                vehicleController = player.GetComponent<CustomVehicleController>();
                if (vehicleController != null)
                {
                    rb = vehicleController.GetComponent<Rigidbody>();
                    actualMaxSpeed = vehicleController.maxSpeed * vehicleController.boostMaxSpeedMultiplier;
                }
            }
        }
        
        // Initialize the bar
        if (speedBarFill != null)
        {
            speedBarFill.fillAmount = 0f;
            speedBarFill.color = lowSpeedColor;
            
            // Ensure the Image is set to Filled type
            if (speedBarFill.type != Image.Type.Filled)
            {
                Debug.LogWarning("[SpeedBar] Speed bar Image should be set to 'Filled' type for proper fill effect!");
            }
        }
        else
        {
            Debug.LogError("[SpeedBar] Speed Bar Fill Image is not assigned!");
        }
    }

    void Update()
    {
        if (rb == null || speedBarFill == null) return;
        
        // Get current speed
        float currentSpeed = rb.linearVelocity.magnitude;
        
        // Calculate target fill (0 to 1)
        float targetFill = Mathf.Clamp01(currentSpeed / actualMaxSpeed);
        
        // Smooth the fill amount
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFill, Time.deltaTime * fillSmoothing);
        
        // Apply boost pulse if boosting
        float finalFillAmount = currentFillAmount;
        if (pulseOnBoost && vehicleController != null && vehicleController.IsBoosting())
        {
            float pulse = Mathf.Sin(Time.time * boostPulseSpeed) * boostPulseAmount;
            finalFillAmount = Mathf.Clamp01(currentFillAmount + pulse);
        }
        
        speedBarFill.fillAmount = finalFillAmount;
        
        // Update color
        UpdateBarColor();
        
        // Update speed text
        UpdateSpeedText();
    }

    void UpdateBarColor()
    {
        // Check if boosting first - overrides normal color
        if (vehicleController != null && vehicleController.IsBoosting())
        {
            speedBarFill.color = boostColor;
            return;
        }
        
        // Normal color gradient based on speed percentage
        Color targetColor;
        
        if (currentFillAmount < 0.5f)
        {
            // Transition from low to mid (0% to 50%)
            float t = currentFillAmount / 0.5f;
            targetColor = Color.Lerp(lowSpeedColor, midSpeedColor, t);
        }
        else
        {
            // Transition from mid to high (50% to 100%)
            float t = (currentFillAmount - 0.5f) / 0.5f;
            targetColor = Color.Lerp(midSpeedColor, highSpeedColor, t);
        }
        
        // Smooth color transition
        speedBarFill.color = Color.Lerp(speedBarFill.color, targetColor, Time.deltaTime * fillSmoothing);
    }
    
    void UpdateSpeedText()
    {
        if (!showSpeedText || speedText == null) return;
        
        float speedKMH = GetCurrentSpeedKMH();
        speedText.text = speedKMH.ToString(speedFormat) + speedUnit;
    }
    
    // Optional: Manually set the vehicle controller if not set in inspector
    public void SetVehicleController(CustomVehicleController controller)
    {
        vehicleController = controller;
        if (controller != null)
        {
            rb = controller.GetComponent<Rigidbody>();
            if (maxDisplaySpeed <= 0f)
            {
                actualMaxSpeed = controller.maxSpeed * controller.boostMaxSpeedMultiplier;
            }
        }
    }
    
    // Helper method to get current speed in KM/H (if you want to display it elsewhere)
    public float GetCurrentSpeedKMH()
    {
        if (rb == null) return 0f;
        return rb.linearVelocity.magnitude * 3.6f;
    }
    
    // Helper method to get current fill percentage (0-1)
    public float GetCurrentFillPercentage()
    {
        return currentFillAmount;
    }
}