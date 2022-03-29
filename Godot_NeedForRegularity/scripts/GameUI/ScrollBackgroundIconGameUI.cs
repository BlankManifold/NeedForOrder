using Godot;

namespace GameUI
{
    public class ScrollBackgroundIconGameUI : ScrollIconGameUI
    {
        [Export]
        private Texture _textureTile;

        [Export]
        private Vector2 _offset;
        
        public Texture TextureTile { get { return _textureTile; } }
        public Vector2 Offset { get { return _offset; } }

    }
}