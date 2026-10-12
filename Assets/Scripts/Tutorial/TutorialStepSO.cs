using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialStep", menuName = "Tutorial Step")]
public class TutorialStepSO : ScriptableObject
{
    [TextArea(2, 4)]
    public string npcDialogue;

    [Header("Typing Drill Settings")]
    [Tooltip("Specific words the player must type in order. Leave empty if this is purely a dialogue/reading step.")]
    public List<string> requiredWords = new List<string>();

    [Header("Visual Highlighting")]
    [Tooltip("Specific KeyCodes to highlight during this step (e.g., A, Space, Return).")]
    public List<KeyCode> keysToHighlight = new List<KeyCode>();
    
    [Tooltip("If true, highlights an entire row instead of individual keys.")]
    public bool highlightEntireRow = false;
    [Tooltip("The index of the row to highlight if highlightEntireRow is true (matches KeyboardLayoutSO).")]
    public int rowIndexToHighlight = -1;

    [Header("Progression")]
    [Tooltip("If true, this step waits for the player to complete the typing drills before moving on.")]
    public bool requireTypingCompletion = true;
}