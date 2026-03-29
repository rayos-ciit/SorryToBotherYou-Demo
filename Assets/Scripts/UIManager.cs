using UnityEngine;
using TMPro; 
using System.Collections; // Needed for Coroutines!

public class UIManager : MonoBehaviour
{
    private Coroutine scrambleRoutine;
    
    [Header("Phone Visuals")]
    public UnityEngine.UI.Image phoneImageComponent; // The UI Image object in your Canvas
    public Sprite phoneOnBaseSprite; // The art of the phone sitting normally
    public Sprite phoneOffBaseSprite; // The art of just the wire/base
    
    [Header("Mimic Sabotage")]
    [Tooltip("How many seconds the Caller ID scrambles before revealing the true text.")]
    public float scrambleDuration = 0.8f;
    
    [Header("Debug Settings")]
    public bool disableCallerIDScrambler = false; // Check this to skip the animation
    
    [Header("Caller ID Screen")]
    public TMP_Text callerNameText;
    public TMP_Text callerNumberText;
    public string defaultIdleText = "SYSTEM IDLE...";
    
    [Header("Pre-Shift Setup")]
    public GameObject startShiftButton;   // Drag your Monitor's Start Button here
    public GameObject callerIDContainer;  // Drag the Parent Object of your Name/Number text here

    [Header("Shift Clock")]
    public TMP_Text digitalClockText;
    
    [Header("Blackout Strike Effect")]
    public AudioSource strikeAudioSource;
    public AudioClip deathBellClip; // ---> NEW: The Bell
    public AudioClip heartbeatClip; // ---> NEW: The Heartbeat
    public UnityEngine.UI.Image blackoutImage; // The black screen
    [Tooltip("How fast the eyes fade to black and back.")]
    public float blinkSpeed = 5f;
    [Tooltip("How many times the screen flashes black per strike.")]
    public int blinkCount = 2;
    public UnityEngine.UI.Image monsterPulseImage; // ---> NEW: The Monster that flashes!
    
    [Header("Victory Sequence")]
    public AudioSource victoryAlarmAudio;
    
    [Header("Game Over Jumpscare")]
    public AudioSource jumpscareAudio;
    public GameObject bossFaceImage; // The terrifying pop-up

    [Header("Game Loop Screens")]
    public GameObject gameOverScreen;
    public GameObject shiftCompleteScreen;
    public TMP_Text shiftCompleteText;

    void Start()
    {
        UpdateStrikes(0);
        UpdateClock(0f);
        if (blackoutImage != null) blackoutImage.gameObject.SetActive(false);

        // NEW: Setup the Pre-Shift Monitor State
        if (startShiftButton != null) startShiftButton.SetActive(true);
        if (callerIDContainer != null) callerIDContainer.SetActive(false); // Hide the Caller ID
    }

    // NEW: The Button will call this method to swap the UI!
    public void ActivateShiftUI()
    {
        if (startShiftButton != null) startShiftButton.SetActive(false);
        if (callerIDContainer != null) callerIDContainer.SetActive(true);
        ClearCallerID(); // Set the text to "SYSTEM IDLE..." 
    }
    
    public void SetCallerContainerActive(bool isActive)
    {
        if (callerIDContainer != null) callerIDContainer.SetActive(isActive);
    }

    public void UpdateCallerID(string name, string number)
    {
        // NEW: If debugging is on, instantly show the real name/number and skip the animation
        if (disableCallerIDScrambler)
        {
            if (callerNameText != null) callerNameText.text = name;
            if (callerNumberText != null) callerNumberText.text = number;
            return; // Stop the code here so the coroutine doesn't run
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
            // A full shift is 6 hours long (from 12:00 AM to 6:00 AM)
            // 6 hours = 360 in-game minutes
            float totalMinutesPassed = shiftPercentage * 360f;
            
            // Figure out how many hours and minutes have passed
            int hours = Mathf.FloorToInt(totalMinutesPassed / 60f);
            int minutes = Mathf.FloorToInt(totalMinutesPassed % 60f);

            // Standard 12-hour format: If 0 hours have passed, it is 12:XX AM
            int displayHours = (hours == 0) ? 12 : hours;
            
            // The "00" format ensures minutes always show two digits (e.g., 12:05 instead of 12:5)
            digitalClockText.text = string.Format("{0}:{1:00} AM", displayHours, minutes);
        }
    }

   public void UpdateStrikes(int currentStrikes, CallerData caller = null)
    {
        if (currentStrikes > 0)
        {
            // Play the two distinct scary noises
            if (strikeAudioSource != null)
            {
                if (deathBellClip != null) strikeAudioSource.PlayOneShot(deathBellClip);
                if (heartbeatClip != null) strikeAudioSource.PlayOneShot(heartbeatClip);
            }
            
            if (blackoutImage != null) StartCoroutine(BlinkEffect(caller));
        }
    }

    // ---> REWRITTEN: Flashes the specific monster on top of the black screen <---
    private IEnumerator BlinkEffect(CallerData caller)
    {
        blackoutImage.gameObject.SetActive(true);
        
        // Prep the monster image if this caller has one!
        if (monsterPulseImage != null && caller != null && caller.monsterSprite != null)
        {
            monsterPulseImage.sprite = caller.monsterSprite;
            monsterPulseImage.gameObject.SetActive(true);
        }

        Color blackoutColor = blackoutImage.color;
        Color monsterColor = monsterPulseImage != null ? monsterPulseImage.color : Color.clear;

        for (int i = 0; i < blinkCount; i++)
        {
            // Fade IN
            while (blackoutColor.a < 0.9f) 
            {
                float speed = Time.deltaTime * blinkSpeed;
                blackoutColor.a += speed;
                monsterColor.a += speed; // Fade the monster in with the black screen!
                
                blackoutImage.color = blackoutColor;
                if (monsterPulseImage != null) monsterPulseImage.color = monsterColor;
                
                yield return null;
            }
            
            // Fade OUT
            while (blackoutColor.a > 0f)
            {
                float speed = Time.deltaTime * blinkSpeed;
                blackoutColor.a -= speed;
                monsterColor.a -= speed;
                
                blackoutImage.color = blackoutColor;
                if (monsterPulseImage != null) monsterPulseImage.color = monsterColor;
                
                yield return null;
            }
        }
        
        // Reset everything to invisible
        blackoutColor.a = 0f;
        monsterColor.a = 0f;
        blackoutImage.color = blackoutColor;
        blackoutImage.gameObject.SetActive(false);
        
        if (monsterPulseImage != null) 
        {
            monsterPulseImage.color = monsterColor;
            monsterPulseImage.gameObject.SetActive(false);
        }
    }
    
    public void ShowGameOver()
    {
        StartCoroutine(JumpscareRoutine());
    }

    private IEnumerator JumpscareRoutine()
    {
        // 1. Instantly snap to pitch black so the player is disoriented
        if (blackoutImage != null)
        {
            blackoutImage.gameObject.SetActive(true);
            Color c = blackoutImage.color;
            c.a = 1f; // Instantly 100% visible (pitch black)
            blackoutImage.color = c;
        }

        // Wait a tiny fraction of a second in the dark to build pure dread
        yield return new WaitForSeconds(0.25f);

        // 2. BOOM! Play the terrifying scream and flash the Boss face
        if (jumpscareAudio != null) jumpscareAudio.Play();
        if (bossFaceImage != null) bossFaceImage.SetActive(true);

        // 3. Keep the face on screen for the scariest 2 seconds of their life
        yield return new WaitForSeconds(2.0f);

        // 4. Hide the face, and fade into the standard Game Over menu so they can restart
        if (bossFaceImage != null) bossFaceImage.SetActive(false);
        if (gameOverScreen != null) gameOverScreen.SetActive(true);
    }
    

    public void ShowShiftComplete(int dayCompleted)
    {
        StartCoroutine(VictorySequenceRoutine(dayCompleted));
    }

    private IEnumerator VictorySequenceRoutine(int dayCompleted)
    {
        // 1. Smoothly fade the entire screen to black
        if (blackoutImage != null)
        {
            blackoutImage.gameObject.SetActive(true);
            Color c = blackoutImage.color;
            c.a = 0f;
            
            while (c.a < 1f)
            {
                // Fades slightly slower than the panic blinks for a calming effect
                c.a += Time.deltaTime * 0.8f; 
                blackoutImage.color = c;
                yield return null;
            }
        }

        // 2. Wait one second in pure darkness for dramatic effect
        yield return new WaitForSeconds(1.0f);

        // 3. Play the satisfying 6:00 AM alarm/chime!
        if (victoryAlarmAudio != null) victoryAlarmAudio.Play();

        // 4. Reveal the final victory text
        if (shiftCompleteScreen != null) shiftCompleteScreen.SetActive(true);
        if (shiftCompleteText != null) shiftCompleteText.text = $"6:00 AM\nDAY {dayCompleted} SURVIVED.";
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

            // Generate a random string of characters matching the length of the real name
            for (int i = 0; i < finalName.Length; i++)
            {
                if (finalName[i] == ' ') scrambledName += " "; // Preserve spaces
                else scrambledName += glyphs[Random.Range(0, glyphs.Length)];
            }

            // Generate a random string matching the length of the real number
            for (int i = 0; i < finalNumber.Length; i++)
            {
                if (finalNumber[i] == ' ' || finalNumber[i] == '-') scrambledNumber += finalNumber[i]; // Preserve formatting
                else scrambledNumber += glyphs[Random.Range(0, glyphs.Length)];
            }

            if (callerNameText != null) callerNameText.text = scrambledName;
            if (callerNumberText != null) callerNumberText.text = scrambledNumber;

            // Wait a tiny fraction of a second before scrambling again to create a flickering effect
            yield return new WaitForSeconds(0.05f); 
        }

        // Timer is up! Lock in the final text (This is when the player spots the Mimic swap!)
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
        // Logs the quota to the console for your debugging purposes!
        Debug.Log($"[SHIFT STATUS] Quota Update: {completed} / {total} calls completed.");
    }
}