using System;

namespace Id4Eax4Patcher
{
    public class GameListItem
    {
        public GameType Type { get; set; }
        public string Name { get; set; }
        public string ExeName { get; set; }

        public GameListItem()
        {
            Type = GameType.None;
            Name = Type.ToString();
            ExeName = string.Empty;
        }

        public GameListItem(GameType type, string name, string exeName)
        {
            Type = type;
            Name = name;
            ExeName = exeName;
        }
    }
}
