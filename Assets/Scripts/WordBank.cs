using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWordBank", menuName = "Word Bank")]
public class WordBank : ScriptableObject
{
    [TextArea(10, 20)]
    [SerializeField] private string rawWordInput; // Paste large text here
    [SerializeField] private List<string> _words = new List<string>();

    public string GetRandomWord(HashSet<char> usedStartingLetters)
    {
        if (_words == null || _words.Count == 0)
            return "default";

        // Filter words that start with letters not currently on screen
        List<string> validWords = _words
            .Where(w => !string.IsNullOrEmpty(w) && !usedStartingLetters.Contains(char.ToLower(w[0])))
            .ToList();

        if (validWords.Count == 0)
        {
            Debug.LogWarning("WordBank: No available words without duplicate starting letters!");
            return null;
        }

        int randomIndex = Random.Range(0, validWords.Count);
        return validWords[randomIndex];
    }

    [ContextMenu("Process Raw Word Input")]
    public void ProcessRawInput()
    {
        if (string.IsNullOrEmpty(rawWordInput)) return;

        // Splits words by newlines, commas, or spaces
        string[] splitWords = rawWordInput.Split(new char[] { '\n', '\r', ','}, System.StringSplitOptions.RemoveEmptyEntries);

        _words.Clear();
        foreach (string w in splitWords)
        {
            string clean = w.Trim().ToLower();
            if (!string.IsNullOrEmpty(clean) && !_words.Contains(clean))
            {
                _words.Add(clean);
            }
        }

        Debug.Log($"Successfully imported {_words.Count} unique words into {name}!");
    }
}