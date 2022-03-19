using Godot;

namespace GameObjects
{
    public class DotObject : BaseObject
    {
        protected int _radius = 30;

        public override void InitRandomObject()
        {
            int offset = _radius + 1;
            int positionX = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.PlayableSize[0] - offset);
            int positionY = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.PlayableSize[1] - offset);

            GlobalPosition = new Vector2(positionX, positionY);
        }

        public override void _Ready()
        {
            base._Ready();
        }

         protected override Vector2 FindPositionInPlayableArea()
        {
            float offset = _radius;
            if (GlobalPosition.x + offset > Globals.ScreenInfo.PlayableSize.x)
            {
                return new Vector2(Globals.ScreenInfo.PlayableSize.x - offset, GlobalPosition.y);
            }
            return GlobalPosition;
        }

    }
}