using Godot;
using GameObjects;
using Globals;

namespace Main
{

    public class Main : Node2D
    {
        private GAMESTATE _gamestate = GAMESTATE.IDLE;
        private OBJECTTYPE _objectType = OBJECTTYPE.SQUARE;
        private uint _objectsNumber = 10;
        private Node _objectsContainer;

        private Area2D _mouseArea;



        public override void _Ready()
        {
            Globals.ScreenInfo.UpdateScreenInfo(GetViewport());

            PackedScene levelBarrierScene = (PackedScene)ResourceLoader.Load("res://scenes/LevelBarrier.tscn");
            BackgroundAndLevel.LevelBarrier levelBarrier = levelBarrierScene.Instance<BackgroundAndLevel.LevelBarrier>();
            levelBarrier.Name = "LevelBarrier";
            AddChild(levelBarrier);

            RandomManager.rng.Randomize();
            _objectsContainer = GetNode<Node>("ObjectsContainer");

            SpawnObjects();

            _mouseArea = GetNode<Area2D>("MouseArea");
        }
        public override void _PhysicsProcess(float delta)
        {
            UpdateState();
            UpdateMouseAreaPosition(GetGlobalMousePosition());


            // Label label = GetNode<Label>("Label");
            // label.Text = "VisibleRect Size:" + GetViewport().GetVisibleRect().Size.ToString();
            // label.Text += "\n Viewport Size:" + GetViewport().Size.ToString();
            // label.Text += "\n Viewport OverrideSize:" + GetViewport().GetSizeOverride().ToString();

            switch (_gamestate)
            {
                case GAMESTATE.MOVING:
                    BaseObject.s_selectedObject.Move(delta);
                    break;
                case GAMESTATE.IDLE:
                    break;

            }
        }

        public BaseObject ReturnTopObject()
        {
            Godot.Collections.Array<BaseObject> objects = new Godot.Collections.Array<BaseObject>(_mouseArea.GetOverlappingBodies());

            int maxIndex = -1;

            if (objects.Count > 0)
            {
                int topIndex = -1;
                for (int i = 0; i < objects.Count; i++)
                {
                    int index = objects[i].GetIndex();
                    if (index > maxIndex)
                    {
                        maxIndex = index;
                        topIndex = i;
                    }
                }
                return objects[topIndex];
            }
            return null;
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            
            // UPDATE SELECTION ONLY IF BUTTON CLICKED/PRESSED, UPDATE HOVERED ALSO IF U RELEASE BUTTON
            if (@event is InputEventMouseButton mouseButtonEvent && @event.IsAction("select"))
            {
                UpdateHoveredAndSelectedObject(@event);
                
            }

            // IF SOMEONE SELECTED -> HANDLE IT
            if (BaseObject.s_selectedObject != null)
            {
                BaseObject.s_selectedObject.InputControlFlow(@event);

                @event.Dispose();
                return;
            }

            @event.Dispose();
        }

        private void SpawnObjects()
        {
            string objectType;

            switch (_objectType)
            {
                case OBJECTTYPE.SQUARE:
                    objectType = "SquareObject";
                    break;
                case OBJECTTYPE.DOT:
                    objectType = "DotObject";
                    break;
                case OBJECTTYPE.CIRCLE:
                    objectType = "CircleObject";
                    break;
                case OBJECTTYPE.LINE:
                    objectType = "LineObject";
                    break;

                default:
                    objectType = "SquareObject";
                    break;
            }

            PackedScene objectScene = ResourceLoader.Load<PackedScene>($"res://scenes/{objectType}.tscn");

            for (int _ = 0; _ < _objectsNumber; _++)
            {
                BaseObject spawnedObject = objectScene.Instance<BaseObject>();
                spawnedObject.InitRandomObject();
                _objectsContainer.AddChild(spawnedObject);
            }


        }
        private void UpdateState()
        {
            if (BaseObject.s_selectedObject != null)
            {

                if (BaseObject.s_selectedObject.State == OBJECTSTATE.MOVING)
                {
                    _gamestate = GAMESTATE.MOVING;
                    return;
                }
            }

            _gamestate = GAMESTATE.IDLE;
        }

        private void UpdateHoveredAndSelectedObject(InputEvent @event)
        {
            BaseObject.s_hoveredObject = ReturnTopObject();

            // IF U JUST CLICKED -> SEE IF U NEED TO CHANGE SELECTION
            if (Input.IsActionJustPressed("select"))
            {
                // IF U NOT CLICK ON A OBJECT...
                if (BaseObject.s_hoveredObject == null)
                {
                    // and SOME OBJECTED WAS SELECTED -> UNSELECT
                    if (BaseObject.s_selectedObject != null)
                    {
                        BaseObject.s_selectedObject.UnSelect();
                        BaseObject.s_selectedObject = null;
                    }

                    // and NO OBJECTED WAS SELECTED -> DO NOTHING
                    return;
                }
                // IF U CLICK ON SOMETHING...
                else
                {
                    // and U CLICKED ON THE SAME ONE -> DO NOTHING
                    if (BaseObject.s_selectedObject == BaseObject.s_hoveredObject)
                    {
                        return;
                    }

                    // and NO ONE WAS SELECTED -> SELECT THAT ONE
                    if (BaseObject.s_selectedObject == null)
                    {
                        BaseObject.s_hoveredObject.Select();
                        BaseObject.s_selectedObject = BaseObject.s_hoveredObject;
                        return;
                    }

                    // and SOME WAS ALREADY SELECTED -> CHANGE SELECTION: UNSELECT OLD AND SELECT HOVERED ONE
                    if (BaseObject.s_selectedObject != null)
                    {
                        BaseObject.s_hoveredObject.Select();
                        BaseObject.s_selectedObject.UnSelect();
                        BaseObject.s_selectedObject = BaseObject.s_hoveredObject;
                    }

                }

                return;
            }
            
            // IF U RELEASE DO NOTHING;
        }
        private void UpdateMouseAreaPosition(Vector2 newPosition)
        {
            _mouseArea.GlobalPosition = newPosition;
        }


    }
}