using Godot;

namespace GameObjects
{
    public class DotObject : BaseObject
    {
        protected int _radius = 20;

        public override void InitRandomObject()
        {
            int offset = _radius / 2 + 1;
            int positionX = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.Size[0] - offset);
            int positionY = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.Size[1] - offset);

            GlobalPosition = new Vector2(positionX, positionY);
        }

        public override void _Ready()
        {
            base._Ready();
        }

    }
}