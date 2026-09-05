using System;

[Serializable]
public class Word
{
    public string Text { get; private set; }
    public int CurrentIndex { get; private set; }

    public Word(string text)
    {
        Text = text;
        CurrentIndex = 0;
    }

    public char GetNextChar()
    {
        return Text[CurrentIndex];
    }

    public char GetCurrentChar()
    {
        return Text[CurrentIndex];
    }

    public char GetPreviousChar()
    {
        if (CurrentIndex > 0)
            return Text[CurrentIndex - 1];
        return '\0'; // Return null character if at the start
    }

    public bool TypeLetter(char letter)
    {
        if (letter == GetNextChar())
        {
            CurrentIndex++;
            return true;
        }
        return false;
    }

    public bool Backspace()
    {
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            return true;
        }
        return false;
    }

    public bool IsCompleted()
    {
        return CurrentIndex >= Text.Length;
    }

    // Green for typed, white for remaining
    public string GetFormattedText()
    {
        string typed = Text.Substring(0, CurrentIndex);
        string untyped = Text.Substring(CurrentIndex);
        return $"<color=#00FF00>{typed}</color>{untyped}";
    }
}