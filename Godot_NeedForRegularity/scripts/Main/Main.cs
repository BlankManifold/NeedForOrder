using Godot;
using GameObjects;
using Globals;

namespace Main
{

    public class Main : Node2D
    {
        private GAMESTATE _gamestate = GAMESTATE.IDLE;
        private OBJECTTYPE _objectType = OBJECTTYPE.SQUARE;
        private ObjectsInterfaces _objectInterfaces = new ObjectsInterfaces();
        private uint _objectsNumber = 3;

        private uint _backgroundNumber = 1;

        private Node _objectsContainer;
        private Area2D _mouseArea;
        private Label _label;
        private GameUI.GameUI _gameUI;
        private Godot.Collections.Array<GameUI.ScrollIconGameUI> _UIButtons;

        private TextureRect _backgroundTile;
        private HBoxContainer _backgroundContainer;
        private Node2D _adsHandler;



        public delegate BaseObject ReturnTopObject();
        private ReturnTopObject _returnTopObject;
        public delegate void UpdateChekingPosition();
        private UpdateChekingPosition _updateChekingPosition;

        private Vector2 _currentCheckingPosition;



        public override void _Ready()
        {

            if (!OS.HasTouchscreenUiHint())
            {
                _returnTopObject = ReturnTopObjectDesktop;
                _updateChekingPosition = UpdateChekingPositionDesktop;

                PackedScene mouseAreaScene = (PackedScene)ResourceLoader.Load("res://scenes/UtilityScenes/MouseArea.tscn");
                _mouseArea = mouseAreaScene.Instance<Area2D>();
                AddChild(_mouseArea);
            }
            else
            {
                _returnTopObject = ReturnTopObjectMobile;
                _updateChekingPosition = UpdateChekingPositionMobile;
            }

            _adsHandler = GetNode<Node2D>("AdsHandler");

            Globals.ScreenInfo.UpdateScreenInfo(GetViewport());
            GetViewport().Connect("size_changed", this, nameof(_on_viewport_size_changed));

            _gameUI = GetNode<GameUI.GameUI>("GameUILayer/GameUI");
            _UIButtons = new Godot.Collections.Array<GameUI.ScrollIconGameUI>(GetTree().GetNodesInGroup("UIButton"));
            _label = GetNode<Label>("Label");

            PackedScene levelBarrierScene = (PackedScene)ResourceLoader.Load("res://scenes/BackgroundAndLevel/LevelBarrier.tscn");
            BackgroundAndLevel.LevelBarrier levelBarrier = levelBarrierScene.Instance<BackgroundAndLevel.LevelBarrier>();
            levelBarrier.Name = "LevelBarrier";
            levelBarrier.YLimitOffset = (int)_gameUI.RectSize.y;
            AddChild(levelBarrier);

            _backgroundTile = GetNode<TextureRect>("BackgroundLayer/PatternTile");
            _backgroundTile.Texture = null;
            _backgroundContainer = _gameUI.GetNode<HBoxContainer>("NinePatchRect/ScrollBackground/CenterContainer/HBoxContainer");


            RandomManager.rng.Randomize();
            _objectsContainer = GetNode<Node>("ObjectsContainer");


            LoadConfiguration(_objectType, _objectsNumber);
        }

        public override void _PhysicsProcess(float delta)
        {
            UpdateState();
            _updateChekingPosition();


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

            // UPDATE SELECTION ONLY IF BUTTON CLICKED/PRESSED

            if ((@event is InputEventMouseButton && Input.IsActionJustPressed("select")) || @event.IsPressed())
            {

                if (_objectInterfaces.IRotatable)
                {
                    if (ClickedOnRotationArea())
                    {
                        RotatableObject rotatableObject = (RotatableObject)BaseObject.s_selectedObject;
                        rotatableObject.Rotatable = true;
                    }
                    else
                    {
                        BaseObject colliderObject = _returnTopObject();
                        UpdateHoveredAndSelectedObjectTouchFriendly(colliderObject);
                    }

                }


                if (!_objectInterfaces.IRotatable)
                {
                    BaseObject colliderObject = _returnTopObject();
                    UpdateHoveredAndSelectedObjectTouchFriendly(colliderObject);
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


        private bool ClickedOnRotationArea()
        {
            Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
            Godot.Collections.Array arrayOfDicts = spaceState.IntersectPoint(GetGlobalMousePosition(), 32, null, 16, true, false);

            if (arrayOfDicts.Count == 0)
            {
                spaceState.Dispose();
                arrayOfDicts.Dispose();
                return false;
            }


            int maxIndex = 0;
            KinematicBody2D bodyClicked = null;

            foreach (Godot.Collections.Dictionary bodyDict in arrayOfDicts)
            {
                KinematicBody2D bodyCollider = (KinematicBody2D)bodyDict["collider"];

                int index = bodyCollider.GetIndex();
                if (index >= maxIndex)
                {
                    maxIndex = index;
                    bodyClicked = bodyCollider;
                }
                bodyDict.Dispose();
            }

            spaceState.Dispose();
            arrayOfDicts.Dispose();

            return bodyClicked.IsInGroup("RotationArea");
        }
        private BaseObject ReturnTopObjectMobile()
        {
            Physics2DDirectSpaceState spaceState = GetWorld2d().DirectSpaceState;
            Godot.Collections.Array arrayOfDicts = spaceState.IntersectPoint(GetGlobalMousePosition(), 32, null, 65, true, false);

            if (arrayOfDicts.Count == 0)
            {
                spaceState.Dispose();
                arrayOfDicts.Dispose();
                return null;
            }


            int maxIndex = 0;
            BaseObject objectsSelected = null;

            foreach (Godot.Collections.Dictionary objectDict in arrayOfDicts)
            {
                if (objectDict["collider"] is BaseObject objectCollider)
                {
                    int index = objectCollider.GetIndex();
                    if (index >= maxIndex)
                    {
                        maxIndex = index;
                        objectsSelected = objectCollider;
                    }
                }
                objectDict.Dispose();
            }

            spaceState.Dispose();
            arrayOfDicts.Dispose();
            return objectsSelected;
        }
        private BaseObject ReturnTopObjectDesktop()
        {
            _mouseArea.GlobalPosition = GetGlobalMousePosition();
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

            _objectInterfaces = new ObjectsInterfaces(type);
            Godot.Collections.Array objects = _objectsContainer.GetChildren();
            if (objects.Count != 0)
            {
                SaveSystem.SaveObjectsHandler.SaveObjects(_objectType, _objectsNumber, objects, _backgroundNumber);

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
        private void LoadBackground(OBJECTTYPE type, uint numberOfObjects)
        {
            bool areLoaded = SaveSystem.SaveObjectsHandler.LoadBackground(type, numberOfObjects, ref _backgroundNumber);

            if (!areLoaded)
            {
                _backgroundNumber = 1;
            }

            UpdateBackground();
        }
        private void LoadConfiguration(OBJECTTYPE type, uint numberOfObjects)
        {
            LoadObjects(type, numberOfObjects);
            LoadBackground(type, numberOfObjects);
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
        private void UpdateHoveredAndSelectedObjectTouchFriendly(BaseObject colliderObject)
        {

            // IF U NOT CLICK ON A OBJECT...
            if (colliderObject == null)
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
                if (BaseObject.s_selectedObject == colliderObject)
                {
                    return;
                }

                // and NO ONE WAS SELECTED -> SELECT THAT ONE
                if (BaseObject.s_selectedObject == null)
                {
                    colliderObject.SelectObject();
                    BaseObject.s_selectedObject = colliderObject;
                    return;
                }

                // and SOME WAS ALREADY SELECTED -> CHANGE SELECTION: UNSELECT OLD AND SELECT HOVERED ONE
                if (BaseObject.s_selectedObject != null)
                {
                    colliderObject.SelectObject();
                    BaseObject.s_selectedObject.UnSelectObject();
                    BaseObject.s_selectedObject = colliderObject;
                }

            }
        }
        private void UpdateChekingPositionDesktop()
        {
            Vector2 newPosition = GetGlobalMousePosition();
            float x = Mathf.Clamp(newPosition.x, 0, Globals.ScreenInfo.VisibleRectSize.x);
            float y = Mathf.Clamp(newPosition.y, 0, Globals.ScreenInfo.VisibleRectSize.y);
            _mouseArea.GlobalPosition = new Vector2(x, y);
        }
        private void UpdateChekingPositionMobile()
        {
            _currentCheckingPosition = GetGlobalMousePosition();
        }
        private void UpdateBackground()
        {
            GameUI.ScrollBackgroundIconGameUI icon = (GameUI.ScrollBackgroundIconGameUI)_backgroundContainer.GetChild((int)_backgroundNumber);
            _backgroundTile.Texture = icon.TextureTile;

            GameUI.ScrollGameUI scrollBackground = _gameUI.GetNode<GameUI.ScrollGameUI>("NinePatchRect/ScrollBackground");
            scrollBackground.UpdateFocus(icon);
        }



        public void _on_ScrollGameUI_ObjectTypeChanged(OBJECTTYPE type)
        {

            LoadConfiguration(type, _objectsNumber);

            _gamestate = GAMESTATE.IDLE;
            _objectType = type;

            //_adsHandler.Call("loadBanner");
        }
        public void _on_ScrollGameUI_NumberChanged(uint newNumber)
        {
            LoadConfiguration(_objectType, newNumber);

            _gamestate = GAMESTATE.IDLE;
            _objectsNumber = newNumber;
        }
        public void _on_ScrollGameUI_BackgroundChanged(Texture textureTile, uint backgroundNumber)
        {
            SaveSystem.SaveObjectsHandler.SaveBackground(_objectType, _objectsNumber, backgroundNumber);

            _gamestate = GAMESTATE.IDLE;
            _backgroundTile.Texture = textureTile;
            _backgroundNumber = backgroundNumber;
        }
        public void _on_RandomizeButton_RandomizePressed()
        {
            RandomizeObjects();

            _adsHandler.Call("loadBanner");
        }
        public void _on_viewport_size_changed()
        {
            foreach (BaseObject child in _objectsContainer.GetChildren())
            {
                child.UpdateToValidPosition();
            }

        }

    }
}