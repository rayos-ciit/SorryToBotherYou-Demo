using System.Collections;
using UnityEngine;
using UnityEngine.UI; // We need this to change the glitch screen's color!

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

    [Header("Reboot Mechanics")]
    public float requiredHoldTimeToReboot = 2.0f;
    public float rebootDuration = 4.0f;

    private bool isHoldingPower = false;
    private float currentHoldTime = 0f;
    private bool isRebooting = false;
    private CallerData currentCaller;

    void Start()
    {
        // Grab the components off the glitch screen so we can manipulate them
        if (glitchScreen != null)
        {
            glitchImage = glitchScreen.GetComponent<Image>();
            glitchRect = glitchScreen.GetComponent<RectTransform>();
            if (glitchRect != null) originalGlitchPos = glitchRect.anchoredPosition;
        }
    }

    public void OnCallStarted(CallerData caller)
    {
        gameObject.SetActive(true);
        
        currentCaller = caller;
        
        if (caller.causesScreenFlicker)
        {
            Debug.Log("The Virus is invading! Monitor is glitching.");
            SetScreenState(glitchScreen);
            
            // Start the violent visual shaking and audio
            if (glitchRoutine == null) glitchRoutine = StartCoroutine(GlitchRoutine());
            if (glitchAudioSource != null) glitchAudioSource.Play();
        }
        else
        {
            SetScreenState(desktopScreen);
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
        if (isHoldingPower && !isRebooting)
        {
            currentHoldTime += Time.deltaTime;
            
            if (currentHoldTime >= requiredHoldTimeToReboot)
            {
                StartCoroutine(RebootSequence());
            }
        }
    }

    public void PointerDownPowerButton()
    {
        if (isRebooting) return; 
        isHoldingPower = true;
    }

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

        yield return new WaitForSeconds(rebootDuration);

        Debug.Log("Reboot Complete. Back to Desktop.");
        SetScreenState(desktopScreen);
        isRebooting = false;
    }

    private void SetScreenState(GameObject screenToShow)
    {
        // 1. Hide every possible screen first
        if (desktopScreen != null) desktopScreen.SetActive(false);
        if (glitchScreen != null) glitchScreen.SetActive(false);
        if (rebootScreen != null) rebootScreen.SetActive(false);

        // 2. Only show the one we actually need right now
        if (screenToShow != null)
        {
            screenToShow.SetActive(true);
        }
    }

    // --- NEW GLITCH BEHAVIORS ---

    private IEnumerator GlitchRoutine()
    {
        while (true) // Runs continuously until stopped
        {
            // 1. Violent Shaking
            if (glitchRect != null)
            {
                float offsetX = Random.Range(-20f, 20f);
                float offsetY = Random.Range(-20f, 20f);
                glitchRect.anchoredPosition = originalGlitchPos + new Vector2(offsetX, offsetY);
            }

            // 2. Harsh Color Inversion
            if (glitchImage != null)
            {
                Color[] harshColors = { Color.red, Color.magenta, Color.green, Color.yellow, Color.white, Color.cyan };
                glitchImage.color = harshColors[Random.Range(0, harshColors.Length)];
            }

            // Flashes super fast (every 0.05 to 0.1 seconds)
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
        
        // Reset the UI back to normal so it isn't permanently broken
        if (glitchRect != null) glitchRect.anchoredPosition = originalGlitchPos;
        if (glitchImage != null) glitchImage.color = Color.white;
        if (glitchAudioSource != null) glitchAudioSource.Stop();
    }
}