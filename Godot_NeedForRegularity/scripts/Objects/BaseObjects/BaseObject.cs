using Godot;
using System;

namespace GameObjects
{
    public class BaseObject : KinematicBody2D, IBaseObject
    {
        [Export]
        protected float _quickness = 70;
        [Export]
        protected bool _overlappaple = true;


        public static bool s_someonePressed = false;
        public static BaseObject s_selectedObject = null;
        


        public Vector2 RelevantPosition { get; set; }
        protected Globals.OBJECTSTATE _state;
        public Globals.OBJECTSTATE State
        {
            get { return _state; }
            set { _state = value; }
        }

        protected Color _color;
        public Color ObjectColor {get { return _color; }}

        protected Vector2 _clickedRelativePosition;
        protected Tween _tween;


        [Signal]
        public delegate void UpdateSelection(bool eliminateOldSelection);


        public virtual void Init(Vector2 position)
        {
            GlobalPosition = position;
        }
        public virtual void InitRandomObject()
        {
            InitRandomProperties();
            InitColorObject();
        }
        protected virtual void InitRandomProperties()
        {
            uint positionX = GD.Randi() % (uint)Globals.ScreenInfo.PlayableSize[0];
            uint positionY = GD.Randi() % (uint)Globals.ScreenInfo.PlayableSize[1];

            GlobalPosition = new Vector2(positionX, positionY);
        }
        private void InitColorObject()
        {
            if (Main.Main.ColorOn)
            {
                _color = Globals.Colors.GetRandomColor();
                return;
            }

            _color = Globals.Colors.DefaultColor;
        }
        public virtual void RandomizeObject()
        {
            InitRandomObject();
            InitColorObject();
            UpdateColor();
        }
        public void ColorObject()
        {
            InitColorObject();
            UpdateColor();
        }



        public override void _Ready()
        {
            _tween = GetNode<Tween>("Tween");
            
            UpdateColor();
            RelevantPosition = GlobalPosition;

            if (!_overlappaple)
            {
                CollisionMask += 1;
            }
        }



        public virtual Godot.Collections.Dictionary<string, object> CreateDict()
        {
            // Globals.BaseObjectDict.Clear();
            // Globals.BaseObjectDict.AddBaseObjectData(this);
            Godot.Collections.Dictionary<string, object> dict = new Godot.Collections.Dictionary<string, object>()
            {
                { "PositionX", Position.x },
                { "PositionY", Position.y },
                { "ColorR", _color.r },
                { "ColorB", _color.b },
                { "ColorG", _color.g },
                { "ColorA", _color.a }
            };

            return dict;
            // return Globals.BaseObjectDict.s_dict;
        }
        public virtual void LoadData(Godot.Collections.Dictionary<string, object> data)
        {
            GlobalPosition = new Vector2((float)data["PositionX"], (float)data["PositionY"]);
            _color = new Color((float)data["ColorR"], (float)data["ColorG"], (float)data["ColorB"], (float)data["ColorA"]);
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
        protected virtual void HandleMotionInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion && IsInstanceValid(@event) && _state != Globals.OBJECTSTATE.SELECTED)
            {
                if (_state == Globals.OBJECTSTATE.PRESSED)
                {
                    _state = Globals.OBJECTSTATE.MOVING;
                }

                InputMovementMotion(mouseMotion);

                @event.Dispose();
                return;
            }


            if (@event is InputEventMouseButton mouseButtonEvent && IsInstanceValid(@event))
            {
                //if (mouseButtonEvent.IsActionReleased("select") || !mouseButtonEvent.IsPressed())
                if (mouseButtonEvent.ButtonIndex == 1 && !mouseButtonEvent.IsPressed())
                {
                    _state = Globals.OBJECTSTATE.SELECTED;
                    s_someonePressed = false;

                    InputMovementReleased();

                    mouseButtonEvent.Dispose();
                    return;
                }

                if (mouseButtonEvent.ButtonIndex == 1 && mouseButtonEvent.IsPressed())
                {
                    _state = Globals.OBJECTSTATE.PRESSED;
                    s_someonePressed = true;

                    InputMovementPressed(mouseButtonEvent);
                    mouseButtonEvent.Dispose();
                    return;
                }
            }
        }
        protected virtual void InputMovementMotion(InputEventMouseMotion mouseMotion)
        {
            setupFollowMouse(mouseMotion.Position);
        }
        protected virtual void InputMovementPressed(InputEventMouseButton mouseButtonEvent)
        {
            _clickedRelativePosition = mouseButtonEvent.Position - GlobalPosition;
        }
        protected virtual void InputMovementReleased() { }



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



        protected virtual void setupFollowMouse(Vector2 followPosition)
        {
            RelevantPosition = followPosition - _clickedRelativePosition;
        }
        public virtual void UpdateToValidPosition()
        {
            Vector2 newPosition = FindPositionInPlayableArea();
            if (newPosition != GlobalPosition)
            {
                _tween.InterpolateProperty(this, "position", GlobalPosition, newPosition, 0.2f);
                _tween.Start();
            }
        }
        protected virtual void UpdateColor()
        {
            Modulate = _color;
        }
        protected virtual Vector2 FindPositionInPlayableArea() { return GlobalPosition; }



        public virtual string InfoString()
        {
            return "";
        }


    }

}




