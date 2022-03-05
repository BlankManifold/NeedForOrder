using Godot;


namespace GameObjects
{
    public class RotatableObject : BaseObject, IRotatable
    {
        protected KinematicBody2D _rotationArea;

        protected bool _rotatable = false;
        protected bool _imOnRotationArea = false;
        protected bool _lockedRotation = false;
        public bool ImOnRotationArea
        {
            get { return _imOnRotationArea; }
        }

        private Vector2 _rotationAreaInitialPos = new Vector2(70, 70);

        protected float _rotationRadius;
        // protected CollisionShape2D _rotationRadiusShape;
        public float RelevantRotationAngle { get; set; } = 0f;
        private float _oldRelevantRotationAngle = 0f;

        public override void _Ready()
        {
            base._Ready();

            _rotationArea = GetNode<KinematicBody2D>("RotationArea");
            _rotationArea.Position = _rotationAreaInitialPos;
            _rotationArea.Visible = false;
            _rotationRadius = _rotationAreaInitialPos.Length();
            RelevantRotationAngle = GlobalRotation;


            // _rotationRadiusShape = GetNode<CollisionShape2D>("RotationAreaRadius2D/RotationRadiusShape");
            // CircleShape2D circleRadiusShape = (CircleShape2D)_rotationRadiusShape.Shape;
            // circleRadiusShape.Radius = _rotationRadius;

        }

        // public override void _PhysicsProcess(float delta)
        // {
        //     // if (_state == Globals.OBJECTSTATE.ROTATING)
        //     // {
        //     //     _lockedRotation = _rotationArea.IsOnWall();
        //     // }

        // }

        protected virtual void setUpRotation(Vector2 positionToFollow)
        {
            float lerpWeight = 0.4f;

            RelevantRotationAngle = GlobalRotation + Mathf.LerpAngle(0, _rotationAreaInitialPos.AngleTo(positionToFollow), lerpWeight);
        }

        public virtual void RotateObject()
        {
            float oldRotation = GlobalRotation;
            //float _oldRelevantRotationAngle = RelevantRotationAngle;
            GlobalRotation = RelevantRotationAngle;

            KinematicCollision2D mainObjectCollision = MoveAndCollide(Vector2.Zero, testOnly: true);
            KinematicCollision2D rotationAreaCollision = _rotationArea.MoveAndCollide(Vector2.Zero, testOnly: true);

            if (CheckRotationCollision(mainObjectCollision) || CheckRotationCollision(rotationAreaCollision))
            {
                _rotationArea.Position = _rotationAreaInitialPos;
                GlobalRotation = oldRotation;
            }

        }

        public bool CheckRotationCollision(KinematicCollision2D collision)
        {
            if (collision != null)
            {
                collision.Dispose();
                return true;
            }
            return false;
        }

        public override void InputControlFlow(InputEvent @event)
        {
            //IF UNSELECTED -> DO NOTHING SELECTION IN HANDLED IN MAIN
            if (_state == Globals.OBJECTSTATE.UNSELECTED)
            {
                return;
            }

            // IF NOT UNSELECTED -> HANDLE MOVING
            if (_state != Globals.OBJECTSTATE.UNSELECTED)
            {
                // IF NOT ON ROTATION AREA -> CHECK IF U WANT TO MOVE
                if (!_rotatable)
                    HandleMotionInput(@event);
                // IF ON ROTATION AREA -> CHECK IF U WANT TO ROTATE
                else
                    HandleRotationInput(@event);
                return;
            }

        }

        public override void SelectObject()
        {
            base.SelectObject();
            _rotationArea.Visible = true;
        }
        public override void UnSelectObject()
        {
            base.UnSelectObject();
            _rotationArea.Visible = false;
        }
        public void HandleRotationInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent && IsInstanceValid(@event))
            {
                if (mouseButtonEvent.IsActionReleased("select"))
                {
                    _state = Globals.OBJECTSTATE.SELECTED;
                    s_someonePressed = false;
                    RelevantRotationAngle = GlobalRotation;

                    if (!_imOnRotationArea)
                    {
                        _rotatable = false;
                        s_selectable = true;
                    }
                    // _rotationRadiusShape.Disabled = true;

                    mouseButtonEvent.Dispose();
                    return;
                }

                if (mouseButtonEvent.IsActionPressed("select"))
                {
                    if (_state != Globals.OBJECTSTATE.ROTATING)
                    {
                        _state = Globals.OBJECTSTATE.ROTATING;
                        // _rotationRadiusShape.Disabled = false;
                    }

                    //SETUP ROTATION TO BE CALLED IN MAIN

                    mouseButtonEvent.Dispose();
                    return;
                }
            }

            if (@event is InputEventMouseMotion && IsInstanceValid(@event))
            {
                if (_state == Globals.OBJECTSTATE.ROTATING)
                {
                    setUpRotation(GetLocalMousePosition());
                }
                @event.Dispose();
                return;
            }
        }

        public override void HandleMotionInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion && IsInstanceValid(@event) && _state != Globals.OBJECTSTATE.SELECTED)
            {
                if (_state == Globals.OBJECTSTATE.PRESSED)
                {
                    _state = Globals.OBJECTSTATE.MOVING;
                }

                setupFollowMouse(mouseMotion.Position);

                KinematicCollision2D collisionInfo = _rotationArea.MoveAndCollide(Vector2.Zero, testOnly: true);

                if (collisionInfo != null)
                {
                    collisionInfo.Dispose();
                    Vector2 intialDirection = GlobalPosition.DirectionTo(_rotationArea.GlobalPosition);
                    Vector2 dilatetedPos = _rotationArea.GlobalPosition + intialDirection * 20f;
                    Vector2 checkingPos = dilatetedPos;
                    for (int i = 1; i <= 3; i++)
                    {
                        checkingPos = GlobalPosition + (checkingPos - GlobalPosition).Rotated(Mathf.Pi / 2);

                        if (Globals.ScreenInfo.VisibleRect.HasPoint(checkingPos))
                        {
                            _rotationArea.GlobalPosition = GlobalPosition + GlobalPosition.DirectionTo(checkingPos) * _rotationRadius;
                            _rotationAreaInitialPos = _rotationArea.Position;
                            break;
                        }

                    }
                }

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

                    _clickedRelativePosition = mouseButtonEvent.Position - GlobalPosition;
                    mouseButtonEvent.Dispose();
                    return;
                }
            }

        }
        public void _on_RotationArea_mouse_exited()
        {
            if (_state != Globals.OBJECTSTATE.UNSELECTED)
            {
                _imOnRotationArea = false;

                if (_state != Globals.OBJECTSTATE.ROTATING)
                {
                    s_selectable = true;
                    _rotatable = false;
                }
            }
        }
        public void _on_RotationArea_mouse_entered()
        {
            if (_state != Globals.OBJECTSTATE.UNSELECTED)
            {
                _imOnRotationArea = true;

                if (_state == Globals.OBJECTSTATE.SELECTED)
                {
                    s_selectable = false;
                    _rotatable = true;
                }
            }
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
            // text += $"\n CheckingPos: {_check.GlobalPosition}";
            // text += $"\n Angle:{Mathf.Rad2Deg(_check.Position.AngleTo(_rotationArea.Position))}";

            return text;
        }

    }
}
