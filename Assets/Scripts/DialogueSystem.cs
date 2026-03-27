using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("Connections")]
    public GameManager gameManager;
    public PhoneController phoneController;
    public AudioSource voiceAudioSource;

    [Header("UI Elements")]
    public GameObject dialogueBoxUI;
    public TMP_Text dialogueText;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;
    
    [Header("Debug Settings")]
    public bool disableScrambler = false;

    private string[] currentLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    
    // NEW: Toggles on for supernatural callers
    private bool useScrambler = false; 

    void Start()
    {
        dialogueBoxUI.SetActive(false);
    }

    // Called by the PhoneController when "Talk" is first pressed
    public void StartDialogue(CallerData caller)
    {
        // 1. Selection: Attempt to pick a random set of lines from the caller's variations.
        string[] selectedLines = caller.GetRandomDialogue();

        // 2. Fallback: If no variations are defined, use the primary dialogueLines array.
        if (selectedLines == null || selectedLines.Length == 0)
        {
            selectedLines = caller.dialogueLines;
        }

        // 3. Safety Check: If no lines are found at all, resolve the call immediately to avoid errors.
        if (selectedLines == null || selectedLines.Length == 0)
        {
            Debug.LogWarning("StartDialogue failed: No dialogue lines found for " + caller.callerName);
            gameManager.HandleCallResult(true);
            return;
        }

        // 4. Scrambler Setup: Determine if the text glitch effect should be used for this caller.
        // The 'disableScrambler' flag allows for easier testing in the Unity inspector.
        useScrambler = !disableScrambler && (caller.typeOfCaller == CallerType.Mimic || 
                                             caller.typeOfCaller == CallerType.Virus || 
                                             caller.typeOfCaller == CallerType.Disturbance);

        // 5. System Reset: Initialize state variables for the new conversation.
        currentLines = selectedLines;
        currentLineIndex = 0;
        dialogueBoxUI.SetActive(true);

        // 6. Audio: Set up the voice clip for the caller.
        if (caller.voiceSFX != null)
        {
            voiceAudioSource.clip = caller.voiceSFX;
        }

        // 7. Start Typing: Stop any current sequence and begin typing the first line.
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(currentLines[currentLineIndex]));
    }

    // Called when the player presses "Talk" again to advance the conversation
    public void DisplayNextLine()
    {
        // If the text is still typing, clicking "Talk" skips to the end of the sentence
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentLines[currentLineIndex];
            isTyping = false;
            return;
        }

        currentLineIndex++;

        // If there are more lines, type the next one. If not, the call is done!
        if (currentLineIndex < currentLines.Length)
        {
            typingCoroutine = StartCoroutine(TypeSentence(currentLines[currentLineIndex]));
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        string finalizedText = ""; // Keeps track of the real letters we've permanently placed
        isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            // NEW: The Scrambler Effect
            if (useScrambler && letter != ' ')
            {
                // Flashes 2 to 4 random characters before placing the real letter
                int glitchFrames = Random.Range(2, 5);
                for (int i = 0; i < glitchFrames; i++)
                {
                    // Picks a random printable ASCII character (symbols, numbers, letters)
                    char randomChar = (char)Random.Range(33, 126); 
                    dialogueText.text = finalizedText + randomChar;
                    
                    // Plays the glitch very fast
                    yield return new WaitForSeconds(typingSpeed / 2f); 
                }
            }

            // Place the actual letter
            finalizedText += letter;
            dialogueText.text = finalizedText;
            
            // Play the beep for each letter (avoids playing on spaces for better rhythm)
            if (voiceAudioSource.clip != null && letter != ' ')
            {
                voiceAudioSource.Play();
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }

    private void EndDialogue()
    {
        dialogueBoxUI.SetActive(false);
        Debug.Log("Dialogue finished.");
        
        // Put the phone down visually
        phoneController.ResetPhoneState();

        // THE TRAP: Did the player finish a conversation they shouldn't have?
        if (phoneController.currentCaller.requiredAction == CorrectAction.Talk)
        {
            Debug.Log("Finished talking to a normal client. Good job.");
            gameManager.HandleCallResult(true); // Player Wins!
        }
        else
        {
            Debug.Log("You stayed on the line with an entity for too long! The boss noticed.");
            gameManager.HandleCallResult(false); // Player Loses!
        }
    }

    // Called by the PhoneController if the player hangs up BEFORE the dialogue finishes
    public void StopDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueBoxUI.SetActive(false);
        isTyping = false;
    }
}