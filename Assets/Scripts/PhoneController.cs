using System.Collections;
using UnityEngine;

public class PhoneController : MonoBehaviour
{
    [Header("Core Systems")]
    public GameManager gameManager;
    public DialogueSystem dialogueSystem;
    public UIManager uiManager;

    [Header("Audio Sources")]
    public AudioSource phoneAudioSource;
    public AudioSource voiceAudioSource;
    public AudioSource extraAmbientSource;

    [Header("Phone Foley & Music")]
    public AudioClip pickUpClip;
    public AudioClip hangUpClip;
    public AudioClip holdMusicClip;
    public AudioClip holdButtonClip;
    public AudioClip talkButtonClip;

    [Header("Shift Settings")]
    public float slaTimeLimit = 50f;
    public float holdTimeLimit = 15f;

    [HideInInspector] public bool isRinging = false;
    [HideInInspector] public bool isOffHook = false;
    [HideInInspector] public bool isOnHold = false;

    private CallerData currentCaller;
    private Coroutine slaRoutine;

    public void StartRinging(CallerData caller)
    {
        currentCaller = caller;
        isRinging = true;

        uiManager?.SetPhoneVisualOnBase();

        // 1. Scramble the UI if necessary
        string cName = (caller.canMaskCallerID && Random.value > 0.5f) ? "UNKNOWN" : caller.callerName;
        string cNum  = (caller.canMaskCallerID && Random.value > 0.5f) ? "UNKNOWN" : caller.callerNumber;
        uiManager?.UpdateCallerID(cName, cNum);

        // 2. Play the Specific Ringtone
        if (caller.ringSFX != null && phoneAudioSource != null)
        {
            phoneAudioSource.clip = caller.ringSFX;
            phoneAudioSource.loop = true;
            phoneAudioSource.Play();
        }

        // 3. Start Environmental Dread (if applicable)
        if (caller.ambientSFX != null && extraAmbientSource != null)
        {
            extraAmbientSource.clip = caller.ambientSFX;
            extraAmbientSource.loop = true;
            extraAmbientSource.Play();
        }

        // 4. Start the Ring Timer (Use their specific limit, or fallback to the global SLA limit)
        float ringTime = caller.timeLimitToRespond > 0 ? caller.timeLimitToRespond : slaTimeLimit;
        RestartSLA(RingTimeoutRoutine(ringTime));
    }

    public void PickUpPhone()
    {
        if (!isRinging) return;

        isRinging = false;
        isOffHook = true;
        
        // 1. Stop the ring and play the plastic pick-up click
        phoneAudioSource?.Stop();
        if (pickUpClip != null && phoneAudioSource != null) phoneAudioSource.PlayOneShot(pickUpClip);

        uiManager?.SetPhoneVisualOffHook();
        RestartSLA(SLATimerRoutine(slaTimeLimit));

        // 2. The Disturbance Jumpscare Trap!
        if (currentCaller != null && currentCaller.typeOfCaller == CallerType.Disturbance)
        {
            Debug.Log("Picked up a Disturbance! Strike!");
            if (currentCaller.voiceSFX != null && voiceAudioSource != null)
            {
                voiceAudioSource.PlayOneShot(currentCaller.voiceSFX);
            }
            gameManager?.ResolveCall(false);
            return;
        }

        // 3. If it's a normal call, start the text
        if (currentCaller != null)
        {
            dialogueSystem?.StartDialogue(currentCaller);
        }
    }

    public void HoldPhone()
    {
        if (!isOffHook || isOnHold) return;

        // Play the physical button sound
        if (holdButtonClip != null && phoneAudioSource != null) phoneAudioSource.PlayOneShot(holdButtonClip);

        Debug.Log("Call placed on hold.");
        isOnHold = true;
        dialogueSystem?.StopDialogue();

        // Start the elevator music over the receiver
        if (holdMusicClip != null && phoneAudioSource != null)
        {
            phoneAudioSource.clip = holdMusicClip;
            phoneAudioSource.loop = true;
            phoneAudioSource.Play();
        }

        RestartSLA(WaitToResolveHold());
    }

    public void PlayTalkButtonSound()
    {
        // Tied strictly to the physical UI button!
        if (talkButtonClip != null && phoneAudioSource != null) phoneAudioSource.PlayOneShot(talkButtonClip);
    }

    public void HangUpPhone()
    {
        if (!isOffHook) return;

        Debug.Log("Player slammed the phone down.");
        if (voiceAudioSource != null) voiceAudioSource.loop = false;

        bool success = currentCaller != null && currentCaller.requiredAction == CorrectAction.HangUp;
        Debug.Log(success ? "Successfully disconnected!" : "Hung up on valid client! STRIKE!");
        
        // The GameManager will officially trigger the audio reset when resolving this!
        gameManager?.ResolveCall(success);
    }

    public void ResetPhoneState()
    {
        // 1. Remember if the player was holding the phone
        bool wasOffHook = isOffHook;

        // 2. Reset the logic
        isRinging = isOffHook = isOnHold = false;
        dialogueSystem?.StopDialogue();
        if (slaRoutine != null) StopCoroutine(slaRoutine);
        
        // 3. MURDER ALL LOOPING AUDIO!
        voiceAudioSource?.Stop();
        phoneAudioSource?.Stop();
        extraAmbientSource?.Stop();

        // 4. Safely play the plastic hang-up click ONLY if it was held previously
        if (wasOffHook && hangUpClip != null && phoneAudioSource != null)
        {
            phoneAudioSource.PlayOneShot(hangUpClip);
        }

        // 5. Restore visuals
        uiManager?.SetPhoneVisualOnBase();
    }

    // --- COROUTINE TIMERS --- //

    private void RestartSLA(IEnumerator routine)
    {
        if (slaRoutine != null) StopCoroutine(slaRoutine);
        slaRoutine = StartCoroutine(routine);
    }

    private IEnumerator RingTimeoutRoutine(float limit)
    {
        yield return new WaitForSeconds(limit);
        if (isRinging)
        {
            Debug.Log("Missed call! STRIKE!");
            gameManager?.ResolveCall(false);
        }
    }

    private IEnumerator SLATimerRoutine(float limit)
    {
        yield return new WaitForSeconds(limit);
        if (isOffHook)
        {
            Debug.Log("Took too long to respond! SLA Breach! STRIKE!");
            gameManager?.ResolveCall(false);
        }
    }

    private IEnumerator WaitToResolveHold()
    {
        yield return new WaitForSeconds(holdTimeLimit);

        bool success = currentCaller != null && currentCaller.requiredAction == CorrectAction.Hold;
        Debug.Log(success ? "Successfully kept Listener on hold." : "Held a valid client! STRIKE!");
        gameManager?.ResolveCall(success);
    }
}