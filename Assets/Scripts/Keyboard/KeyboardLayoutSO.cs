using System;
using System.Collections.Generic;
using UnityEngine;

public enum KeyboardRow
{
    FunctionRow,
    NumberRow,
    TopRow,
    HomeRow,
    BottomRow,
    SpaceRow
}

[System.Serializable]
public class KeyRowDefinition
{
    public KeyboardRow rowType;
    public List<KeyDataSO> keys = new List<KeyDataSO>();
}

[CreateAssetMenu(fileName = "KeyboardLayout_QWERTY", menuName = "Keyboard/Keyboard Layout")]
public class KeyboardLayoutSO : ScriptableObject
{
    public List<KeyRowDefinition> rows = new List<KeyRowDefinition>();
}