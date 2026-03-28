using System.Collections;
using UnityEngine;

public class PhoneController : MonoBehaviour
{
    [Header("Connections")]
    public GameManager gameManager;
    public UIManager uiManager;
    public DialogueSystem dialogueSystem;

    [Header("Audio Sources")]
    public AudioSource phoneAudioSource;
    public AudioSource voiceAudioSource;

    [Header("Call Logic")]
    public float slaTimeLimit = 50.0f;
    
    public bool isRinging { get; private set; }
    public bool isOffHook { get; private set; }
    public bool isOnHold { get; private set; }

    public CallerData currentCaller { get; private set; }
    private Coroutine slaRoutine;

    public void StartRinging(CallerData caller)
    {
        currentCaller = caller;
        isRinging = true;
        
        uiManager?.SetPhoneVisualOnBase();

        string cName = (caller.canMaskCallerID && Random.value > 0.5f) ? "UNKNOWN" : caller.callerName;
        string cNum  = (caller.canMaskCallerID && Random.value > 0.5f) ? "UNKNOWN" : caller.callerNumber;
        uiManager?.UpdateCallerID(cName, cNum);

        if (caller.ringSFX != null && phoneAudioSource != null)
        {
            phoneAudioSource.clip = caller.ringSFX;
            phoneAudioSource.loop = true;
            phoneAudioSource.Play();
        }

        // ---> THE STREAMLINED FIX <---
        // If the caller has a specific limit (like Karen's 7s), use it. 
        // If it is 0 (like John), use the global SLA Timer!
        float ringTime = caller.timeLimitToRespond > 0 ? caller.timeLimitToRespond : slaTimeLimit;
        RestartSLA(RingTimeoutRoutine(ringTime));
    }

    private IEnumerator RingTimeoutRoutine(float timeLimit)
    {
        yield return new WaitForSeconds(timeLimit);
        if (!isRinging) yield break; // If they already answered, kill the timer

        Debug.Log("The phone stopped ringing.");
        isRinging = false;
        phoneAudioSource?.Stop();
        
        // Consolidating the win/loss check into two lines!
        bool success = currentCaller.requiredAction == CorrectAction.Ignore;
        Debug.Log(success ? "Successfully ignored!" : "Missed a valid call! STRIKE!");
        gameManager?.ResolveCall(success);
    }

    public void PickUpPhone()
    {
        if (!isRinging) return;

        isRinging = false;
        isOffHook = true;
        phoneAudioSource?.Stop(); 
        
        RestartSLA(SLATimerRoutine(slaTimeLimit)); 

        Debug.Log("Picked up the receiver.");
        uiManager?.SetPhoneVisualOffHook();
        gameManager?.computerController?.OnPhonePickedUp();

        if (currentCaller.requiredAction == CorrectAction.Ignore)
        {
            Debug.Log("You answered a Disturbance! STRIKE!");
            if (currentCaller.voiceSFX != null) voiceAudioSource?.PlayOneShot(currentCaller.voiceSFX); 
            gameManager?.ResolveCall(false);
            return;
        }

        if (currentCaller.typeOfCaller == CallerType.Mimic)
            Debug.Log("It's a MIMIC! You have until the dialogue finishes to hang up!");

        dialogueSystem?.StartDialogue(currentCaller);
    }

    public void HoldPhone()
    {
        if (!isOffHook || isOnHold) return;

        Debug.Log("Call placed on hold.");
        isOnHold = true;
        dialogueSystem?.StopDialogue();
        
        StartCoroutine(WaitToResolveHold());
    }

    private IEnumerator WaitToResolveHold()
    {
        yield return new WaitForSeconds(2.0f);
        bool success = currentCaller?.requiredAction == CorrectAction.Hold;
        Debug.Log(success ? "Successfully placed on hold!" : "Invalid hold! STRIKE!");
        gameManager?.ResolveCall(success);
    }

    public void HangUpPhone()
    {
        if (!isOffHook) return;

        Debug.Log("Player slammed the phone down.");
        if (voiceAudioSource != null) voiceAudioSource.loop = false;

        bool success = currentCaller?.requiredAction == CorrectAction.HangUp;
        Debug.Log(success ? "Successfully disconnected!" : "Hung up on valid client! STRIKE!");
        gameManager?.ResolveCall(success);
    }

    private IEnumerator SLATimerRoutine(float limit)
    {
        yield return new WaitForSeconds(limit);
        Debug.Log("SLA Breach! You took too long.");
        gameManager?.ResolveCall(false);
    }

    // Helper method to keep your routine stopping/starting clean
    private void RestartSLA(IEnumerator newRoutine)
    {
        if (slaRoutine != null) StopCoroutine(slaRoutine);
        slaRoutine = StartCoroutine(newRoutine);
    }

    public void ResetPhoneState()
    {
        isRinging = isOffHook = isOnHold = false;
        
        dialogueSystem?.StopDialogue();
        if (slaRoutine != null) StopCoroutine(slaRoutine); 
        voiceAudioSource?.Stop();
        phoneAudioSource?.Stop();
        
        uiManager?.SetPhoneVisualOnBase();
    }
}