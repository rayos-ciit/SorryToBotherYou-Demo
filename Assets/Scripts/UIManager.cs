using UnityEngine;
using TMPro; 
using System.Collections; // Needed for Coroutines!

public class UIManager : MonoBehaviour
{
    private Coroutine scrambleRoutine;
    
    [Header("Caller ID Screen")]
    public TMP_Text callerNameText;
    public TMP_Text callerNumberText;
    public string defaultIdleText = "SYSTEM IDLE...";

    [Header("Shift Clock")]
    public RectTransform clockHand;
    public float startRotationZ = 0f;   
    public float endRotationZ = -180f;  
    
    [Header("Blackout Strike Effect")]
    public AudioSource strikeAudioSource; // The Death Bell
    public UnityEngine.UI.Image blackoutImage; // The black screen
    [Tooltip("How fast the eyes fade to black and back.")]
    public float blinkSpeed = 5f;
    [Tooltip("How many times the screen flashes black per strike.")]
    public int blinkCount = 2;

    [Header("Game Loop Screens")]
    public GameObject gameOverScreen;
    public GameObject shiftCompleteScreen;
    public TMP_Text shiftCompleteText;

    void Start()
    {
        ClearCallerID();
        UpdateStrikes(0);
        UpdateClock(0f);
        
        if (blackoutImage != null) blackoutImage.gameObject.SetActive(false);
    }

    public void UpdateCallerID(string name, string number)
    {
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
        if (clockHand != null)
        {
            float currentRot = Mathf.Lerp(startRotationZ, endRotationZ, shiftPercentage);
            clockHand.localEulerAngles = new Vector3(0, 0, currentRot);
        }
    }

    public void UpdateStrikes(int currentStrikes)
    {
        if (currentStrikes > 0)
        {
            if (strikeAudioSource != null) strikeAudioSource.Play();
            if (blackoutImage != null) StartCoroutine(BlinkEffect());
        }
    }

    private IEnumerator BlinkEffect()
    {
        blackoutImage.gameObject.SetActive(true);
        Color c = blackoutImage.color;

        for (int i = 0; i < blinkCount; i++)
        {
            // Fade to black (0.9f keeps it just barely transparent for panic)
            while (c.a < 0.9f) 
            {
                c.a += Time.deltaTime * blinkSpeed;
                blackoutImage.color = c;
                yield return null;
            }
            // Fade back to clear
            while (c.a > 0f)
            {
                c.a -= Time.deltaTime * blinkSpeed;
                blackoutImage.color = c;
                yield return null;
            }
        }
        
        // Ensure it is perfectly invisible when done
        c.a = 0f;
        blackoutImage.color = c;
        blackoutImage.gameObject.SetActive(false);
    }
    
    public void ShowGameOver()
    {
        if (gameOverScreen != null) gameOverScreen.SetActive(true);
    }

    public void ShowShiftComplete(int dayCompleted)
    {
        if (shiftCompleteScreen != null) shiftCompleteScreen.SetActive(true);
        if (shiftCompleteText != null) shiftCompleteText.text = $"DAY {dayCompleted} COMPLETE.\nPRESS TO CONTINUE.";
    }
    
    private IEnumerator ScrambleTextRoutine(string finalName, string finalNumber)
    {
        string glyphs = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
        float scrambleDuration = 1.2f; // Takes 1.2 seconds to "decode" the caller ID
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
}