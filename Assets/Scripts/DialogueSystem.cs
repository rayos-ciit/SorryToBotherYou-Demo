using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    public GameManager gameManager;
    public PhoneController phoneController;
    public GameObject dialogueBoxUI;
    public TMP_Text dialogueText;
    public AudioSource voiceAudioSource;
    public float typingSpeed = 0.05f;
    public bool disableScrambler = false;

    private string[] currentLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool useScrambler = false;
    private Coroutine typingRoutine; // ---> NEW: Tracks the active coroutine
    
    void Start()
    {
        if (dialogueBoxUI != null) dialogueBoxUI.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";
    }

    public void StartDialogue(CallerData caller)
    {
        string[] lines = caller.GetRandomDialogue();
        if (lines == null || lines.Length == 0) { gameManager.ResolveCall(true); return; }

        currentLines = lines;
        currentLineIndex = 0;
        useScrambler = !disableScrambler && (caller.typeOfCaller == CallerType.Mimic || caller.typeOfCaller == CallerType.Virus);
        
        if (voiceAudioSource != null) voiceAudioSource.clip = caller.voiceSFX;

        dialogueBoxUI.SetActive(true);
        
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeSentence(currentLines[0]));
    }

    public void DisplayNextLine()
    {
        if (phoneController != null && phoneController.isOnHold)
        {
            Debug.Log("Cannot talk while the caller is on hold!");
            return;
        }

        // ---> NEW: Tell the phone we are actively talking so they don't hang up!
        if (phoneController != null) phoneController.ResetSLAForTalking();

        // ---> REWRITTEN: Bulletproof skip logic
        if (isTyping) 
        { 
            // 1. Forceably murder the typing coroutine so it doesn't wake up
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            
            // 2. Instantly display the full text and stop the voice
            dialogueText.text = currentLines[currentLineIndex];
            isTyping = false;
            if (voiceAudioSource != null) voiceAudioSource.Stop();
            
            return; 
        } 

        currentLineIndex++;
        if (currentLineIndex < currentLines.Length) 
        {
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            typingRoutine = StartCoroutine(TypeSentence(currentLines[currentLineIndex]));
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

        if (voiceAudioSource != null && voiceAudioSource.clip != null) voiceAudioSource.Play();

        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
        if (voiceAudioSource != null) voiceAudioSource.Stop();
    }

    private void EndDialogue()
    {
        dialogueBoxUI.SetActive(false);
        gameManager.ResolveCall(gameManager.activeCaller.requiredAction == CorrectAction.Talk);
    }

    public void StopDialogue()
    {
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        StopAllCoroutines();
        isTyping = false;
        if (voiceAudioSource != null) voiceAudioSource.Stop();
        if (dialogueBoxUI != null) dialogueBoxUI.SetActive(false);
    }
}