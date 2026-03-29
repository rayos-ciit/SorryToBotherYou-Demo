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
    
    void Start()
    {
        // Force the dialogue box to hide itself the moment the game launches!
        if (dialogueBoxUI != null) 
        {
            dialogueBoxUI.SetActive(false);
        }
        
        if (dialogueText != null) 
        {
            dialogueText.text = "";
        }
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
        StartCoroutine(TypeSentence(currentLines[0]));
    }

    public void DisplayNextLine()
    {
        // ---> THE FIX: Block the Talk button if the phone is on hold <---
        if (phoneController != null && phoneController.isOnHold)
        {
            Debug.Log("Cannot talk while the caller is on hold!");
            return;
        }

        if (isTyping) { isTyping = false; return; } // Simple skip logic

        currentLineIndex++;
        if (currentLineIndex < currentLines.Length) StartCoroutine(TypeSentence(currentLines[currentLineIndex]));
        else EndDialogue();
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        // ---> NEW: Start playing their voice clip! <---
        if (voiceAudioSource != null && voiceAudioSource.clip != null) voiceAudioSource.Play();

        foreach (char letter in sentence)
        {
            if (!isTyping) { dialogueText.text = sentence; break; } // Skip typing
            dialogueText.text += letter;
            
            // (The PlayOneShot blip was deleted from right here!)
            
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;

        // ---> NEW: Stop playing their voice once the text finishes typing! <---
        if (voiceAudioSource != null) voiceAudioSource.Stop();
    }

    private void EndDialogue()
    {
        dialogueBoxUI.SetActive(false);
        // Just report back to GameManager
        gameManager.ResolveCall(gameManager.activeCaller.requiredAction == CorrectAction.Talk);
    }

    public void StopDialogue()
    {
        StopAllCoroutines();
        isTyping = false;
        if (voiceAudioSource != null) voiceAudioSource.Stop();
        if (dialogueBoxUI != null) dialogueBoxUI.SetActive(false);
    }
}