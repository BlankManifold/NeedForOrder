using Godot;
using System;


namespace GameObjects

{
    public class SquareObject : RotatableObject
    {
        [Export]
        private int _lenght;
    
        private TextureRect _textureRect;
        

        protected override void InitRandomProperties()
        {
            int offset = (int)(_lenght/Mathf.Sqrt2) + 1;
            int positionX = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.PlayableSize[0] - offset);
            int positionY = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.PlayableSize[1] - offset);

            float angleDegrees = Globals.RandomManager.rng.RandfRange(0, 90);

            GlobalPosition = new Vector2(positionX, positionY);
            GlobalRotationDegrees = angleDegrees;
            RelevantRotationAngle = Mathf.Deg2Rad(angleDegrees); 
        }
        protected override void UpdateColor()
        {
            _textureRect.SelfModulate = _color;
        }


        public override void _Ready()
        {
            _textureRect = GetNode<TextureRect>("TextureRect");
            _textureRect.RectSize = new Vector2(_lenght, _lenght);
            _checkingRotationAreaAngle = Mathf.Pi / 2;
            
            base._Ready();
        }


        protected override Vector2 FindPositionInPlayableArea()
        {
            float offset = _lenght/Mathf.Sqrt2;
            if (GlobalPosition.x + offset > Globals.ScreenInfo.PlayableSize.x)
            {
                return new Vector2(Globals.ScreenInfo.PlayableSize.x - offset, GlobalPosition.y);
            }
            return GlobalPosition;
        }
    }


}