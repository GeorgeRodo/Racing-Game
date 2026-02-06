using UnityEngine;
using TMPro; // If you're using TextMeshPro

public class TrackCheckpointsUI : MonoBehaviour
{
    [SerializeField] private TrackCheckPoints trackCheckPoints;
    [SerializeField] private GameObject wrongCheckpointWarning; // A UI panel/text that says "WRONG CHECKPOINT!"
    [SerializeField] private float warningDisplayTime = 2f; // How long to show the warning
    
    private float warningTimer = 0f;

    private void Start()
    {
        trackCheckPoints.OnPlayerCorrectCheckpoint += TrackCheckPoints_OnPlayerCorrectCheckpoint;
        trackCheckPoints.OnPlayerWrongCheckpoint += TrackCheckPoints_OnPlayerWrongCheckpoint;
        
        // Hide warning initially
        if (wrongCheckpointWarning != null)
        {
            wrongCheckpointWarning.SetActive(false);
        }
    }

    private void Update()
    {
        // Auto-hide warning after timer expires
        if (warningTimer > 0)
        {
            warningTimer -= Time.deltaTime;
            if (warningTimer <= 0)
            {
                HideWarning();
            }
        }
    }

    private void TrackCheckPoints_OnPlayerWrongCheckpoint(object sender, System.EventArgs e)
    {
        Debug.Log("UI: Player hit wrong checkpoint!");
        ShowWarning();
    }

    private void TrackCheckPoints_OnPlayerCorrectCheckpoint(object sender, System.EventArgs e)
    {
        Debug.Log("UI: Player hit correct checkpoint!");
        // Optionally hide warning immediately if they hit correct checkpoint
        HideWarning();
    }

    private void ShowWarning()
    {
        if (wrongCheckpointWarning != null)
        {
            wrongCheckpointWarning.SetActive(true);
            warningTimer = warningDisplayTime;
        }
    }

    private void HideWarning()
    {
        if (wrongCheckpointWarning != null)
        {
            wrongCheckpointWarning.SetActive(false);
        }
        warningTimer = 0f;
    }
}
