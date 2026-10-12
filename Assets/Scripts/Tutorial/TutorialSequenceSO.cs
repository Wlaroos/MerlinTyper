using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Tutorial Sequence")]
public class TutorialSequenceSO : ScriptableObject
{
    public List<TutorialStepSO> tutorialSteps = new List<TutorialStepSO>();
}