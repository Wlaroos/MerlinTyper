using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class KeyDataGenerator : EditorWindow
{
    private const string SpriteFolderPath = "Assets/Art/Keys";
    private const string KeyDataOutputPath = "Assets/ScriptableObjects/Keys";
    private const string LayoutOutputPath = "Assets/ScriptableObjects/KeyboardLayout_QWERTY.asset";

    [MenuItem("Tools/Generate Keyboard Data")]
    public static void GenerateKeysAndLayout()
    {
        // 1. Ensure target directory exists
        if (!Directory.Exists(KeyDataOutputPath))
        {
            Directory.CreateDirectory(KeyDataOutputPath);
            AssetDatabase.Refresh();
        }

        // Define exact row sequences (Top-to-Bottom, Left-to-Right)
        Dictionary<KeyboardRow, KeyCode[]> rowSequenceMap = GetOrderedRowLayout();

        Dictionary<KeyboardRow, List<KeyDataSO>> rowsData = new Dictionary<KeyboardRow, List<KeyDataSO>>();
        foreach (KeyboardRow row in Enum.GetValues(typeof(KeyboardRow)))
        {
            rowsData[row] = new List<KeyDataSO>();
        }

        int createdCount = 0;

        // Iterate through rows in exact physical order
        foreach (KeyValuePair<KeyboardRow, KeyCode[]> rowEntry in rowSequenceMap)
        {
            KeyboardRow currentRow = rowEntry.Key;
            KeyCode[] keysInRow = rowEntry.Value;

            foreach (KeyCode code in keysInRow)
            {
                if (code == KeyCode.None) continue;

                string keyName = code.ToString().ToLower();

                // Format filenames based on convention: key_{lowercase keycode}_{state}
                string defaultSpritePath = $"{SpriteFolderPath}/key_{keyName}_default.png";
                string highlightedSpritePath = $"{SpriteFolderPath}/key_{keyName}_highlighted.png";
                string pressedSpritePath = $"{SpriteFolderPath}/key_{keyName}_pressed.png";
                string darkSpritePath = $"{SpriteFolderPath}/key_{keyName}_dark.png";

                Sprite defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>(defaultSpritePath);
                Sprite highlightedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(highlightedSpritePath);
                Sprite pressedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(pressedSpritePath);
                Sprite darkSprite = AssetDatabase.LoadAssetAtPath<Sprite>(darkSpritePath);

                // Skip key codes that don't have matching sprites in the folder
                if (defaultSprite == null && highlightedSprite == null && pressedSprite == null && darkSprite == null)
                {
                    continue;
                }

                // 2. Find or create the KeyDataSO
                string assetPath = $"{KeyDataOutputPath}/KeyData_{code}.asset";
                KeyDataSO keyData = AssetDatabase.LoadAssetAtPath<KeyDataSO>(assetPath);

                if (keyData == null)
                {
                    keyData = ScriptableObject.CreateInstance<KeyDataSO>();
                    AssetDatabase.CreateAsset(keyData, assetPath);
                }

                // Assign fields
                keyData.keyId = $"Key_{code}";
                keyData.keyCode = code;
                keyData.defaultSprite = defaultSprite;
                keyData.highlightedSprite = highlightedSprite;
                keyData.pressedSprite = pressedSprite;
                keyData.darkSprite = darkSprite;

                EditorUtility.SetDirty(keyData);
                createdCount++;

                // Append in sequential left-to-right order
                rowsData[currentRow].Add(keyData);
            }
        }

        // 3. Create or update KeyboardLayoutSO preserving row order
        KeyboardLayoutSO layoutSO = AssetDatabase.LoadAssetAtPath<KeyboardLayoutSO>(LayoutOutputPath);
        if (layoutSO == null)
        {
            string layoutDir = Path.GetDirectoryName(LayoutOutputPath);
            if (!Directory.Exists(layoutDir))
            {
                Directory.CreateDirectory(layoutDir);
            }

            layoutSO = ScriptableObject.CreateInstance<KeyboardLayoutSO>();
            AssetDatabase.CreateAsset(layoutSO, LayoutOutputPath);
        }

        layoutSO.rows.Clear();
        foreach (KeyboardRow rowType in Enum.GetValues(typeof(KeyboardRow)))
        {
            if (rowsData[rowType].Count > 0)
            {
                layoutSO.rows.Add(new KeyRowDefinition
                {
                    rowType = rowType,
                    keys = rowsData[rowType]
                });
            }
        }

        EditorUtility.SetDirty(layoutSO);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Successfully generated {createdCount} KeyDataSO assets and populated KeyboardLayoutSO at '{LayoutOutputPath}'.");
    }

    private static Dictionary<KeyboardRow, KeyCode[]> GetOrderedRowLayout()
    {
        var map = new Dictionary<KeyboardRow, KeyCode[]>();

        // Function Row
        map[KeyboardRow.FunctionRow] = new KeyCode[]
        {
            KeyCode.Escape, KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5,
            KeyCode.F6, KeyCode.F7, KeyCode.F8, KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12
        };

        // Number Row
        map[KeyboardRow.NumberRow] = new KeyCode[]
        {
            KeyCode.BackQuote, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
            KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,
            KeyCode.Alpha0, KeyCode.Minus, KeyCode.Equals, KeyCode.Backspace
        };

        // Top Row
        map[KeyboardRow.TopRow] = new KeyCode[]
        {
            KeyCode.Tab, KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y,
            KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.Backslash
        };

        // Home Row
        map[KeyboardRow.HomeRow] = new KeyCode[]
        {
            KeyCode.CapsLock, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G,
            KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Semicolon, KeyCode.Quote, KeyCode.Return
        };

        // Bottom Row
        map[KeyboardRow.BottomRow] = new KeyCode[]
        {
            KeyCode.LeftShift, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B,
            KeyCode.N, KeyCode.M, KeyCode.Comma, KeyCode.Period, KeyCode.Slash, KeyCode.RightShift
        };

        // Space Row
        map[KeyboardRow.SpaceRow] = new KeyCode[]
        {
            KeyCode.LeftControl, KeyCode.LeftMeta, KeyCode.LeftAlt, KeyCode.Space,
            KeyCode.RightAlt, KeyCode.RightMeta, KeyCode.Menu, KeyCode.RightControl
        };

        return map;
    }
}