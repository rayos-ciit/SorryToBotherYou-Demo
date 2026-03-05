using System.Collections;
using UnityEngine;

public class ComputerController : MonoBehaviour
{
    [Header("Connections")]
    public GameManager gameManager;
    
    [Header("Screen States (Assign GameObjects)")]
    [Tooltip("The normal, idle desktop screen.")]
    public GameObject desktopScreen;
    [Tooltip("The distorted, terrifying screen for The Virus.")]
    public GameObject glitchScreen;
    [Tooltip("The retro DOS text boot sequence.")]
    public GameObject rebootScreen;

    [Header("Reboot Mechanics")]
    [Tooltip("How long the player must hold the power button to force a reset.")]
    public float requiredHoldTimeToReboot = 2.0f;
    [Tooltip("How long the screen stays on the DOS boot sequence before finishing.")]
    public float rebootDuration = 4.0f;

    // Tracking variables
    private bool isHoldingPower = false;
    private float currentHoldTime = 0f;
    private bool isRebooting = false;
    private CallerData currentCaller;

    // The GameManager will call this the exact moment the phone starts ringing
    public void OnCallStarted(CallerData caller)
    {
        currentCaller = caller;
        
        if (caller.causesScreenFlicker)
        {
            Debug.Log("The Virus is invading! Monitor is glitching.");
            SetScreenState(glitchScreen);
        }
        else
        {
            SetScreenState(desktopScreen);
        }
    }

    // Call this from the GameManager when a shift ends or a call is safely resolved
    public void ResetMonitor()
    {
        currentCaller = null;
        if (!isRebooting)
        {
            SetScreenState(desktopScreen);
        }
    }

    void Update()
    {
        // If the player is holding the button, fill up the invisible timer
        if (isHoldingPower && !isRebooting)
        {
            currentHoldTime += Time.deltaTime;
            
            if (currentHoldTime >= requiredHoldTimeToReboot)
            {
                StartCoroutine(RebootSequence());
            }
        }
    }

    // Hook this up to the Power Button's "Pointer Down" Event Trigger
    public void PointerDownPowerButton()
    {
        if (isRebooting) return; // Can't start a reboot if we're already rebooting
        
        isHoldingPower = true;
        Debug.Log("Holding PC Power Button...");
    }

    // Hook this up to the Power Button's "Pointer Up" Event Trigger
    public void PointerUpPowerButton()
    {
        isHoldingPower = false;
        currentHoldTime = 0f; // Reset the timer if they let go too early
    }

    private IEnumerator RebootSequence()
    {
        isRebooting = true;
        isHoldingPower = false;
        currentHoldTime = 0f;

        Debug.Log("System Rebooting...");
        SetScreenState(rebootScreen);

        // Win/Loss Logic: Did they reboot when they were supposed to?
        if (currentCaller != null)
        {
            if (currentCaller.requiredAction == CorrectAction.Reboot)
            {
                Debug.Log("Successfully rebooted to clear The Virus!");
                gameManager.HandleCallResult(true);
            }
            else
            {
                Debug.Log("Rebooted the PC during a normal call! Strike earned.");
                gameManager.HandleCallResult(false);
            }
        }

        // Wait for the retro DOS screen to "load"
        yield return new WaitForSeconds(rebootDuration);

        Debug.Log("Reboot Complete. Back to Desktop.");
        SetScreenState(desktopScreen);
        isRebooting = false;
    }

    // A handy helper to smoothly swap the screen layer GameObjects
    private void SetScreenState(GameObject activeScreen)
    {
        if (desktopScreen != null) desktopScreen.SetActive(false);
        if (glitchScreen != null) glitchScreen.SetActive(false);
        if (rebootScreen != null) rebootScreen.SetActive(false);

        if (activeScreen != null) activeScreen.SetActive(true);
    }
}