using System;
using System.Text;

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

    public char GetFirstChar()
    {
        return char.ToLower(Text[0]);
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
        return '\0';
    }

    public bool IsNextCharSpace()
    {
        return !IsCompleted() && GetNextChar() == ' ';
    }

    public bool IsCurrentCharSpace()
    {
        return !IsCompleted() && GetCurrentChar() == ' ';
    }

    public bool TypeLetter(char letter)
    {
        if (char.ToLower(letter) == char.ToLower(GetNextChar()))
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

    public string GetFormattedText(bool showError = false)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < Text.Length; i++)
        {
            char rawChar = Text[i];
            bool isSpace = rawChar == ' ';
            string displayChar = isSpace ? " " : rawChar.ToString();

            // <mspace=0.75em> ensures identical horizontal slot width
            sb.Append("<mspace=0.75em>");

            // Typed (Green letter over Green underline)
            if (i < CurrentIndex)
            {
                sb.Append($"<color=#00FF00>_<space=-0.75em><voffset=0.2em>{displayChar}</voffset></color>");
            }
            // Current Target (Red if error, Yellow if normal)
            else if (i == CurrentIndex)
            {
                if (showError)
                {
                    // Red letter and Red underline on error
                    sb.Append($"<color=#FF0000><b>_<space=-0.75em><voffset=0.2em>{displayChar}</voffset></b></color>");
                }
                else
                {
                    // Yellow letter and Yellow underline
                    sb.Append($"<color=#FFD700><b>_<space=-0.75em><voffset=0.2em>{displayChar}</voffset></b></color>");
                }
            }
            // Remaining (Grey letter over Grey underline)
            else
            {
                sb.Append($"<color=#CCCCCC>_<space=-0.75em><voffset=0.2em>{displayChar}</voffset></color>");
            }

            sb.Append("</mspace>");

            // Small gap between character slots
            sb.Append("<mspace=0.1em> </mspace>");
        }

        return sb.ToString();
    }
}