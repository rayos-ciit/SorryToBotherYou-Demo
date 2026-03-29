using UnityEngine;
using TMPro; 
using System.Collections;

public class UIManager : MonoBehaviour
{
    private Coroutine scrambleRoutine;
    
    [Header("Phone Visuals")]
    public UnityEngine.UI.Image phoneImageComponent; 
    public Sprite phoneOnBaseSprite; 
    public Sprite phoneOffBaseSprite; 
    
    [Header("Mimic Sabotage")]
    [Tooltip("How many seconds the Caller ID scrambles before revealing the true text.")]
    public float scrambleDuration = 0.8f;
    
    [Header("Debug Settings")]
    public bool disableCallerIDScrambler = false; 
    
    [Header("Caller ID Screen")]
    public TMP_Text callerNameText;
    public TMP_Text callerNumberText;
    public string defaultIdleText = "SYSTEM IDLE...";
    
    [Header("Pre-Shift Setup")]
    public GameObject startShiftButton;   
    public GameObject callerIDContainer;  

    [Header("Shift Clock")]
    public TMP_Text digitalClockText;
    
    // ---> THE NEW STRIKE SYSTEM <---
    [Header("Blackout Strike Effect")]
    public AudioSource strikeAudioSource; 
    public AudioClip deathBellClip;       
    public AudioClip heartbeatClip;       
    public UnityEngine.UI.Image blackoutImage; 
    public UnityEngine.UI.Image monsterPulseImage; 
    
    [Tooltip("How fast the screen fades to black and back. Higher number = faster fade.")]
    public float fadeSpeed = 5f;
    [Tooltip("How many seconds the screen stays completely black at the peak of the flash.")]
    public float peakHoldTime = 0.2f; // ---> NEW: Controls how long the monster lingers!
    [Tooltip("How many times the screen flashes black per strike.")]
    public int blinkCount = 2;
    
    [Header("Victory Sequence")]
    public AudioSource victoryAlarmAudio;
    
    [Header("Game Over Jumpscare")]
    public AudioSource jumpscareAudio;
    public GameObject bossFaceImage; 

    [Header("Game Loop Screens")]
    public GameObject gameOverScreen;
    public GameObject shiftCompleteScreen;
    public TMP_Text shiftCompleteText;

    void Start()
    {
        UpdateStrikes(0);
        UpdateClock(0f);
        if (blackoutImage != null) blackoutImage.gameObject.SetActive(false);

        if (startShiftButton != null) startShiftButton.SetActive(true);
        if (callerIDContainer != null) callerIDContainer.SetActive(false); 
    }

    public void ActivateShiftUI()
    {
        if (startShiftButton != null) startShiftButton.SetActive(false);
        if (callerIDContainer != null) callerIDContainer.SetActive(true);
        ClearCallerID(); 
    }
    
    public void SetCallerContainerActive(bool isActive)
    {
        if (callerIDContainer != null) callerIDContainer.SetActive(isActive);
    }

    public void UpdateCallerID(string name, string number)
    {
        if (disableCallerIDScrambler)
        {
            if (callerNameText != null) callerNameText.text = name;
            if (callerNumberText != null) callerNumberText.text = number;
            return; 
        }

        if (scrambleRoutine != null) StopCoroutine(scrambleRoutine);
        scrambleRoutine = StartCoroutine(ScrambleTextRoutine(name, number));
    }
    
    public void ClearCallerID()
    {
        if (callerNameText != null) callerNameText.text = defaultIdleText;
        if (callerNumberText != null) callerNumberText.text = "";
    }

    public void UpdateClock(float shiftPercentage)
    {
        if (digitalClockText != null)
        {
            float totalMinutesPassed = shiftPercentage * 360f;
            int hours = Mathf.FloorToInt(totalMinutesPassed / 60f);
            int minutes = Mathf.FloorToInt(totalMinutesPassed % 60f);
            int displayHours = (hours == 0) ? 12 : hours;
            
            digitalClockText.text = string.Format("{0}:{1:00} AM", displayHours, minutes);
        }
    }

    // ---> REWRITTEN: Flashes the specific monster and plays the two audios! <---
    public void UpdateStrikes(int currentStrikes, CallerData caller = null)
    {
        if (currentStrikes > 0)
        {
            if (strikeAudioSource != null)
            {
                if (deathBellClip != null) strikeAudioSource.PlayOneShot(deathBellClip);
                if (heartbeatClip != null) strikeAudioSource.PlayOneShot(heartbeatClip);
            }
            
            if (blackoutImage != null) StartCoroutine(BlinkEffect(caller));
        }
    }
    
    // REWRITTEN: The Monster Image fades flawlessly with the black screen
    // REWRITTEN: The Monster Image fades flawlessly with the black screen
    // REWRITTEN: Added customizable hold times and fade speeds
    private IEnumerator BlinkEffect(CallerData caller)
    {
        blackoutImage.gameObject.SetActive(true);
        
        bool hasMonster = (monsterPulseImage != null && caller != null && caller.monsterSprite != null);

        if (hasMonster)
        {
            monsterPulseImage.sprite = caller.monsterSprite;
            monsterPulseImage.gameObject.SetActive(true);
        }

        // Force both images to start completely invisible (Alpha = 0)
        Color blackoutColor = blackoutImage.color;
        blackoutColor.a = 0f; 
        blackoutImage.color = blackoutColor;

        Color baseMonsterColor = hasMonster ? monsterPulseImage.color : Color.clear;
        if (hasMonster) 
        {
            baseMonsterColor.a = 0f;
            monsterPulseImage.color = baseMonsterColor;
        }

        for (int i = 0; i < blinkCount; i++)
        {
            // Fade IN
            while (blackoutColor.a < 0.9f) 
            {
                blackoutColor.a += Time.deltaTime * fadeSpeed; // Using the new fadeSpeed!
                blackoutImage.color = blackoutColor;

                if (hasMonster) 
                {
                    baseMonsterColor.a = blackoutColor.a; 
                    monsterPulseImage.color = baseMonsterColor;
                }
                
                yield return null;
            }
            
            // ---> NEW: Hold the blackout at its peak before fading away! <---
            yield return new WaitForSeconds(peakHoldTime);
            
            // Fade OUT
            while (blackoutColor.a > 0f)
            {
                blackoutColor.a -= Time.deltaTime * fadeSpeed; // Using the new fadeSpeed!
                blackoutImage.color = blackoutColor;

                if (hasMonster) 
                {
                    baseMonsterColor.a = blackoutColor.a; 
                    monsterPulseImage.color = baseMonsterColor;
                }
                
                yield return null;
            }
        }
        
        // Reset everything safely to invisible when finished
        blackoutColor.a = 0f;
        blackoutImage.color = blackoutColor;
        blackoutImage.gameObject.SetActive(false);
        
        if (hasMonster) 
        {
            baseMonsterColor.a = 0f;
            monsterPulseImage.color = baseMonsterColor;
            monsterPulseImage.gameObject.SetActive(false);
        }

        // Murder the heartbeat and bell audio exactly when the visual ends!
        if (strikeAudioSource != null)
        {
            strikeAudioSource.Stop();
        }
    }
    
    public void ShowGameOver()
    {
        StartCoroutine(JumpscareRoutine());
    }

    private IEnumerator JumpscareRoutine()
    {
        if (blackoutImage != null)
        {
            blackoutImage.gameObject.SetActive(true);
            Color c = blackoutImage.color;
            c.a = 1f; 
            blackoutImage.color = c;
        }

        yield return new WaitForSeconds(0.25f);

        if (jumpscareAudio != null) jumpscareAudio.Play();
        if (bossFaceImage != null) bossFaceImage.SetActive(true);

        yield return new WaitForSeconds(2.0f);

        if (bossFaceImage != null) bossFaceImage.SetActive(false);
        if (gameOverScreen != null) gameOverScreen.SetActive(true);
    }
    
    public void ShowShiftComplete(int dayCompleted)
    {
        StartCoroutine(VictorySequenceRoutine(dayCompleted));
    }

    private IEnumerator VictorySequenceRoutine(int dayCompleted)
    {
        if (blackoutImage != null)
        {
            blackoutImage.gameObject.SetActive(true);
            Color c = blackoutImage.color;
            c.a = 0f;
            
            while (c.a < 1f)
            {
                c.a += Time.deltaTime * 0.8f; 
                blackoutImage.color = c;
                yield return null;
            }
        }

        yield return new WaitForSeconds(1.0f);

        if (victoryAlarmAudio != null) victoryAlarmAudio.Play();

        if (shiftCompleteScreen != null) shiftCompleteScreen.SetActive(true);
        if (shiftCompleteText != null) shiftCompleteText.text = $"6:00 AM\nSHIFT SURVIVED.";
    }
    
    private IEnumerator ScrambleTextRoutine(string finalName, string finalNumber)
    {
        string glyphs = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
        float timer = 0f;

        while (timer < scrambleDuration)
        {
            timer += Time.deltaTime;

            string scrambledName = "";
            string scrambledNumber = "";

            for (int i = 0; i < finalName.Length; i++)
            {
                if (finalName[i] == ' ') scrambledName += " "; 
                else scrambledName += glyphs[Random.Range(0, glyphs.Length)];
            }

            for (int i = 0; i < finalNumber.Length; i++)
            {
                if (finalNumber[i] == ' ' || finalNumber[i] == '-') scrambledNumber += finalNumber[i]; 
                else scrambledNumber += glyphs[Random.Range(0, glyphs.Length)];
            }

            if (callerNameText != null) callerNameText.text = scrambledName;
            if (callerNumberText != null) callerNumberText.text = scrambledNumber;

            yield return new WaitForSeconds(0.05f); 
        }

        if (callerNameText != null) callerNameText.text = finalName;
        if (callerNumberText != null) callerNumberText.text = finalNumber;
    }
    
    public void SetPhoneVisualOffHook()
    {
        if (phoneImageComponent != null && phoneOffBaseSprite != null)
        {
            phoneImageComponent.sprite = phoneOffBaseSprite;
        }
    }

    public void SetPhoneVisualOnBase()
    {
        if (phoneImageComponent != null && phoneOnBaseSprite != null)
        {
            phoneImageComponent.sprite = phoneOnBaseSprite;
        }
    }
    
    public void UpdateQuota(int completed, int total)
    {
        Debug.Log($"[SHIFT STATUS] Quota Update: {completed} / {total} calls completed.");
    }
    
    // ---> NEW: Simple Scene Navigation for your Victory/Game Over Screens <---
    public void RestartShift()
    {
        // Reloads the current scene from scratch
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Exiting Game...");
        Application.Quit();
    }
}