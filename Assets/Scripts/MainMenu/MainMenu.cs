using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject trackSelectionPanel;
    public CanvasGroup mainMenuCanvasGroup;      // Add CanvasGroup components
    public CanvasGroup trackSelectionCanvasGroup;
    
    [Header("Animations")]
    public MainMenuAnimations animationController;
    
    [Header("Track Selection")]
    public string track1SceneName = "RaceTrack1";
    public string track2SceneName = "RaceTrack2";
    public string track3SceneName = "Track3";
    public string track4SceneName = "Track4";
    
    void Start()
    {
        // Show main menu on start
        ShowMainMenu();
    }
    
    // ===== MAIN MENU BUTTONS =====
    public void OnPlayButton()
    {
        // Hide main menu
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 0f;
            mainMenuCanvasGroup.interactable = false;
            mainMenuCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            mainMenuPanel.SetActive(false);
        }
        
        // Show track panel
        if (trackSelectionCanvasGroup != null)
        {
            trackSelectionCanvasGroup.alpha = 1f;
            trackSelectionCanvasGroup.interactable = true;
            trackSelectionCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            trackSelectionPanel.SetActive(true);
        }
        
        // Trigger track panel animations
        if (animationController != null)
        {
            animationController.AnimateTrackPanel();
        }
    }
    
    public void OnExitButton()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    // ===== TRACK SELECTION BUTTONS =====
    public void OnTrack1Button()
    {
        if (!string.IsNullOrEmpty(track1SceneName))
        {
            SceneManager.LoadScene(track1SceneName);
        }
        else
        {
            Debug.LogWarning("Track 1 scene name not set!");
        }
    }
    
    public void OnTrack2Button()
    {
        if (!string.IsNullOrEmpty(track2SceneName))
        {
            SceneManager.LoadScene(track2SceneName);
        }
        else
        {
            Debug.Log("Track 2 - Coming Soon!");
        }
    }
    
    public void OnTrack3Button()
    {
        if (!string.IsNullOrEmpty(track3SceneName))
        {
            SceneManager.LoadScene(track3SceneName);
        }
        else
        {
            Debug.Log("Track 3 - Coming Soon!");
        }
    }
    
    public void OnTrack4Button()
    {
        if (!string.IsNullOrEmpty(track4SceneName))
        {
            SceneManager.LoadScene(track4SceneName);
        }
        else
        {
            Debug.Log("Track 4 - Coming Soon!");
        }
    }
    
    // ===== BACK BUTTONS =====
    public void OnBackFromTracks()
    {
        ShowMainMenu();
        
        // Trigger main menu animations
        if (animationController != null)
        {
            animationController.AnimateMainMenu();
        }
    }
    
    // ===== HELPER =====
    void ShowMainMenu()
    {
        // Show main menu
        if (mainMenuCanvasGroup != null)
        {
            mainMenuCanvasGroup.alpha = 1f;
            mainMenuCanvasGroup.interactable = true;
            mainMenuCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            mainMenuPanel.SetActive(true);
        }
        
        // Hide track panel
        if (trackSelectionCanvasGroup != null)
        {
            trackSelectionCanvasGroup.alpha = 0f;
            trackSelectionCanvasGroup.interactable = false;
            trackSelectionCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            trackSelectionPanel.SetActive(false);
        }
    }
}