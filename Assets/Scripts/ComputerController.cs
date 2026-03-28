using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComputerController : MonoBehaviour
{
    [Header("Connections")]
    public GameManager gameManager;
    
    [Header("Screen States (Assign GameObjects)")]
    public GameObject desktopScreen;
    public GameObject glitchScreen;
    public GameObject rebootScreen;

    [Header("Glitch Effect Settings")]
    public AudioSource glitchAudioSource;
    private Image glitchImage;
    private RectTransform glitchRect;
    private Vector2 originalGlitchPos;
    private Coroutine glitchRoutine;
    public bool disableMonitorGlitch = false;

    [Header("Reboot Mechanics")]
    public float requiredHoldTimeToReboot = 2.0f;
    public float rebootDuration = 4.0f;

    private bool isHoldingPower = false;
    private float currentHoldTime = 0f;
    public bool isRebooting = false;
    private CallerData currentCaller;

    void Awake()
    {
        // Grab the components off the glitch screen so we can manipulate them
        if (glitchScreen != null)
        {
            glitchImage = glitchScreen.GetComponent<Image>();
            glitchRect = glitchScreen.GetComponent<RectTransform>();
            if (glitchRect != null) originalGlitchPos = glitchRect.anchoredPosition;
        }
        
        // Always start the game looking at the clean desktop!
        SetScreenState(desktopScreen);
    }

    public void OnCallStarted(CallerData caller)
    {
        gameObject.SetActive(true); 
        currentCaller = caller;
        
        // SAFEGUARD: Only force the normal desktop if the PC isn't currently restarting!
        if (!isRebooting) 
        {
            SetScreenState(desktopScreen); 
        }
    }

    public void OnPhonePickedUp()
    {
        if (currentCaller != null && currentCaller.causesScreenFlicker)
        {
            // SAFEGUARD: Don't trigger the virus glitch if the PC is restarting
            if (!isRebooting)
            {
                Debug.Log("The Virus attacks! Monitor is glitching.");
                SetScreenState(glitchScreen);
                
                if (!disableMonitorGlitch)
                {
                    if (glitchRoutine == null) glitchRoutine = StartCoroutine(GlitchRoutine());
                    if (glitchAudioSource != null) glitchAudioSource.Play();
                }
            }
        }
    }

    public void ResetMonitor()
    {
        currentCaller = null;
        if (!isRebooting)
        {
            StopGlitch();
            SetScreenState(desktopScreen);
        }
    }

    void Update()
    {
        // If the player is holding the power button, count up the timer
        if (isHoldingPower && !isRebooting)
        {
            currentHoldTime += Time.deltaTime;
            
            if (currentHoldTime >= requiredHoldTimeToReboot)
            {
                StartCoroutine(RebootSequence());
            }
        }
    }

    // Call this from your UI Button's "Pointer Down" event
    public void PointerDownPowerButton()
    {
        if (isRebooting) return; 
        isHoldingPower = true;
    }

    // Call this from your UI Button's "Pointer Up" event
    public void PointerUpPowerButton()
    {
        isHoldingPower = false;
        currentHoldTime = 0f; 
    }

    private IEnumerator RebootSequence()
    {
        isRebooting = true;
        isHoldingPower = false;
        currentHoldTime = 0f;

        // Instantly kill the virus effects the moment the reboot triggers
        StopGlitch();
        Debug.Log("System Rebooting...");
        SetScreenState(rebootScreen);

        if (currentCaller != null)
        {
            // NEW V2 LOGIC: Tell GameManager to resolve the call safely!
            if (currentCaller.requiredAction == CorrectAction.Reboot)
            {
                Debug.Log("Successfully rebooted to clear The Virus!");
                gameManager.ResolveCall(true);
            }
            else
            {
                Debug.Log("Rebooted the PC during a normal call! Strike earned.");
                gameManager.ResolveCall(false);
            }
        }

        yield return new WaitForSeconds(rebootDuration);

        Debug.Log("Reboot Complete. Back to Desktop.");
        SetScreenState(desktopScreen);
        isRebooting = false;
    }

    private void SetScreenState(GameObject screenToShow)
    {
        // 1. ALWAYS keep the monitor frame / desktop turned ON
        if (desktopScreen != null) desktopScreen.SetActive(true);

        // 2. Hide the conditional screens 
        if (glitchScreen != null) glitchScreen.SetActive(false);
        if (rebootScreen != null) rebootScreen.SetActive(false);

        // 3. If we need to show a special screen (like Glitch or Reboot), turn it on over the desktop!
        if (screenToShow != null && screenToShow != desktopScreen)
        {
            screenToShow.SetActive(true);
        }
    }

    private IEnumerator GlitchRoutine()
    {
        while (true)
        {
            if (glitchRect != null)
            {
                float offsetX = Random.Range(-20f, 20f);
                float offsetY = Random.Range(-20f, 20f);
                glitchRect.anchoredPosition = originalGlitchPos + new Vector2(offsetX, offsetY);
            }

            if (glitchImage != null)
            {
                Color[] harshColors = { Color.red, Color.magenta, Color.green, Color.yellow, Color.white, Color.cyan };
                glitchImage.color = harshColors[Random.Range(0, harshColors.Length)];
            }

            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));
        }
    }

    private void StopGlitch()
    {
        if (glitchRoutine != null)
        {
            StopCoroutine(glitchRoutine);
            glitchRoutine = null;
        }
        
        if (glitchRect != null) glitchRect.anchoredPosition = originalGlitchPos;
        if (glitchImage != null) glitchImage.color = Color.white;
        if (glitchAudioSource != null) glitchAudioSource.Stop();
    }
}