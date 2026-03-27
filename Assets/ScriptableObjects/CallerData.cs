using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueVariation
{
    [TextArea(2, 5)]
    public string[] lines;
}

[CreateAssetMenu(fileName = "NewCaller", menuName = "SorryToBotherYou/Caller Profile")]
public class CallerData : ScriptableObject
{
    
    [Header("Caller Identity")]
    public string callerName = "Unknown";
    public string callerNumber = "000-0000";
    
    [Tooltip("If true, there's a 50% chance the Caller ID will just read UNKNOWN to confuse the player.")]
    public bool canMaskCallerID = false;

    [Tooltip("The type of entity calling.")]
    public CallerType typeOfCaller;

    [Header("Spawning & Difficulty")]
    [Range(1, 10)]
    public int spawnWeight = 5;

    [Header("Mechanics & Rules")]
    public CorrectAction requiredAction;
    
    [Tooltip("If the action is 'Talk', does it require immediate action? (The Impatient)")]
    public bool requiresQuickResponse = false;
    public float timeLimitToRespond = 5.0f;

    [Header("Audio & Visuals")]
    public AudioClip ringSFX;
    
    [Tooltip("The audio played AFTER the phone is picked up (Crucial for hearing The Mimic or The Listener).")]
    public AudioClip voiceSFX;

    public bool causesScreenFlicker = false;

    [Header("Dialogue (For Normal Clients)")]
    [TextArea(2, 5)]
    public string[] dialogueLines;
    
    [Header("Dialogue Variations")]
    [Tooltip("Add multiple sets of dialogue here. The game will pick one at random.")]
    public List<DialogueVariation> dialogueVariations;

    // Helper method to pick a random set of lines
    public string[] GetRandomDialogue()
    {
        if (dialogueVariations == null || dialogueVariations.Count == 0) 
            return null;
        
        int index = Random.Range(0, dialogueVariations.Count);
        return dialogueVariations[index].lines;
    }
}

public enum CallerType { NormalClient, Impatient, Mimic, Listener, Virus, Disturbance }
public enum CorrectAction { Talk, Hold, Reboot, HangUp, Ignore }