using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Sequence")]
    [SerializeField] private List<TutorialSequenceSO> _tutorialSequences = new List<TutorialSequenceSO>();
    
    private int _currentSequenceIndex = 0;
    private int _currentStepIndex = 0;

    [Header("UI References")]
    [SerializeField] private TMP_Text _npcDialogueTextField;
    [SerializeField] private GameObject _advanceIndicatorSprite;

    [Header("Systems Integration")]
    [SerializeField] private TypingHotSpot _typingHotSpot;
    [SerializeField] private KeyboardManager _keyboardManager;
    private Queue<string> _remainingDrillWords = new Queue<string>();
    private bool _waitingForSpace = false;
    private bool _isStepFinished = false;

    private void OnEnable()
    {
        WordManager.WordCompletedEvent += OnWordCompleted;
    }

    private void OnDisable()
    {
        WordManager.WordCompletedEvent -= OnWordCompleted;
    }

    private void Start()
    {
        if (_advanceIndicatorSprite != null)
        {
            _advanceIndicatorSprite.SetActive(false);
        }

        if (_tutorialSequences != null && _tutorialSequences.Count > 0)
        {
            _currentSequenceIndex = 0;
            _currentStepIndex = 0;
            LoadCurrentStep();
        }
        else
        {
            Debug.LogWarning("TutorialManager: No tutorial sequences assigned");
        }
    }

    private void Update()
    {
        // Check for spacebar input to advance dialogue when ready
        if (_waitingForSpace)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AdvanceToNextStep();
            }
        }
    }

    // Helper to safely fetch the active step across sequences
    private TutorialStepSO GetCurrentStep()
    {
        if (_currentSequenceIndex < 0 || _currentSequenceIndex >= _tutorialSequences.Count) return null;
        
        var sequence = _tutorialSequences[_currentSequenceIndex];
        if (sequence == null || sequence.tutorialSteps == null || sequence.tutorialSteps.Count == 0) return null;
        
        if (_currentStepIndex < 0 || _currentStepIndex >= sequence.tutorialSteps.Count) return null;

        return sequence.tutorialSteps[_currentStepIndex];
    }

    private void LoadCurrentStep()
    {
        TutorialStepSO step = GetCurrentStep();
        if (step == null)
        {
            CompleteTutorial();
            return;
        }

        // Clear any highlights from the previous step before applying new ones
        if (_keyboardManager != null)
        {
            _keyboardManager.ClearHighlights();
        }

        _isStepFinished = false;
        _waitingForSpace = false;

        if (_advanceIndicatorSprite != null)
        {
            _advanceIndicatorSprite.SetActive(false);
        }

        // Display NPC text
        if (_npcDialogueTextField != null)
        {
            _npcDialogueTextField.text = step.npcDialogue;
        }

        // Setup Keyboard Highlighting
        ApplyStepHighlights(step);

        // Setup Typing Drills Queue
        _remainingDrillWords.Clear();
        if (step.requiredWords != null && step.requiredWords.Count > 0)
        {
            foreach (string word in step.requiredWords)
            {
                _remainingDrillWords.Enqueue(word);
            }

            // Enable typing hot spot and feed the first target word
            if (_typingHotSpot != null)
            {
                _typingHotSpot.gameObject.SetActive(true);
                FeedNextDrillWord();
            }
        }
        else
        {
            // No typing drills required for this step
            if (_typingHotSpot != null)
            {
                _typingHotSpot.gameObject.SetActive(false);
            }

            EnableAdvancePrompt();
        }
    }

    private void ApplyStepHighlights(TutorialStepSO step)
    {
        if (step.highlightEntireRow && step.rowIndexToHighlight >= 0)
        {
            _keyboardManager.HighlightRow(step.rowIndexToHighlight);
        }
        else if (step.keysToHighlight != null && step.keysToHighlight.Count > 0)
        {
            foreach (KeyCode key in step.keysToHighlight)
            {
                _keyboardManager.HighlightKey(key);
            }
        }
    }

    private void FeedNextDrillWord()
    {
        if (_remainingDrillWords.Count > 0)
        {
            string nextWord = _remainingDrillWords.Dequeue();
            if (_typingHotSpot != null)
            {
                _typingHotSpot.SetTutorialWord(nextWord);
            }
        }
        else
        {
            // All sequence words finished for this step
            if (_typingHotSpot != null)
            {
                _typingHotSpot.gameObject.SetActive(false);
            }

            TutorialStepSO step = GetCurrentStep();
            if (step != null && step.requireTypingCompletion)
            {
                EnableAdvancePrompt();
            }
        }
    }

    private void OnWordCompleted(Word word)
    {
        if (_isStepFinished) return;

        // When a word finishes, check if there are more drill words left in the queue
        if (_remainingDrillWords.Count > 0)
        {
            FeedNextDrillWord();
        }
        else
        {
            // No more drill words left, advance to the next step if typing completion is required
            TutorialStepSO step = GetCurrentStep();
            if (step != null && step.requireTypingCompletion)
            {
                AdvanceToNextStep();
            }
        }
    }

    private void EnableAdvancePrompt()
    {
        _isStepFinished = true;
        _waitingForSpace = true;

        if (_advanceIndicatorSprite != null)
        {
            _advanceIndicatorSprite.SetActive(true);
        }
    }

    private void AdvanceToNextStep()
    {
        _waitingForSpace = false;

        var currentSequence = _tutorialSequences[_currentSequenceIndex];

        // Check if there are more steps within the current sequence
        if (_currentStepIndex + 1 < currentSequence.tutorialSteps.Count)
        {
            _currentStepIndex++;
            LoadCurrentStep();
        }
        else
        {
            // Move on to the next sequence in the list
            _currentSequenceIndex++;
            _currentStepIndex = 0;
            if (_currentSequenceIndex < _tutorialSequences.Count)
            {
                LoadCurrentStep();
            }
            else
            {
                // No more sequences left, complete the tutorial
                CompleteTutorial();
            }
        }
    }

    private void CompleteTutorial()
    {
        Debug.Log("Tutorial completed");
        
        // Ensure all highlights are cleared when the tutorial ends
        if (_keyboardManager != null)
        {
            _keyboardManager.ClearHighlights();
        }

        if (_npcDialogueTextField != null) _npcDialogueTextField.text = string.Empty;
        if (_advanceIndicatorSprite != null) _advanceIndicatorSprite.SetActive(false);
        if (_typingHotSpot != null) _typingHotSpot.gameObject.SetActive(false);

        // Disable tutorial manager
        gameObject.SetActive(false); 
    }
}