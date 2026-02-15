using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingPanel;
    public Image expandingCircle;  // The circle that expands from click
    public TextMeshProUGUI loadingTitle;  // The "LOADING" title text
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI loadingTipText;
    
    [Header("Loading Settings")]
    public float minimumLoadTime = 2f;
    
    [Header("Expand Animation")]
    public float expandDuration = 0.6f;  // How long the circle takes to fill screen
    public AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float maxCircleSize = 3000f;  // Size to cover whole screen
    public float delayBeforeShrink = 0.2f;  // Delay after hiding text before shrinking
    
    [Header("Text Fade Animation")]
    public float textFadeDuration = 0.4f;  // How long text fades in/out
    public AnimationCurve textFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Loading Tips")]
    public string[] loadingTips = new string[]
    {
        "Tip: Collect boost rings for extra speed!",
        "Tip: Hit checkpoints in order to complete laps",
        "Tip: Drifting helps maintain speed through corners",
        "Tip: Watch out for water hazards!",
        "Tip: Use mouse to look around while driving",
        "Tip: Complete all laps to win the race!"
    };
    
    private static LoadingScreen instance;
    private bool isLoading = false;
    private Vector2 lastClickPosition;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[LoadingScreen] Created and set to DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("[LoadingScreen] Duplicate found, destroying");
            Destroy(gameObject);
            return;
        }
        
        // Hide loading panel initially
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        // Hide expanding circle
        if (expandingCircle != null)
        {
            expandingCircle.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Call this to load a scene with expanding animation from mouse position
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        Debug.Log($"[LoadingScreen] LoadScene called for: {sceneName}");
        
        if (instance != null)
        {
            // Store mouse position when button was clicked
            instance.lastClickPosition = Input.mousePosition;
            instance.StartCoroutine(instance.LoadSceneWithAnimation(sceneName));
        }
        else
        {
            Debug.LogWarning("[LoadingScreen] No instance found! Loading directly...");
            SceneManager.LoadScene(sceneName);
        }
    }
    
    IEnumerator LoadSceneWithAnimation(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("[LoadingScreen] Already loading a scene!");
            yield break;
        }
        
        isLoading = true;
        
        // Show loading panel BUT keep it transparent/invisible initially
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            
            // Make the panel background transparent during circle expansion
            Image panelImage = loadingPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0, 0, 0, 0); // Transparent black
            }
            
            // Hide all text during expansion
            if (loadingTitle != null) loadingTitle.gameObject.SetActive(false);
            if (progressText != null) progressText.gameObject.SetActive(false);
            if (loadingTipText != null) loadingTipText.gameObject.SetActive(false);
        }
        
        // Start the expanding circle animation FIRST
        yield return StartCoroutine(ExpandCircleAnimation());
        
        // Circle has filled screen - now HIDE it so we can see the text
        if (expandingCircle != null)
        {
            expandingCircle.gameObject.SetActive(false);
        }
        
        // AFTER circle has filled screen, make background fully black
        if (loadingPanel != null)
        {
            Image panelImage = loadingPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0, 0, 0, 1); // Solid black
            }
        }
        
        // Small delay to ensure circle is completely gone before showing text
        yield return new WaitForSeconds(0.1f);
        
        // NOW fade in the loading UI smoothly
        yield return StartCoroutine(FadeInText());
        
        Debug.Log($"[LoadingScreen] Starting async load of: {sceneName}");
        
        // Start loading the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        float elapsedTime = 0f;
        int lastDisplayedProgress = 0;
        
        // Loading loop
        while (elapsedTime < minimumLoadTime || asyncLoad.progress < 0.9f)
        {
            elapsedTime += Time.deltaTime;
            
            float realProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);
            float displayProgress = Mathf.Max(realProgress, timeProgress);
            
            int currentProgress = Mathf.RoundToInt(displayProgress * 100f);
            
            if (currentProgress != lastDisplayedProgress && progressText != null)
            {
                progressText.text = currentProgress + "%";
                lastDisplayedProgress = currentProgress;
            }
            
            yield return null;
        }
        
        if (progressText != null)
        {
            progressText.text = "100%";
        }
        
        Debug.Log("[LoadingScreen] Loading complete, activating scene");
        
        // Small delay to show 100%
        yield return new WaitForSeconds(0.2f);
        
        // FADE OUT TEXT smoothly before scene activates
        Debug.Log("[LoadingScreen] Fading out loading text");
        yield return StartCoroutine(FadeOutText());
        
        Debug.Log("[LoadingScreen] About to activate scene");
        
        // Activate the scene (but we'll still see loading screen)
        asyncLoad.allowSceneActivation = true;
        
        Debug.Log("[LoadingScreen] Scene activation triggered, waiting for isDone");
        
        // Wait for scene to actually load
        int waitFrames = 0;
        while (!asyncLoad.isDone)
        {
            waitFrames++;
            if (waitFrames % 10 == 0)
            {
                Debug.Log($"[LoadingScreen] Waiting for scene... frame {waitFrames}, isDone={asyncLoad.isDone}");
            }
            yield return null;
        }
        
        Debug.Log($"[LoadingScreen] Scene is DONE after {waitFrames} frames!");
        Debug.Log("[LoadingScreen] Scene activated, starting exit animation");
        
        // Ensure our loading screen objects are still active
        if (loadingPanel == null || expandingCircle == null)
        {
            Debug.LogError("[LoadingScreen] Loading panel or circle became null after scene load!");
            isLoading = false;
            yield break;
        }
        
        if (!loadingPanel.activeSelf)
        {
            Debug.LogWarning("[LoadingScreen] Loading panel was deactivated! Reactivating...");
            loadingPanel.SetActive(true);
        }
        
        // Wait for the first frame of new scene to fully render
        yield return new WaitForEndOfFrame();
        
        // REVERSE ANIMATION: Show circle FIRST, then transparent, then shrink
        
        Debug.Log("[LoadingScreen] Showing circle FIRST at full size");
        
        // SHOW circle FIRST (full screen) BEFORE making panel transparent
        if (expandingCircle != null)
        {
            RectTransform circleRect = expandingCircle.rectTransform;
            circleRect.sizeDelta = new Vector2(maxCircleSize, maxCircleSize);
            
            // Position at screen center for shrinking
            circleRect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            
            expandingCircle.gameObject.SetActive(true);
            
            Debug.Log($"[LoadingScreen] Circle activated at center, size: {maxCircleSize}");
        }
        else
        {
            Debug.LogError("[LoadingScreen] Expanding circle is null!");
        }
        
        // Wait one frame to ensure circle renders on top
        yield return null;
        
        Debug.Log("[LoadingScreen] NOW making panel transparent");
        
        // NOW make panel transparent (circle is already covering everything)
        if (loadingPanel != null)
        {
            Image panelImage = loadingPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0, 0, 0, 0); // Transparent
                Debug.Log("[LoadingScreen] Panel made transparent");
            }
        }
        
        // Wait the delay with circle visible and full screen
        yield return new WaitForSeconds(delayBeforeShrink);
        
        Debug.Log("[LoadingScreen] Starting shrink animation NOW");
        
        // NOW shrink the circle to reveal the new scene!
        yield return StartCoroutine(ShrinkCircleAnimation());
        
        // Now hide everything
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        if (expandingCircle != null)
        {
            expandingCircle.gameObject.SetActive(false);
        }
        
        isLoading = false;
    }
    
    IEnumerator ExpandCircleAnimation()
    {
        if (expandingCircle == null)
        {
            Debug.LogWarning("[LoadingScreen] No expanding circle assigned!");
            yield break;
        }
        
        // Position circle at mouse click location
        RectTransform circleRect = expandingCircle.rectTransform;
        circleRect.position = lastClickPosition;
        
        // Start small
        circleRect.sizeDelta = Vector2.zero;
        expandingCircle.gameObject.SetActive(true);
        
        Debug.Log($"[LoadingScreen] Starting expand animation from {lastClickPosition}");
        
        float elapsed = 0f;
        
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandDuration;
            float curvedT = expandCurve.Evaluate(t);
            
            // Expand the circle
            float size = Mathf.Lerp(0f, maxCircleSize, curvedT);
            circleRect.sizeDelta = new Vector2(size, size);
            
            yield return null;
        }
        
        // Ensure it's fully expanded
        circleRect.sizeDelta = new Vector2(maxCircleSize, maxCircleSize);
        
        Debug.Log("[LoadingScreen] Expand animation complete");
    }
    
    IEnumerator ShrinkCircleAnimation()
    {
        if (expandingCircle == null)
        {
            Debug.LogError("[LoadingScreen] No expanding circle assigned for shrink!");
            yield break;
        }
        
        RectTransform circleRect = expandingCircle.rectTransform;
        
        Debug.Log($"[LoadingScreen] Starting shrink animation - Initial size: {circleRect.sizeDelta}");
        
        float elapsed = 0f;
        
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandDuration;
            float curvedT = expandCurve.Evaluate(t);
            
            // Shrink the circle (reverse of expand)
            float size = Mathf.Lerp(maxCircleSize, 0f, curvedT);
            circleRect.sizeDelta = new Vector2(size, size);
            
            yield return null;
        }
        
        // Ensure it's fully shrunk
        circleRect.sizeDelta = Vector2.zero;
        
        Debug.Log("[LoadingScreen] Shrink animation complete - Final size: 0");
    }
    
    IEnumerator FadeInText()
    {
        // Activate all text objects first (but invisible)
        if (loadingTitle != null)
        {
            loadingTitle.gameObject.SetActive(true);
            loadingTitle.alpha = 0f;
        }
        
        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
            progressText.alpha = 0f;
            progressText.text = "0%";
        }
        
        if (loadingTipText != null && loadingTips.Length > 0)
        {
            loadingTipText.gameObject.SetActive(true);
            loadingTipText.alpha = 0f;
            loadingTipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }
        
        float elapsed = 0f;
        
        // Fade in smoothly
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textFadeDuration;
            float alpha = textFadeCurve.Evaluate(t);
            
            if (loadingTitle != null) loadingTitle.alpha = alpha;
            if (progressText != null) progressText.alpha = alpha;
            if (loadingTipText != null) loadingTipText.alpha = alpha;
            
            yield return null;
        }
        
        // Ensure fully visible
        if (loadingTitle != null) loadingTitle.alpha = 1f;
        if (progressText != null) progressText.alpha = 1f;
        if (loadingTipText != null) loadingTipText.alpha = 1f;
    }
    
    IEnumerator FadeOutText()
    {
        float elapsed = 0f;
        
        // Fade out smoothly
        while (elapsed < textFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / textFadeDuration;
            float alpha = 1f - textFadeCurve.Evaluate(t);
            
            if (loadingTitle != null) loadingTitle.alpha = alpha;
            if (progressText != null) progressText.alpha = alpha;
            if (loadingTipText != null) loadingTipText.alpha = alpha;
            
            yield return null;
        }
        
        // Ensure fully invisible and deactivate
        if (loadingTitle != null)
        {
            loadingTitle.alpha = 0f;
            loadingTitle.gameObject.SetActive(false);
        }
        
        if (progressText != null)
        {
            progressText.alpha = 0f;
            progressText.gameObject.SetActive(false);
        }
        
        if (loadingTipText != null)
        {
            loadingTipText.alpha = 0f;
            loadingTipText.gameObject.SetActive(false);
        }
    }
}