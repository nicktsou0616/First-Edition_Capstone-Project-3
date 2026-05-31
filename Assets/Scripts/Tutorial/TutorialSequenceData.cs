using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Tutorial/Sequence")]
public class TutorialSequenceData : ScriptableObject
{
    public List<TutorialStepData> steps = new List<TutorialStepData>();
}