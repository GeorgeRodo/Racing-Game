using UnityEngine;
using TMPro;
using System.Collections;

public class RaceManager : MonoBehaviour
{
    [Header("Countdown Settings")]
    public float countdownTime = 3f;
    public TextMeshProUGUI countdownText;
    
    [Header("Vehicle Reference")]
    public CustomVehicleController vehicleController;
    
    [Header("Track Reference")]
    public TrackCheckPoints trackCheckPoints;

    [Header("Audio Settings")]
    public AudioSource raceMusic; // Σύρε εδώ το AudioSource της μουσικής

    private bool raceStarted = false;
    private bool countdownActive = false;
    private float raceTime = 0f;
    
    void Start()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }
        
        // Disable vehicle controls at start
        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }
        
        // Start countdown automatically
        StartCoroutine(CountdownSequence());
    }

    void Update()
    {
        // Track race time
        if (raceStarted && !trackCheckPoints.IsRaceFinished())
        {
            raceTime += Time.deltaTime;
        }
    }
    
    IEnumerator CountdownSequence()
    {
        countdownActive = true;
        
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                countdownText.color = Color.white;
                countdownText.fontSize = 120;
            }
            yield return new WaitForSeconds(1f);
        }
        
        // Show "GO!"
        if (countdownText != null)
        {
            countdownText.text = "GO!";
            countdownText.color = Color.green;
        }
        yield return new WaitForSeconds(0.5f);
        
        // Hide countdown and start race
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        StartRace();
    }
    
    void StartRace()
    {
        raceStarted = true;
        countdownActive = false;
        raceTime = 0f;

        if (raceMusic != null)
        {
            raceMusic.Play();
        }

        // Enable vehicle controls
        if (vehicleController != null)
        {
            vehicleController.enabled = true;
        }
    }
    
    public bool IsRaceStarted() { return raceStarted; }
    public float GetRaceTime() { return raceTime; }
}