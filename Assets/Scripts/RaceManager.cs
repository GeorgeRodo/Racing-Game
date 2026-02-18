using UnityEngine;
using TMPro;
using System.Collections;

public class RaceManager : MonoBehaviour
{
    [Header("UI Settings")]
    public TextMeshProUGUI countdownText;

    [Header("Audio Settings")]
    public AudioSource raceMusic;
    public AudioSource sfxSource;  
    
    [Header("Music Boost Effect")]
    public float normalMusicPitch = 1f;     
    public float boostMusicPitch = 1.15f;   
    public float musicPitchTransitionSpeed = 3f;  
    
    [Header("Countdown Sound Clips")]
    public AudioClip countdownBeep;  
    public AudioClip goSound;         

    [Header("Vehicle Reference")]
    public CustomVehicleController vehicleController;

    [Header("Track Reference")]
    public TrackCheckPoints trackCheckPoints;
    
    [Header("Timing Settings")]
    public float startupIdleDuration = 5f;
    public float countdownInterval = 1f;   
    
    [Header("UI Animation")]
    public RectTransform lapCounterUI;         
    public RectTransform checkpointCounterUI;   
    public float uiSlideInDuration = 0.8f;      
    public float uiSlideInDelay = 0.5f;         
    public float uiOffsetDistance = 100f;       
    
    [Header("Countdown Animation")]
    public float countdownDropDistance = 200f;  
    public float countdownDropDuration = 0.3f;  
    public float countdownBounce = 20f;         

    private bool raceStarted = false;
    private float raceTime = 0f;
    
    private Vector2 lapOriginalPos;
    private Vector2 checkpointOriginalPos;
    private Vector2 countdownOriginalPos;
    private RectTransform countdownRectTransform;

    void Start()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownRectTransform = countdownText.GetComponent<RectTransform>();
            
            if (countdownRectTransform != null)
            {
                countdownOriginalPos = countdownRectTransform.anchoredPosition;
            }
        }

        if (vehicleController != null)
            vehicleController.enabled = false;

        if (raceMusic != null)
            raceMusic.Stop();
        
        HideRaceUI();

        StartCoroutine(StartupSequence());
    }
    
    void HideRaceUI()
    {
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
            
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(checkpointCounterUI);
        }
    }

    void Update()
    {
        if (raceStarted && trackCheckPoints != null && !trackCheckPoints.IsRaceFinished())
        {
            raceTime += Time.deltaTime;
        }
        
        UpdateMusicPitch();
    }
    
    void UpdateMusicPitch()
    {
        if (raceMusic == null || vehicleController == null) return;
        
        bool isBoosting = vehicleController.IsBoosting();
        
        float targetPitch = isBoosting ? boostMusicPitch : normalMusicPitch;
        
        raceMusic.pitch = Mathf.Lerp(
            raceMusic.pitch, 
            targetPitch, 
            musicPitchTransitionSpeed * Time.deltaTime
        );
    }

    IEnumerator StartupSequence()
    {
        if (countdownText != null)
        {
            countdownText.text = "GET READY";
            countdownText.color = Color.yellow;
            countdownText.fontSize = 80;
        }
        
        yield return new WaitForSeconds(startupIdleDuration);
        
        StartCoroutine(CountdownSequence());
    }

    IEnumerator CountdownSequence()
    {
        // Countdown: 3, 2, 1 
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                countdownText.color = Color.white;
                countdownText.fontSize = 120;
            }
            
            StartCoroutine(AnimateCountdownDrop());
            
            if (sfxSource != null && countdownBeep != null)
            {
                sfxSource.Stop(); 
                sfxSource.PlayOneShot(countdownBeep);
            }
            
            yield return new WaitForSeconds(countdownInterval);
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            countdownText.color = Color.green;
            countdownText.fontSize = 140;
        }
        
        StartCoroutine(AnimateCountdownDrop());
        
        if (sfxSource != null && goSound != null)
        {
            sfxSource.PlayOneShot(goSound);
        }

        StartRace();

        yield return new WaitForSeconds(1.5f);

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);
        
        yield return new WaitForSeconds(uiSlideInDelay);
        StartCoroutine(SlideInUI());
    }

    void StartRace()
    {
        raceStarted = true;

        if (raceMusic != null)
        {
            raceMusic.pitch = normalMusicPitch; 
            raceMusic.Play();
        }

        if (vehicleController != null)
            vehicleController.enabled = true;
    }

    public bool IsRaceStarted() { return raceStarted; }
    public float GetRaceTime() { return raceTime; }
    
    IEnumerator AnimateCountdownDrop()
    {
        if (countdownRectTransform == null) yield break;
        
        Vector2 startPos = countdownOriginalPos + new Vector2(0f, countdownDropDistance);
        
        Vector2 overshootPos = countdownOriginalPos - new Vector2(0f, countdownBounce);
        
        countdownRectTransform.anchoredPosition = startPos;
        
        float elapsed = 0f;
        
        float phase1Duration = countdownDropDuration * 0.7f;
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase1Duration;
            
            float curvedT = t * t;
            
            countdownRectTransform.anchoredPosition = Vector2.Lerp(startPos, overshootPos, curvedT);
            yield return null;
        }
        
        float phase2Duration = countdownDropDuration * 0.3f;
        elapsed = 0f;
        
        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase2Duration;
            
            float curvedT = Mathf.Sin(t * Mathf.PI * 0.5f);
            
            countdownRectTransform.anchoredPosition = Vector2.Lerp(overshootPos, countdownOriginalPos, curvedT);
            yield return null;
        }
        
        countdownRectTransform.anchoredPosition = countdownOriginalPos;
    }
    
    IEnumerator SlideInUI()
    {
        var lapLayoutGroups = lapCounterUI != null ? lapCounterUI.GetComponents<UnityEngine.UI.LayoutGroup>() : null;
        var checkpointLayoutGroups = checkpointCounterUI != null ? checkpointCounterUI.GetComponents<UnityEngine.UI.LayoutGroup>() : null;
        
        if (lapLayoutGroups != null)
            foreach (var layout in lapLayoutGroups) layout.enabled = false;
        if (checkpointLayoutGroups != null)
            foreach (var layout in checkpointLayoutGroups) layout.enabled = false;
        
        float elapsed = 0f;
        
        Vector2 lapStartPos = lapCounterUI != null ? lapCounterUI.anchoredPosition : Vector2.zero;
        Vector2 checkpointStartPos = checkpointCounterUI != null ? checkpointCounterUI.anchoredPosition : Vector2.zero;
        
        Vector2 lapTargetPos = lapOriginalPos;
        Vector2 checkpointTargetPos = checkpointOriginalPos;
        
        Debug.Log($"ANIMATION START");
        Debug.Log($"Lap: {lapStartPos} -> {lapTargetPos}");
        Debug.Log($"Checkpoint: {checkpointStartPos} -> {checkpointTargetPos}");
        
        while (elapsed < uiSlideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / uiSlideInDuration;
            
            float smoothT = 1f - Mathf.Pow(1f - t, 3f);
            
            if (lapCounterUI != null)
            {
                lapCounterUI.anchoredPosition = Vector2.Lerp(lapStartPos, lapTargetPos, smoothT);
            }
            
            if (checkpointCounterUI != null)
            {
                checkpointCounterUI.anchoredPosition = Vector2.Lerp(checkpointStartPos, checkpointTargetPos, smoothT);
                
                if (Time.frameCount % 10 == 0)
                {
                    Debug.Log($"[CHECKPOINT] t={smoothT:F2} | pos={checkpointCounterUI.anchoredPosition} | target={checkpointTargetPos}");
                }
            }
            
            yield return null;
        }
        
        Debug.Log($"ANIMATION END");
        
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
        
        if (lapLayoutGroups != null)
            foreach (var layout in lapLayoutGroups) layout.enabled = true;
        if (checkpointLayoutGroups != null)
            foreach (var layout in checkpointLayoutGroups) layout.enabled = true;
    }
}