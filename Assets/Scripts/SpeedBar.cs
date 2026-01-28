using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedBar : MonoBehaviour
{
    [Header("References")]
    public Image fillBar;
    public TextMeshProUGUI speedText;
    public Rigidbody vehicleRigidbody;
    
    [Header("Speed Settings")]
    public float maxSpeed = 30f;              // Max speed in m/s (matches your vehicle)
    public bool useKMH = true;                // Display in KM/H
    
    [Header("Visual Settings")]
    public Gradient speedGradient;            // Color gradient from slow to fast
    public float smoothing = 5f;              // How smooth the bar fills
    
    private float currentFillAmount = 0f;
    private float targetFillAmount = 0f;

    void Start()
    {
        // Find vehicle if not assigned
        if (vehicleRigidbody == null)
        {
            vehicleRigidbody = FindFirstObjectByType<VehicleController>()?.GetComponent<Rigidbody>();
        }
        
        // ALWAYS create a new gradient to ensure it works
        speedGradient = new Gradient();
        
        // Create color keys: Green -> Yellow -> Orange -> Red
        GradientColorKey[] colorKeys = new GradientColorKey[4];
        colorKeys[0].color = new Color(0.2f, 1f, 0.2f);    // Green at 0%
        colorKeys[0].time = 0f;
        colorKeys[1].color = new Color(1f, 1f, 0.2f);      // Yellow at 40%
        colorKeys[1].time = 0.4f;
        colorKeys[2].color = new Color(1f, 0.6f, 0.2f);    // Orange at 70%
        colorKeys[2].time = 0.7f;
        colorKeys[3].color = new Color(1f, 0.2f, 0.2f);    // Red at 100%
        colorKeys[3].time = 1f;
        
        // Alpha keys (full opacity)
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1f;
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 1f;
        alphaKeys[1].time = 1f;
        
        speedGradient.SetKeys(colorKeys, alphaKeys);
        
        // Initialize fill bar
        if (fillBar != null)
        {
            fillBar.fillAmount = 0f;
            fillBar.type = Image.Type.Filled;
            fillBar.fillMethod = Image.FillMethod.Horizontal;
            fillBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            
            // Set initial color to green
            fillBar.color = speedGradient.Evaluate(0f);
        }
    }

    void Update()
    {
        if (vehicleRigidbody == null || fillBar == null) return;
        
        // Get current speed
        float speed = vehicleRigidbody.linearVelocity.magnitude;
        
        // Calculate fill amount (0 to 1)
        float speedRatio = Mathf.Clamp01(speed / maxSpeed);
        targetFillAmount = speedRatio;
        
        // Smooth fill animation
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, smoothing * Time.deltaTime);
        fillBar.fillAmount = currentFillAmount;
        
        // Update color based on speed
        fillBar.color = speedGradient.Evaluate(speedRatio);
        
        // Update speed text (OPTIONAL - only if assigned)
        if (speedText != null)
        {
            if (useKMH)
            {
                float speedKMH = speed * 3.6f;
                speedText.text = $"{Mathf.RoundToInt(speedKMH)} KM/H";
            }
            else
            {
                speedText.text = $"{Mathf.RoundToInt(speed)} M/S";
            }
        }
    }
}