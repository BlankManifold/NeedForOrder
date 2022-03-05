using Godot;


namespace GameObjects
{
    public class BaseObject : KinematicBody2D, IBaseObject
    {
        [Export]
        protected float _quickness = 70;
        [Export]
        protected bool _overlappaple = true;

        protected Vector2 _clickedRelativePosition;
        public Vector2 ClickedRelativePosition
        {
            get { return _clickedRelativePosition; }
            set { _clickedRelativePosition = value; }
        }

        public static bool s_selectable = true;
        public static bool s_someonePressed = false;
        public static BaseObject s_selectedObject = null;
        public static BaseObject s_hoveredObject = null;

      
       
        public Vector2 RelevantPosition { get; set; }
        protected Globals.OBJECTSTATE _state;
        public Globals.OBJECTSTATE State
        {
            get { return _state; }
            set { _state = value; }
        }

        private Node2D _main;

        [Signal]
        public delegate void UpdateSelection(bool eliminateOldSelection);
       

        public virtual void Init(Vector2 position)
        {
            GlobalPosition = position;
        }
        public virtual void InitRandomObject()
        {
            uint positionX = GD.Randi() % (uint)Globals.ScreenInfo.PlayableSize[0];
            uint positionY = GD.Randi() % (uint)Globals.ScreenInfo.PlayableSize[1];

            GlobalPosition = new Vector2(positionX, positionY);
        }

        // public override void _Process(float delta)
        // {
        //     if (_state >= Globals.OBJECTSTATE.SELECTED)
        //         Modulate = new Color(0, 0, 0);
        //     else
        //         Modulate = new Color(1, 1, 1);

        //     if (s_hoveredObject == this && _state < Globals.OBJECTSTATE.SELECTED)
        //         Modulate = new Color(1, 0, 0);
        // }

        public override void _Ready()
        {
            RelevantPosition = GlobalPosition;
            // Modulate = new Color(GD.Randf(), GD.Randf(), GD.Randf());

            if (!_overlappaple)
            {
                CollisionMask += 1;
            }

        }

        public virtual void InputControlFlow(InputEvent @event)
        {
            //IF UNSELECTED -> DO NOTHING SELECTION IN HANDLED IN MAIN
            if (_state == Globals.OBJECTSTATE.UNSELECTED)
            {
                return;
            }

            // IF NOT UNSELECTED -> HANDLE MOVING
            if (_state != Globals.OBJECTSTATE.UNSELECTED)
            {
                
                HandleMotionInput(@event);
                   
                return;
            }
        }

        public virtual void SelectObject()
        {
            _state = Globals.OBJECTSTATE.PRESSED;
        }
        public virtual void UnSelectObject()
        {
            _state = Globals.OBJECTSTATE.UNSELECTED;
        }
        public virtual void MoveObject(float _)
        {
            Vector2 direction = (GlobalPosition).DirectionTo(RelevantPosition);
            float speed = RelevantPosition.DistanceTo(GlobalPosition) * _quickness;

            MoveAndSlide(direction * speed);
            // float x = Mathf.Clamp(RelevantPosition.x,0,Globals.ScreenInfo.VisibleRectSize.x);
            // float y = Mathf.Clamp(RelevantPosition.y,0,Globals.ScreenInfo.VisibleRectSize.y);
            // GlobalPosition = new Vector2(x,y);

        }

        public virtual void setupFollowMouse(Vector2 followPosition)
        {
            RelevantPosition = followPosition - _clickedRelativePosition;
        }

        public virtual void HandleMotionInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion && IsInstanceValid(@event) && _state != Globals.OBJECTSTATE.SELECTED)
            {
                if (_state == Globals.OBJECTSTATE.PRESSED)
                {
                    _state = Globals.OBJECTSTATE.MOVING;
                }

                setupFollowMouse(mouseMotion.Position);

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

        public virtual string InfoString()
        {
            string text =  $"STATE: {_state}\n Selectable: {s_selectable}";


            return text;
        }

       
    }

}




