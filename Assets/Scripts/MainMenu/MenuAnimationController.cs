using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Animates main menu UI elements with smooth entrance animations
/// Title slides in from left with overshoot
/// Buttons fade in and slide up with stagger
/// </summary>
public class MainMenuAnimations : MonoBehaviour
{
    [Header("Title Animation")]
    public RectTransform titleText;
    public float titleSlideDistance = 300f;      // How far from left
    public float titleOvershoot = 50f;           // How much it overshoots
    public float titleDuration = 0.8f;           // Animation time
    public float titleDelay = 0.2f;              // Delay before starting
    
    [Header("Button Animations")]
    public RectTransform playButton;
    public RectTransform exitButton;
    public float buttonSlideDistance = 100f;     // Slide up from below
    public float buttonDuration = 0.6f;          // Animation time per button
    public float buttonStagger = 0.15f;          // Delay between each button
    public float buttonStartDelay = 0.5f;        // Delay before first button
    
    [Header("Track Panel - Back Button")]
    public RectTransform trackBackButton;
    public float trackBackSlideDistance = 200f;
    public float trackBackOvershoot = 30f;
    public float trackBackDuration = 0.6f;
    public float trackBackDelay = 0.1f;
    
    [Header("Track Panel - Title (Wiggle)")]
    public RectTransform trackPanelTitle;
    public float trackTitleDropDistance = 150f;
    public float trackTitleDuration = 0.5f;
    public float trackTitleDelay = 0.2f;
    public float trackTitleWiggleAmount = 3f;    // Rotation wiggle in degrees
    public float trackTitleWiggleSpeed = 2f;     // Wiggle frequency
    
    [Header("Track Panel - Grid Buttons")]
    public RectTransform trackGridContainer;     // The parent container with all 4 buttons
    public float trackGridSlideDistance = 150f;
    public float trackGridDuration = 0.4f;
    public float trackGridStartDelay = 0.3f;
    
    [Header("Animation Curves")]
    public AnimationCurve titleEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve buttonEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    // Store original positions
    private Vector2 titleOriginalPos;
    private Vector2 playOriginalPos;
    private Vector2 exitOriginalPos;
    
    // Track panel original positions
    private Vector2 trackBackOriginalPos;
    private Vector2 trackTitleOriginalPos;
    private Vector2 trackGridOriginalPos;
    
    // Track title wiggle state
    private bool isTitleWiggling = false;
    
    void Awake()
    {
        // Store original positions FIRST - Main Menu
        if (titleText != null) titleOriginalPos = titleText.anchoredPosition;
        if (playButton != null) playOriginalPos = playButton.anchoredPosition;
        if (exitButton != null) exitOriginalPos = exitButton.anchoredPosition;
        
        // Store original positions - Track Panel
        if (trackBackButton != null) trackBackOriginalPos = trackBackButton.anchoredPosition;
        if (trackPanelTitle != null) trackTitleOriginalPos = trackPanelTitle.anchoredPosition;
        if (trackGridContainer != null) trackGridOriginalPos = trackGridContainer.anchoredPosition;
        
        // IMMEDIATELY hide elements off-screen (before first frame renders!)
        HideUIElements();
    }
    
    void Start()
    {
        // Start animations after first frame
        StartCoroutine(AnimateMenuEntrance());
    }
    
    void Update()
    {
        // Continuous wiggle for track panel title
        if (isTitleWiggling && trackPanelTitle != null)
        {
            float wiggle = Mathf.Sin(Time.time * trackTitleWiggleSpeed) * trackTitleWiggleAmount;
            trackPanelTitle.localRotation = Quaternion.Euler(0f, 0f, wiggle);
        }
    }
    
    void HideUIElements()
    {
        // Hide main menu title off-screen left
        if (titleText != null)
        {
            titleText.anchoredPosition = titleOriginalPos + new Vector2(-titleSlideDistance, 0f);
        }
        
        // Hide main menu buttons below and make transparent
        HideButton(playButton, playOriginalPos);
        HideButton(exitButton, exitOriginalPos);
        
        // Hide track panel elements
        HideTrackPanelElements();
    }
    
    void HideTrackPanelElements()
    {
        // Hide back button to the left
        if (trackBackButton != null)
        {
            trackBackButton.anchoredPosition = trackBackOriginalPos + new Vector2(-trackBackSlideDistance, 0f);
        }
        
        // Hide track title above
        if (trackPanelTitle != null)
        {
            trackPanelTitle.anchoredPosition = trackTitleOriginalPos + new Vector2(0f, trackTitleDropDistance);
            trackPanelTitle.localRotation = Quaternion.identity;
        }
        
        // Hide track grid container below
        HideButton(trackGridContainer, trackGridOriginalPos, trackGridSlideDistance);
    }
    
    void HideButton(RectTransform button, Vector2 originalPos, float slideDistance = -1f)
    {
        if (button == null) return;
        
        // Use custom slide distance or default
        float distance = slideDistance > 0 ? slideDistance : buttonSlideDistance;
        
        // Position below
        button.anchoredPosition = originalPos + new Vector2(0f, -distance);
        
        // Make transparent
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
    }
    
    IEnumerator AnimateMenuEntrance()
    {
        // Wait initial delay
        yield return new WaitForSeconds(titleDelay);
        
        // Animate title
        if (titleText != null)
        {
            StartCoroutine(AnimateTitle());
        }
        
        // Wait before buttons
        yield return new WaitForSeconds(buttonStartDelay);
        
        // Animate buttons with stagger
        if (playButton != null)
        {
            StartCoroutine(AnimateButton(playButton, playOriginalPos, 0f));
        }
        
        yield return new WaitForSeconds(buttonStagger);
        
        if (exitButton != null)
        {
            StartCoroutine(AnimateButton(exitButton, exitOriginalPos, 0f));
        }
    }
    
    IEnumerator AnimateTitle()
    {
        // Start position (off-screen left)
        Vector2 startPos = titleOriginalPos + new Vector2(-titleSlideDistance, 0f);
        
        // Overshoot position (slightly past target)
        Vector2 overshootPos = titleOriginalPos + new Vector2(titleOvershoot, 0f);
        
        titleText.anchoredPosition = startPos;
        
        float elapsed = 0f;
        
        // Phase 1: Slide in with overshoot (70% of duration)
        float phase1Duration = titleDuration * 0.7f;
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase1Duration;
            float curvedT = titleEase.Evaluate(t);
            
            titleText.anchoredPosition = Vector2.Lerp(startPos, overshootPos, curvedT);
            yield return null;
        }
        
        // Phase 2: Bounce back to final position (30% of duration)
        float phase2Duration = titleDuration * 0.3f;
        elapsed = 0f;
        
        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase2Duration;
            float curvedT = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease out
            
            titleText.anchoredPosition = Vector2.Lerp(overshootPos, titleOriginalPos, curvedT);
            yield return null;
        }
        
        // Ensure final position is exact
        titleText.anchoredPosition = titleOriginalPos;
    }
    
    IEnumerator AnimateButton(RectTransform button, Vector2 targetPos, float delay, float duration = -1f)
    {
        yield return new WaitForSeconds(delay);
        
        // Use custom duration or default
        float animDuration = duration > 0 ? duration : buttonDuration;
        
        // Get starting position (already set in HideUIElements)
        Vector2 startPos = button.anchoredPosition;
        
        // Get canvas group (already added in HideUIElements)
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        
        float elapsed = 0f;
        
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;
            float curvedT = buttonEase.Evaluate(t);
            
            // Slide up
            button.anchoredPosition = Vector2.Lerp(startPos, targetPos, curvedT);
            
            // Fade in
            canvasGroup.alpha = curvedT;
            
            yield return null;
        }
        
        // Ensure final position and alpha
        button.anchoredPosition = targetPos;
        canvasGroup.alpha = 1f;
    }
    
    // Call this if you want to replay animations
    public void ReplayAnimations()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateMenuEntrance());
    }
    
    // Call this when track selection panel is shown
    public void AnimateTrackPanel()
    {
        StopAllCoroutines();
        isTitleWiggling = false;
        HideTrackPanelElements();
        StartCoroutine(AnimateTrackPanelEntrance());
    }
    
    // Call this when returning to main menu
    public void AnimateMainMenu()
    {
        StopAllCoroutines();
        isTitleWiggling = false;
        HideUIElements();
        StartCoroutine(AnimateMenuEntrance());
    }
    
    IEnumerator AnimateTrackPanelEntrance()
    {
        // Animate back button from left
        if (trackBackButton != null)
        {
            yield return new WaitForSeconds(trackBackDelay);
            StartCoroutine(AnimateTrackBackButton());
        }
        
        // Animate title dropping from top
        if (trackPanelTitle != null)
        {
            yield return new WaitForSeconds(trackTitleDelay);
            StartCoroutine(AnimateTrackTitle());
        }
        
        // Animate entire grid container popping up from bottom
        yield return new WaitForSeconds(trackGridStartDelay);
        
        if (trackGridContainer != null)
        {
            StartCoroutine(AnimateButton(trackGridContainer, trackGridOriginalPos, 0f, trackGridDuration));
        }
    }
    
    IEnumerator AnimateTrackBackButton()
    {
        // Start position (off-screen left)
        Vector2 startPos = trackBackOriginalPos + new Vector2(-trackBackSlideDistance, 0f);
        
        // Overshoot position
        Vector2 overshootPos = trackBackOriginalPos + new Vector2(trackBackOvershoot, 0f);
        
        trackBackButton.anchoredPosition = startPos;
        
        float elapsed = 0f;
        
        // Phase 1: Slide in with overshoot (70%)
        float phase1Duration = trackBackDuration * 0.7f;
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase1Duration;
            float curvedT = titleEase.Evaluate(t);
            
            trackBackButton.anchoredPosition = Vector2.Lerp(startPos, overshootPos, curvedT);
            yield return null;
        }
        
        // Phase 2: Bounce back to final (30%)
        float phase2Duration = trackBackDuration * 0.3f;
        elapsed = 0f;
        
        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase2Duration;
            float curvedT = Mathf.Sin(t * Mathf.PI * 0.5f);
            
            trackBackButton.anchoredPosition = Vector2.Lerp(overshootPos, trackBackOriginalPos, curvedT);
            yield return null;
        }
        
        trackBackButton.anchoredPosition = trackBackOriginalPos;
    }
    
    IEnumerator AnimateTrackTitle()
    {
        // Start position (above screen)
        Vector2 startPos = trackTitleOriginalPos + new Vector2(0f, trackTitleDropDistance);
        
        trackPanelTitle.anchoredPosition = startPos;
        trackPanelTitle.localRotation = Quaternion.identity;
        
        float elapsed = 0f;
        
        // Drop down
        while (elapsed < trackTitleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / trackTitleDuration;
            float curvedT = titleEase.Evaluate(t);
            
            trackPanelTitle.anchoredPosition = Vector2.Lerp(startPos, trackTitleOriginalPos, curvedT);
            yield return null;
        }
        
        trackPanelTitle.anchoredPosition = trackTitleOriginalPos;
        
        // Start continuous wiggle
        isTitleWiggling = true;
    }
}