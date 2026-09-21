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

    public char GetFirstChar() => char.ToLower(Text[0]);
    public char GetNextChar() => Text[CurrentIndex];
    public char GetCurrentChar() => Text[CurrentIndex];
    public char GetPreviousChar() => CurrentIndex > 0 ? Text[CurrentIndex - 1] : '\0';

    public bool IsNextCharSpace() => !IsCompleted() && GetNextChar() == ' ';
    public bool IsCurrentCharSpace() => !IsCompleted() && GetCurrentChar() == ' ';

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

    public bool IsCompleted() => CurrentIndex >= Text.Length;
}