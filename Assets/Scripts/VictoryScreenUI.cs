using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VictoryScreenUI : MonoBehaviour
{
    [SerializeField] private TrackCheckPoints trackCheckPoints;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private CustomVehicleController vehicleController;
    [SerializeField] private CameraFollow cameraFollow;  
    
    [Header("UI Elements to Hide")]
    [SerializeField] private GameObject lapCounterUI;
    [SerializeField] private GameObject speedometerUI;
    [SerializeField] private GameObject wrongCheckpointWarning;
    [SerializeField] private GameObject checkpointCounterUI;
    
    [Header("Victory Screen Animations")]
    [SerializeField] private RectTransform victoryTitle;
    [SerializeField] private RectTransform restartButton;
    [SerializeField] private RectTransform mainMenuButton;
    
    [Header("Title Animation")]
    public float titleDropDistance = 200f;
    public float titleDuration = 0.6f;
    public float titleDelay = 0.2f;
    
    [Header("Button Animations")]
    public float buttonSlideDistance = 150f;
    public float buttonDuration = 0.5f;
    public float buttonStagger = 0.15f;
    public float buttonStartDelay = 0.4f;
    
    [Header("Animation Curves")]
    public AnimationCurve titleEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve buttonEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    
    private Vector2 titleOriginalPos;
    private Vector2 restartOriginalPos;
    private Vector2 mainMenuOriginalPos;

    private void Awake()
    {
        if (victoryTitle != null) titleOriginalPos = victoryTitle.anchoredPosition;
        if (restartButton != null) restartOriginalPos = restartButton.anchoredPosition;
        if (mainMenuButton != null) mainMenuOriginalPos = mainMenuButton.anchoredPosition;
    }

    private void Start()
    {
        trackCheckPoints.OnRaceFinished += ShowVictoryScreen;
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void ShowVictoryScreen(object sender, System.EventArgs e)
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (victoryText != null)
        {
            victoryText.text = "RACE COMPLETE!\n\nYOU WIN!";
        }

        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }
        
        if (cameraFollow != null)
        {
            cameraFollow.enabled = false;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log($"[VictoryScreen] Camera enabled: {(cameraFollow != null ? cameraFollow.enabled : false)}");
        Debug.Log($"[VictoryScreen] Vehicle enabled: {(vehicleController != null ? vehicleController.enabled : false)}");
        Debug.Log($"[VictoryScreen] Cursor visible: {Cursor.visible}, lockState: {Cursor.lockState}");
        
        UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("[VictoryScreen] NO EVENTSYSTEM FOUND! UI will not work!");
        }
        else
        {
            Debug.Log($"[VictoryScreen] EventSystem found: {eventSystem.gameObject.name}");
        }
        
        HideRacingUI();
        
        HideCheckpoints();
        
        StartCoroutine(AnimateVictoryScreen());
    }

    private void HideRacingUI()
    {
        if (lapCounterUI != null)
        {
            lapCounterUI.SetActive(false);
        }
        
        if (speedometerUI != null)
        {
            speedometerUI.SetActive(false);
        }
        
        if (wrongCheckpointWarning != null)
        {
            wrongCheckpointWarning.SetActive(false);
        }
        
        if (checkpointCounterUI != null)
        {
            checkpointCounterUI.SetActive(false);
        }
    }
    
    private void HideCheckpoints()
    {
        if (trackCheckPoints != null)
        {
            trackCheckPoints.gameObject.SetActive(false);
        }
    }
    
    // ANIMATION SYSTEM 
    
    IEnumerator AnimateVictoryScreen()
    {
        // Hide elements off-screen initially
        HideUIElements();
        
        yield return new WaitForSeconds(titleDelay);
        
        // Animate title dropping from top
        if (victoryTitle != null)
        {
            StartCoroutine(AnimateTitle());
        }
        
        yield return new WaitForSeconds(buttonStartDelay);
        
        if (restartButton != null)
        {
            StartCoroutine(AnimateButton(restartButton, restartOriginalPos, 0f));
        }
        
        yield return new WaitForSeconds(buttonStagger);
        
        if (mainMenuButton != null)
        {
            StartCoroutine(AnimateButton(mainMenuButton, mainMenuOriginalPos, 0f));
        }
    }
    
    void HideUIElements()
    {
        if (victoryTitle != null)
        {
            victoryTitle.anchoredPosition = titleOriginalPos + new Vector2(0f, titleDropDistance);
        }
        
        HideButton(restartButton, restartOriginalPos);
        HideButton(mainMenuButton, mainMenuOriginalPos);
    }
    
    void HideButton(RectTransform button, Vector2 originalPos)
    {
        if (button == null) return;
        
        button.anchoredPosition = originalPos + new Vector2(0f, -buttonSlideDistance);
        
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = button.gameObject.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        Button buttonComponent = button.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.interactable = true;  // Keep button itself enabled
        }
    }
    
    IEnumerator AnimateTitle()
    {
        // Start position 
        Vector2 startPos = titleOriginalPos + new Vector2(0f, titleDropDistance);
        
        victoryTitle.anchoredPosition = startPos;
        
        float elapsed = 0f;
        
        // Drop down
        while (elapsed < titleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / titleDuration;
            float curvedT = titleEase.Evaluate(t);
            
            victoryTitle.anchoredPosition = Vector2.Lerp(startPos, titleOriginalPos, curvedT);
            yield return null;
        }
        
        victoryTitle.anchoredPosition = titleOriginalPos;
    }
    
    IEnumerator AnimateButton(RectTransform button, Vector2 targetPos, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Vector2 startPos = button.anchoredPosition;
        
        CanvasGroup canvasGroup = button.GetComponent<CanvasGroup>();
        
        float elapsed = 0f;
        
        while (elapsed < buttonDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / buttonDuration;
            float curvedT = buttonEase.Evaluate(t);
            
            button.anchoredPosition = Vector2.Lerp(startPos, targetPos, curvedT);
            
            canvasGroup.alpha = curvedT;
            
            yield return null;
        }
        
        button.anchoredPosition = targetPos;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        Button buttonComponent = button.GetComponent<Button>();
        if (buttonComponent != null)
        {
            buttonComponent.interactable = true;
            Debug.Log($"[VictoryScreen] Button {button.name} - Button.interactable: {buttonComponent.interactable}");
        }
        else
        {
            Debug.LogWarning($"[VictoryScreen] Button {button.name} has NO Button component!");
        }
        
        CanvasGroup[] parentGroups = button.GetComponentsInParent<CanvasGroup>();
        Debug.Log($"[VictoryScreen] Button {button.name} has {parentGroups.Length} CanvasGroups in parents");
        foreach (var group in parentGroups)
        {
            Debug.Log($"  - CanvasGroup on {group.gameObject.name}: interactable={group.interactable}, blocksRaycasts={group.blocksRaycasts}");
        }
        
        Debug.Log($"[VictoryScreen] Button {button.name} animation complete - CanvasGroup interactable: {canvasGroup.interactable}, blocksRaycasts: {canvasGroup.blocksRaycasts}");
    }
    
    // BUTTON HANDLERS
 
    public void OnRestartButton()
    {
        Debug.Log("[VictoryScreen] Restart button clicked");
        
        // Get the current scene and reload it
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // Use LoadingScreen for smooth transition
        LoadingScreen.LoadScene(currentSceneName);
    }
    
    public void OnMainMenuButton()
    {
        Debug.Log("[VictoryScreen] Main Menu button clicked");
        
        LoadingScreen.LoadScene(mainMenuSceneName);
    }

    private void OnDestroy()
    {
        trackCheckPoints.OnRaceFinished -= ShowVictoryScreen;
    }
}