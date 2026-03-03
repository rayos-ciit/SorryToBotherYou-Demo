using UnityEngine;

[CreateAssetMenu(fileName = "NewCaller", menuName = "SorryToBotherYou/Caller Profile")]
public class CallerData : ScriptableObject
{
    [Header("Caller Identity")]
    public string callerName = "Unknown";
    public string callerNumber = "000-0000";
    
    [Tooltip("The type of entity calling.")]
    public CallerType typeOfCaller;

    [Header("Spawning & Difficulty")]
    [Tooltip("Higher number = higher chance to be pulled from the 'Weighted Deck'")]
    [Range(1, 10)]
    public int spawnWeight = 5;

    [Header("Mechanics & Rules")]
    [Tooltip("The single correct action the player must take to survive this call.")]
    public CorrectAction requiredAction;
    
    [Tooltip("If the action is 'Talk', does it require immediate action? (The Impatient)")]
    public bool requiresQuickResponse = false;
    public float timeLimitToRespond = 5.0f;

    [Header("Audio & Visuals")]
    [Tooltip("The specific ringing sound for this caller (normal, distorted, silent).")]
    public AudioClip ringSFX;
    
    [Tooltip("Does the CRT monitor flicker when this entity calls? (The Virus)")]
    public bool causesScreenFlicker = false;

    [Header("Dialogue (For Normal Clients)")]
    [TextArea(2, 5)]
    [Tooltip("The lines of text that will play when the player presses 'Talk'.")]
    public string[] dialogueLines;
}

// Enums to define our specific rules clearly
public enum CallerType 
{ 
    NormalClient, 
    Impatient, 
    Mimic, 
    Listener, 
    Virus, 
    Disturbance 
}

public enum CorrectAction 
{ 
    Talk, 
    Hold, 
    Reboot, 
    HangUp, 
    Ignore 
}