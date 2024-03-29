namespace Pie.Windowing;

/// <summary>
/// Provides various SDL helpers, useful internally in Pie.
/// </summary>
public static class SdlHelper
{
    /// <summary>
    /// Convert an SDL keycode to Pie <see cref="Key"/>.
    /// </summary>
    /// <param name="keycode">The SDL keycode to convert.</param>
    /// <returns>The appropriate Pie <see cref="Key"/>. If an unknown key is provided, <see cref="Key.Unknown"/> is returned.</returns>
    public static Key KeycodeToKey(uint keycode)
    {
        const uint ScancodeMask = 1 << 30;
        
        return keycode switch
        {
            '\r' => Key.Return,
            '\x1B' => Key.Escape,
            '\b' => Key.Backspace,
            '\t' => Key.Tab,
            ' ' => Key.Space,
            '#' => Key.Hash,
            '\'' => Key.Apostrophe,
            ',' => Key.Comma,
            '-' => Key.Minus,
            '.' => Key.Period,
            '/' => Key.ForwardSlash,
            '0' => Key.Num0,
            '1' => Key.Num1,
            '2' => Key.Num2,
            '3' => Key.Num3,
            '4' => Key.Num4,
            '5' => Key.Num5,
            '6' => Key.Num6,
            '7' => Key.Num7,
            '8' => Key.Num8,
            '9' => Key.Num9,
            ';' => Key.Semicolon,
            '=' => Key.Equals,
            
            '[' => Key.LeftBracket,
            ']' => Key.RightBracket,
            '\\' => Key.Backslash,
            
            '`' => Key.Backquote,
            
            'a' => Key.A,
            'b' => Key.B,
            'c' => Key.C,
            'd' => Key.D,
            'e' => Key.E,
            'f' => Key.F,
            'g' => Key.G,
            'h' => Key.H,
            'i' => Key.I,
            'j' => Key.J,
            'k' => Key.K,
            'l' => Key.L,
            'm' => Key.M,
            'n' => Key.N,
            'o' => Key.O,
            'p' => Key.P,
            'q' => Key.Q,
            'r' => Key.R,
            's' => Key.S,
            't' => Key.T,
            'u' => Key.U,
            'v' => Key.V,
            'w' => Key.W,
            'x' => Key.X,
            'y' => Key.Y,
            'z' => Key.Z,
            
            '\x7F' => Key.Delete,
            
            57 | ScancodeMask => Key.CapsLock,
            
            58 | ScancodeMask => Key.F1,
            59 | ScancodeMask => Key.F2,
            60 | ScancodeMask => Key.F3,
            61 | ScancodeMask => Key.F4,
            62 | ScancodeMask => Key.F5,
            63 | ScancodeMask => Key.F6,
            64 | ScancodeMask => Key.F7,
            65 | ScancodeMask => Key.F8,
            66 | ScancodeMask => Key.F9,
            67 | ScancodeMask => Key.F10,
            68 | ScancodeMask => Key.F11,
            69 | ScancodeMask => Key.F12,
            104 | ScancodeMask => Key.F13,
            105 | ScancodeMask => Key.F14,
            106 | ScancodeMask => Key.F15,
            107 | ScancodeMask => Key.F16,
            108 | ScancodeMask => Key.F17,
            109 | ScancodeMask => Key.F18,
            110 | ScancodeMask => Key.F19,
            111 | ScancodeMask => Key.F20,
            112 | ScancodeMask => Key.F21,
            113 | ScancodeMask => Key.F22,
            114 | ScancodeMask => Key.F23,
            115 | ScancodeMask => Key.F24,
            
            70 | ScancodeMask => Key.PrintScreen,
            71 | ScancodeMask => Key.ScrollLock,
            72 | ScancodeMask => Key.Pause,
            73 | ScancodeMask => Key.Insert,
            
            74 | ScancodeMask => Key.Home,
            75 | ScancodeMask => Key.PageUp,
            77 | ScancodeMask => Key.End,
            78 | ScancodeMask => Key.PageDown,
            79 | ScancodeMask => Key.Right,
            80 | ScancodeMask => Key.Left,
            81 | ScancodeMask => Key.Down,
            82 | ScancodeMask => Key.Up,
            
            83 | ScancodeMask => Key.NumLock,
            84 | ScancodeMask => Key.KeypadDivide,
            85 | ScancodeMask => Key.KeypadMultiply,
            86 | ScancodeMask => Key.KeypadSubtract,
            87 | ScancodeMask => Key.KeypadAdd,
            88 | ScancodeMask => Key.KeypadEnter,
            
            89 | ScancodeMask => Key.Keypad1,
            90 | ScancodeMask => Key.Keypad2,
            91 | ScancodeMask => Key.Keypad3,
            92 | ScancodeMask => Key.Keypad4,
            93 | ScancodeMask => Key.Keypad5,
            94 | ScancodeMask => Key.Keypad6,
            95 | ScancodeMask => Key.Keypad7,
            96 | ScancodeMask => Key.Keypad8,
            97 | ScancodeMask => Key.Keypad9,
            98 | ScancodeMask => Key.Keypad0,
            99 | ScancodeMask => Key.KeypadDecimal,
            
            101 | ScancodeMask => Key.Menu,
            103 | ScancodeMask => Key.KeypadEqual,

            224 | ScancodeMask => Key.LeftControl,
            225 | ScancodeMask => Key.LeftShift,
            226 | ScancodeMask => Key.LeftAlt,
            227 | ScancodeMask => Key.LeftSuper,
            228 | ScancodeMask => Key.RightControl,
            229 | ScancodeMask => Key.RightShift,
            230 | ScancodeMask => Key.RightAlt,
            231 | ScancodeMask => Key.RightSuper,

            _ => Key.Unknown
        };
    }
}