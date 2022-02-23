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

        public static bool someoneIsSelected { get; set; } = false;
        public Vector2 targetPosition { get; set; }
        protected Globals.OBJECTSTATE _state;
        public Globals.OBJECTSTATE State
        {
            get { return _state; }
        }

        private Node2D _main;

        [Signal]
        public delegate void UpdateSelection(BaseObject sender, bool eliminateOldSelection, bool unSelectAll);

        public void Init(Vector2 position)
        {
            GlobalPosition = position;
        }

        public override void _Process(float delta)
        {
            // if (_state >= Globals.OBJECTSTATE.SELECTED)
            //     Modulate = new Color(0, 0, 0);
            // else
            //     Modulate = new Color(1, 1, 1);

        }
        public override void _Ready()
        {
            targetPosition = GlobalPosition;
            Modulate = new Color(GD.Randf(),GD.Randf(),GD.Randf());


            if (_overlappaple)
            {
                CollisionMask = 2;
            }

            _main = (Node2D)GetTree().GetNodesInGroup("main")[0];
            Connect(nameof(UpdateSelection), _main, "_on_GameObjects_UpdateSelection");

            Connect("input_event", this, nameof(_on_SelectionAreaShape_input_event));
        }



        public void Select(bool unSelectAll = false)
        {
            bool eliminateOldSelection = false;

            if (someoneIsSelected)
                eliminateOldSelection = true;

            if (unSelectAll)
                someoneIsSelected = false;

            EmitSignal(nameof(UpdateSelection), this, eliminateOldSelection, unSelectAll);
        }
        public virtual void Move(float _)
        {
            Vector2 direction = GlobalPosition.DirectionTo(targetPosition);
            float speed = targetPosition.DistanceTo(GlobalPosition) * _quickness;

            MoveAndSlide(direction *  speed);

        }

        public bool checkIfOnTop(Vector2 inputPosition)
        {
            Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
            Godot.Collections.Array objectsClicked = spaceState.IntersectPoint(inputPosition, 32, null, 2147483647, true, false);

            if (objectsClicked.Count > 1)
            {
                int currentIndex = GetIndex();
                int maxIndex = 0;
                foreach (Godot.Collections.Dictionary objectClicked in objectsClicked)
                {
                    Node node = ((Node)objectClicked["collider"]);
                    int index = node.GetIndex();
                    if (index > maxIndex)
                    {
                        maxIndex = index;
                    }
                }
                return (currentIndex == maxIndex);
            }
            return true;

        }


        public virtual void HandleOutsideInput(InputEvent @event)
        {
            if (_state == Globals.OBJECTSTATE.MOVING)
            {
                if (@event is InputEventMouseMotion mouseMotion)
                {
                    setupFollowMouse(mouseMotion);

                    @event.Dispose();
                    return;
                }

                if (@event is InputEventMouseButton)
                {
                    if (@event.IsActionReleased("select"))
                    {
                        _state = Globals.OBJECTSTATE.SELECTED;

                    }

                    @event.Dispose();
                    return;
                }
            }

            if (_state == Globals.OBJECTSTATE.SELECTED)
            {
                if (@event is InputEventMouseButton)
                {
                    if (@event.IsActionPressed("select"))
                    {
                        _state = Globals.OBJECTSTATE.UNSELECETED;

                        targetPosition = GlobalPosition;
                        Select(true);
                        someoneIsSelected = false;
                    }

                    @event.Dispose();
                    return;
                }
            }

            @event.Dispose();
        }
        public virtual void HandleInsideInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent && checkIfOnTop(mouseButtonEvent.Position))
            {

                if (mouseButtonEvent.IsActionPressed("select"))
                {
                    _state = Globals.OBJECTSTATE.PRESSED;

                    _clickedRelativePosition = mouseButtonEvent.GlobalPosition - GlobalPosition;

                    Select(false);
                    someoneIsSelected = true;
                }

                if (mouseButtonEvent.IsActionReleased("select") &&  _state != Globals.OBJECTSTATE.UNSELECETED)
                {
                    _state = Globals.OBJECTSTATE.SELECTED;
                }

                GetTree().SetInputAsHandled();
                mouseButtonEvent.Dispose();
                return;

            }

            if (@event is InputEventMouseMotion mouseMotion && _state != Globals.OBJECTSTATE.UNSELECETED)
            {

                if (_state == Globals.OBJECTSTATE.PRESSED)
                {
                    _state = Globals.OBJECTSTATE.MOVING;
                }

                setupFollowMouse(mouseMotion);

                GetTree().SetInputAsHandled();
                @event.Dispose();
                return;
            }

            @event.Dispose();
        }


        public void setupFollowMouse(InputEventMouseMotion mouseMotion)
        {
            targetPosition = mouseMotion.GlobalPosition - _clickedRelativePosition;
        }
        public void _on_SelectionAreaShape_input_event(Node _, InputEvent @event, int __)
        {
            HandleInsideInput(@event);
        }


    }

}




