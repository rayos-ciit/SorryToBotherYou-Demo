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

    private string[] currentLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        dialogueBoxUI.SetActive(false);
    }

    // Called by the PhoneController when "Talk" is first pressed on a Normal Client
    public void StartDialogue(CallerData caller)
    {
        if (caller.dialogueLines == null || caller.dialogueLines.Length == 0)
        {
            // If you forgot to write dialogue, just win the call immediately to prevent soft-locks
            gameManager.HandleCallResult(true);
            return;
        }

        currentLines = caller.dialogueLines;
        currentLineIndex = 0;
        dialogueBoxUI.SetActive(true);

        if (caller.voiceSFX != null)
        {
            voiceAudioSource.clip = caller.voiceSFX;
        }

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
        isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            
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
        Debug.Log("Dialogue finished. Normal Client resolved.");
        gameManager.HandleCallResult(true);
    }

    // Called by the PhoneController if the player hangs up BEFORE the dialogue finishes
    public void StopDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueBoxUI.SetActive(false);
        isTyping = false;
    }
}