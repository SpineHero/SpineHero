namespace SpineHero.Utils.WinAPI
{
    public class Shortcut
    {
        public Key Modifier { get; set; }

        public Key Key { get; set; }

        public Shortcut(Key modifier, Key key)
        {
            Modifier = modifier;
            Key = key;
        }

        public override string ToString()
        {
            return $"{Modifier} + {Key}";
        }
    }
}