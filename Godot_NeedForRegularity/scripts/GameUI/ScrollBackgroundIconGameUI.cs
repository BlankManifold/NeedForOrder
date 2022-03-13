using Godot;

namespace GameUI
{
    public class ScrollBackgroundIconGameUI : ScrollIconGameUI
    {
        [Export]
        private Texture _textureTile;
        public Texture TextureTile { get { return _textureTile; } }

    }
}