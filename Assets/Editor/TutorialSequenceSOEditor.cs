using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(TutorialSequenceSO))]
public class TutorialSequenceSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector fields (including the list)
        DrawDefaultInspector();

        TutorialSequenceSO sequence = (TutorialSequenceSO)target;

        GUILayout.Space(10);

        // Add a button to trigger auto-population
        if (GUILayout.Button("Auto-Populate Steps from Directory", GUILayout.Height(30)))
        {
            AutoPopulateSteps(sequence);
        }
    }

    private void AutoPopulateSteps(TutorialSequenceSO sequence)
    {
        // Get the path of the current ScriptableObject
        string assetPath = AssetDatabase.GetAssetPath(sequence);
        if (string.IsNullOrEmpty(assetPath))
        {
            Debug.LogWarning("Please save the ScriptableObject asset before auto-populating.");
            return;
        }

        string directoryPath = Path.GetDirectoryName(assetPath);

        // Find all TutorialStepSO assets within this specific directory
        string[] guids = AssetDatabase.FindAssets("t:TutorialStepSO", new[] { directoryPath });

        sequence.tutorialSteps.Clear();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TutorialStepSO step = AssetDatabase.LoadAssetAtPath<TutorialStepSO>(path);
            
            if (step != null)
            {
                sequence.tutorialSteps.Add(step);
            }
        }

        // Sort steps alphabetically by name so they appear in a predictable order
        sequence.tutorialSteps.Sort((a, b) => string.Compare(a.name, b.name));

        // Mark the object as dirty so Unity knows to save the changes
        EditorUtility.SetDirty(sequence);
        AssetDatabase.SaveAssets();

        Debug.Log($"Successfully auto-populated {sequence.tutorialSteps.Count} tutorial steps for '{sequence.name}'.");
    }
}