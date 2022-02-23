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

        private Vector2 _clickedRelativePosition;
        public Vector2 ClickedRelativePosition
        {
            get { return _clickedRelativePosition; }
            set { _clickedRelativePosition = value; }
        }

        public static bool s_someoneSelected { get; set; } = false;
        public static bool s_someonePressed { get; set; } = false;
        public static bool s_someoneHovered { get; set; } = false;
        public static bool s_imOnArea { get; set; } = false;

        public static BaseObject s_selectedObject = null;
        public static BaseObject s_hoveredObject = null;
        protected bool _imOnThisArea = false;
        public bool ImOnThisArea
        {
            get { return ImOnThisArea; }
            set { _imOnThisArea = value; }
        }

        public Vector2 targetPosition { get; set; }
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
            uint positionX = GD.Randi() % (uint)Globals.ScreenInfo.Size[0];
            uint positionY = GD.Randi() % (uint)Globals.ScreenInfo.Size[1];

            GlobalPosition = new Vector2(positionX, positionY);
        }

        public override void _Process(float delta)
        {
            if (_state >= Globals.OBJECTSTATE.SELECTED)
                Modulate = new Color(0, 0, 0);
            else
                Modulate = new Color(1, 1, 1);

            if (s_hoveredObject == this)
                Modulate = new Color(1, 0, 0);



        }
        public override void _Ready()
        {
            targetPosition = GlobalPosition;
            // Modulate = new Color(GD.Randf(), GD.Randf(), GD.Randf());


            if (_overlappaple)
            {
                CollisionMask = 2;
            }
            else
            {
                CollisionMask = 3;
            }

            _main = (Node2D)GetTree().GetNodesInGroup("main")[0];
            Connect(nameof(UpdateSelection), _main, "_on_GameObjects_UpdateSelection");
        }

        public virtual void InputControlFlow(InputEvent @event)
        {
            if (_imOnThisArea)
            {
                HandleSelectionInput(@event);
                HandleMotionInput(@event);
            }
            else
            {
                HandleUnselectionInput(@event);
                HandleMotionInput(@event);
            }
        }

        public void Select()
        {
            bool eliminateOldSelection = false;

            if (s_someoneSelected)
                eliminateOldSelection = true;


            EmitSignal(nameof(UpdateSelection), eliminateOldSelection);
        }
        public virtual void Move(float _)
        {
            Vector2 direction = (GlobalPosition).DirectionTo(targetPosition);
            float speed = targetPosition.DistanceTo(GlobalPosition) * _quickness;

            MoveAndSlide(direction * speed);

        }

        public void setupFollowMouse(InputEventMouseMotion mouseMotion)
        {
            targetPosition = mouseMotion.Position - _clickedRelativePosition;
        }

        public bool checkIfOnTop(Vector2 inputPosition)
        {
            Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
            Godot.Collections.Array objectsClicked = spaceState.IntersectPoint(inputPosition, 32, null, 2147483647, true, false);

            if (objectsClicked.Count > 1)
            {
                int currentIndex = GetIndex();
                // int maxIndex = 0;
                foreach (Godot.Collections.Dictionary objectClicked in objectsClicked)
                {
                    Node node = ((Node)objectClicked["collider"]);
                    int index = node.GetIndex();
                    if (index > currentIndex)
                    {
                        return false;
                        //maxIndex = index;
                    }
                }
                // return (currentIndex == maxIndex);
            }
            return true;

        }
        public BaseObject ReturnTopOne(Vector2 inputPosition)
        {
            Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
            Godot.Collections.Array objectsClicked = spaceState.IntersectPoint(inputPosition, 32, null, 2147483647, true, false);

            BaseObject topNode = null;
            int objectsNumber = objectsClicked.Count;

      
            if (objectsNumber > 0)
            {
                int currentIndex = GetIndex();
                int maxIndex = -1;
                foreach (Godot.Collections.Dictionary objectClicked in objectsClicked)
                {
                    if (objectClicked["collider"] is BaseObject node)
                    {
                        int index = node.GetIndex();
                        if (index > maxIndex)
                        {
                            topNode = node;
                            maxIndex = index;
                        }
                    }
                }
            }
            
            return topNode;
        }

        public virtual void HandleMotionInput(InputEvent @event)
        {
            if (@event is InputEventMouseMotion mouseMotion && IsInstanceValid(@event) && _state != Globals.OBJECTSTATE.SELECTED)
            {
                if (_state == Globals.OBJECTSTATE.PRESSED)
                {
                    _state = Globals.OBJECTSTATE.MOVING;
                }

                setupFollowMouse(mouseMotion);

                @event.Dispose();
                return;
            }


            if (@event is InputEventMouseButton mouseButtonEvent && IsInstanceValid(@event))
            {
                if (mouseButtonEvent.IsActionReleased("select"))
                {
                    _state = Globals.OBJECTSTATE.SELECTED;
                    s_someonePressed = false;

                    BaseObject topNodeInExitedPoint = ReturnTopOne(mouseButtonEvent.Position);
                    if (topNodeInExitedPoint != this && topNodeInExitedPoint != null)
                    {
                        _imOnThisArea = false;
                        s_hoveredObject = topNodeInExitedPoint;
                        s_hoveredObject.ImOnThisArea = true;
                    }
                   
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

        public virtual void HandleUnselectionInput(InputEvent @event)
        {
            // HANDLE UNSELECT THE OBJECT IF U CLICK OUTSIDE OF IT -> UNSELECTED
            if (@event is InputEventMouseButton mouseButtonEvent && IsInstanceValid(@event))
            {
                if (mouseButtonEvent.IsActionPressed("select"))
                {
                    _state = Globals.OBJECTSTATE.UNSELECETED;

                    if (s_someoneHovered)
                    {
                        s_selectedObject = s_hoveredObject;
                        s_selectedObject.State = Globals.OBJECTSTATE.PRESSED;
                        s_selectedObject.ClickedRelativePosition = mouseButtonEvent.Position - s_selectedObject.GlobalPosition;
                        Select();
                        s_someoneSelected = true;
                        s_someonePressed = true;
                    }
                    else
                    {
                        s_selectedObject = null;
                        Select();
                        s_someoneSelected = false;
                        s_someonePressed = false;
                    }
                    mouseButtonEvent.Dispose();
                    return;
                }
            }
        }
        public virtual void HandleSelectionInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent && checkIfOnTop(mouseButtonEvent.Position) && IsInstanceValid(@event))
            {

                if (mouseButtonEvent.IsActionPressed("select"))
                {
                    _state = Globals.OBJECTSTATE.PRESSED;

                    _clickedRelativePosition = mouseButtonEvent.Position - GlobalPosition;

                    s_selectedObject = this;
                    Select();
                    s_someoneSelected = true;
                    s_someonePressed = true;

                    mouseButtonEvent.Dispose();
                    return;
                }
            }
        }

        public void _on_BaseObject_mouse_exited()
        {
            if (!s_someonePressed || _state == Globals.OBJECTSTATE.PRESSED)
            {
                BaseObject topNodeInExitedPoint = ReturnTopOne(GetViewport().GetMousePosition());
                
                if (topNodeInExitedPoint == null)
                {
                    _imOnThisArea = false;
                    s_imOnArea = false;
                    s_hoveredObject = null;
                    s_someoneHovered = false;
                }
                else
                {
                    _imOnThisArea = false;
                    s_hoveredObject = topNodeInExitedPoint;
                    s_hoveredObject.ImOnThisArea = true;
                }
            }
        }
        public void _on_BaseObject_mouse_entered()
        {

            if (!s_someonePressed || _state == Globals.OBJECTSTATE.PRESSED)
            {
                if (s_hoveredObject == null)
                {
                    _imOnThisArea = true;
                    s_imOnArea = true;
                    s_hoveredObject = this;
                    s_someoneHovered = true;

                    return;
                }

                if (checkIfOnTop(GetViewport().GetMousePosition()))
                {
                    s_hoveredObject.ImOnThisArea = false;
                    _imOnThisArea = true;
                    s_imOnArea = true;
                    s_hoveredObject = this;
                    s_hoveredObject.ImOnThisArea = true;
                    s_someoneHovered = true;
                }
            }

        }


    }

}




