using System;
using UnityEngine.InputSystem;

public class TypingInputReader : IDisposable
{
    public event Action<char> OnCharTyped;
    public event Action OnEnterPressed;
    public event Action OnSpacePressed;

    public void Enable()
    {
        Subscribe(Keyboard.current);
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    public void Disable()
    {
        Unsubscribe(Keyboard.current);
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    public void Poll()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            OnEnterPressed?.Invoke();

        if (keyboard.spaceKey.wasPressedThisFrame)
            OnSpacePressed?.Invoke();
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Keyboard keyboard) return;

        if (change == InputDeviceChange.Added)
            Subscribe(keyboard);
        else if (change == InputDeviceChange.Removed)
            Unsubscribe(keyboard);
    }

    void Subscribe(Keyboard keyboard)
    {
        if (keyboard == null) return;
        keyboard.onTextInput -= HandleTextInput;
        keyboard.onTextInput += HandleTextInput;
    }

    void Unsubscribe(Keyboard keyboard)
    {
        if (keyboard == null) return;
        keyboard.onTextInput -= HandleTextInput;
    }

    void HandleTextInput(char c)
    {
        OnCharTyped?.Invoke(c);
    }

    public void Dispose()
    {
        Disable();
    }
}
