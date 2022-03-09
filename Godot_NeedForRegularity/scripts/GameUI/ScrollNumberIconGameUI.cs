using Godot;

namespace GameUI
{
    public class ScrollNumberIconGameUI : ScrollIconGameUI
    {
        [Export]
        private uint _number;
        public uint Number { get { return _number; } }

    }
}