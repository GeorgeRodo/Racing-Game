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
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI loadingTipText;
    
    [Header("Loading Settings")]
    public float minimumLoadTime = 2f;
    
    [Header("Expand Animation")]
    public float expandDuration = 0.6f;  // How long the circle takes to fill screen
    public AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float maxCircleSize = 3000f;  // Size to cover whole screen
    
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
        
        // NOW show the loading UI
        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
            progressText.text = "0%";
        }
        
        if (loadingTipText != null && loadingTips.Length > 0)
        {
            loadingTipText.gameObject.SetActive(true);
            loadingTipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }
        
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
        yield return new WaitForSeconds(0.3f);
        
        // Activate the scene (but we'll still see loading screen)
        asyncLoad.allowSceneActivation = true;
        
        // Wait for scene to actually load
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        Debug.Log("[LoadingScreen] Scene activated, starting exit animation");
        
        // REVERSE ANIMATION: Hide text, show circle, shrink it
        
        // Hide loading text
        if (progressText != null) progressText.gameObject.SetActive(false);
        if (loadingTipText != null) loadingTipText.gameObject.SetActive(false);
        
        // Show the circle again (full screen size)
        if (expandingCircle != null)
        {
            RectTransform circleRect = expandingCircle.rectTransform;
            circleRect.sizeDelta = new Vector2(maxCircleSize, maxCircleSize);
            
            // Position at screen center for shrinking
            circleRect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            
            expandingCircle.gameObject.SetActive(true);
        }
        
        // Make panel transparent (circle is the black overlay now)
        if (loadingPanel != null)
        {
            Image panelImage = loadingPanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = new Color(0, 0, 0, 0); // Transparent
            }
        }
        
        // Shrink the circle to reveal the new scene!
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
            Debug.LogWarning("[LoadingScreen] No expanding circle assigned!");
            yield break;
        }
        
        RectTransform circleRect = expandingCircle.rectTransform;
        
        Debug.Log("[LoadingScreen] Starting shrink animation");
        
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
        
        Debug.Log("[LoadingScreen] Shrink animation complete");
    }
}