using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWordBank", menuName = "Word Bank")]
public class WordBank : ScriptableObject
{
    [SerializeField] private List<string> words = new List<string>();

    public string GetRandomWord()
    {
        if (words == null || words.Count == 0)
            return "default";

        int randomIndex = Random.Range(0, words.Count);
        return words[randomIndex];
    }
}