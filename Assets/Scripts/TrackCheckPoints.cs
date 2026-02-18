using UnityEngine;
using System.Collections.Generic;
using System;

public class TrackCheckPoints : MonoBehaviour
{
    public event EventHandler OnPlayerCorrectCheckpoint;
    public event EventHandler OnPlayerWrongCheckpoint;
    public event EventHandler OnLapCompleted;
    public event EventHandler OnRaceFinished;
    
    [Header("Race Settings")]
    public int totalLaps = 3;
    
    private List<CheckpointSingle> checkpointSingleList = new List<CheckpointSingle>();
    private int nextCheckpointIndex = 0;
    private int currentLap = 1;
    private bool raceFinished = false;
    
    private void Awake()
    {
        foreach (Transform checkpointSingleTransform in transform)
        {
            CheckpointSingle checkpointSingle = checkpointSingleTransform.GetComponent<CheckpointSingle>();
            
            if (checkpointSingle != null)
            {
                checkpointSingle.SetTrackCheckPoints(this);
                checkpointSingleList.Add(checkpointSingle);
            }
        }
    }

    private void Start()
    {
        // Show first two checkpoints at the start
        UpdateVisibleCheckpoints();
    }
    
    public void PlayerThroughCheckPoint(CheckpointSingle checkpointSingle)
    {
        if (raceFinished) return;
        
        int checkpointIndex = checkpointSingleList.IndexOf(checkpointSingle);
        
        if (checkpointIndex == nextCheckpointIndex)
        {
            Debug.Log($"CORRECT checkpoint {checkpointSingle.name}");
            
            nextCheckpointIndex++;
            
            // Check if we completed a lap (passed all checkpoints)
            if (nextCheckpointIndex >= checkpointSingleList.Count)
            {
                nextCheckpointIndex = 0;
                CompleteLap();
            }
            
            UpdateVisibleCheckpoints();

            OnPlayerCorrectCheckpoint?.Invoke(this, EventArgs.Empty);
        }
        else if ( checkpointIndex == nextCheckpointIndex - 1) 
        {
            Debug.Log($"Pased the same checkpoint ");
        }
        else
        {
            Debug.Log($"WRONG checkpoint! Expected {nextCheckpointIndex}, got {checkpointIndex}");
            OnPlayerWrongCheckpoint?.Invoke(this, EventArgs.Empty);
        }
    }

    private void CompleteLap()
    {
        Debug.Log($"Lap {currentLap} completed!");
        
        OnLapCompleted?.Invoke(this, EventArgs.Empty);
        
        currentLap++;
        
        // Check if race is complete
        if (currentLap > totalLaps)
        {
            nextCheckpointIndex = checkpointSingleList.Count; 
            FinishRace();
        }
    }

    private void FinishRace()
    {
        raceFinished = true;
        Debug.Log("RACE FINISHED!");
        
        OnRaceFinished?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateVisibleCheckpoints()
    {
        if (raceFinished || nextCheckpointIndex >= checkpointSingleList.Count)
        {
            foreach (var checkpoint in checkpointSingleList)
            {
                checkpoint.Hide();
            }
            return;
        }
        
        foreach (var checkpoint in checkpointSingleList)
        {
            checkpoint.Hide();
        }

        checkpointSingleList[nextCheckpointIndex].Show();

        int nextPlusOneIndex = (nextCheckpointIndex + 1) % checkpointSingleList.Count;
        checkpointSingleList[nextPlusOneIndex].Show();
    }
    
    public int GetNextCheckpointIndex() { return nextCheckpointIndex; }
    public int GetTotalCheckpoints() { return checkpointSingleList.Count; }
    public int GetCurrentLap() { return currentLap; }
    public int GetTotalLaps() { return totalLaps; }
    public bool IsRaceFinished() { return raceFinished; }
}