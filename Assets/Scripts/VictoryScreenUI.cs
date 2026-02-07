using UnityEngine;
using TMPro;

public class VictoryScreenUI : MonoBehaviour
{
    [SerializeField] private TrackCheckPoints trackCheckPoints;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private CustomVehicleController vehicleController;
    
    [Header("UI Elements to Hide")]
    [SerializeField] private GameObject lapCounterUI;
    [SerializeField] private GameObject speedometerUI;
    [SerializeField] private GameObject wrongCheckpointWarning;
    [SerializeField] private GameObject checkpointCounterUI; // ADD THIS

    private void Start()
    {
        trackCheckPoints.OnRaceFinished += ShowVictoryScreen;
        
        // Hide victory screen at start
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void ShowVictoryScreen(object sender, System.EventArgs e)
    {
        // Show victory panel
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (victoryText != null)
        {
            victoryText.text = "RACE COMPLETE!\n\nYOU WIN!";
        }

        // Disable vehicle controls
        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }
        
        // Hide all racing UI elements
        HideRacingUI();
        
        // Hide checkpoint visuals on track
        HideCheckpoints();
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
        
        // ADD THIS
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

    private void OnDestroy()
    {
        trackCheckPoints.OnRaceFinished -= ShowVictoryScreen;
    }
}