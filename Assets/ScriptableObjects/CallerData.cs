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
    
    [Tooltip("If true, there's a 50% chance the Caller ID will just read UNKNOWN")]
    public bool canMaskCallerID = false;
    
    [Header("Caller Identity")]
    public string callerName = "Unknown";
    public string callerNumber = "000-0000";
    public CallerType typeOfCaller;

    [Header("Spawning")]
    [Range(1, 10)] public int spawnWeight = 5;

    [Header("Mechanics")]
    public CorrectAction requiredAction;
    public bool requiresQuickResponse = false;
    public float timeLimitToRespond = 5.0f;

    [Header("Audio & Visuals")]
    public AudioClip ringSFX;
    public AudioClip voiceSFX;
    public AudioClip ambientSFX;
    public bool causesScreenFlicker = false;

    [Header("Dialogue Variations")]
    public List<DialogueVariation> dialogueVariations;

    public string[] GetRandomDialogue()
    {
        if (dialogueVariations == null || dialogueVariations.Count == 0) return null;
        return dialogueVariations[Random.Range(0, dialogueVariations.Count)].lines;
    }
}

public enum CallerType { NormalClient, Impatient, Mimic, Listener, Virus, Disturbance }
public enum CorrectAction { Talk, Hold, Reboot, HangUp, Ignore }