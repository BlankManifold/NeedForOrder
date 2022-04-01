using Godot;
using System;


namespace GameObjects
{
    public class RotatableObject : BaseObject, IRotatable
    {
        protected bool _rotatable = false;
        public bool Rotatable
        {
            get { return _rotatable; }
            set { _rotatable = value; }
        }
        protected bool _imOnRotationArea = false;


        protected KinematicBody2D _rotationArea;
        protected KinematicBody2D _rotationCollisionArea;
        protected CollisionShape2D _rotationAreaShape;

        [Export]
        protected Vector2 _rotationAreaInitialPos;
        protected float _rotationRadius;
        protected Vector2 _clickedRotationAreaRelativePosition;
        protected float _checkingRotationAreaAngle;
        // protected CollisionShape2D _rotationRadiusShape;

        public float RelevantRotationAngle { get; set; } = 0f;


        [Export]
        protected bool _rotationSnappable = true;
        [Export]
        private int _snappingAngle = 45;
        [Export]
        private int _snappingThresholdAngle = 3;



        public override void _Ready()
        {
            _rotationArea = (KinematicBody2D)FindNode("RotationArea");
            _rotationCollisionArea = (KinematicBody2D)FindNode("RotationCollisionArea");
            _rotationAreaShape = _rotationArea.GetNode<CollisionShape2D>("CollisionShape2D");

            _rotationArea.Position = _rotationAreaInitialPos;
            _rotationArea.Visible = false;
            _rotationAreaShape.Disabled = true;
            _rotationRadius = _rotationAreaInitialPos.Length();
            RelevantRotationAngle = GlobalRotation;
         
            base._Ready();
        }


        public override Godot.Collections.Dictionary<string, object> CreateDict()
        {

            Godot.Collections.Dictionary<string, object> dict = base.CreateDict();
            dict.Add("GlobalRotation", GlobalRotation);

            return dict;
        }
        public override void LoadData(Godot.Collections.Dictionary<string, object> data)
        {
            base.LoadData(data);
            GlobalRotation = (float)data["GlobalRotation"];
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

                //@event.Dispose();    
                return;
            }

        }
        protected virtual void HandleRotationInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent && IsInstanceValid(@event))
            {
                if (mouseButtonEvent.ButtonIndex == 1 && !mouseButtonEvent.IsPressed())
                {
                    _state = Globals.OBJECTSTATE.SELECTED;
                    s_someonePressed = false;

                    InputRotationReleased();

                    mouseButtonEvent.Dispose();
                    return;
                }

                if (mouseButtonEvent.ButtonIndex == 1 && mouseButtonEvent.IsPressed())
                {
                    if (_state != Globals.OBJECTSTATE.ROTATING)
                    {
                        _state = Globals.OBJECTSTATE.ROTATING;
                    }
        
                    InputRotationPressed();
                    mouseButtonEvent.Dispose();
                    return;
                }
            }

            if (@event is InputEventMouseMotion && IsInstanceValid(@event))
            {
                if (_state == Globals.OBJECTSTATE.ROTATING)
                {
                    SetUpRotation(GetGlobalMousePosition());
                }
                @event.Dispose();
                return;
            }
        }
        protected virtual void InputRotationReleased()
        {
            RelevantRotationAngle = GlobalRotation;
            _rotatable = false;
        }
        protected virtual void InputRotationPressed() { }
        protected override void InputMovementMotion(InputEventMouseMotion mouseMotion)
        {
            setupFollowMouse(mouseMotion.Position);
            CheckRotationAreaCollision(GlobalPosition);
        }
        


        public virtual void RotateObject()
        {
            float oldRotation = GlobalRotation;
            GlobalRotation = RelevantRotationAngle;

            KinematicCollision2D mainObjectCollision = MoveAndCollide(Vector2.Zero, testOnly: true);
            KinematicCollision2D rotationAreaCollision = _rotationCollisionArea.MoveAndCollide(Vector2.Zero, testOnly: true);

            if (CheckRotationCollision(mainObjectCollision) || CheckRotationCollision(rotationAreaCollision))
            {
                _rotationArea.Position = _rotationAreaInitialPos;
                GlobalRotation = oldRotation;
            }

        }
        public override void SelectObject()
        {
            base.SelectObject();

            _rotationAreaShape.Disabled = false;
            _rotationArea.ZIndex = 1;
            
            CheckRotationAreaCollision(GlobalPosition);
            _rotationArea.Visible = true;
        }
        public override void UnSelectObject()
        {
            base.UnSelectObject();

            _rotationArea.Visible = false;
            _rotationArea.ZIndex = 0;
            _rotationAreaShape.Disabled = true;
        }
        public virtual void ClickedOnRotationArea(InputEventMouseButton mouseButtonEvent)
        {
            _clickedRotationAreaRelativePosition = mouseButtonEvent.Position - _rotationArea.GlobalPosition;
            _rotatable = true;
            BaseObject.s_someonePressed = true;
        }


        protected virtual void SetUpRotation(Vector2 positionToFollow)
        {
            float lerpWeight = 0.4f;

            Vector2 referencePos = _rotationArea.GlobalPosition + _clickedRotationAreaRelativePosition - GlobalPosition;
            RelevantRotationAngle = GlobalRotation + Mathf.LerpAngle(0, referencePos.AngleTo(positionToFollow-GlobalPosition), lerpWeight);

            if (_rotationSnappable)
                SetUpRotationSnappping();
        }
        protected virtual bool SetUpRotationSnappping()
        {

            int roundAngleDegrees = (int)Math.Round(Mathf.Rad2Deg(RelevantRotationAngle));
            int quotient = Math.DivRem(Math.Abs(roundAngleDegrees), _snappingAngle, out int remainder);

            if (remainder < _snappingThresholdAngle)
            {
                RelevantRotationAngle = Mathf.Sign(roundAngleDegrees) * Mathf.Deg2Rad(quotient * _snappingAngle);
                return true;
            }
            if (remainder > _snappingAngle - _snappingThresholdAngle)
            {
                RelevantRotationAngle = Mathf.Sign(roundAngleDegrees) * Mathf.Deg2Rad((quotient + 1) * _snappingAngle);
                return true;
            }

            return false;
        }



        protected bool CheckRotationCollision(KinematicCollision2D collision)
        {
            if (collision != null)
            {
                collision.Dispose();
                return true;
            }
            return false;
        }
        protected void CheckRotationAreaCollision(Vector2 referencePos)
        {
            KinematicCollision2D collisionInfo = _rotationCollisionArea.MoveAndCollide(Vector2.Zero, testOnly: true);

            if (collisionInfo != null)
            {
                collisionInfo.Dispose();
                Vector2 intialDirection = referencePos.DirectionTo(_rotationArea.GlobalPosition);
                Vector2 dilatetedPos = _rotationArea.GlobalPosition + intialDirection * 20f;
                Vector2 checkingPos = dilatetedPos;

                for (int i = 1; i <= (int)(2*Mathf.Pi / _checkingRotationAreaAngle); i++)
                {
                    checkingPos = referencePos + (checkingPos - referencePos).Rotated(_checkingRotationAreaAngle);

                    if (Globals.ScreenInfo.VisibleRect.HasPoint(checkingPos))
                    {
                        _rotationArea.GlobalPosition = referencePos + referencePos.DirectionTo(checkingPos) * _rotationRadius;
                        _rotationArea.Rotation += i * _checkingRotationAreaAngle;
                        UpdateRotationAreaInitialPos(referencePos);
                        break;
                    }

                }
            }

        }
        protected virtual void UpdateRotationAreaInitialPos(Vector2 _)
        {
            _rotationAreaInitialPos = _rotationArea.Position;
        }



        public override string InfoString()
        {
            string text = $"STATE: {_state}";
            // text += $"\nInRotationArea: {_imOnRotationArea}\nRotatable: {_rotatable}";
            // text += $"\nSize: {GetViewport().GetVisibleRect().Size}";
            // text += $"\nRotation: {GlobalRotationDegrees}";
            // text += $"\nGlobal: {GlobalPosition}";
            // text += $"\nArea Local: {_rotationArea.Position}";
            // text += $"\nArea Global: {_rotationArea.GlobalPosition}";
            // text += $"\nHasPoint: {GetViewport().GetVisibleRect().HasPoint(_rotationArea.GlobalPosition)}";
            // text += $"\n CheckingPos: {_check.GlobalPosition}";
            // text += $"\n Angle:{Mathf.Rad2Deg(_check.Position.AngleTo(_rotationArea.Position))}";

            return text;
        }

    }
}
