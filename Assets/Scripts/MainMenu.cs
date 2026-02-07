using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject trackSelectionPanel;
    public GameObject settingsPanel;
    
    [Header("Track Selection")]
    public string track1SceneName = "GameScene"; // Your racing scene name
    
    void Start()
    {
        // Show main menu on start
        ShowMainMenu();
    }
    
    // ===== MAIN MENU BUTTONS =====
    public void OnPlayButton()
    {
        mainMenuPanel.SetActive(false);
        trackSelectionPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }
    
    public void OnSettingsButton()
    {
        mainMenuPanel.SetActive(false);
        trackSelectionPanel.SetActive(false);
        settingsPanel.SetActive(true);
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
        SceneManager.LoadScene(track1SceneName);
    }
    
    public void OnTrack2Button()
    {
        Debug.Log("Track 2 - Coming Soon!");
    }
    
    public void OnTrack3Button()
    {
        Debug.Log("Track 3 - Coming Soon!");
    }
    
    public void OnTrack4Button()
    {
        Debug.Log("Track 4 - Coming Soon!");
    }
    
    // ===== BACK BUTTONS =====
    public void OnBackFromTracks()
    {
        ShowMainMenu();
    }
    
    public void OnBackFromSettings()
    {
        ShowMainMenu();
    }
    
    // ===== HELPER =====
    void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        trackSelectionPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }
}