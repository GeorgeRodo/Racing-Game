using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CustomVehicleController vehicleController;
    [SerializeField] private Image speedBarFill;  // UI Image that will fill
    [SerializeField] private TextMeshProUGUI speedText;  
    
    [Header("Speed Settings")]
    [Tooltip("Will automatically use vehicle's max speed if not set")]
    public float maxDisplaySpeed = 0f;  
    
    [Header("Visual Settings")]
    public float fillSmoothing = 8f;  
    
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
    [Tooltip("Show speed as text")]
    public bool showSpeedText = true;
    [Tooltip("Number format: 0 = no decimals, 0.0 = one decimal")]
    public string speedFormat = "0";  
    public string speedUnit = " km/h";  
    
    private Rigidbody rb;
    private float currentFillAmount = 0f;
    private float actualMaxSpeed;

    void Start()
    {
        // Get Rigidbody from vehicle
        if (vehicleController != null)
        {
            rb = vehicleController.GetComponent<Rigidbody>();
            
            if (maxDisplaySpeed <= 0f)
            {
                actualMaxSpeed = vehicleController.maxSpeed;
                
                actualMaxSpeed *= vehicleController.boostMaxSpeedMultiplier;
            }
            else
            {
                actualMaxSpeed = maxDisplaySpeed;
            }
        }
        else
        {
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
        
        // Initialize bar
        if (speedBarFill != null)
        {
            speedBarFill.fillAmount = 0f;
            speedBarFill.color = lowSpeedColor;
            
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
        
        float currentSpeed = rb.linearVelocity.magnitude;
        
        float targetFill = Mathf.Clamp01(currentSpeed / actualMaxSpeed);
        
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFill, Time.deltaTime * fillSmoothing);
        
        float finalFillAmount = currentFillAmount;
        if (pulseOnBoost && vehicleController != null && vehicleController.IsBoosting())
        {
            float pulse = Mathf.Sin(Time.time * boostPulseSpeed) * boostPulseAmount;
            finalFillAmount = Mathf.Clamp01(currentFillAmount + pulse);
        }
        
        speedBarFill.fillAmount = finalFillAmount;
        
        UpdateBarColor();
        
        UpdateSpeedText();
    }

    void UpdateBarColor()
    {
        if (vehicleController != null && vehicleController.IsBoosting())
        {
            speedBarFill.color = boostColor;
            return;
        }
        
        Color targetColor;
        
        if (currentFillAmount < 0.5f)
        {
            float t = currentFillAmount / 0.5f;
            targetColor = Color.Lerp(lowSpeedColor, midSpeedColor, t);
        }
        else
        {
            float t = (currentFillAmount - 0.5f) / 0.5f;
            targetColor = Color.Lerp(midSpeedColor, highSpeedColor, t);
        }
        
        speedBarFill.color = Color.Lerp(speedBarFill.color, targetColor, Time.deltaTime * fillSmoothing);
    }
    
    void UpdateSpeedText()
    {
        if (!showSpeedText || speedText == null) return;
        
        float speedKMH = GetCurrentSpeedKMH();
        speedText.text = speedKMH.ToString(speedFormat) + speedUnit;
    }
    
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
    
    public float GetCurrentSpeedKMH()
    {
        if (rb == null) return 0f;
        return rb.linearVelocity.magnitude * 3.6f;
    }
    
    public float GetCurrentFillPercentage()
    {
        return currentFillAmount;
    }
}