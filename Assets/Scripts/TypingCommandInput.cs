using System;
using System.Collections.Generic;
using UnityEngine;

public class TypingCommandInput
{
    readonly Dictionary<string, Action> commands = new();
    string buffer = "";

    public string Buffer => buffer;
    public event Action<string> OnBufferChanged;
    public event Action<string> OnCommandExecuted;

    public void RegisterCommand(string word, Action callback)
    {
        commands[word.ToLower()] = callback;
    }

    public void ProcessChar(char c)
    {
        if (char.IsControl(c)) return;

        char lower = char.ToLower(c);
        buffer += lower;

        if (commands.TryGetValue(buffer, out var exact))
        {
            exact.Invoke();
            OnCommandExecuted?.Invoke(buffer);
            buffer = "";
            OnBufferChanged?.Invoke(buffer);
            return;
        }

        bool hasPrefix = false;
        foreach (var key in commands.Keys)
        {
            if (key.StartsWith(buffer))
            {
                hasPrefix = true;
                break;
            }
        }

        if (!hasPrefix)
            buffer = lower.ToString();

        OnBufferChanged?.Invoke(buffer);
    }

    public void Clear()
    {
        buffer = "";
        OnBufferChanged?.Invoke(buffer);
    }

    public string GetHint()
    {
        if (string.IsNullOrEmpty(buffer)) return "";

        foreach (var key in commands.Keys)
        {
            if (key.StartsWith(buffer))
                return key.Substring(buffer.Length);
        }

        return "";
    }
}
