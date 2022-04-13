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
        private int _width = 8;
        private float _angleDegrees;
        private float _rotationAreaOffsetAngle = Mathf.Pi / 2;

        private Position2D _lineCenterNode;
        private Vector2 _oldLineCenterPos;
        private Vector2 _rightVector = Vector2.Right;

        private Line2D _line;
        private CollisionShape2D _selectionAreaShape;

        private Vector2 _oldPosition;




        protected override void InitRandomProperties()
        {
            int offset = _width / 2;
            int positionX = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.PlayableSize[0] - offset);
            int positionY = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.PlayableSize[1] - offset);

            _angleDegrees = Globals.RandomManager.rng.RandfRange(-Mathf.Pi / 2, Mathf.Pi / 2);

            _coefficients.x = positionX;
            _coefficients.y = positionY;
            _m = Mathf.Tan(_angleDegrees);
            RelevantPosition = Vector2.Zero;
            RelevantRotationAngle = _angleDegrees;
        }
        public override void RandomizeObject()
        {
            base.RandomizeObject();
            UpdateAll();
        }


        public override void _Ready()
        {
            _overlappaple = true;
            _checkingRotationAreaAngle = Mathf.Pi;


            _line = GetNode<Line2D>("Line2D");
            _selectionAreaShape = GetNode<CollisionShape2D>("SelectionAreaShape");
            _selectionAreaShape.Shape = (RectangleShape2D)_selectionAreaShape.Shape.Duplicate();

            _lineCenterNode = GetNode<Position2D>("LineCenter");

            base._Ready();

            UpdateAll();

            _oldLineCenterPos = _lineCenterNode.GlobalPosition;
            RelevantPosition = Vector2.Zero;
            RelevantRotationAngle = _angleDegrees;
        }




        public override Godot.Collections.Dictionary<string, object> CreateDict()
        {

            Godot.Collections.Dictionary<string, object> dict = base.CreateDict();

            dict.Add("AngleDegrees", _angleDegrees);
            dict.Add("CoeffientX", _coefficients.x);
            dict.Add("CoeffientY", _coefficients.y);
            dict.Add("AngularCoeff", _m);

            return dict;
        }
        public override void LoadData(Godot.Collections.Dictionary<string, object> data)
        {
            base.LoadData(data);
            _angleDegrees = (float)data["AngleDegrees"];
            _m = (float)data["AngularCoeff"];
            _coefficients = new Vector2((float)data["CoeffientX"], (float)data["CoeffientY"]);
        }



        public override void MoveObject(float delta)
        {
            Vector2 oldCoefficient = _coefficients;
            Vector2 direction = _coefficients.DirectionTo(RelevantPosition);
            float speed = _coefficients.DistanceTo(RelevantPosition) * _quickness;

            _coefficients += direction * speed * delta;
            // _coefficients = _coefficients.LinearInterpolate(RelevantPosition, 1f);

            UpdateAll();

            int offset = 10;
            if (offset < _lineCenterNode.GlobalPosition.x && _lineCenterNode.GlobalPosition.x < Globals.ScreenInfo.PlayableSize.x - offset &&
                offset < _lineCenterNode.GlobalPosition.y && _lineCenterNode.GlobalPosition.y < Globals.ScreenInfo.PlayableSize.y - offset)
            {
                return;
            }

            _coefficients = oldCoefficient;
            RelevantPosition = _coefficients;

            UpdateAll();
        }
        public override void RotateObject()
        {
            _angleDegrees = RelevantRotationAngle;
            _m = Mathf.Tan(_angleDegrees);
            UpdateIntersectionPoints();
            UpdateShape();
        }

        public override void SelectObject()
        {
            _state = Globals.OBJECTSTATE.PRESSED;

            _rotationAreaShape.Disabled = false;
            _rotationArea.ZIndex = 1;

            UpdateRotationArea(_lineCenterNode.GlobalPosition);
            CheckRotationAreaCollision(GlobalPosition);
            _rotationArea.Visible = true;
        }


        protected override void setupFollowMouse(Vector2 relativeMotion)
        {
            RelevantPosition = _coefficients + relativeMotion;
        }
        protected override void SetUpRotation(Vector2 positionToFollow)
        {
            float lerpWeight = 0.4f;

            Vector2 referencePos = _clickedRotationAreaRelativePosition + _rotationArea.GlobalPosition - _oldLineCenterPos;
            Vector2 currentPos = positionToFollow - _oldLineCenterPos;
            float deltaAngle = Mathf.LerpAngle(0, referencePos.AngleTo(currentPos), lerpWeight);

            float oldRotation = RelevantRotationAngle;
            RelevantRotationAngle = (_angleDegrees + deltaAngle) % (2 * Mathf.Pi);
            UpdateRotationArea(_oldLineCenterPos);

            KinematicCollision2D rotationAreaCollision = _rotationCollisionArea.MoveAndCollide(Vector2.Zero, testOnly: true);
            if (CheckRotationCollision(rotationAreaCollision))
            {
                RelevantRotationAngle = oldRotation;
                UpdateRotationArea(_oldLineCenterPos);
                return;
            }

            if (_rotationSnappable)
            {
                if (SetUpRotationSnappping())
                {
                    UpdateRotationArea(_oldLineCenterPos);
                }
            }
        }



        protected override void InputRotationPressed()
        {
            _coefficients = _oldLineCenterPos;
        }
        protected override void InputRotationReleased()
        {
            RelevantRotationAngle = _angleDegrees;
            _rotatable = false;
        }
        protected override void InputMovementMotion(InputEventMouseMotion mouseMotion)
        {
            Vector2 relative = mouseMotion.Position-_oldPosition;
            _oldPosition = mouseMotion.Position;
            
            setupFollowMouse(relative);
            CheckRotationAreaCollision(_lineCenterNode.GlobalPosition);
        }
        protected override void InputMovementPressed(InputEventMouseButton mouseButtonEvent) 
        {
            _oldPosition = mouseButtonEvent.Position; 
        }
        protected override void InputMovementReleased()
        {
            _oldLineCenterPos = _lineCenterNode.GlobalPosition;
        }


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
                }
                else
                {
                    _line.SetPointPosition(0, new Vector2(maxX, yRight));
                }

                if (xBottom >= 0)
                {
                    _line.SetPointPosition(1, new Vector2(xBottom, minY));
                }
                else
                {
                    _line.SetPointPosition(1, new Vector2(minX, yLeft));
                }

                return;
            }

            if (_m < 0)
            {
                if (xTop >= 0)
                {
                    _line.SetPointPosition(0, new Vector2(xTop, maxY));
                }
                else
                {
                    _line.SetPointPosition(0, new Vector2(minX, yLeft));
                }

                if (xBottom <= maxX)
                {
                    _line.SetPointPosition(1, new Vector2(xBottom, minY));
                }
                else
                {
                    _line.SetPointPosition(1, new Vector2(maxX, yRight));
                }

                return;
            }

            if (_m == 0)
            {
                _line.SetPointPosition(0, new Vector2(minX, yLeft));
                _line.SetPointPosition(1, new Vector2(maxX, yRight));

                return;
            }
        }
        private void UpdateShape()
        {
            _lineCenterNode.GlobalPosition = (_line.Points[1] + _line.Points[0]) / 2;
            _selectionAreaShape.GlobalPosition = _lineCenterNode.GlobalPosition;
            _selectionAreaShape.GlobalRotation = Mathf.Pi / 2 + _angleDegrees;

            RectangleShape2D shape = (RectangleShape2D)_selectionAreaShape.Shape;
            shape.Extents = new Vector2(shape.Extents.x, _lineCenterNode.GlobalPosition.DistanceTo(_line.Points[0]));

        }
        private void UpdateRotationArea(Vector2 pivotPoint)
        {
            //_rotationArea.GlobalPosition = _lineCenterNode.GlobalPosition + _rotationAreaInitialPos;
            _rotationArea.GlobalPosition = pivotPoint + _rightVector.Rotated(_rotationAreaOffsetAngle + RelevantRotationAngle) * _rotationRadius;
            _rotationArea.GlobalRotation = _rotationAreaOffsetAngle + RelevantRotationAngle;

        }
        private void UpdateAll()
        {
            UpdateIntersectionPoints();
            UpdateShape();
            UpdateRotationArea(_lineCenterNode.GlobalPosition);
        }
        protected override void UpdateRotationAreaInitialPos(Vector2 referencePos)
        {
            _rotationAreaInitialPos = _rotationArea.GlobalPosition - referencePos;
            _rotationAreaOffsetAngle += _checkingRotationAreaAngle;
        }
        public override void UpdateToValidPosition()
        {
            UpdateAll();
        }
        protected override void UpdateColor()
        {
            _line.SelfModulate = _color;
        }

    }

}