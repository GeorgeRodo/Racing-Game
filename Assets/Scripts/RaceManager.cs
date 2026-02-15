using UnityEngine;
using TMPro;
using System.Collections;

public class RaceManager : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI countdownText;

    [Header("Audio Settings")]
    public AudioSource raceMusic;
    public AudioSource sfxSource;  // Single AudioSource for all sound effects
    
    [Header("Music Boost Effect")]
    public float normalMusicPitch = 1f;      // Normal music speed
    public float boostMusicPitch = 1.15f;    // Music speed during boost (15% faster)
    public float musicPitchTransitionSpeed = 3f;  // How fast pitch changes
    
    [Header("Countdown Sound Clips")]
    public AudioClip countdownBeep;   // Sound for 3, 2, 1 (same sound plays 3 times)
    public AudioClip goSound;          // Sound for GO!

    [Header("Vehicle Reference")]
    public CustomVehicleController vehicleController;

    [Header("Track Reference")]
    public TrackCheckPoints trackCheckPoints;
    
    [Header("Timing Settings")]
    public float startupIdleDuration = 5f;  // Time to idle before countdown
    public float countdownInterval = 1f;    // Time between 3, 2, 1 (increase if sound is long)
    
    [Header("UI Animation")]
    public RectTransform lapCounterUI;          // Lap counter UI element
    public RectTransform checkpointCounterUI;   // Checkpoint counter UI element
    public float uiSlideInDuration = 0.8f;      // How long the slide animation takes
    public float uiSlideInDelay = 0.5f;         // Delay before UI slides in after GO
    public float uiOffsetDistance = 100f;       // How far to move UI off-screen (pixels)

    private bool raceStarted = false;
    private float raceTime = 0f;
    
    // Store original positions
    private Vector2 lapOriginalPos;
    private Vector2 checkpointOriginalPos;

    void Start()
    {
        if (countdownText != null)
            countdownText.gameObject.SetActive(true);

        // Disable vehicle controls until race starts
        if (vehicleController != null)
            vehicleController.enabled = false;

        // Stop race music until countdown finishes
        if (raceMusic != null)
            raceMusic.Stop();
        
        // Hide UI elements during countdown
        HideRaceUI();

        StartCoroutine(StartupSequence());
    }
    
    void HideRaceUI()
    {
        // Store original positions and move UI off-screen
        if (lapCounterUI != null)
        {
            lapCounterUI.gameObject.SetActive(true);
            lapOriginalPos = lapCounterUI.anchoredPosition;
            Debug.Log($"[LAP] Original pos: {lapOriginalPos}");
            lapCounterUI.anchoredPosition = new Vector2(lapOriginalPos.x + uiOffsetDistance, lapOriginalPos.y);
            Debug.Log($"[LAP] Hidden pos: {lapCounterUI.anchoredPosition}");
        }
        
        if (checkpointCounterUI != null)
        {
            checkpointCounterUI.gameObject.SetActive(true);
            checkpointOriginalPos = checkpointCounterUI.anchoredPosition;
            Debug.Log($"[CHECKPOINT] Original pos: {checkpointOriginalPos}");
            checkpointCounterUI.anchoredPosition = new Vector2(checkpointOriginalPos.x + uiOffsetDistance, checkpointOriginalPos.y);
            Debug.Log($"[CHECKPOINT] Hidden pos: {checkpointCounterUI.anchoredPosition}");
            
            // Force rebuild layout to prevent teleporting
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(checkpointCounterUI);
        }
    }

    void Update()
    {
        if (raceStarted && trackCheckPoints != null && !trackCheckPoints.IsRaceFinished())
        {
            raceTime += Time.deltaTime;
        }
        
        // Update music pitch based on boost state
        UpdateMusicPitch();
    }
    
    void UpdateMusicPitch()
    {
        if (raceMusic == null || vehicleController == null) return;
        
        // Check if vehicle is boosting
        bool isBoosting = vehicleController.IsBoosting();
        
        // Target pitch based on boost state
        float targetPitch = isBoosting ? boostMusicPitch : normalMusicPitch;
        
        // Smoothly transition to target pitch
        raceMusic.pitch = Mathf.Lerp(
            raceMusic.pitch, 
            targetPitch, 
            musicPitchTransitionSpeed * Time.deltaTime
        );
    }

    IEnumerator StartupSequence()
    {
        // Show "GET READY" or engine warming up message
        if (countdownText != null)
        {
            countdownText.text = "GET READY";
            countdownText.color = Color.yellow;
            countdownText.fontSize = 80;
        }
        
        // Wait for startup/idle duration (car engine sounds play during this)
        yield return new WaitForSeconds(startupIdleDuration);
        
        // Now start the countdown
        StartCoroutine(CountdownSequence());
    }

    IEnumerator CountdownSequence()
    {
        // Countdown: 3, 2, 1 (plays same beep sound for each)
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                countdownText.color = Color.white;
                countdownText.fontSize = 120;
            }
            
            // Stop any previous countdown sound and play new one
            if (sfxSource != null && countdownBeep != null)
            {
                sfxSource.Stop(); // Stop previous sound
                sfxSource.PlayOneShot(countdownBeep);
            }
            
            yield return new WaitForSeconds(countdownInterval);
        }

        // GO!
        if (countdownText != null)
        {
            countdownText.text = "GO!";
            countdownText.color = Color.green;
            countdownText.fontSize = 140;
        }
        
        // Play GO! sound (different from countdown)
        if (sfxSource != null && goSound != null)
        {
            sfxSource.PlayOneShot(goSound);
        }

        StartRace();

        yield return new WaitForSeconds(1.5f);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        
        // Start UI slide-in animation
        yield return new WaitForSeconds(uiSlideInDelay);
        StartCoroutine(SlideInUI());
    }

    void StartRace()
    {
        raceStarted = true;

        // Start race music
        if (raceMusic != null)
        {
            raceMusic.pitch = normalMusicPitch; // Set to normal pitch
            raceMusic.Play();
        }

        // Enable vehicle controls
        if (vehicleController != null)
            vehicleController.enabled = true;
    }

    public bool IsRaceStarted() { return raceStarted; }
    public float GetRaceTime() { return raceTime; }
    
    IEnumerator SlideInUI()
    {
        // Disable any layout groups that might interfere
        var lapLayoutGroups = lapCounterUI != null ? lapCounterUI.GetComponents<UnityEngine.UI.LayoutGroup>() : null;
        var checkpointLayoutGroups = checkpointCounterUI != null ? checkpointCounterUI.GetComponents<UnityEngine.UI.LayoutGroup>() : null;
        
        // Disable layout groups temporarily
        if (lapLayoutGroups != null)
            foreach (var layout in lapLayoutGroups) layout.enabled = false;
        if (checkpointLayoutGroups != null)
            foreach (var layout in checkpointLayoutGroups) layout.enabled = false;
        
        float elapsed = 0f;
        
        // Store starting positions (currently off-screen)
        Vector2 lapStartPos = lapCounterUI != null ? lapCounterUI.anchoredPosition : Vector2.zero;
        Vector2 checkpointStartPos = checkpointCounterUI != null ? checkpointCounterUI.anchoredPosition : Vector2.zero;
        
        // IMPORTANT: Store target positions separately (don't reuse the same variable!)
        Vector2 lapTargetPos = lapOriginalPos;
        Vector2 checkpointTargetPos = checkpointOriginalPos;
        
        Debug.Log($"=== ANIMATION START ===");
        Debug.Log($"Lap: {lapStartPos} → {lapTargetPos}");
        Debug.Log($"Checkpoint: {checkpointStartPos} → {checkpointTargetPos}");
        
        while (elapsed < uiSlideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / uiSlideInDuration;
            
            // Smooth easing (ease out)
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            
            // Animate lap counter
            if (lapCounterUI != null)
            {
                lapCounterUI.anchoredPosition = Vector2.Lerp(lapStartPos, lapTargetPos, smoothT);
            }
            
            // Animate checkpoint counter WITHOUT delay (remove the stagger that causes it to stop early)
            if (checkpointCounterUI != null)
            {
                // Use same t as lap counter, but we can add delay before starting if needed
                checkpointCounterUI.anchoredPosition = Vector2.Lerp(checkpointStartPos, checkpointTargetPos, smoothT);
                
                // Debug every 10 frames to avoid spam
                if (Time.frameCount % 10 == 0)
                {
                    Debug.Log($"[CHECKPOINT] t={smoothT:F2} | pos={checkpointCounterUI.anchoredPosition} | target={checkpointTargetPos}");
                }
            }
            
            yield return null;
        }
        
        Debug.Log($"=== ANIMATION END ===");
        
        // Ensure final positions are exact (back to original)
        if (lapCounterUI != null)
        {
            lapCounterUI.anchoredPosition = lapTargetPos;
            Debug.Log($"Lap final: {lapCounterUI.anchoredPosition}");
        }
        
        if (checkpointCounterUI != null)
        {
            checkpointCounterUI.anchoredPosition = checkpointTargetPos;
            Debug.Log($"Checkpoint final: {checkpointCounterUI.anchoredPosition}");
        }
        
        // Re-enable layout groups
        if (lapLayoutGroups != null)
            foreach (var layout in lapLayoutGroups) layout.enabled = true;
        if (checkpointLayoutGroups != null)
            foreach (var layout in checkpointLayoutGroups) layout.enabled = true;
    }
}