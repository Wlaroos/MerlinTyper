using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TypingHotSpot : MonoBehaviour
{
    [Header("HotSpot Config")]
    [SerializeField] private WordBank _hotSpotWordBank;
    [SerializeField] private TypingDisplayUI _displayUI;
    
    [Header("Auto-Refill Settings")]
    [SerializeField] private bool _autoRefillOnComplete = true;

    [Header("Events")]
    public UnityEvent OnWordCompletedEvent;

    public Word CurrentWord { get; private set; }

    private void OnEnable()
    {
        WordManager.OnWordCompleted += OnWordCompleted;
        if (CurrentWord == null && WordManager.Instance != null)
        {
            AssignNewWord();
        }
    }

    private void OnDisable()
    {
        WordManager.OnWordCompleted -= OnWordCompleted;

        // Clean up word reservation in WordManager if this hotspot is disabled/destroyed
        if (CurrentWord != null && WordManager.Instance != null)
        {
            WordManager.Instance.ReleaseWord(CurrentWord);
            CurrentWord = null;
        }
    }

    private void Start()
    {
        if (CurrentWord == null)
        {
            AssignNewWord();
        }
    }

    public void AssignNewWord()
    {
        if (WordManager.Instance == null) return;

        Word newWord = WordManager.Instance.RequestWordForHotSpot(_hotSpotWordBank);
        
        if (newWord != null)
        {
            CurrentWord = newWord;
            if (_displayUI != null)
            {
                _displayUI.Setup(CurrentWord, OnDisplayRecycled);
            }
        }
        else
        {
            // If all starting letters were taken, retry next frame
            StartCoroutine(RetryAssignWordNextFrame());
        }
    }

    private IEnumerator RetryAssignWordNextFrame()
    {
        yield return null;
        if (CurrentWord == null)
        {
            AssignNewWord();
        }
    }

    private void OnWordCompleted(Word word)
    {
        if (CurrentWord == word)
        {
            CurrentWord = null;
            OnWordCompletedEvent?.Invoke();

            if (_autoRefillOnComplete)
            {
                AssignNewWord();
            }
        }
    }

    private void OnDisplayRecycled(TypingDisplayUI display)
    {
        // Callback handling if using Pooling
    }
}