using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ComputerController : MonoBehaviour
{
    [Header("Connections")]
    public GameManager gameManager;
    
    [Header("Screen States")]
    public GameObject desktopScreen;
    public GameObject glitchScreen;
    public GameObject rebootScreen;

    [Header("Glitch Effect Settings")]
    public AudioSource glitchAudioSource;
    public AudioClip rebootButtonClip; 
    
    [Tooltip("Drag the specific Image component you want to violently flash colors here!")]
    public Image glitchImage; // ---> NEW: Now explicitly public!
    
    public bool disableMonitorGlitch = false;

    private RectTransform glitchRect;
    private Vector2 originalGlitchPos;
    private Coroutine glitchRoutine;

    [Header("Reboot Mechanics")]
    public float requiredHoldTimeToReboot = 2.0f;
    public float rebootDuration = 4.0f;

    private bool isHoldingPower = false;
    private float currentHoldTime = 0f;
    public bool isRebooting = false;
    private CallerData currentCaller;

    void Awake()
    {
        if (glitchScreen != null)
        {
            // If you forget to assign it in the inspector, it will try to find it in the children!
            if (glitchImage == null) glitchImage = glitchScreen.GetComponentInChildren<Image>();
            
            glitchRect = glitchScreen.GetComponent<RectTransform>();
            if (glitchRect != null) originalGlitchPos = glitchRect.anchoredPosition;
        }
        
        SetScreenState(desktopScreen);
    }

    public void OnCallStarted(CallerData caller)
    {
        gameObject.SetActive(true); 
        currentCaller = caller;
        
        if (!isRebooting) 
        {
            SetScreenState(desktopScreen); 
        }
    }

    public void OnPhonePickedUp()
    {
        if (currentCaller != null && currentCaller.causesScreenFlicker)
        {
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
        
        if (rebootButtonClip != null && glitchAudioSource != null) 
        {
            glitchAudioSource.PlayOneShot(rebootButtonClip);
        }

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

        StopGlitch();
        Debug.Log("System Rebooting...");
        SetScreenState(rebootScreen);

        if (gameManager != null && gameManager.uiManager != null)
        {
            gameManager.uiManager.SetCallerContainerActive(false);
        }

        if (currentCaller != null)
        {
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

        if (gameManager != null && gameManager.uiManager != null)
        {
            gameManager.uiManager.SetCallerContainerActive(true);
            gameManager.uiManager.ClearCallerID();
        }
    }

    private void SetScreenState(GameObject screenToShow)
    {
        if (desktopScreen != null) desktopScreen.SetActive(true);
        if (glitchScreen != null) glitchScreen.SetActive(false);
        if (rebootScreen != null) rebootScreen.SetActive(false);

        if (screenToShow != null && screenToShow != desktopScreen)
        {
            screenToShow.SetActive(true);
        }
    }
    
    private IEnumerator GlitchRoutine()
    {
        while (true)
        {
            if (glitchImage != null)
            {
                Color[] harshColors = { Color.red, Color.magenta, Color.green, Color.yellow, Color.white, Color.cyan };
                glitchImage.color = harshColors[Random.Range(0, harshColors.Length)];
            }
            yield return new WaitForSeconds(Random.Range(0.02f, 0.05f));
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