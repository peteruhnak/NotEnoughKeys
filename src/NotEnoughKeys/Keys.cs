using System.Diagnostics.CodeAnalysis;

namespace NotEnoughKeys;

public static class KeyMapper
{
    public static Modifiers KeysToModifiers(VirtualKey[] keys)
    {
        Modifiers modifiers = 0;
        if (keys.Contains(VirtualKey.Control))
            modifiers |= Modifiers.Control;
        if (keys.Contains(VirtualKey.Alt))
            modifiers |= Modifiers.Alt;
        if (keys.Contains(VirtualKey.Shift))
            modifiers |= Modifiers.Shift;
        return modifiers;
    }

    public static VirtualKey[] ModifiersToKeys(Modifiers modifiers)
    {
        var keys = new List<VirtualKey>();
        if ((modifiers & Modifiers.Control) != 0)
            keys.Add(VirtualKey.Control);
        if ((modifiers & Modifiers.Alt) != 0)
            keys.Add(VirtualKey.Alt);
        if ((modifiers & Modifiers.Shift) != 0)
            keys.Add(VirtualKey.Shift);
        return keys.ToArray();
    }

    public static VirtualKey FromString(string s)
    {
        if (s.Length == 1 && s[0] >= '0' && s[0] <= '9')
            s = "K" + s;
        else if (s.Equals("ctrl", StringComparison.InvariantCultureIgnoreCase))
            s = "Control";
        if (Enum.TryParse(s, true, out UsKey usKey))
            return (VirtualKey)usKey;
        if (!Enum.TryParse(s, true, out VirtualKey key))
            throw new ArgumentException($"Invalid Virtual Key '{s}'");
        return key;
    }

    public static bool IsExtended(VirtualKey virtualKey)
        => ExtendedKeys.Contains(virtualKey);

    private static readonly HashSet<VirtualKey> ExtendedKeys = new()
    {
        VirtualKey.Left,
        VirtualKey.Right,
        VirtualKey.Up,
        VirtualKey.Down,
        VirtualKey.Home,
        VirtualKey.End,
        VirtualKey.PageUp,
        VirtualKey.PageDown,
        VirtualKey.Insert,
        VirtualKey.Delete,
        VirtualKey.Apps,
        VirtualKey.Lwin,
        VirtualKey.Rwin
    };
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[Flags]
public enum Modifiers : uint
{
    Alt = 0x1,
    Control = 0x2,
    Shift = 0x4,
    Win = 0x8
}

[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public enum UsKey : ushort
{
    Semicolon = VirtualKey.Oem1,
    Slash = VirtualKey.Oem2,
    Tilde = VirtualKey.Oem3,
    BracketOpen = VirtualKey.Oem4,
    Backslash = VirtualKey.Oem5,
    BracketClose = VirtualKey.Oem6,
    Quote = VirtualKey.Oem7,
    Minus = VirtualKey.OemMinus,
    Plus = VirtualKey.OemPlus,
    Comma = VirtualKey.OemComma,
    Period = VirtualKey.OemPeriod
}

[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("Design", "CA1069:Enums values should not be duplicated")]
public enum VirtualKey : ushort
{
    K0 = 48,
    K1 = 49,
    K2 = 50,
    K3 = 51,
    K4 = 52,
    K5 = 53,
    K6 = 54,
    K7 = 55,
    K8 = 56,
    K9 = 57,
    A = 65,
    B = 66,
    C = 67,
    D = 68,
    E = 69,
    F = 70,
    G = 71,
    H = 72,
    I = 73,
    J = 74,
    K = 75,
    L = 76,
    M = 77,
    N = 78,
    O = 79,
    P = 80,
    Q = 81,
    R = 82,
    S = 83,
    T = 84,
    U = 85,
    V = 86,
    W = 87,
    X = 88,
    Y = 89,
    Z = 90,
    Lbutton = 1,
    Rbutton = 2,
    Cancel = 3,
    Mbutton = 4,
    Xbutton1 = 5,
    Xbutton2 = 6,
    Back = 8,
    Tab = 9,
    Clear = 12,
    Enter = 13,
    Return = 13,
    Shift = 16,
    Control = 17,
    Alt = 18,
    Menu = 18,
    Pause = 19,
    Capital = 20,
    CapsLock = 20,
    Kana = 21,
    Hangeul = 21,
    Hangul = 21,
    ImeOn = 22,
    Junja = 23,
    Final = 24,
    Hanja = 25,
    Kanji = 25,
    ImeOff = 26,
    Escape = 27,
    Convert = 28,
    Nonconvert = 29,
    Accept = 30,
    Modechange = 31,
    Space = 32,
    Prior = 33,
    PageUp = 33,
    Next = 34,
    PageDown = 34,
    End = 35,
    Home = 36,
    Left = 37,
    Up = 38,
    Right = 39,
    Down = 40,
    Select = 41,
    Print = 42,
    Execute = 43,
    Snapshot = 44,
    Insert = 45,
    Delete = 46,
    Help = 47,
    Lwin = 91,
    Rwin = 92,
    Apps = 93,
    Sleep = 95,
    Numpad0 = 96,
    Numpad1 = 97,
    Numpad2 = 98,
    Numpad3 = 99,
    Numpad4 = 100,
    Numpad5 = 101,
    Numpad6 = 102,
    Numpad7 = 103,
    Numpad8 = 104,
    Numpad9 = 105,
    Multiply = 106,
    Add = 107,
    Separator = 108,
    Subtract = 109,
    Decimal = 110,
    Divide = 111,
    F1 = 112,
    F2 = 113,
    F3 = 114,
    F4 = 115,
    F5 = 116,
    F6 = 117,
    F7 = 118,
    F8 = 119,
    F9 = 120,
    F10 = 121,
    F11 = 122,
    F12 = 123,
    F13 = 124,
    F14 = 125,
    F15 = 126,
    F16 = 127,
    F17 = 128,
    F18 = 129,
    F19 = 130,
    F20 = 131,
    F21 = 132,
    F22 = 133,
    F23 = 134,
    F24 = 135,
    NavigationView = 136,
    NavigationMenu = 137,
    NavigationUp = 138,
    NavigationDown = 139,
    NavigationLeft = 140,
    NavigationRight = 141,
    NavigationAccept = 142,
    NavigationCancel = 143,
    Numlock = 144,
    Scroll = 145,
    OemNecEqual = 146,
    OemFjJisho = 146,
    OemFjMasshou = 147,
    OemFjTouroku = 148,
    OemFjLoya = 149,
    OemFjRoya = 150,
    Lshift = 160,
    Rshift = 161,
    Lcontrol = 162,
    Rcontrol = 163,
    Lmenu = 164,
    Rmenu = 165,
    BrowserBack = 166,
    BrowserForward = 167,
    BrowserRefresh = 168,
    BrowserStop = 169,
    BrowserSearch = 170,
    BrowserFavorites = 171,
    BrowserHome = 172,
    VolumeMute = 173,
    VolumeDown = 174,
    VolumeUp = 175,
    MediaNextTrack = 176,
    MediaPrevTrack = 177,
    MediaStop = 178,
    MediaPlayPause = 179,
    LaunchMail = 180,
    LaunchMediaSelect = 181,
    LaunchApp1 = 182,
    LaunchApp2 = 183,
    Oem1 = 186,
    OemPlus = 187,
    OemComma = 188,
    OemMinus = 189,
    OemPeriod = 190,
    Oem2 = 191,
    Oem3 = 192,
    GamepadA = 195,
    GamepadB = 196,
    GamepadX = 197,
    GamepadY = 198,
    GamepadRightShoulder = 199,
    GamepadLeftShoulder = 200,
    GamepadLeftTrigger = 201,
    GamepadRightTrigger = 202,
    GamepadDpadUp = 203,
    GamepadDpadDown = 204,
    GamepadDpadLeft = 205,
    GamepadDpadRight = 206,
    GamepadMenu = 207,
    GamepadView = 208,
    GamepadLeftThumbstickButton = 209,
    GamepadRightThumbstickButton = 210,
    GamepadLeftThumbstickUp = 211,
    GamepadLeftThumbstickDown = 212,
    GamepadLeftThumbstickRight = 213,
    GamepadLeftThumbstickLeft = 214,
    GamepadRightThumbstickUp = 215,
    GamepadRightThumbstickDown = 216,
    GamepadRightThumbstickRight = 217,
    GamepadRightThumbstickLeft = 218,
    Oem4 = 219,
    Oem5 = 220,
    Oem6 = 221,
    Oem7 = 222,
    Oem8 = 223,
    OemAx = 225,
    Oem102 = 226,
    IcoHelp = 227,
    Ico00 = 228,
    Processkey = 229,
    IcoClear = 230,
    Packet = 231,
    OemReset = 233,
    OemJump = 234,
    OemPa1 = 235,
    OemPa2 = 236,
    OemPa3 = 237,
    OemWsctrl = 238,
    OemCusel = 239,
    OemAttn = 240,
    OemFinish = 241,
    OemCopy = 242,
    OemAuto = 243,
    OemEnlw = 244,
    OemBacktab = 245,
    Attn = 246,
    Crsel = 247,
    Exsel = 248,
    Ereof = 249,
    Play = 250,
    Zoom = 251,
    Noname = 252,
    Pa1 = 253,
    OemClear = 254
}