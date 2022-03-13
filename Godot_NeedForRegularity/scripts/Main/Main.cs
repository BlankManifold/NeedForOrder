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

        private uint _backgroundNumber = 1; 

        private Node _objectsContainer;
        private Area2D _mouseArea;
        private Label _label;
        private GameUI.GameUI _gameUI;
        private Godot.Collections.Array<GameUI.ScrollIconGameUI> _UIButtons;

        private TextureRect _backgroundTile;



        public override void _Ready()
        {
            Globals.ScreenInfo.UpdateScreenInfo(GetViewport());

            _gameUI = GetNode<GameUI.GameUI>("GameUILayer/GameUI");
            _UIButtons = new Godot.Collections.Array<GameUI.ScrollIconGameUI>(GetTree().GetNodesInGroup("UIButton"));
            _mouseArea = GetNode<Area2D>("MouseArea");
            _label = GetNode<Label>("Label");

            PackedScene levelBarrierScene = (PackedScene)ResourceLoader.Load("res://scenes/BackgroundAndLevel/LevelBarrier.tscn");
            BackgroundAndLevel.LevelBarrier levelBarrier = levelBarrierScene.Instance<BackgroundAndLevel.LevelBarrier>();
            levelBarrier.Name = "LevelBarrier";
            levelBarrier.YLimitOffset = (int)_gameUI.RectSize.y;
            AddChild(levelBarrier);

            _backgroundTile = GetNode<TextureRect>("BackgroundLayer/PatternTile");
            _backgroundTile.Texture = null;


            RandomManager.rng.Randomize();
            _objectsContainer = GetNode<Node>("ObjectsContainer");

            LoadObjects(_objectType, _objectsNumber);
        }
        public override void _PhysicsProcess(float delta)
        {
            UpdateState();
            UpdateMouseAreaPosition(GetGlobalMousePosition());


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



        private BaseObject ReturnTopObject()
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
        private void SpawnRandomObjects(OBJECTTYPE type, uint numberOfObjects)
        {
            PackedScene objectScene = ResourceLoader.Load<PackedScene>(Utilities.GetObjectScenePath(type));

            for (int i = 0; i < numberOfObjects; i++)
            {
                BaseObject spawnedObject = objectScene.Instance<BaseObject>();
                spawnedObject.InitRandomObject();
                _objectsContainer.AddChild(spawnedObject);
            }
        }
        private void RandomizeObjects(bool isNumberFixed = true)
        {
            _gamestate = GAMESTATE.IDLE;
            if (isNumberFixed)
            {
                foreach (BaseObject child in _objectsContainer.GetChildren())
                {
                    child.RandomizeObject();
                }

            }
        }
        private void LoadObjects(OBJECTTYPE type, uint numberOfObjects)
        {
            Godot.Collections.Array objects = _objectsContainer.GetChildren();
            if (objects.Count != 0)
            {
                SaveSystem.SaveObjectsHandler.SaveObjects(_objectType, _objectsNumber, objects);

                foreach (Node child in objects)
                {
                    child.QueueFree();
                }
            }


            bool areLoaded = SaveSystem.SaveObjectsHandler.LoadObjects(type, numberOfObjects, _objectsContainer);

            if (!areLoaded)
            {
                SpawnRandomObjects(type, numberOfObjects);
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
            float x = Mathf.Clamp(newPosition.x, 0, Globals.ScreenInfo.VisibleRectSize.x);
            float y = Mathf.Clamp(newPosition.y, 0, Globals.ScreenInfo.VisibleRectSize.y);
            _mouseArea.GlobalPosition = new Vector2(x, y);
        }



        public void _on_ScrollGameUI_ObjectTypeChanged(OBJECTTYPE type)
        {
            LoadObjects(type, _objectsNumber);

            _gamestate = GAMESTATE.IDLE;
            _objectType = type;
        }
        public void _on_ScrollGameUI_NumberChanged(uint newNumber)
        {
            LoadObjects(_objectType, newNumber);

            _gamestate = GAMESTATE.IDLE;
            _objectsNumber = newNumber;
        }
        public void _on_ScrollGameUI_BackgroundChanged(Texture textureTile, uint backgroundNumber)
        {
            LoadObjects(_objectType, _objectsNumber);

            _gamestate = GAMESTATE.IDLE;
            _backgroundTile.Texture = textureTile;
            _backgroundNumber = backgroundNumber;
        }

        public void _on_RandomizeButton_RandomizePressed()
        {
            RandomizeObjects();
        }
    }

}