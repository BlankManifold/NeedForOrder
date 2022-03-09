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

        private Position2D _lineCenterNode;
        private Vector2 _oldLineCenterPos;

        private Line2D _line;
        private CollisionShape2D _selectionAreaShape;

        private float _delta;
        


        public override void InitRandomObject()
        {
            int offset = _width / 2;
            int positionX = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.PlayableSize[0] - offset);
            int positionY = Globals.RandomManager.rng.RandiRange(offset, (int)Globals.ScreenInfo.PlayableSize[1] - offset);

            _angleDegrees = Globals.RandomManager.rng.RandfRange(-Mathf.Pi / 2, Mathf.Pi / 2);

            // GlobalPosition = new Vector2(positionX, positionY);
            // GlobalRotationDegrees = angleDegrees;


            _coefficients.x = positionX;
            _coefficients.y = positionY;
            _m = Mathf.Tan(_angleDegrees);
            RelevantPosition = Vector2.Zero;

        }


        public override void _Ready()
        {
            _overlappaple = true;

            base._Ready();

            _line = GetNode<Line2D>("Line2D");
            _selectionAreaShape = GetNode<CollisionShape2D>("SelectionAreaShape");
            _selectionAreaShape.Shape = (RectangleShape2D)_selectionAreaShape.Shape.Duplicate();

            _lineCenterNode = GetNode<Position2D>("LineCenter");
            UpdateAll();

            RelevantRotationAngle = _angleDegrees;

        }
        public override void _Process(float delta)
        {
            base._Process(delta);

            if (_state >= Globals.OBJECTSTATE.SELECTED)
                _line.DefaultColor = new Color(0, 0, 0);

            else
                _line.DefaultColor = new Color(1, 1, 1);

            if (_imOnRotationArea)
                _rotationArea.Modulate = new Color(0, 0, 0);
            else
                _rotationArea.Modulate = new Color(1, 1, 1);

        }



        public override void MoveObject(float delta)
        {
            Vector2 oldCoefficient = _coefficients;
            Vector2 direction = _coefficients.DirectionTo(RelevantPosition);
            float speed = _coefficients.DistanceTo(RelevantPosition);

            _coefficients += direction * speed;

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
            float oldRotation = _angleDegrees;
            _angleDegrees = RelevantRotationAngle;

            KinematicCollision2D rotationAreaCollision = _rotationArea.MoveAndCollide(Vector2.Zero, testOnly: true);

            if (CheckRotationCollision(rotationAreaCollision))
            {
                _rotationArea.GlobalPosition = _oldLineCenterPos + _rotationAreaInitialPos;
                _angleDegrees = oldRotation;
                return;
            }

            _m = Mathf.Tan(_angleDegrees);
            UpdateIntersectionPoints();
            UpdateShape();
        }



        protected override void setupFollowMouse(Vector2 relativeMotion)
        {
            RelevantPosition = _coefficients + relativeMotion;
        }
        protected override void SetUpRotation(Vector2 positionToFollow)
        {
            float lerpWeight = 0.4f;

            Vector2 referencePos = _rotationArea.GlobalPosition - _oldLineCenterPos;
            Vector2 currentPos = positionToFollow - _oldLineCenterPos;
            float deltaAngle = Mathf.LerpAngle(0, referencePos.AngleTo(currentPos), lerpWeight);

            _delta = deltaAngle;

            Vector2 oldRotationAreaPosition = _rotationArea.GlobalPosition;
            _rotationArea.GlobalPosition = _oldLineCenterPos + referencePos.Rotated(deltaAngle);

            KinematicCollision2D rotationAreaCollision = _rotationArea.MoveAndCollide(Vector2.Zero, testOnly: true);
            if (CheckRotationCollision(rotationAreaCollision))
            {
                _rotationArea.GlobalPosition = oldRotationAreaPosition;
                deltaAngle = 0f;
            }

            RelevantRotationAngle = (_angleDegrees + deltaAngle) % (2 * Mathf.Pi);

            if (_rotationSnappable)
            {
                if (SetUpRotationSnappping())
                {
                    _rotationArea.GlobalPosition = oldRotationAreaPosition;
                }
            }
        }
        private void SetUpCoefficientsForRotation()
        {
            _coefficients = _oldLineCenterPos;
        }



        protected override void InputRotationPressed()
        {

            _oldLineCenterPos = _lineCenterNode.GlobalPosition;
            SetUpCoefficientsForRotation();

        }
        protected override void InputRotationReleased()
        {
            RelevantRotationAngle = _angleDegrees;
        }
        protected override void InputMovementMotion(InputEventMouseMotion mouseMotion)
        {
            setupFollowMouse(mouseMotion.Relative);
            UpdateRotationAreaInitialPos(_lineCenterNode.GlobalPosition);
        }
        protected override void InputMovementPressed(InputEventMouseButton _) { }



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
        private void UpdateRotationArea()
        {
            _rotationArea.GlobalPosition = _lineCenterNode.GlobalPosition + _rotationAreaInitialPos;
        }
        private void UpdateAll()
        {
            UpdateIntersectionPoints();
            UpdateShape();
            UpdateRotationArea();
        }



        public override string InfoString()
        {
            string text = $"STATE: {_state} \nSelectable: {s_selectable}";
            text = $"\nInRotationArea: {_imOnRotationArea}\nRotatable: {_rotatable}";
            text += $"\nSize: {GetViewport().GetVisibleRect().Size}";
            text += $"\nRotation: {GlobalRotationDegrees}";
            text += $"\nGlobal: {GlobalPosition}";
            text += $"\nArea Local: {_rotationArea.Position}";
            text += $"\nArea Global: {_rotationArea.GlobalPosition}";
            text += $"\nHasPoint: {GetViewport().GetVisibleRect().HasPoint(_rotationArea.GlobalPosition)}";
            text += $"\n AngleDegrees: {_angleDegrees}";
            text += $"\n _m: {_m}";
            text += $"\n _delta: {_delta}";
            text += $"\n RelativePos: {GetLocalMousePosition() - _oldLineCenterPos}";
            text += $"\n CurrentPivot:  {_oldLineCenterPos}";
            text += $"\n CurrentCenter:  {_lineCenterNode.GlobalPosition}";
            // text += $"\n Angle:{Mathf.Rad2Deg(_check.Position.AngleTo(_rotationArea.Position))}";

            return text;
        }

    }

}