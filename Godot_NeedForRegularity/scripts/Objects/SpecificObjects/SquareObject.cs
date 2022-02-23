
using Godot;


namespace GameObjects

{
    public class SquareObject : RotatableObject, IScalable
    {
        [Export(PropertyHint.Range, "0,100")]
        private int _lenght = 100;
        public int Lenght
        {
            get { return _lenght; }
            private set
            {
                _lenght = value;
                _colorRect.RectSize = new Vector2(_lenght, _lenght);
            }
        }
        

        [Export]
        private int _standardLenght = 100;

        private ColorRect _colorRect;


        public override void _Ready()
        {
            base._Ready();
            _colorRect = GetNode<ColorRect>("ColorRect");
        }

        public void ScaleObject(float scale) 
        { 
            int newLenght = (int) (_standardLenght * scale);
            _lenght = newLenght;
        }


    }


}