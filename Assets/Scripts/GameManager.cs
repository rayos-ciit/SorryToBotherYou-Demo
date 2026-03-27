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
    public int minCallsPerDay = 5;
    public int maxCallsPerDay = 8;
    
    [Header("Pacing")]
    public float minDelayBetweenCalls = 5.0f;
    public float maxDelayBetweenCalls = 15.0f;

    [Header("The Deck")]
    public List<CallerData> availableCallers;

    [Header("Debug")]
    public bool debugForceType = false;
    public CallerType debugForcedType;

    public CallerData activeCaller { get; private set; }
    private int totalCallsToday;
    private int callsCompletedToday;
    private int currentStrikes;
    private int maxStrikesAllowed;
    private bool callInProgress = false;

    void Start() { StartShift(); }

    public void StartShift()
    {
        currentStrikes = 0;
        callsCompletedToday = 0;
        totalCallsToday = Random.Range(minCallsPerDay, maxCallsPerDay + 1);
        maxStrikesAllowed = Mathf.CeilToInt(totalCallsToday * 0.6f);
        StartCoroutine(ShiftRoutine());
    }

    private IEnumerator ShiftRoutine()
    {
        while (callsCompletedToday < totalCallsToday)
        {
            yield return new WaitForSeconds(Random.Range(minDelayBetweenCalls, maxDelayBetweenCalls));

            // Pick Caller (Debug or Random)
            activeCaller = debugForceType ? availableCallers.Find(c => c.typeOfCaller == debugForcedType) : PullCallerFromDeck();
            if (activeCaller == null) activeCaller = availableCallers[0];

            callInProgress = true;
            phoneController.StartRinging(activeCaller);
            computerController.OnCallStarted(activeCaller);

            yield return new WaitUntil(() => !callInProgress);
            
            callsCompletedToday++;
            uiManager.UpdateClock((float)callsCompletedToday / totalCallsToday);
            uiManager.UpdateQuota(callsCompletedToday, totalCallsToday);
        }
        EndShift();
    }

    // THE CENTRAL SYSTEM: All successes/fails come through here
    public void ResolveCall(bool success)
    {
        if (!callInProgress) return;

        if (!success)
        {
            currentStrikes++;
            uiManager.UpdateStrikes(currentStrikes);
            if (currentStrikes >= maxStrikesAllowed) TriggerGameOver();
        }

        callInProgress = false;
        computerController.ResetMonitor();
        uiManager.ClearCallerID();
    }

    private CallerData PullCallerFromDeck()
    {
        int totalWeight = 0;
        foreach (var c in availableCallers) totalWeight += c.spawnWeight;
        int roll = Random.Range(0, totalWeight);
        int sum = 0;
        foreach (var c in availableCallers) { sum += c.spawnWeight; if (roll < sum) return c; }
        return availableCallers[0];
    }

    private void TriggerGameOver() { uiManager.ShowGameOver(); StopAllCoroutines(); }
    private void EndShift() { uiManager.ShowShiftComplete(currentDay); }
}