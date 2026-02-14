using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedBar : MonoBehaviour
{
    [Header("References")]
    public Image fillBar;
    public RectTransform needle; // Σύρε εδώ τη βελόνα (αν έχεις)
    public TextMeshProUGUI speedText;
    public Rigidbody vehicleRigidbody;

    [Header("Speed Settings")]
    public float maxSpeed = 160f;        // Τελική ταχύτητα σε KM/H
    public float smoothing = 8f;         // Πόσο ομαλά κουνιέται η μπάρα

    [Header("Visual Settings")]
    public Gradient speedGradient;       // Χρώματα από πράσινο σε κόκκινο
    public float minNeedleAngle = 180f;  // Γωνία βελόνας στο 0
    public float maxNeedleAngle = -90f;  // Γωνία βελόνας στο τέρμα

    [Header("Shake Effect")]
    public bool enableShake = true;
    public float shakeIntensity = 1.2f;

    private float currentSpeedKMH;
    private Vector3 originalTextPos;

    void Start()
    {
        if (speedText != null) originalTextPos = speedText.rectTransform.localPosition;

        // Αν δεν έχεις βάλει Rigidbody, προσπαθεί να το βρει στο Taxi
        if (vehicleRigidbody == null)
        {
            vehicleRigidbody = GameObject.FindWithTag("Player")?.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        if (vehicleRigidbody == null) return;

        // Υπολογισμός ταχύτητας σε KM/H
        float realSpeedKMH = vehicleRigidbody.linearVelocity.magnitude * 3.6f;

        // Smoothing: Κάνει την μπάρα να "γλιστράει" ομαλά αντί να πηδάει
        currentSpeedKMH = Mathf.Lerp(currentSpeedKMH, realSpeedKMH, Time.deltaTime * smoothing);

        float speedRatio = Mathf.Clamp01(currentSpeedKMH / maxSpeed);

        UpdateSpeedUI(speedRatio);
    }

    void UpdateSpeedUI(float ratio)
    {
        // 1. Γέμισμα μπάρας
        if (fillBar != null)
        {
            fillBar.fillAmount = ratio;
            if (speedGradient != null)
                fillBar.color = speedGradient.Evaluate(ratio);
        }

        // 2. Κίνηση βελόνας (προαιρετικό)
        if (needle != null)
        {
            float targetAngle = Mathf.Lerp(minNeedleAngle, maxNeedleAngle, ratio);
            needle.rotation = Quaternion.Euler(0, 0, targetAngle);
        }

        // 3. Κείμενο και Shake (Τρέμουλο)
        if (speedText != null)
        {
            speedText.text = Mathf.RoundToInt(currentSpeedKMH).ToString();

            // Το τρέμουλο ενεργοποιείται μετά το 80% της τελικής ταχύτητας
            if (enableShake && ratio > 0.8f)
            {
                float shake = (ratio - 0.8f) * shakeIntensity * 12f;
                speedText.rectTransform.localPosition = originalTextPos + (Vector3)Random.insideUnitCircle * shake;
            }
            else
            {
                speedText.rectTransform.localPosition = originalTextPos;
            }
        }
    }
}