﻿namespace NotEnoughKeys;

public class HotkeyHandler
{
    public void HandleHotkey(Binding binding)
    {
        GlobalLog.Debug($"handling binding {binding.Modifiers} {binding.Keys[0]}");
        ActionDispatch.TryExecuteAction(binding);
    }
}