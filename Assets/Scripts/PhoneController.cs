using System.Collections;
using UnityEngine;

public class PhoneController : MonoBehaviour
{
    [Header("Connections")]
    public GameManager gameManager;
    public AudioSource phoneAudioSource;
    
    [Header("Hold Music")]
    public AudioClip holdMusicClip;

    // State tracking
    private bool isRinging = false;
    private bool isOffHook = false;
    private bool isOnHold = false;
    private CallerData currentCaller;

    // This is called by the GameManager when a new call is pulled from the deck
    public void StartRinging(CallerData caller)
    {
        currentCaller = caller;
        isRinging = true;
        isOffHook = false;
        isOnHold = false;

        // Play the specific ring or distortion for this caller
        if (caller.ringSFX != null)
        {
            phoneAudioSource.clip = caller.ringSFX;
            phoneAudioSource.loop = true;
            phoneAudioSource.Play();
        }

        Debug.Log("Phone is now ringing...");
    }

    // Tied to the physical "Receiver" being clicked
    public void PickUpPhone()
    {
        if (!isRinging) return; // Can't pick up a phone that isn't ringing!

        isRinging = false;
        isOffHook = true;
        phoneAudioSource.Stop(); 

        Debug.Log("Player picked up the receiver.");

        // INSTANT FAIL CHECK: Did they pick up The Disturbance?
        if (currentCaller.requiredAction == CorrectAction.Ignore)
        {
            Debug.Log("Lethal sensory overload triggered!");
            gameManager.HandleCallResult(false);
            return;
        }

        // TODO: Trigger the Impatient timer check here if requiresQuickResponse is true
    }

    // Tied to the "Talk" UI button
    public void PressTalk()
    {
        if (!isOffHook || isOnHold) return;

        Debug.Log("Player chose to Talk.");

        if (currentCaller.requiredAction == CorrectAction.Talk)
        {
            // TODO: Pass currentCaller.dialogueLines to the UI text box system
            Debug.Log("Initiating dialogue...");
            gameManager.HandleCallResult(true); 
        }
        else
        {
            // E.g., Talking to The Listener or The Mimic
            gameManager.HandleCallResult(false);
        }
    }

    // Tied to the "Hold" UI button
    public void PressHold()
    {
        if (!isOffHook) return;

        isOnHold = true;
        phoneAudioSource.clip = holdMusicClip;
        phoneAudioSource.loop = true;
        phoneAudioSource.Play();

        Debug.Log("Player put the caller on Hold.");

        if (currentCaller.requiredAction == CorrectAction.Hold)
        {
            // For The Listener: We wait a few seconds, then they hang up
            StartCoroutine(WaitToResolveHold());
        }
        else
        {
            // Putting anyone else on hold (like The Impatient) is a strike
            gameManager.HandleCallResult(false);
        }
    }

    // Tied to the "Hang Up / Put Down" UI button
    public void HangUpPhone()
    {
        if (!isOffHook) return;

        isOffHook = false;
        isOnHold = false;
        phoneAudioSource.Stop();

        Debug.Log("Player hung up the phone.");

        if (currentCaller.requiredAction == CorrectAction.HangUp)
        {
            gameManager.HandleCallResult(true);
        }
        else
        {
            gameManager.HandleCallResult(false);
        }
    }

    // Handles the agonizing wait for The Listener to leave while on hold
    private IEnumerator WaitToResolveHold()
    {
        Debug.Log("Waiting for the entity to leave the line...");
        yield return new WaitForSeconds(Random.Range(3f, 7f)); // Random wait time
        
        Debug.Log("Click. The entity hung up.");
        phoneAudioSource.Stop();
        isOnHold = false;
        
        gameManager.HandleCallResult(true);
    }
}