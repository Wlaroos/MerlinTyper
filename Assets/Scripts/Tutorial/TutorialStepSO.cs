using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial Step")]
public class TutorialStepSO : ScriptableObject
{
    [TextArea(2, 4)]
    public string npcDialogue;

    [Header("Typing Drill Settings")]
    [Tooltip("Specific words the player must type in order.")]
    public List<string> requiredWords = new List<string>();
    public bool randomizeOrderOfList = false;

    [Header("Visual Highlighting")]
    [Tooltip("Type keys separated by commas (A, D, Space, Return, LeftShift)")]
    public string keysToHighlightInput; 

    // Helper property to easily get them as a List<KeyCode> in your game code
    public List<KeyCode> GetKeysToHighlight()
    {
        List<KeyCode> keyCodes = new List<KeyCode>();
        if (string.IsNullOrEmpty(keysToHighlightInput)) return keyCodes;

        string[] splitKeys = keysToHighlightInput.Split(',');
        foreach (string k in splitKeys)
        {
            string cleanKey = k.Trim();
            // Try parsing the string into a KeyCode enum value
            if (System.Enum.TryParse(cleanKey, true, out KeyCode parsedKey))
            {
                keyCodes.Add(parsedKey);
            }
            else
            {
                Debug.LogWarning($"Could not parse key: '{cleanKey}' in step {name}");
            }
        }
        return keyCodes;
    }
    
    [Tooltip("If true, highlights an entire row instead of individual keys.")]
    public bool highlightEntireRow = false;
    [Tooltip("The index of the row to highlight if highlightEntireRow is true.")]
    public int rowIndexToHighlight = -1;

    [Header("Progression")]
    [Tooltip("If true, this step waits for the player to complete the typing drills before moving on.")]
    public bool requireTypingCompletion = true;
}