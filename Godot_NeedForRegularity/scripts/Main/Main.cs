using Godot;
using GameObjects;
using Globals;

namespace Main
{

    public class Main : Node2D
    {
        private GAMESTATE _gamestate = GAMESTATE.IDLE;
        private OBJECTTYPE _objectType = OBJECTTYPE.SQUARE;
        private uint _objectsNumber = 3;
        private Node _objectsContainer;

        private Area2D _mouseArea;
        private Label _label;

        private GameUI.GameUI _gameUI;

        private Godot.Collections.Array<GameUI.ScrollButtonGameUI> _UIButtons;

        private bool _someoneWasPressed = false;



        public override void _Ready()
        {
            Globals.ScreenInfo.UpdateScreenInfo(GetViewport());

            // GetViewport().GuiDisableInput = true;   
            _gameUI = GetNode<GameUI.GameUI>("GameUILayer/GameUI");
            _UIButtons = new Godot.Collections.Array<GameUI.ScrollButtonGameUI>(GetTree().GetNodesInGroup("UIButton"));

            PackedScene levelBarrierScene = (PackedScene)ResourceLoader.Load("res://scenes/BackgroundAndLevel/LevelBarrier.tscn");
            BackgroundAndLevel.LevelBarrier levelBarrier = levelBarrierScene.Instance<BackgroundAndLevel.LevelBarrier>();
            levelBarrier.Name = "LevelBarrier";
            levelBarrier.YLimitOffset = (int)_gameUI.RectSize.y;
            AddChild(levelBarrier);

            RandomManager.rng.Randomize();
            _objectsContainer = GetNode<Node>("ObjectsContainer");

            SpawnObjects();

            _mouseArea = GetNode<Area2D>("MouseArea");

            _label = GetNode<Label>("Label");
        }
        public override void _PhysicsProcess(float delta)
        {
            UpdateState();
            UpdateMouseAreaPosition(GetGlobalMousePosition());
            // UpdateMouseFilterUI();
            
            if (BaseObject.s_selectedObject != null)
            {
                _label.Text = BaseObject.s_selectedObject.InfoString();
            }

            switch (_gamestate)
            {
                case GAMESTATE.MOVING:
                    BaseObject.s_selectedObject.MoveObject(delta);
                    break;
                case GAMESTATE.ROTATING:
                    RotatableObject castedObject = (RotatableObject)BaseObject.s_selectedObject;
                    castedObject.RotateObject();
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
            if (BaseObject.s_selectable)
            {
                BaseObject.s_hoveredObject = ReturnTopObject();

                if (@event is InputEventMouseButton && Input.IsActionJustPressed("select"))
                {
                    UpdateHoveredAndSelectedObject();
                }
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

            PackedScene objectScene = ResourceLoader.Load<PackedScene>($"res://scenes/SpecificObjects/{objectType}.tscn");

          
            for (int i = 0; i < _objectsNumber; i++)
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
                switch (BaseObject.s_selectedObject.State)
                {

                    case OBJECTSTATE.MOVING:
                        _gamestate = GAMESTATE.MOVING;
                        break;
                    case OBJECTSTATE.ROTATING:
                        _gamestate = GAMESTATE.ROTATING;
                        break;
                    default:
                        _gamestate = GAMESTATE.IDLE;
                        break;
                }
                return;
            }

            _gamestate = GAMESTATE.IDLE;
        }

        private void UpdateHoveredAndSelectedObject()
        {

            // IF U NOT CLICK ON A OBJECT...
            if (BaseObject.s_hoveredObject == null)
            {
                // and SOME OBJECTED WAS SELECTED -> UNSELECT
                if (BaseObject.s_selectedObject != null)
                {
                    BaseObject.s_selectedObject.UnSelectObject();
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
                    BaseObject.s_hoveredObject.SelectObject();
                    BaseObject.s_selectedObject = BaseObject.s_hoveredObject;
                    return;
                }

                // and SOME WAS ALREADY SELECTED -> CHANGE SELECTION: UNSELECT OLD AND SELECT HOVERED ONE
                if (BaseObject.s_selectedObject != null)
                {
                    BaseObject.s_hoveredObject.SelectObject();
                    BaseObject.s_selectedObject.UnSelectObject();
                    BaseObject.s_selectedObject = BaseObject.s_hoveredObject;
                }

            }
        }
        private void UpdateMouseAreaPosition(Vector2 newPosition)
        {
            float x = Mathf.Clamp(newPosition.x,0,Globals.ScreenInfo.VisibleRectSize.x);
            float y = Mathf.Clamp(newPosition.y,0,Globals.ScreenInfo.VisibleRectSize.y);
            _mouseArea.GlobalPosition = new Vector2(x,y);
        }

        // private void UpdateMouseFilterUI()
        // {
        //     if (BaseObject.s_someonePressed && !_someoneWasPressed)
        //     {
        //         _someoneWasPressed = true;
        //         // GetViewport().GuiDisableInput = true;
        //     }

        //     if (!BaseObject.s_someonePressed && _someoneWasPressed)
        //     {
        //         _someoneWasPressed = false;
        //         // GetViewport().GuiDisableInput = false;

        //     }

        // }

        public void _on_ScrollGameUI_ObjectTypeChanged(OBJECTTYPE type)
        {
            foreach (Node child in _objectsContainer.GetChildren())
            {
                child.QueueFree();                
            }
            _gamestate = GAMESTATE.IDLE;
            _objectType = type;
            
            SpawnObjects(); 
        }

    }
}