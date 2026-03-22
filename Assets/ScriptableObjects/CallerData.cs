using UnityEngine;

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
}

public enum CallerType { NormalClient, Impatient, Mimic, Listener, Virus, Disturbance }
public enum CorrectAction { Talk, Hold, Reboot, HangUp, Ignore }