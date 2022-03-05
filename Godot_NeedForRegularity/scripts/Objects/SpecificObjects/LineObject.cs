using Godot;


namespace GameObjects

{
    // y = _m(x-_coefficients.x)+b
    public class LineObject : RotatableObject
    {
        private float _m = 0f;
        // private float _coefficients.x = 0f;

        private Vector2 _coefficients;
        // private float _coefficients.y = 0f;
        private int _width = 4;
        private float _angleDegrees;

        private Vector2 _lineCenter;
        private Vector2 _oldMousePosition;

        private Line2D _line;
        private CollisionShape2D _selectionAreaShape;

        public override void InitRandomObject()
        {
            int offset = _width / 2;
            int positionX = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.PlayableSize[0] - offset);
            int positionY = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.PlayableSize[1] - offset);

            _angleDegrees = Globals.RandomManager.rng.RandfRange(-89, 89);

            // GlobalPosition = new Vector2(positionX, positionY);
            // GlobalRotationDegrees = angleDegrees;

            
            _coefficients.x = positionX;
            _coefficients.y = positionY;
            _m = Mathf.Tan(Mathf.Deg2Rad(_angleDegrees));
            RelevantPosition = Vector2.Zero;

        }

        public override void _Ready()
        {
            _overlappaple = true;

            base._Ready();

            _line = GetNode<Line2D>("Line2D");
            _selectionAreaShape = GetNode<CollisionShape2D>("SelectionAreaShape");
            UpdateIntersectionPoints();
            UpdateShape();

            _oldMousePosition = Vector2.Zero;

            RectangleShape2D shape = (RectangleShape2D)_selectionAreaShape.Shape;
            shape.Extents = new Vector2(shape.Extents.x, Globals.ScreenInfo.PlayableSize.Length());
        }
        public override void setupFollowMouse(Vector2 relativeMotion)
        {
            RelevantPosition = _coefficients + relativeMotion - _clickedRelativePosition;
        }

        public override void MoveObject(float delta)
        {
            //Vector2 direction = (GlobalPosition).DirectionTo(RelevantPosition);
            //float speed = RelevantPosition.DistanceTo(GlobalPosition) * _quickness;

            //Vector2 linearVelocity = direction * speed;

            // MoveAndSlide(linearVelocity);
        

            Vector2 oldCoefficient = _coefficients;
            Vector2 direction = RelevantPosition.Normalized();//_coefficients.DirectionTo(RelevantPosition);
            float speed = RelevantPosition.Length();//_coefficients.DistanceTo(RelevantPosition);

            _coefficients += direction*speed;  

            UpdateIntersectionPoints();
            UpdateShape();

            int offset = 10;
            if (offset < _lineCenter.x && _lineCenter.x < Globals.ScreenInfo.PlayableSize.x - offset &&
                offset < _lineCenter.y && _lineCenter.y < Globals.ScreenInfo.PlayableSize.y - offset)
            {
                return;
            }

            _coefficients = oldCoefficient;
            RelevantPosition = _coefficients -  _clickedRelativePosition;

            UpdateIntersectionPoints();
            UpdateShape();

            // float x = Mathf.Clamp(RelevantPosition.x,0,Globals.ScreenInfo.VisibleRectSize.x);
            // float y = Mathf.Clamp(RelevantPosition.y,0,Globals.ScreenInfo.VisibleRectSize.y);
            // GlobalPosition = new Vector2(x,y);

        }

        public override void HandleMotionInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion && IsInstanceValid(@event) && _state != Globals.OBJECTSTATE.SELECTED)
            {
                if (_state == Globals.OBJECTSTATE.PRESSED)
                {
                    _state = Globals.OBJECTSTATE.MOVING;
                }

                setupFollowMouse(mouseMotion.Relative);
                // _oldMousePosition = mouseMotion.Position;

                // KinematicCollision2D collisionInfo = _rotationArea.MoveAndCollide(Vector2.Zero, testOnly: true);

                // if (collisionInfo != null)
                // {
                //     collisionInfo.Dispose();
                //     Vector2 intialDirection = GlobalPosition.DirectionTo(_rotationArea.GlobalPosition);
                //     Vector2 dilatetedPos = _rotationArea.GlobalPosition + intialDirection * 20f;
                //     Vector2 checkingPos = dilatetedPos;
                //     for (int i = 1; i <= 3; i++)
                //     {
                //         checkingPos = GlobalPosition + (checkingPos - GlobalPosition).Rotated(Mathf.Pi / 2);

                //         if (Globals.ScreenInfo.VisibleRect.HasPoint(checkingPos))
                //         {
                //             _rotationArea.GlobalPosition = GlobalPosition + GlobalPosition.DirectionTo(checkingPos) * _rotationRadius;
                //             _rotationAreaInitialPos = _rotationArea.Position;
                //             break;
                //         }

                //     }
                // }

                @event.Dispose();
                return;
            }


            if (@event is InputEventMouseButton mouseButtonEvent && IsInstanceValid(@event))
            {
                if (mouseButtonEvent.IsActionReleased("select"))
                {
                    _state = Globals.OBJECTSTATE.SELECTED;
                    s_someonePressed = false;

                    mouseButtonEvent.Dispose();
                    return;
                }

                if (mouseButtonEvent.IsActionPressed("select"))
                {
                    _state = Globals.OBJECTSTATE.PRESSED;
                    s_someonePressed = true;

                    _clickedRelativePosition = _coefficients;
                    mouseButtonEvent.Dispose();
                    return;
                }
            }

        }

        // private void CheckIsInbound(float x0, float b, float m)
        // {
        //     //int offset = 5;
        //     int maxX = (int)Globals.ScreenInfo.PlayableSize.x; //+ offset;
        //     int maxY = (int)Globals.ScreenInfo.PlayableSize.y;//+ offset;
        //     int minX = 0;//-offset
        //     int minY = 0;//-offset;

        //     float xTop = (maxY - b) / m + x0;
        //     float xBottom = (minY - b) / m + x0;
        //     float yRight = m * (maxX - _coefficients.x) + b;
        //     float yLeft = m * (minX - _coefficients.x) + b;

        //     int cont = 0;

        //     if (m > 0)
        //     {
        //         if (xTop <= maxX)
        //         {
        //             cont++;
        //         }
        //         else
        //         {
        //             cont++;
        //         }

        //         if (xBottom >= 0)
        //         {
        //             cont++;
        //         }
        //         else
        //         {
        //             cont++;
        //         }

        //         UpdateShape();

        //         return;
        //     }

        //     if (m < 0)
        //     {
        //         if (xTop >= 0)
        //         {
        //             _line.SetPointPosition(0, new Vector2(xTop, maxY));
        //             //set Point (xTop,maxY)
        //         }
        //         else
        //         {
        //             _line.SetPointPosition(0, new Vector2(minX, yLeft));
        //             // set Point (0, yLeft)
        //         }

        //         if (xBottom <= maxX)
        //         {
        //             _line.SetPointPosition(1, new Vector2(xBottom, minY));
        //             //set Point (xBottom,0)
        //         }
        //         else
        //         {
        //             _line.SetPointPosition(1, new Vector2(maxX, yRight));
        //             // set Point (maxX, yRight)
        //         }

        //        UpdateShape();

        //         return;
        //     }
        // }

        private void UpdateIntersectionPoints()
        {
            int offset = 5;
            int maxX = (int)Globals.ScreenInfo.PlayableSize.x + offset;
            int maxY = (int)Globals.ScreenInfo.PlayableSize.y + offset;
            int minX = -offset;
            int minY = -offset;

            float xTop = (maxY - _coefficients.y) / _m + _coefficients.x;
            float xBottom = (minY - _coefficients.y) / _m + _coefficients.x;
            float yRight = _m * (maxX - _coefficients.x) + _coefficients.y;
            float yLeft = _m * (minX - _coefficients.x) + _coefficients.y;


            if (_m > 0)
            {
                if (xTop <= maxX)
                {
                    _line.SetPointPosition(0, new Vector2(xTop, maxY));
                    //set Point (xTop,maxY)
                }
                else
                {
                    _line.SetPointPosition(0, new Vector2(maxX, yRight));
                    // set Point (maxX, yRight)
                }

                if (xBottom >= 0)
                {
                    _line.SetPointPosition(1, new Vector2(xBottom, minY));
                    //set Point (xBottom,0)
                }
                else
                {
                    _line.SetPointPosition(1, new Vector2(minX, yLeft));
                    // set Point (0, yLeft)
                }

                //UpdateShape();

                return;
            }

            if (_m < 0)
            {
                if (xTop >= 0)
                {
                    _line.SetPointPosition(0, new Vector2(xTop, maxY));
                    //set Point (xTop,maxY)
                }
                else
                {
                    _line.SetPointPosition(0, new Vector2(minX, yLeft));
                    // set Point (0, yLeft)
                }

                if (xBottom <= maxX)
                {
                    _line.SetPointPosition(1, new Vector2(xBottom, minY));
                    //set Point (xBottom,0)
                }
                else
                {
                    _line.SetPointPosition(1, new Vector2(maxX, yRight));
                    // set Point (maxX, yRight)
                }

                //UpdateShape();

                return;
            }
        }

        private void UpdateShape()
        {
            _lineCenter = (_line.Points[1] + _line.Points[0]) / 2;
            _selectionAreaShape.GlobalPosition = _lineCenter;
            _selectionAreaShape.GlobalRotationDegrees = 90 + _angleDegrees;

        }

    }

}