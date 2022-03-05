using Godot;


namespace BackgroundAndLevel
{
    public class LevelBarrier : Node2D
    {

        private CollisionShape2D _top;
        private CollisionShape2D _bottom;
        private CollisionShape2D _left;
        private CollisionShape2D _right;

        private int _yLimitOffset;
        public int YLimitOffset
        {
            get { return _yLimitOffset; }
            set { _yLimitOffset = value; }
        }

        private int _width = 100;

        public override void _Ready()
        {
            _top = GetNode<CollisionShape2D>("Top/CollisionShape2D");
            _bottom = GetNode<CollisionShape2D>("Bottom/CollisionShape2D");
            _right = GetNode<CollisionShape2D>("Right/CollisionShape2D");
            _left = GetNode<CollisionShape2D>("Left/CollisionShape2D");

            UpdateCollsionShapes(Globals.ScreenInfo.VisibleRectSize);
            Globals.ScreenInfo.UpdatePlayableSize(new Vector2(0,_yLimitOffset));
           
            GetViewport().Connect("size_changed", this, nameof(_on_viewport_size_changed));
        }

    
        private void UpdateCollsionShapes(Vector2 size)
        {
            int limitedSizeY = (int)size.y - _yLimitOffset;
            Vector2 topAndBottomSize = new Vector2(size.x / 2, _width);
            Vector2 rigthAndLeftSize = new Vector2(_width, limitedSizeY / 2);

            RectangleShape2D topRect = (RectangleShape2D)_top.Shape;
            topRect.Extents = topAndBottomSize;

            RectangleShape2D bottomRect = (RectangleShape2D)_bottom.Shape;
            bottomRect.Extents = topAndBottomSize;

            RectangleShape2D rightRect = (RectangleShape2D)_right.Shape;
            rightRect.Extents = rigthAndLeftSize;

            RectangleShape2D leftRect = (RectangleShape2D)_left.Shape;
            leftRect.Extents = rigthAndLeftSize;


            int offset = _width;
            int xCenter = (int)size.x / 2;
            int yCenter = (int)limitedSizeY / 2;

            _top.GlobalPosition = new Vector2(xCenter, -offset);
            _bottom.GlobalPosition = new Vector2(xCenter, limitedSizeY + offset);
            _right.GlobalPosition = new Vector2(size.x + offset, yCenter);
            _left.GlobalPosition = new Vector2(-offset, yCenter);
        }

        public void _on_viewport_size_changed()
        {
            Globals.ScreenInfo.UpdateScreenInfo(GetViewport());
            Globals.ScreenInfo.UpdatePlayableSize(new Vector2(0,_yLimitOffset));

            UpdateCollsionShapes(Globals.ScreenInfo.VisibleRectSize);
        }


    }
}