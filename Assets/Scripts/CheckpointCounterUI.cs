using UnityEngine;
using TMPro;

public class CheckpointCounterUI : MonoBehaviour
{
    [SerializeField] private TrackCheckPoints trackCheckPoints;
    [SerializeField] private TextMeshProUGUI checkpointText;

    private void Start()
    {
        // Subscribe to checkpoint events
        trackCheckPoints.OnPlayerCorrectCheckpoint += TrackCheckPoints_OnPlayerCorrectCheckpoint;
        
        // Update the text immediately
        UpdateCheckpointText();
        
        // Note: RaceManager will handle showing/hiding this UI during countdown
    }

    private void TrackCheckPoints_OnPlayerCorrectCheckpoint(object sender, System.EventArgs e)
    {
        UpdateCheckpointText();
    }

    private void UpdateCheckpointText()
    {
        int currentCheckpoint = trackCheckPoints.GetNextCheckpointIndex();
        int totalCheckpoints = trackCheckPoints.GetTotalCheckpoints();
        
        checkpointText.text = $"{currentCheckpoint}/{totalCheckpoints}";
    }
    
    private void OnDestroy()
    {
        trackCheckPoints.OnPlayerCorrectCheckpoint -= TrackCheckPoints_OnPlayerCorrectCheckpoint;
    }
}