using UnityEngine;
using TMPro; // Standard for Unity UI text

public class UIManager : MonoBehaviour
{
    [Header("Caller ID Screen")]
    [Tooltip("Drag the TextMeshPro UI elements here.")]
    public TMP_Text callerNameText;
    public TMP_Text callerNumberText;
    public string defaultIdleText = "SYSTEM IDLE...";

    [Header("Shift Clock")]
    [Tooltip("Drag the UI Image representing the clock hand here.")]
    public RectTransform clockHand;
    [Tooltip("Z-axis rotation for 12:00 AM")]
    public float startRotationZ = 0f;   
    [Tooltip("Z-axis rotation for 6:00 AM (usually -180 degrees for half a clock face)")]
    public float endRotationZ = -180f;  

    [Header("Corporate Strikes")]
    [Tooltip("Drag your Strike UI GameObjects (e.g., pink slips) into this array in order.")]
    public GameObject[] strikeVisuals; 

    void Start()
    {
        ClearCallerID();
        UpdateStrikes(0);
        UpdateClock(0f);
    }

    // Called by the PhoneController when a call rings
    public void UpdateCallerID(string name, string number)
    {
        if (callerNameText != null) callerNameText.text = name;
        if (callerNumberText != null) callerNumberText.text = number;
    }

    // Called when the call ends or is on standby
    public void ClearCallerID()
    {
        if (callerNameText != null) callerNameText.text = defaultIdleText;
        if (callerNumberText != null) callerNumberText.text = "";
    }

    // Called by the GameManager when a call is successfully resolved
    public void UpdateClock(float shiftPercentage)
    {
        if (clockHand != null)
        {
            // This calculates the exact angle the hand should point to based on progress
            float currentRot = Mathf.Lerp(startRotationZ, endRotationZ, shiftPercentage);
            clockHand.localEulerAngles = new Vector3(0, 0, currentRot);
        }
    }

    // Called by the GameManager when the player messes up
    public void UpdateStrikes(int currentStrikes)
    {
        for (int i = 0; i < strikeVisuals.Length; i++)
        {
            // Turns on the visual if the loop index is less than the current strikes
            if (i < currentStrikes)
                strikeVisuals[i].SetActive(true);
            else
                strikeVisuals[i].SetActive(false);
        }
    }
}