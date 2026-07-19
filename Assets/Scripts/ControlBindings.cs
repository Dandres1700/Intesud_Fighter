using System;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum ControlAction
{
    MoveLeft,
    MoveRight,
    Jump,
    Attack
}

public static class ControlBindings
{
    private const string Player1LeftKey = "Player1LeftKey";
    private const string Player1RightKey = "Player1RightKey";
    private const string Player1JumpKey = "Player1JumpKey";
    private const string Player1AttackKey = "Player1AttackKey";

    private const string Player2LeftKey = "Player2LeftKey";
    private const string Player2RightKey = "Player2RightKey";
    private const string Player2JumpKey = "Player2JumpKey";
    private const string Player2AttackKey = "Player2AttackKey";

    public static KeyCode Player1Left => GetKeyCode(Player1LeftKey, KeyCode.A);
    public static KeyCode Player1Right => GetKeyCode(Player1RightKey, KeyCode.D);
    public static KeyCode Player1Jump => GetKeyCode(Player1JumpKey, KeyCode.W);
    public static KeyCode Player1Attack => GetKeyCode(Player1AttackKey, KeyCode.F);

    public static KeyCode Player2Left => GetKeyCode(Player2LeftKey, KeyCode.LeftArrow);
    public static KeyCode Player2Right => GetKeyCode(Player2RightKey, KeyCode.RightArrow);
    public static KeyCode Player2Jump => GetKeyCode(Player2JumpKey, KeyCode.UpArrow);
    public static KeyCode Player2Attack => GetKeyCode(Player2AttackKey, KeyCode.M);

    public static KeyCode GetBinding(int playerIndex, ControlAction action)
    {
        if (playerIndex == 1)
        {
            switch (action)
            {
                case ControlAction.MoveLeft: return Player1Left;
                case ControlAction.MoveRight: return Player1Right;
                case ControlAction.Jump: return Player1Jump;
                case ControlAction.Attack: return Player1Attack;
            }
        }
        else
        {
            switch (action)
            {
                case ControlAction.MoveLeft: return Player2Left;
                case ControlAction.MoveRight: return Player2Right;
                case ControlAction.Jump: return Player2Jump;
                case ControlAction.Attack: return Player2Attack;
            }
        }

        return KeyCode.None;
    }

    public static void SetBinding(int playerIndex, ControlAction action, KeyCode keyCode)
    {
        string prefKey = GetPrefKey(playerIndex, action);
        if (!string.IsNullOrEmpty(prefKey))
        {
            PlayerPrefs.SetInt(prefKey, (int)keyCode);
            PlayerPrefs.Save();
        }
    }

    public static void ResetDefaults()
    {
        SetBinding(1, ControlAction.MoveLeft, KeyCode.A);
        SetBinding(1, ControlAction.MoveRight, KeyCode.D);
        SetBinding(1, ControlAction.Jump, KeyCode.W);
        SetBinding(1, ControlAction.Attack, KeyCode.F);

        SetBinding(2, ControlAction.MoveLeft, KeyCode.LeftArrow);
        SetBinding(2, ControlAction.MoveRight, KeyCode.RightArrow);
        SetBinding(2, ControlAction.Jump, KeyCode.UpArrow);
        SetBinding(2, ControlAction.Attack, KeyCode.M);
    }

    public static bool IsKeyInUse(KeyCode keyCode, int ignoredPlayerIndex, ControlAction ignoredAction)
    {
        return GetBinding(1, ControlAction.MoveLeft) == keyCode && !(ignoredPlayerIndex == 1 && ignoredAction == ControlAction.MoveLeft)
            || GetBinding(1, ControlAction.MoveRight) == keyCode && !(ignoredPlayerIndex == 1 && ignoredAction == ControlAction.MoveRight)
            || GetBinding(1, ControlAction.Jump) == keyCode && !(ignoredPlayerIndex == 1 && ignoredAction == ControlAction.Jump)
            || GetBinding(1, ControlAction.Attack) == keyCode && !(ignoredPlayerIndex == 1 && ignoredAction == ControlAction.Attack)
            || GetBinding(2, ControlAction.MoveLeft) == keyCode && !(ignoredPlayerIndex == 2 && ignoredAction == ControlAction.MoveLeft)
            || GetBinding(2, ControlAction.MoveRight) == keyCode && !(ignoredPlayerIndex == 2 && ignoredAction == ControlAction.MoveRight)
            || GetBinding(2, ControlAction.Jump) == keyCode && !(ignoredPlayerIndex == 2 && ignoredAction == ControlAction.Jump)
            || GetBinding(2, ControlAction.Attack) == keyCode && !(ignoredPlayerIndex == 2 && ignoredAction == ControlAction.Attack);
    }

    public static string GetReadableName(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.LeftArrow: return "Left";
            case KeyCode.RightArrow: return "Right";
            case KeyCode.UpArrow: return "Up";
            case KeyCode.DownArrow: return "Down";
            default: return keyCode.ToString();
        }
    }

    private static KeyCode GetKeyCode(string prefKey, KeyCode defaultKey)
    {
        return (KeyCode)PlayerPrefs.GetInt(prefKey, (int)defaultKey);
    }

    private static string GetPrefKey(int playerIndex, ControlAction action)
    {
        if (playerIndex == 1)
        {
            switch (action)
            {
                case ControlAction.MoveLeft: return Player1LeftKey;
                case ControlAction.MoveRight: return Player1RightKey;
                case ControlAction.Jump: return Player1JumpKey;
                case ControlAction.Attack: return Player1AttackKey;
                default: return string.Empty;
            }
        }

        switch (action)
        {
            case ControlAction.MoveLeft: return Player2LeftKey;
            case ControlAction.MoveRight: return Player2RightKey;
            case ControlAction.Jump: return Player2JumpKey;
            case ControlAction.Attack: return Player2AttackKey;
            default: return string.Empty;
        }
    }
}

public static class PadInput
{
    private const int MaxPlayers = 2;
    private const float DeadZone = 0.2f;
    private const float MenuDeadZone = 0.35f;

#if ENABLE_INPUT_SYSTEM
    private static readonly Gamepad[] assignedPads = new Gamepad[MaxPlayers];
    private static readonly List<Gamepad> cachedPads = new List<Gamepad>(MaxPlayers);
#endif

    public static bool HasGamepad(int playerIndex)
    {
#if ENABLE_INPUT_SYSTEM
        return GetAssignedGamepad(playerIndex) != null;
#else
        return IsJoystickConnected(playerIndex);
#endif
    }

    public static Vector2 ReadMove(int playerIndex)
    {
#if ENABLE_INPUT_SYSTEM
        return ReadMoveFromGamepad(GetAssignedGamepad(playerIndex));
#else
        return ReadMoveFromInputManager(playerIndex);
#endif
    }

    public static Vector2 ReadMenuMove(int playerIndex)
    {
#if ENABLE_INPUT_SYSTEM
        return ReadMenuMoveFromGamepad(GetAssignedGamepad(playerIndex));
#else
        return ReadMoveFromInputManager(playerIndex);
#endif
    }

    public static float ReadHorizontal(int playerIndex) => ReadMove(playerIndex).x;
    public static float ReadVertical(int playerIndex) => ReadMove(playerIndex).y;

    public static Vector2 ReadAnyMove(int maxPlayers)
    {
        int limit = Mathf.Clamp(maxPlayers, 1, MaxPlayers);
        Vector2 best = Vector2.zero;
        float bestMagnitude = 0f;

        for (int playerIndex = 1; playerIndex <= limit; playerIndex++)
        {
            Vector2 move = ReadMove(playerIndex);
            float magnitude = move.sqrMagnitude;
            if (magnitude > bestMagnitude)
            {
                best = move;
                bestMagnitude = magnitude;
            }
        }

        return best;
    }

    public static bool SouthPressedThisFrame(int playerIndex)
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = GetAssignedGamepad(playerIndex);
        return pad != null && pad.buttonSouth.wasPressedThisFrame;
#else
        return IsJoystickConnected(playerIndex) && Input.GetKeyDown(GetJoystickButton(playerIndex, 1));
#endif
    }

    public static bool EastPressedThisFrame(int playerIndex)
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = GetAssignedGamepad(playerIndex);
        return pad != null && pad.buttonEast.wasPressedThisFrame;
#else
        return IsJoystickConnected(playerIndex) && Input.GetKeyDown(GetJoystickButton(playerIndex, 2));
#endif
    }

    public static bool WestPressedThisFrame(int playerIndex)
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = GetAssignedGamepad(playerIndex);
        return pad != null && pad.buttonWest.wasPressedThisFrame;
#else
        return IsJoystickConnected(playerIndex) && Input.GetKeyDown(GetJoystickButton(playerIndex, 0));
#endif
    }

    public static bool NorthPressedThisFrame(int playerIndex)
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = GetAssignedGamepad(playerIndex);
        return pad != null && pad.buttonNorth.wasPressedThisFrame;
#else
        return IsJoystickConnected(playerIndex) && Input.GetKeyDown(GetJoystickButton(playerIndex, 3));
#endif
    }

    public static bool AnySouthPressedThisFrame(int maxPlayers) => AnyButtonPressed(maxPlayers, SouthPressedThisFrame);
    public static bool AnyEastPressedThisFrame(int maxPlayers) => AnyButtonPressed(maxPlayers, EastPressedThisFrame);
    public static bool AnyWestPressedThisFrame(int maxPlayers) => AnyButtonPressed(maxPlayers, WestPressedThisFrame);
    public static bool AnyNorthPressedThisFrame(int maxPlayers) => AnyButtonPressed(maxPlayers, NorthPressedThisFrame);

    public static string GetAssignedGamepadName(int playerIndex)
    {
#if ENABLE_INPUT_SYSTEM
        Gamepad pad = GetAssignedGamepad(playerIndex);
        if (pad == null) return "<none>";

        string description = pad.description.product;
        if (string.IsNullOrEmpty(description))
            description = pad.displayName;

        return string.IsNullOrEmpty(description) ? pad.name : description;
#else
        string[] names = Input.GetJoystickNames();
        int index = playerIndex - 1;
        if (index < 0 || index >= names.Length) return "<none>";
        return string.IsNullOrEmpty(names[index]) ? "<none>" : names[index];
#endif
    }

    public static int GetAssignedGamepadCount()
    {
#if ENABLE_INPUT_SYSTEM
        int count = 0;
        for (int i = 0; i < MaxPlayers; i++)
        {
            if (GetAssignedGamepad(i + 1) != null)
                count++;
        }
        return count;
#else
        int count = 0;
        string[] names = Input.GetJoystickNames();
        for (int i = 0; i < Mathf.Min(MaxPlayers, names.Length); i++)
        {
            if (!string.IsNullOrEmpty(names[i]))
                count++;
        }
        return count;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static Gamepad GetAssignedGamepad(int playerIndex)
    {
        if (playerIndex < 1 || playerIndex > MaxPlayers)
            return null;

        RefreshAssignments();
        return assignedPads[playerIndex - 1];
    }

    private static void RefreshAssignments()
    {
        cachedPads.Clear();
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad pad = Gamepad.all[i];
            if (pad != null)
                cachedPads.Add(pad);
        }

        for (int i = 0; i < MaxPlayers; i++)
        {
            Gamepad pad = assignedPads[i];
            if (pad != null && !cachedPads.Contains(pad))
                assignedPads[i] = null;
        }

        HashSet<int> claimed = new HashSet<int>();
        for (int i = 0; i < MaxPlayers; i++)
        {
            if (assignedPads[i] != null)
                claimed.Add(assignedPads[i].deviceId);
        }

        for (int i = 0; i < MaxPlayers; i++)
        {
            if (assignedPads[i] != null)
                continue;

            Gamepad candidate = FindBestUnclaimedPad(claimed);
            if (candidate != null)
            {
                assignedPads[i] = candidate;
                claimed.Add(candidate.deviceId);
            }
        }
    }

    private static Gamepad FindBestUnclaimedPad(HashSet<int> claimed)
    {
        Gamepad fallback = null;

        for (int i = 0; i < cachedPads.Count; i++)
        {
            Gamepad pad = cachedPads[i];
            if (pad == null || claimed.Contains(pad.deviceId))
                continue;

            if (HasAnyActivity(pad))
                return pad;

            if (fallback == null)
                fallback = pad;
        }

        return fallback;
    }

    private static bool HasAnyActivity(Gamepad pad)
    {
        if (pad == null)
            return false;

        if (pad.leftStick.ReadValue().sqrMagnitude > DeadZone * DeadZone)
            return true;

        if (pad.dpad.ReadValue().sqrMagnitude > 0.01f)
            return true;

        if (pad.leftTrigger.ReadValue() > DeadZone || pad.rightTrigger.ReadValue() > DeadZone)
            return true;

        return pad.buttonSouth.isPressed
            || pad.buttonEast.isPressed
            || pad.buttonWest.isPressed
            || pad.buttonNorth.isPressed
            || pad.startButton.isPressed;
    }

    private static Vector2 ReadMoveFromGamepad(Gamepad pad)
    {
        if (pad == null)
            return Vector2.zero;

        Vector2 stick = ApplyDeadZone(pad.leftStick.ReadValue(), DeadZone);
        Vector2 dpad = Digitalize(pad.dpad.ReadValue());
        return dpad.sqrMagnitude > stick.sqrMagnitude ? dpad : stick;
    }

    private static Vector2 ReadMenuMoveFromGamepad(Gamepad pad)
    {
        if (pad == null)
            return Vector2.zero;

        Vector2 dpad = Digitalize(pad.dpad.ReadValue());
        if (dpad.sqrMagnitude > 0.01f)
            return dpad;

        return ApplyDeadZone(pad.leftStick.ReadValue(), MenuDeadZone);
    }

    private static Vector2 ApplyDeadZone(Vector2 value, float deadZone)
    {
        if (value.magnitude < deadZone)
            return Vector2.zero;

        return value;
    }

    private static Vector2 Digitalize(Vector2 value)
    {
        return new Vector2(DigitalAxis(value.x), DigitalAxis(value.y));
    }

    private static float DigitalAxis(float value)
    {
        if (value > 0.5f)
            return 1f;

        if (value < -0.5f)
            return -1f;

        return 0f;
    }
#else
    private static Vector2 ReadMoveFromInputManager(int playerIndex)
    {
        if (!IsJoystickConnected(playerIndex))
            return Vector2.zero;

        string horizontalAxis = $"J{playerIndex}_Horizontal";
        string verticalAxis = $"J{playerIndex}_Vertical";

        float horizontal;
        float vertical;

        try
        {
            horizontal = Input.GetAxisRaw(horizontalAxis);
            vertical = Input.GetAxisRaw(verticalAxis);
        }
        catch
        {
            return Vector2.zero;
        }

        return new Vector2(horizontal, vertical);
    }

    private static bool IsJoystickConnected(int playerIndex)
    {
        string[] names = Input.GetJoystickNames();
        int index = playerIndex - 1;
        return index >= 0 && index < names.Length && !string.IsNullOrEmpty(names[index]);
    }

    private static KeyCode GetJoystickButton(int playerIndex, int buttonIndex)
    {
        int baseIndex = (int)KeyCode.Joystick1Button0 + (playerIndex - 1) * 20;
        return (KeyCode)(baseIndex + buttonIndex);
    }
#endif

    private static bool AnyButtonPressed(int maxPlayers, Func<int, bool> predicate)
    {
        int limit = Mathf.Clamp(maxPlayers, 1, MaxPlayers);
        for (int playerIndex = 1; playerIndex <= limit; playerIndex++)
        {
            if (predicate(playerIndex))
                return true;
        }

        return false;
    }
}
