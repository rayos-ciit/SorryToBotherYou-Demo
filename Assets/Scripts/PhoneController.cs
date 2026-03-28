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
        
        if (uiManager != null) uiManager.SetPhoneVisualOnBase();

        // 50% chance to mask the Caller ID to "UNKNOWN" if the caller allows it
        if (caller.canMaskCallerID && Random.value > 0.5f)
        {
            if (uiManager != null) uiManager.UpdateCallerID("UNKNOWN", "UNKNOWN");
        }
        else
        {
            if (uiManager != null) uiManager.UpdateCallerID(caller.callerName, caller.callerNumber);
        }

        if (phoneAudioSource != null && caller.ringSFX != null)
        {
            phoneAudioSource.clip = caller.ringSFX;
            phoneAudioSource.loop = true;
            phoneAudioSource.Play();
        }

        // NEW: Start a timer! If the player ignores the phone, what happens?
        if (slaRoutine != null) StopCoroutine(slaRoutine);
        slaRoutine = StartCoroutine(RingTimeoutRoutine(caller.timeLimitToRespond));
    }

    // NEW: Handles what happens if the player never picks up the phone
    private IEnumerator RingTimeoutRoutine(float timeLimit)
    {
        yield return new WaitForSeconds(timeLimit);
        
        if (isRinging) 
        {
            Debug.Log("The phone stopped ringing.");
            isRinging = false;
            if (phoneAudioSource != null) phoneAudioSource.Stop();
            
            // Did they successfully ignore a Disturbance?
            if (currentCaller.requiredAction == CorrectAction.Ignore)
            {
                Debug.Log("Successfully ignored the Disturbance!");
                gameManager.ResolveCall(true);
            }
            else
            {
                // They ignored a normal caller or a Mimic!
                Debug.Log("You missed a call you were supposed to answer! STRIKE!");
                gameManager.ResolveCall(false);
            }
        }
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
        if (phoneAudioSource != null) phoneAudioSource.Stop(); 
        
        // Stop the ringing timeout!
        if (slaRoutine != null) StopCoroutine(slaRoutine); 

        Debug.Log("Picked up the receiver.");
        if (uiManager != null) uiManager.SetPhoneVisualOffHook();

        if (gameManager != null && gameManager.computerController != null)
        {
            gameManager.computerController.OnPhonePickedUp();
        }

        // ---> THE DISTURBANCE PENALTY <---
        if (currentCaller.requiredAction == CorrectAction.Ignore)
        {
            Debug.Log("You answered a Disturbance! STRIKE!");
            if (voiceAudioSource != null && currentCaller.voiceSFX != null)
            {
                voiceAudioSource.PlayOneShot(currentCaller.voiceSFX); 
            }
            gameManager.ResolveCall(false);
            return;
        }

        // ---> THE MIMIC TRAP (UPDATED) <---
        if (currentCaller.typeOfCaller == CallerType.Mimic)
        {
            Debug.Log("It's a MIMIC! You have until the dialogue finishes to hang up!");
            // Notice how we removed the timer and the "return;"? 
            // We want it to fall through to the dialogue system below!
        }

        // Normal Caller Logic (This will now run for Mimics too, typing out their text!)
        if (dialogueSystem != null) dialogueSystem.StartDialogue(currentCaller);
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

        Debug.Log("Player slammed the phone down.");
        if (voiceAudioSource != null) voiceAudioSource.loop = false;

        // Check if slamming the phone was the correct action (Mimic)
        if (currentCaller != null && currentCaller.requiredAction == CorrectAction.HangUp)
        {
            Debug.Log("Successfully disconnected the Mimic!");
            gameManager.ResolveCall(true);
        }
        else
        {
            Debug.Log("You hung up on a valid client! STRIKE!");
            gameManager.ResolveCall(false);
        }
    }

    private void StopCallAudioAndTimer()
    {
        if (dialogueSystem != null) dialogueSystem.StopDialogue();
        
        // Explicitly destroy the timer
        if (slaRoutine != null) 
        { 
            StopCoroutine(slaRoutine); 
            slaRoutine = null; 
        }
        
        if (voiceAudioSource != null) voiceAudioSource.Stop();
        if (phoneAudioSource != null) phoneAudioSource.Stop();
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
        isRinging = false;
        isOffHook = false;
        isOnHold = false;
        
        // Ensure all timers and audio are killed instantly
        StopCallAudioAndTimer(); 
        
        // Swap the art back to the base automatically
        if (uiManager != null)
        {
            uiManager.SetPhoneVisualOnBase();
        }
    }
    
}