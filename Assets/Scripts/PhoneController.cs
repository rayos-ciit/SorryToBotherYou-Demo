using System.Collections;
using UnityEngine;

public class PhoneController : MonoBehaviour
{
    [Header("Connections")]
    public GameManager gameManager;
    public UIManager uiManager;
    public AudioSource phoneAudioSource; // For the ringing
    public AudioSource voiceAudioSource; // For the voice/beeps when picked up
    public DialogueSystem dialogueSystem;
    
    [Header("Hold Music")]
    public AudioClip holdMusicClip;

    [Header("Corporate Rules")]
    [Tooltip("How many seconds the player has to resolve a call before the Boss gives a strike.")]
    public float slaTimeLimit = 12.0f; 

    // State tracking
    private bool isRinging = false;
    private bool isOffHook = false;
    private bool isOnHold = false;
    public CallerData currentCaller;
    private Coroutine slaRoutine;

    public void StartRinging(CallerData caller)
    {
        currentCaller = caller;
        isRinging = true;
        isOffHook = false;
        isOnHold = false;

        // Visual Sabotage: The "False Positive" Caller ID
        if (caller.canMaskCallerID && Random.value > 0.5f)
        {
            Debug.Log("UI Sabotage: Caller ID reads UNKNOWN - UNKNOWN");
            uiManager.UpdateCallerID("UNKNOWN", "UNKNOWN");
        }
        else
        {
            Debug.Log($"Caller ID reads: {caller.callerName} - {caller.callerNumber}");
            uiManager.UpdateCallerID(caller.callerName, caller.callerNumber);
        }

        if (caller.ringSFX != null)
        {
            phoneAudioSource.clip = caller.ringSFX;
            phoneAudioSource.loop = true;
            phoneAudioSource.Play();
        }

        // Start the SLA Timer! (Shorter if it's The Impatient)
        float timer = caller.requiresQuickResponse ? caller.timeLimitToRespond : slaTimeLimit;
        slaRoutine = StartCoroutine(SLATimerRoutine(timer));
    }

    private IEnumerator SLATimerRoutine(float timeLimit)
    {
        yield return new WaitForSeconds(timeLimit);

        // If the timer runs out and the call hasn't been resolved...
        if (isRinging || isOffHook) 
        {
            Debug.Log("SLA TIMER EXPIRED!");
            phoneAudioSource.Stop();
            voiceAudioSource.Stop();
            
            // Winning Condition for The Disturbance: Doing nothing until the timer ends!
            if (currentCaller.requiredAction == CorrectAction.Ignore && !isOffHook)
            {
                Debug.Log("Success: The Disturbance gave up and left.");
                isRinging = false;
                gameManager.ResolveCall(true);
            }
            else
            {
                // Taking too long on anyone else earns a strike
                Debug.Log("Boss: You took too long to handle that call!");
                isRinging = false;
                isOffHook = false;
                gameManager.ResolveCall(false);
            }
        } 
        uiManager.ClearCallerID();
    }

    public void PickUpPhone()
    {
        if (!isRinging) return;

        isRinging = false;
        isOffHook = true;
        phoneAudioSource.Stop(); 

        Debug.Log("Picked up the receiver.");
        
        uiManager.SetPhoneVisualOffHook();

        // INSTANT FAIL: Picking up The Disturbance
        if (currentCaller.requiredAction == CorrectAction.Ignore)
        {
            if(slaRoutine != null) StopCoroutine(slaRoutine);
            Debug.Log("Lethal sensory overload triggered! You picked up The Disturbance.");
            gameManager.ResolveCall(false);
            return;
        }

        // Audio Subtlety: Play the voice/breathing so the player can investigate
        if (currentCaller.voiceSFX != null)
        {
            voiceAudioSource.clip = currentCaller.voiceSFX;
            voiceAudioSource.loop = true;
            voiceAudioSource.Play();
        }
        
        // Note: The SLA Timer keeps ticking in the background! They must act fast.
    }

    public void PressTalk()
    {
        if (!isOffHook || isOnHold) return;
        
        Debug.Log("Chose to Talk.");

        if (!dialogueSystem.dialogueBoxUI.activeInHierarchy)
        {
            // First click: Stop timers and start the text
            if(slaRoutine != null) StopCoroutine(slaRoutine);
            voiceAudioSource.Stop();
            phoneAudioSource.Stop();
            
            dialogueSystem.StartDialogue(currentCaller);
        }
        else
        {
            // Subsequent clicks: Advance the conversation
            dialogueSystem.DisplayNextLine();
        }
    }

    public void PressHold()
    {
        if (!isOffHook) return;
        
        StopCallAudioAndTimer();
        isOnHold = true;
        phoneAudioSource.clip = holdMusicClip;
        phoneAudioSource.loop = true;
        phoneAudioSource.Play();

        Debug.Log("Put the caller on Hold.");

        if (currentCaller.requiredAction == CorrectAction.Hold)
        {
            StartCoroutine(WaitToResolveHold());
        }
        else
        {
            gameManager.ResolveCall(false);
        }
    }

    public void HangUpPhone()
    {
        if (!isOffHook) return;
        
        StopCallAudioAndTimer();
        isOffHook = false;
        isOnHold = false;

        Debug.Log("Hung up the phone.");
        
        uiManager.SetPhoneVisualOnBase();

        // This is where the player successfully defeats The Mimic!
        if (currentCaller.requiredAction == CorrectAction.HangUp)
        {
            Debug.Log("Correctly slammed the phone down on the entity!");
            gameManager.ResolveCall(true);
        }
        else
        {
            Debug.Log("Hung up on a normal client! Strike earned.");
            gameManager.ResolveCall(false);
        }
        uiManager.ClearCallerID();
    }

    private void StopCallAudioAndTimer()
    {
        dialogueSystem.StopDialogue();
        if(slaRoutine != null) StopCoroutine(slaRoutine);
        voiceAudioSource.Stop();
        phoneAudioSource.Stop();
    }

    private IEnumerator WaitToResolveHold()
    {
        Debug.Log("Waiting for the entity to leave the line...");
        yield return new WaitForSeconds(Random.Range(3f, 7f));
        
        Debug.Log("Click. The entity hung up.");
        phoneAudioSource.Stop();
        isOnHold = false;
        gameManager.ResolveCall(true);
    }
    
    // Safely puts the phone down automatically without triggering penalty logic
    public void ResetPhoneState()
    {
        isOffHook = false;
        isOnHold = false;
        
        // Ensure all ringing/talking audio is completely stopped
        StopCallAudioAndTimer(); 
        
        // Swap the art back to the base!
        if (uiManager != null)
        {
            uiManager.SetPhoneVisualOnBase();
        }
    }
}