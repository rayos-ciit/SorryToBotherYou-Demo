using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Connections")]
    public UIManager uiManager;
    public PhoneController phoneController;
    public ComputerController computerController;
    
    [Header("Shift Settings")]
    public int currentDay = 1;
    public int minCallsPerDay = 5, maxCallsPerDay = 8;
    public float minDelayBetweenCalls = 5.0f, maxDelayBetweenCalls = 15.0f;

    [Header("The Deck")]
    public List<CallerData> availableCallers;

    [Header("Debug")]
    public bool debugForceType = false;
    public CallerType debugForcedType;

    public CallerData activeCaller { get; private set; }
    private int totalCallsToday, callsCompletedToday, currentStrikes, maxStrikesAllowed;
    private bool callInProgress = false;

    void Start() 
    { 
        // Do nothing! Let the player read the rulebook.
        Debug.Log("Pre-shift phase. Waiting for player to clock in...");
    }

    public void StartShift()
    {
        currentStrikes = callsCompletedToday = 0;
        totalCallsToday = Random.Range(minCallsPerDay, maxCallsPerDay + 1);
        maxStrikesAllowed = Mathf.CeilToInt(totalCallsToday * 0.6f);
        StartCoroutine(ShiftRoutine());
    }

    private IEnumerator ShiftRoutine()
    {
        while (callsCompletedToday < totalCallsToday)
        {
            yield return new WaitForSeconds(Random.Range(minDelayBetweenCalls, maxDelayBetweenCalls));
            
            if (computerController != null) yield return new WaitUntil(() => !computerController.isRebooting);

            // One line selection checking for debug!
            activeCaller = debugForceType 
                ? availableCallers.Find(c => c.typeOfCaller == debugForcedType) ?? PullCallerFromDeck()
                : PullCallerFromDeck();

            callInProgress = true;
            phoneController?.StartRinging(activeCaller);
            computerController?.OnCallStarted(activeCaller);

            yield return new WaitUntil(() => !callInProgress);
            
            callsCompletedToday++;
            uiManager?.UpdateClock((float)callsCompletedToday / totalCallsToday);
        }
        uiManager?.ShowShiftComplete(currentDay);
    }

    public void ResolveCall(bool success)
    {
        if (!callInProgress) return;

        if (!success)
        {
            currentStrikes++;
            uiManager?.UpdateStrikes(currentStrikes);
            if (currentStrikes >= maxStrikesAllowed) { uiManager?.ShowGameOver(); StopAllCoroutines(); }
        }

        callInProgress = false;
        phoneController?.ResetPhoneState(); 
        computerController?.ResetMonitor();
        uiManager?.ClearCallerID();
    }

    private CallerData PullCallerFromDeck()
    {
        int totalWeight = 0, sum = 0;
        availableCallers.ForEach(c => totalWeight += c.spawnWeight); // Condensed weight math
        
        int roll = Random.Range(0, totalWeight);
        foreach (var c in availableCallers) if (roll < (sum += c.spawnWeight)) return c;
        
        return availableCallers[0]; // Fallback
    }
}