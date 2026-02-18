using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Animations")]
    public MainMenuAnimations animationController;
    
    [Header("Track Selection")]
    public string track1SceneName = "RaceTrack1";
    public string track2SceneName = "RaceTrack2";
    public string track3SceneName = "Track3";
    public string track4SceneName = "Track4";
    
    // MAIN MENU BUTTONS 
    public void OnPlayButton()
    {
        if (animationController != null)
        {
            animationController.AnimateTrackPanel();
        }
    }
    
    public void OnCreditsButton()
    {
        if (animationController != null)
        {
            animationController.AnimateCreditsPanel();
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
    
    // TRACK SELECTION BUTTONS 
    public void OnTrack1Button()
    {
        if (!string.IsNullOrEmpty(track1SceneName))
        {
            LoadingScreen.LoadScene(track1SceneName);
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
            LoadingScreen.LoadScene(track2SceneName);
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
            LoadingScreen.LoadScene(track3SceneName);
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
            LoadingScreen.LoadScene(track4SceneName);
        }
        else
        {
            Debug.Log("Track 4 - Coming Soon!");
        }
    }
    
    // BACK BUTTONS
    public void OnBackFromTracks()
    {
        if (animationController != null)
        {
            animationController.AnimateMainMenu();
        }
    }
    
    public void OnBackFromCredits()
    {
        if (animationController != null)
        {
            animationController.AnimateMainMenu();
        }
    }
}