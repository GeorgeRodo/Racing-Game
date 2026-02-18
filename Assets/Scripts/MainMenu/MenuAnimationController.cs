using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuAnimations : MonoBehaviour
{
    [Header("Title Animation")]
    public RectTransform titleText;
    public float titleSlideDistance = 300f;     
    public float titleOvershoot = 50f;          
    public float titleDuration = 0.8f;          
    public float titleDelay = 0.2f;             
    
    [Header("Button Animations")]
    public RectTransform playButton;
    public RectTransform creditsButton;
    public RectTransform exitButton;
    public float buttonSlideDistance = 100f;     
    public float buttonDuration = 0.6f;          
    public float buttonStagger = 0.15f;          
    public float buttonStartDelay = 0.5f;        
    
    [Header("Exit Animation Settings")]
    public float exitDuration = 0.4f;            
    public float exitStagger = 0.08f;            
    
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
    public float trackTitleWiggleAmount = 3f;    
    public float trackTitleWiggleSpeed = 2f;     
    
    [Header("Track Panel - Grid Buttons")]
    public RectTransform trackGridContainer;     
    public float trackGridSlideDistance = 150f;
    public float trackGridDuration = 0.4f;
    public float trackGridStartDelay = 0.3f;
    
    [Header("Credits Panel - Back Button")]
    public RectTransform creditsBackButton;
    public float creditsBackSlideDistance = 200f;
    public float creditsBackOvershoot = 30f;
    public float creditsBackDuration = 0.6f;
    public float creditsBackDelay = 0.1f;
    
    [Header("Credits Panel - Title")]
    public RectTransform creditsPanelTitle;
    public float creditsTitleDropDistance = 150f;
    public float creditsTitleDuration = 0.5f;
    public float creditsTitleDelay = 0.2f;
    public float creditsTitlePulseAmount = 1.1f;   
    public float creditsTitlePulseSpeed = 1.5f;    
    
    [Header("Credits Panel - Name Cards")]
    public RectTransform creditsName1;           
    public RectTransform creditsName2;          
    public float creditsNameSlideDistance = 200f;
    public float creditsNameDuration = 0.5f;
    public float creditsNameStagger = 0.2f;      
    public float creditsNameStartDelay = 0.4f;
    
    [Header("Animation Curves")]
    public AnimationCurve titleEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve buttonEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve exitEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Panel CanvasGroups (for visibility control)")]
    public CanvasGroup mainMenuCanvasGroup;
    public CanvasGroup trackSelectionCanvasGroup;
    public CanvasGroup creditsCanvasGroup;
    
    private Vector2 titleOriginalPos;
    private Vector2 playOriginalPos;
    private Vector2 creditsOriginalPos;
    private Vector2 exitOriginalPos;
    
    private Vector2 trackBackOriginalPos;
    private Vector2 trackTitleOriginalPos;
    private Vector2 trackGridOriginalPos;
    
    private Vector2 creditsBackOriginalPos;
    private Vector2 creditsTitleOriginalPos;
    private Vector2 creditsName1OriginalPos;
    private Vector2 creditsName2OriginalPos;
    
    private bool isTitleWiggling = false;
    private bool isCreditsTitlePulsing = false;
    
    private enum ActivePanel { MainMenu, TrackSelection, Credits }
    private ActivePanel currentPanel = ActivePanel.MainMenu;
    
    void Awake()
    {
        if (titleText != null) titleOriginalPos = titleText.anchoredPosition;
        if (playButton != null) playOriginalPos = playButton.anchoredPosition;
        if (creditsButton != null) creditsOriginalPos = creditsButton.anchoredPosition;
        if (exitButton != null) exitOriginalPos = exitButton.anchoredPosition;
        
        if (trackBackButton != null) trackBackOriginalPos = trackBackButton.anchoredPosition;
        if (trackPanelTitle != null) trackTitleOriginalPos = trackPanelTitle.anchoredPosition;
        if (trackGridContainer != null) trackGridOriginalPos = trackGridContainer.anchoredPosition;
        
        if (creditsBackButton != null) creditsBackOriginalPos = creditsBackButton.anchoredPosition;
        if (creditsPanelTitle != null) creditsTitleOriginalPos = creditsPanelTitle.anchoredPosition;
        if (creditsName1 != null) creditsName1OriginalPos = creditsName1.anchoredPosition;
        if (creditsName2 != null) creditsName2OriginalPos = creditsName2.anchoredPosition;
        
        HideUIElements();
    }
    
    void Start()
    {
        // Set initial panel visibility states
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;
        }
        
        if (trackSelectionCanvasGroup != null)
        {
            trackSelectionCanvasGroup.alpha = 0f;
            trackSelectionCanvasGroup.interactable = false;
            trackSelectionCanvasGroup.blocksRaycasts = false;
        }
        
        if (creditsCanvasGroup != null)
        {
            creditsCanvasGroup.alpha = 0f;
            creditsCanvasGroup.interactable = false;
            creditsCanvasGroup.blocksRaycasts = false;
        }
        
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
        
        // Continuous pulse for credits title
        if (isCreditsTitlePulsing && creditsPanelTitle != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * creditsTitlePulseSpeed) * (creditsTitlePulseAmount - 1f);
            creditsPanelTitle.localScale = Vector3.one * pulse;
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
        HideButton(creditsButton, creditsOriginalPos);
        HideButton(exitButton, exitOriginalPos);
        
        // Hide track panel elements
        HideTrackPanelElements();
        
        // Hide credits panel elements
        HideCreditsPanelElements();
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
    
    void HideCreditsPanelElements()
    {
        // Hide back button to the left
        if (creditsBackButton != null)
        {
            creditsBackButton.anchoredPosition = creditsBackOriginalPos + new Vector2(-creditsBackSlideDistance, 0f);
        }
        
        // Hide credits title above
        if (creditsPanelTitle != null)
        {
            creditsPanelTitle.anchoredPosition = creditsTitleOriginalPos + new Vector2(0f, creditsTitleDropDistance);
            creditsPanelTitle.localScale = Vector3.one;
        }
        
        // Hide name cards below
        HideButton(creditsName1, creditsName1OriginalPos, creditsNameSlideDistance);
        HideButton(creditsName2, creditsName2OriginalPos, creditsNameSlideDistance);
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
    
    // ========== ENTRANCE ANIMATIONS ==========
    
    IEnumerator AnimateMenuEntrance()
    {
        yield return new WaitForSeconds(titleDelay);
        
        if (titleText != null)
        {
            StartCoroutine(AnimateTitle());
        }
        
        yield return new WaitForSeconds(buttonStartDelay);
        
        if (playButton != null)
        {
            StartCoroutine(AnimateButton(playButton, playOriginalPos, 0f));
        }
        
        yield return new WaitForSeconds(buttonStagger);
        
        if (creditsButton != null)
        {
            StartCoroutine(AnimateButton(creditsButton, creditsOriginalPos, 0f));
        }
        
        yield return new WaitForSeconds(buttonStagger);
        
        if (exitButton != null)
        {
            StartCoroutine(AnimateButton(exitButton, exitOriginalPos, 0f));
        }
    }
    
    IEnumerator AnimateTrackPanelEntrance()
    {
        if (trackBackButton != null)
        {
            yield return new WaitForSeconds(trackBackDelay);
            StartCoroutine(AnimateTrackBackButton());
        }
        
        if (trackPanelTitle != null)
        {
            yield return new WaitForSeconds(trackTitleDelay);
            StartCoroutine(AnimateTrackTitle());
        }
        
        yield return new WaitForSeconds(trackGridStartDelay);
        
        if (trackGridContainer != null)
        {
            StartCoroutine(AnimateButton(trackGridContainer, trackGridOriginalPos, 0f, trackGridDuration));
        }
    }
    
    IEnumerator AnimateCreditsPanelEntrance()
    {
        if (creditsBackButton != null)
        {
            yield return new WaitForSeconds(creditsBackDelay);
            StartCoroutine(AnimateCreditsBackButton());
        }
        
        if (creditsPanelTitle != null)
        {
            yield return new WaitForSeconds(creditsTitleDelay);
            StartCoroutine(AnimateCreditsTitle());
        }
        
        yield return new WaitForSeconds(creditsNameStartDelay);
        
        if (creditsName1 != null)
        {
            StartCoroutine(AnimateButton(creditsName1, creditsName1OriginalPos, 0f, creditsNameDuration));
        }
        
        yield return new WaitForSeconds(creditsNameStagger);
        
        if (creditsName2 != null)
        {
            StartCoroutine(AnimateButton(creditsName2, creditsName2OriginalPos, 0f, creditsNameDuration));
        }
    }
    
    // ========== EXIT ANIMATIONS ==========
    
    IEnumerator ExitMainMenu()
    {
        // Exit in reverse order: buttons first, then title
        if (exitButton != null)
        {
            StartCoroutine(ExitButton(exitButton, exitOriginalPos, 0f));
        }
        
        yield return new WaitForSeconds(exitStagger);
        
        if (creditsButton != null)
        {
            StartCoroutine(ExitButton(creditsButton, creditsOriginalPos, 0f));
        }
        
        yield return new WaitForSeconds(exitStagger);
        
        if (playButton != null)
        {
            StartCoroutine(ExitButton(playButton, playOriginalPos, 0f));
        }
        
        yield return new WaitForSeconds(exitStagger);
        
        if (titleText != null)
        {
            StartCoroutine(ExitTitle());
        }
        
        // Wait for all animations to complete
        yield return new WaitForSeconds(exitDuration);
    }
    
    IEnumerator ExitTrackPanel()
    {
        // Stop wiggle animation
        isTitleWiggling = false;
        if (trackPanelTitle != null)
        {
            trackPanelTitle.localRotation = Quaternion.identity;
        }
        
        // Exit in reverse: grid, title, back button
        if (trackGridContainer != null)
        {
            StartCoroutine(ExitButton(trackGridContainer, trackGridOriginalPos, 0f, trackGridSlideDistance));
        }
        
        yield return new WaitForSeconds(exitStagger);
        
        if (trackPanelTitle != null)
        {
            StartCoroutine(ExitTrackTitle());
        }
        
        yield return new WaitForSeconds(exitStagger);
        
        if (trackBackButton != null)
        {
            StartCoroutine(ExitTrackBackButton());
        }
        
        yield return new WaitForSeconds(exitDuration);
    }
    
    IEnumerator ExitCreditsPanel()
    {
        // Stop pulse animation
        isCreditsTitlePulsing = false;
        if (creditsPanelTitle != null)
        {
            creditsPanelTitle.localScale = Vector3.one;
        }
        
        // Exit in reverse: names, title, back button
        if (creditsName2 != null)
        {
            StartCoroutine(ExitButton(creditsName2, creditsName2OriginalPos, 0f, creditsNameSlideDistance));
        }
        
        yield return new WaitForSeconds(exitStagger);
        
        if (creditsName1 != null)
        {
            StartCoroutine(ExitButton(creditsName1, creditsName1OriginalPos, 0f, creditsNameSlideDistance));
        }
        
        yield return new WaitForSeconds(exitStagger);
        
        if (creditsPanelTitle != null)
        {
            StartCoroutine(ExitCreditsTitle());
        }
        
        yield return new WaitForSeconds(exitStagger);
        
        if (creditsBackButton != null)
        {
            StartCoroutine(ExitCreditsBackButton());
        }
        
        yield return new WaitForSeconds(exitDuration);
    }
    
    // ========== PUBLIC TRANSITION METHODS ==========
    
    public void AnimateTrackPanel()
    {
        StopAllCoroutines();
        isTitleWiggling = false;
        isCreditsTitlePulsing = false;
        StartCoroutine(TransitionToTrackPanel());
    }
    
    public void AnimateCreditsPanel()
    {
        StopAllCoroutines();
        isTitleWiggling = false;
        isCreditsTitlePulsing = false;
        StartCoroutine(TransitionToCreditsPanel());
    }
    
    public void AnimateMainMenu()
    {
        StopAllCoroutines();
        isTitleWiggling = false;
        isCreditsTitlePulsing = false;
        StartCoroutine(TransitionToMainMenu());
    }
    
    // ========== TRANSITION SEQUENCES ==========
    
    IEnumerator TransitionToTrackPanel()
    {
        // Exit main menu first
        yield return StartCoroutine(ExitMainMenu());
        
        // Make sure track panel is visible (but elements are still off-screen)
        if (trackSelectionCanvasGroup != null)
        {
            trackSelectionCanvasGroup.alpha = 1f;
            trackSelectionCanvasGroup.interactable = true;
            trackSelectionCanvasGroup.blocksRaycasts = true;
        }
        
        // Hide main menu panel
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;
        }
        
        // Then enter track panel
        HideTrackPanelElements();
        yield return StartCoroutine(AnimateTrackPanelEntrance());
        
        // Update current panel
        currentPanel = ActivePanel.TrackSelection;
    }
    
    IEnumerator TransitionToCreditsPanel()
    {
        // Exit main menu first
        yield return StartCoroutine(ExitMainMenu());
        
        // Make sure credits panel is visible (but elements are still off-screen)
        if (creditsCanvasGroup != null)
        {
            creditsCanvasGroup.alpha = 1f;
            creditsCanvasGroup.interactable = true;
            creditsCanvasGroup.blocksRaycasts = true;
        }
        
        // Hide main menu panel
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;
        }
        
        // Then enter credits panel
        HideCreditsPanelElements();
        yield return StartCoroutine(AnimateCreditsPanelEntrance());
        
        // Update current panel
        currentPanel = ActivePanel.Credits;
    }
    
    IEnumerator TransitionToMainMenu()
    {
        // Exit the current panel based on tracking
        if (currentPanel == ActivePanel.TrackSelection)
        {
            yield return StartCoroutine(ExitTrackPanel());
            
            // Hide track panel
            if (trackSelectionCanvasGroup != null)
            {
                trackSelectionCanvasGroup.alpha = 0f;
                trackSelectionCanvasGroup.interactable = false;
                trackSelectionCanvasGroup.blocksRaycasts = false;
            }
        }
        else if (currentPanel == ActivePanel.Credits)
        {
            yield return StartCoroutine(ExitCreditsPanel());
            
            // Hide credits panel
            if (creditsCanvasGroup != null)
            {
                creditsCanvasGroup.alpha = 0f;
                creditsCanvasGroup.interactable = false;
                creditsCanvasGroup.blocksRaycasts = false;
            }
        }
        
        // Make sure main menu panel is visible (but elements are still off-screen)
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;
        }
        
        // Then enter main menu
        HideUIElements();
        yield return StartCoroutine(AnimateMenuEntrance());
        
        // Update current panel
        currentPanel = ActivePanel.MainMenu;
    }
    
    // ========== INDIVIDUAL ANIMATION COROUTINES ==========
    
    IEnumerator AnimateTitle()
    {
        Vector2 startPos = titleOriginalPos + new Vector2(-titleSlideDistance, 0f);
        Vector2 overshootPos = titleOriginalPos + new Vector2(titleOvershoot, 0f);
        titleText.anchoredPosition = startPos;
        
        float elapsed = 0f;
        float phase1Duration = titleDuration * 0.7f;
        
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase1Duration;
            titleText.anchoredPosition = Vector2.Lerp(startPos, overshootPos, titleEase.Evaluate(t));
            yield return null;
        }
        
        elapsed = 0f;
        float phase2Duration = titleDuration * 0.3f;
        
        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase2Duration;
            titleText.anchoredPosition = Vector2.Lerp(overshootPos, titleOriginalPos, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }
        
        titleText.anchoredPosition = titleOriginalPos;
    }
    
    IEnumerator ExitTitle()
    {
        Vector2 startPos = titleOriginalPos;
        Vector2 endPos = titleOriginalPos + new Vector2(-titleSlideDistance, 0f);
        
        float elapsed = 0f;
        
        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / exitDuration;
            titleText.anchoredPosition = Vector2.Lerp(startPos, endPos, exitEase.Evaluate(t));
            yield return null;
        }
        
        titleText.anchoredPosition = endPos;
    }
    
    IEnumerator AnimateButton(RectTransform button, Vector2 targetPos, float delay, float duration = -1f)
    {
        yield return new WaitForSeconds(delay);
        
        float animDuration = duration > 0 ? duration : buttonDuration;
        Vector2 startPos = button.anchoredPosition;
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        
        float elapsed = 0f;
        
        while (elapsed < animDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animDuration;
            float curvedT = buttonEase.Evaluate(t);
            
            button.anchoredPosition = Vector2.Lerp(startPos, targetPos, curvedT);
            canvasGroup.alpha = curvedT;
            
            yield return null;
        }
        
        button.anchoredPosition = targetPos;
        canvasGroup.alpha = 1f;
    }
    
    IEnumerator ExitButton(RectTransform button, Vector2 originalPos, float delay, float slideDistance = -1f)
    {
        yield return new WaitForSeconds(delay);
        
        float distance = slideDistance > 0 ? slideDistance : buttonSlideDistance;
        Vector2 startPos = button.anchoredPosition;
        Vector2 endPos = originalPos + new Vector2(0f, -distance);
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        
        float elapsed = 0f;
        
        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / exitDuration;
            float curvedT = exitEase.Evaluate(t);
            
            button.anchoredPosition = Vector2.Lerp(startPos, endPos, curvedT);
            canvasGroup.alpha = 1f - curvedT;
            
            yield return null;
        }
        
        button.anchoredPosition = endPos;
        canvasGroup.alpha = 0f;
    }
    
    IEnumerator AnimateTrackBackButton()
    {
        Vector2 startPos = trackBackOriginalPos + new Vector2(-trackBackSlideDistance, 0f);
        Vector2 overshootPos = trackBackOriginalPos + new Vector2(trackBackOvershoot, 0f);
        trackBackButton.anchoredPosition = startPos;
        
        float elapsed = 0f;
        float phase1Duration = trackBackDuration * 0.7f;
        
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase1Duration;
            trackBackButton.anchoredPosition = Vector2.Lerp(startPos, overshootPos, titleEase.Evaluate(t));
            yield return null;
        }
        
        elapsed = 0f;
        float phase2Duration = trackBackDuration * 0.3f;
        
        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase2Duration;
            trackBackButton.anchoredPosition = Vector2.Lerp(overshootPos, trackBackOriginalPos, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }
        
        trackBackButton.anchoredPosition = trackBackOriginalPos;
    }
    
    IEnumerator ExitTrackBackButton()
    {
        Vector2 startPos = trackBackOriginalPos;
        Vector2 endPos = trackBackOriginalPos + new Vector2(-trackBackSlideDistance, 0f);
        
        float elapsed = 0f;
        
        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / exitDuration;
            trackBackButton.anchoredPosition = Vector2.Lerp(startPos, endPos, exitEase.Evaluate(t));
            yield return null;
        }
        
        trackBackButton.anchoredPosition = endPos;
    }
    
    IEnumerator AnimateTrackTitle()
    {
        Vector2 startPos = trackTitleOriginalPos + new Vector2(0f, trackTitleDropDistance);
        trackPanelTitle.anchoredPosition = startPos;
        trackPanelTitle.localRotation = Quaternion.identity;
        
        float elapsed = 0f;
        
        while (elapsed < trackTitleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / trackTitleDuration;
            trackPanelTitle.anchoredPosition = Vector2.Lerp(startPos, trackTitleOriginalPos, titleEase.Evaluate(t));
            yield return null;
        }
        
        trackPanelTitle.anchoredPosition = trackTitleOriginalPos;
        isTitleWiggling = true;
    }
    
    IEnumerator ExitTrackTitle()
    {
        Vector2 startPos = trackTitleOriginalPos;
        Vector2 endPos = trackTitleOriginalPos + new Vector2(0f, trackTitleDropDistance);
        
        float elapsed = 0f;
        
        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / exitDuration;
            trackPanelTitle.anchoredPosition = Vector2.Lerp(startPos, endPos, exitEase.Evaluate(t));
            yield return null;
        }
        
        trackPanelTitle.anchoredPosition = endPos;
    }
    
    IEnumerator AnimateCreditsBackButton()
    {
        Vector2 startPos = creditsBackOriginalPos + new Vector2(-creditsBackSlideDistance, 0f);
        Vector2 overshootPos = creditsBackOriginalPos + new Vector2(creditsBackOvershoot, 0f);
        creditsBackButton.anchoredPosition = startPos;
        
        float elapsed = 0f;
        float phase1Duration = creditsBackDuration * 0.7f;
        
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase1Duration;
            creditsBackButton.anchoredPosition = Vector2.Lerp(startPos, overshootPos, titleEase.Evaluate(t));
            yield return null;
        }
        
        elapsed = 0f;
        float phase2Duration = creditsBackDuration * 0.3f;
        
        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / phase2Duration;
            creditsBackButton.anchoredPosition = Vector2.Lerp(overshootPos, creditsBackOriginalPos, Mathf.Sin(t * Mathf.PI * 0.5f));
            yield return null;
        }
        
        creditsBackButton.anchoredPosition = creditsBackOriginalPos;
    }
    
    IEnumerator ExitCreditsBackButton()
    {
        Vector2 startPos = creditsBackOriginalPos;
        Vector2 endPos = creditsBackOriginalPos + new Vector2(-creditsBackSlideDistance, 0f);
        
        float elapsed = 0f;
        
        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / exitDuration;
            creditsBackButton.anchoredPosition = Vector2.Lerp(startPos, endPos, exitEase.Evaluate(t));
            yield return null;
        }
        
        creditsBackButton.anchoredPosition = endPos;
    }
    
    IEnumerator AnimateCreditsTitle()
    {
        Vector2 startPos = creditsTitleOriginalPos + new Vector2(0f, creditsTitleDropDistance);
        creditsPanelTitle.anchoredPosition = startPos;
        creditsPanelTitle.localScale = Vector3.one;
        
        float elapsed = 0f;
        
        while (elapsed < creditsTitleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / creditsTitleDuration;
            creditsPanelTitle.anchoredPosition = Vector2.Lerp(startPos, creditsTitleOriginalPos, titleEase.Evaluate(t));
            yield return null;
        }
        
        creditsPanelTitle.anchoredPosition = creditsTitleOriginalPos;
        isCreditsTitlePulsing = true;
    }
    
    IEnumerator ExitCreditsTitle()
    {
        Vector2 startPos = creditsTitleOriginalPos;
        Vector2 endPos = creditsTitleOriginalPos + new Vector2(0f, creditsTitleDropDistance);
        
        float elapsed = 0f;
        
        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / exitDuration;
            creditsPanelTitle.anchoredPosition = Vector2.Lerp(startPos, endPos, exitEase.Evaluate(t));
            yield return null;
        }
        
        creditsPanelTitle.anchoredPosition = endPos;
    }
}