using Godot;


namespace GameObjects
{
    public class RotatableObject : BaseObject, IRotatable
    {
        private Area2D _rotationArea;

        protected bool _imOnRotationArea = false;
        public override void _Ready()
        {
            base._Ready();

            _rotationArea = GetNode<Area2D>("RotationArea");
            _rotationArea.Position = new Vector2(70, 70);

        }

        // public override void _Process(float delta)
        // {
        //     if (_state >= Globals.OBJECTSTATE.SELECTED)
        //         Modulate = new Color(0, 0, 0);
        //     else
        //         Modulate = new Color(1, 1, 1);

        //     if (_state == Globals.OBJECTSTATE.ROTATING)
        //         _rotationArea.SelfModulate = new Color(1, 0, 0);
        //     else
        //         _rotationArea.SelfModulate = new Color(1, 1, 1);

        // }


        public virtual void RotateObject(float _) { }

        public override void InputControlFlow(InputEvent @event)
        {
            //IF UNSELECTED -> DO NOTHING SELECTION IN HANDLED IN MAIN
            if (_state == Globals.OBJECTSTATE.UNSELECETED)
            {
                return;
            }

            // IF NOT UNSELECTED -> HANDLE MOVING
            if (_state != Globals.OBJECTSTATE.UNSELECETED)
            {
                
                HandleMotionInput(@event);
                   
                return;
            }

        }

        public void HandleRotationInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent && _state == Globals.OBJECTSTATE.SELECTED && IsInstanceValid(@event))
            {
                if (mouseButtonEvent.IsActionPressed("select"))
                {
                    _state = Globals.OBJECTSTATE.ROTATING;

                    mouseButtonEvent.Dispose();
                    return;
                }
            }
        }
        public void _on_RotationArea_mouse_exited()
        {
            _imOnRotationArea = false;
        }
        public void _on_RotationArea_mouse_entered()
        {
            _imOnRotationArea = true;
        }




    }
}
