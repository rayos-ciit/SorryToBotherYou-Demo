using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Connections")]
    public UIManager uiManager;
    
    [Header("Shift Settings")]
    public int currentDay = 1;
    public int minCallsPerDay = 5;
    public int maxCallsPerDay = 8;
    
    // Tracks our progress through the night
    private int totalCallsToday;
    private int callsCompletedToday;
    
    [Header("Health & Difficulty")]
    [Tooltip("Player loses if they fail 60% of the total calls for the day.")]
    private int maxStrikesAllowed;
    private int currentStrikes;

    [Header("Pacing")]
    public float minDelayBetweenCalls = 5.0f;
    public float maxDelayBetweenCalls = 15.0f;

    [Header("The Deck")]
    [Tooltip("Drag and drop all your CallerData profiles here in the Inspector.")]
    public List<CallerData> availableCallers;

    // We use a Coroutine to handle the waiting periods without freezing the game
    private Coroutine shiftRoutine;

    void Start()
    {
        StartShift();
    }

    public void StartShift()
    {
        // 1. Reset health and progress for the new day
        currentStrikes = 0;
        callsCompletedToday = 0;

        // 2. Determine RNG Quota
        totalCallsToday = Random.Range(minCallsPerDay, maxCallsPerDay + 1);
        
        // 3. Calculate 60% failure threshold (rounded up to be fair to the player)
        maxStrikesAllowed = Mathf.CeilToInt(totalCallsToday * 0.6f);

        Debug.Log($"Day {currentDay} Start! Quota: {totalCallsToday} calls. Max Strikes Allowed: {maxStrikesAllowed}");

        // 4. Start the agonizing wait for the first call
        shiftRoutine = StartCoroutine(ShiftRoutine());
    }

    private IEnumerator ShiftRoutine()
    {
        while (callsCompletedToday < totalCallsToday)
        {
            // Pick a random delay
            float waitTime = Random.Range(minDelayBetweenCalls, maxDelayBetweenCalls);
            Debug.Log($"Waiting {waitTime} seconds for the next call...");
            yield return new WaitForSeconds(waitTime);

            // Pull a caller from our weighted deck
            CallerData nextCaller = PullCallerFromDeck();
            
            // TODO: Tell the Phone/UI script to start ringing with 'nextCaller' data!
            Debug.Log($"RING RING! Caller: {nextCaller.callerName} (Type: {nextCaller.typeOfCaller})");

            // Pause this loop until the call is resolved by the player
            // (We will build the system to unpause this later)
            yield return new WaitUntil(() => CallIsResolved()); 

            callsCompletedToday++;
            UpdateInGameClock();
        }

        EndShift();
    }

    private CallerData PullCallerFromDeck()
    {
        // Calculate the total weight of all callers in the deck
        int totalWeight = 0;
        foreach (CallerData caller in availableCallers)
        {
            totalWeight += caller.spawnWeight;
        }

        // Pick a random number between 0 and totalWeight
        int randomRoll = Random.Range(0, totalWeight);
        int currentWeightSum = 0;

        // Find which caller that random roll landed on
        foreach (CallerData caller in availableCallers)
        {
            currentWeightSum += caller.spawnWeight;
            if (randomRoll < currentWeightSum)
            {
                return caller;
            }
        }

        // Fallback just in case
        return availableCallers[0];
    }

    // This method will be called by your UI/Phone buttons when the player makes a choice
    public void HandleCallResult(bool handledCorrectly)
    {
        if (!handledCorrectly)
        {
            currentStrikes++;
            uiManager.UpdateStrikes(currentStrikes);
            Debug.Log($"STRIKE! Current Strikes: {currentStrikes} / {maxStrikesAllowed}");

            if (currentStrikes >= maxStrikesAllowed)
            {
                TriggerGameOver();
                return;
            }
        }
        else
        {
            Debug.Log("Call handled perfectly.");
        }

        // Mark the call as resolved so the ShiftRoutine can continue
        isCallActive = false; 
    }

    private void TriggerGameOver()
    {
        StopCoroutine(shiftRoutine);
        Debug.Log("GAME OVER: The Boss is here.");
        
        // Muffle all audio, turn on the scary UI screen
        FindObjectOfType<RulebookController>().ambientAudioSource.Stop(); 
        uiManager.ShowGameOver();
    }

    private void EndShift()
    {
        Debug.Log($"Shift Complete! You survived Day {currentDay}.");
        uiManager.ShowShiftComplete(currentDay);
        
        // Wait for player to click "Next Day" to continue
    }

    private void UpdateInGameClock()
    {
        // Calculate how far along we are (e.g., 50% through the quota = 3:00 AM)
        float shiftProgress = (float)callsCompletedToday / totalCallsToday;
        Debug.Log($"Shift is {shiftProgress * 100}% complete. Update UI Clock!");
        uiManager.UpdateClock(shiftProgress);
    }

    // --- Temporary Variables to handle pausing the coroutine ---
    private bool isCallActive = true;
    private bool CallIsResolved()
    {
        // This checks every frame while waiting. When isCallActive is false, the loop continues.
        bool result = !isCallActive;
        isCallActive = true; // Reset for the next call
        return result;
    }
    
    // Hook this up to a "Restart" button on your Game Over screen
    public void RestartGame()
    {
        // Reloads the current active scene to start completely fresh
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    // Hook this up to a "Continue" button on your Shift Complete screen
    public void StartNextDay()
    {
        currentDay++;
        if (uiManager.shiftCompleteScreen != null) uiManager.shiftCompleteScreen.SetActive(false);
        StartShift();
    }
}