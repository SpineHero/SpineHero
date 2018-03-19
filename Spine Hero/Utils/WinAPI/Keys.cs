namespace SpineHero.Utils.WinAPI
{
    public static class Keys
    {
        // Modifiers
        public static readonly Key Alt = new Key { Code = 0x0001, Name = "Alt" };
        public static readonly Key Control = new Key { Code = 0x0002, Name = "Control" };
        public static readonly Key Shift = new Key { Code = 0x0004, Name = "Shift" };
        public static readonly Key Win = new Key { Code = 0x0008, Name = "Win" };

        // Normal keys
        public static readonly Key Escape = new Key {Code = 0x1B, Name = "Escape"};
        public static readonly Key Backspace = new Key {Code = 0x08, Name = "Backspace"};
    }

    public class Key
    {
        public uint Code { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}