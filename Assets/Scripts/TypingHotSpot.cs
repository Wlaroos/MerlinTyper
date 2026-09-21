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
    [SerializeField] private bool _autoRefillOnTimer = true;
    [SerializeField] private float _refillTimerDuration = 5f;

    [Header("Events")]
    // Maybe use UnityEvent<Word> to pass the completed word as a parameter if needed
    // Or use a c# event 
    public UnityEvent OnWordCompletedEvent;

    public Word CurrentWord { get; private set; }

    private Coroutine _retryCoroutine;

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

        if (_retryCoroutine != null)
        {
            StopCoroutine(_retryCoroutine);
            _retryCoroutine = null;
        }

        CleanupCurrentWord();
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

        if (_retryCoroutine != null)
        {
            StopCoroutine(_retryCoroutine);
            _retryCoroutine = null;
        }

        Word newWord = WordManager.Instance.RequestWordForHotSpot(_hotSpotWordBank);
        
        if (newWord != null)
        {
            CurrentWord = newWord;
            if (_displayUI != null)
            {
                _displayUI.Setup(CurrentWord, OnDisplayRecycled);
            }
        }
        else if (_autoRefillOnComplete)
        {
            // Retry next frame if no starting letter was available
            _retryCoroutine = StartCoroutine(RetryAssignWordNextFrame());
        }
    }

    private IEnumerator RetryAssignWordNextFrame()
    {
        yield return null;
        _retryCoroutine = null;
        
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
            else if (_autoRefillOnTimer)
            {
                _displayUI?.ClearDisplay();
                // Start refill timer UI
                StartCoroutine(DelayedRefill());
            }
            else
            {
                _displayUI?.ClearDisplay();
            }
        }
    }

    private void CleanupCurrentWord()
    {
        if (CurrentWord != null && WordManager.Instance != null)
        {
            _displayUI?.ClearDisplay();
            WordManager.Instance.ReleaseWord(CurrentWord);
            CurrentWord = null;
        }
    }

    private IEnumerator DelayedRefill()
    {
        yield return new WaitForSeconds(_refillTimerDuration);
        AssignNewWord();
    }

    private void OnDisplayRecycled(TypingDisplayUI display)
    {
        // Callback handling if using Pooling
    }
}