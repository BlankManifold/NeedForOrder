using Godot;
using GameObjects;
using Globals;
using System;

namespace Main
{

    public class Main : Node2D
    {
        private GAMESTATE _gamestate = GAMESTATE.IDLE;
        private OBJECTTYPE _objectType = OBJECTTYPE.SQUARE;
        private ObjectsInterfaces _objectInterfaces = new ObjectsInterfaces();
        private uint _objectsNumber = 10;
        public static bool ColorOn = false;
        private bool _objectPressed = false;

        private int _backgroundNumber = 1;

        private Node _objectsContainer;
        private Area2D _mouseArea;
        private Label _label;
        private GameUI.GameUI _gameUI;
        private Godot.Collections.Array<TextureButton> _UIButtons;

        private TextureRect _backgroundTile;
        private HBoxContainer _backgroundContainer;
        private Node2D _adsHandler;
        private GameUI.SettingsPanel _settingsPanel;



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
            _settingsPanel = GetNode<GameUI.SettingsPanel>("GameUILayer/SettingsPanel");
            _UIButtons = new Godot.Collections.Array<TextureButton>(GetTree().GetNodesInGroup("UIButton"));
            _label = GetNode<Label>("Label");

            PackedScene levelBarrierScene = (PackedScene)ResourceLoader.Load("res://scenes/BackgroundAndLevel/LevelBarrier.tscn");
            BackgroundAndLevel.LevelBarrier levelBarrier = levelBarrierScene.Instance<BackgroundAndLevel.LevelBarrier>();
            levelBarrier.Name = "LevelBarrier";
            levelBarrier.YLimitOffset = (int)_gameUI.RectSize.y;
            AddChild(levelBarrier);

            _backgroundTile = GetNode<TextureRect>("BackgroundLayer/PatternPanel/PatternTile");
            _backgroundTile.Texture = null;
            _backgroundContainer = _gameUI.GetNode<HBoxContainer>("NinePatchRect/ScrollBackground/CenterContainer/HBoxContainer");


            RandomManager.rng.Randomize();
            _objectsContainer = GetNode<Node>("ObjectsContainer");


            LoadConfiguration(_objectType, _objectsNumber);
        }

        public override void _PhysicsProcess(float delta)
        {
            UpdateState();
            UpdateButtonsState();
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

                case GAMESTATE.PAUSED:
                    break;

            }
        }
        public override void _UnhandledInput(InputEvent @event)
        {
            // UPDATE SELECTION ONLY IF BUTTON CLICKED/PRESSED
            if (_gamestate == GAMESTATE.PAUSED) //|| (!ScreenInfo.CheckIfValidPosition(GetGlobalMousePosition()) && !_objectPressed))
            {
                @event.Dispose();
                return;
            }

            if ((@event is InputEventMouseButton && Input.IsActionJustPressed("select")) || @event.IsPressed())
            {
                if (_objectInterfaces.IRotatable)
                {
                    if (ClickedOnRotationArea())
                    {
                        RotatableObject rotatableObject = (RotatableObject)BaseObject.s_selectedObject;
                        rotatableObject.ClickedOnRotationArea((InputEventMouseButton)@event);
                        
                        _objectPressed = true;
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
            //_gamestate = GAMESTATE.IDLE;
            if (isNumberFixed)
            {
                foreach (BaseObject child in _objectsContainer.GetChildren())
                {
                    child.RandomizeObject();
                }

            }
        }
        private void ColorObjects()
        {
            foreach (BaseObject child in _objectsContainer.GetChildren())
            {
                child.ColorObject();
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

            SaveSystem.SaveObjectsHandler.SaveBackground(_objectType, _objectsNumber, _backgroundNumber, ColorOn);

            bool areLoaded = SaveSystem.SaveObjectsHandler.LoadBackground(type, numberOfObjects, ref _backgroundNumber, ref ColorOn);

            if (!areLoaded)
            {
                _backgroundNumber = 1;
                ColorOn = false;
            }

            UpdateBackground();
        }
        private void LoadConfiguration(OBJECTTYPE type, uint numberOfObjects)
        {
            LoadBackground(type, numberOfObjects);
            LoadObjects(type, numberOfObjects);

            if (!ColorOn)
            {
                ColorObjects();
            }
        }



        private void UpdateState()
        {
            // if (!ScreenInfo.CheckIfValidPosition(GetGlobalMousePosition()) && !_objectPressed)
            // {
            //     _gamestate = GAMESTATE.PAUSED;
            //     return;
            // }
            if (_gamestate == GAMESTATE.PAUSED)
            {
                return;
            }

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
            GameUI.ScrollBackgroundIconGameUI icon = (GameUI.ScrollBackgroundIconGameUI)_backgroundContainer.GetChild(_backgroundNumber);
            _backgroundTile.Texture = icon.TextureTile;
            _backgroundTile.RectGlobalPosition = icon.Offset;

            GameUI.ScrollGameUI scrollBackground = _gameUI.GetNode<GameUI.ScrollGameUI>("NinePatchRect/ScrollBackground");
            scrollBackground.UpdateFocus(icon);

            GameUI.ColorButton button = _gameUI.GetNode<GameUI.ColorButton>("NinePatchRect/ColorButton");
            button.UpdateColorOn(ColorOn);
        }
        private void UpdateButtonsState()
        {
            if (BaseObject.s_someonePressed != _objectPressed)
            {
                _objectPressed = BaseObject.s_someonePressed;

                foreach (TextureButton button in _UIButtons)
                {
                    if (_objectPressed)
                        button.MouseFilter = Control.MouseFilterEnum.Ignore;
                    else
                        button.MouseFilter = Control.MouseFilterEnum.Stop;
                }

            }
        }


        public void _on_ScrollGameUI_ObjectTypeChanged(OBJECTTYPE type)
        {
            LoadConfiguration(type, _objectsNumber);
            
            _gamestate = GAMESTATE.IDLE;
            _objectType = type;
            BaseObject.s_selectedObject = null;

            if (_settingsPanel.Visible)
            {
                _gamestate = GAMESTATE.PAUSED;
            }
            //_adsHandler.Call("loadBanner");
            GC.Collect();
        }
        public void _on_ScrollGameUI_NumberChanged(uint newNumber)
        {
            LoadConfiguration(_objectType, newNumber);

            _gamestate = GAMESTATE.IDLE;
            _objectsNumber = newNumber;
            BaseObject.s_selectedObject = null;

            if (_settingsPanel.Visible)
            {
                _gamestate = GAMESTATE.PAUSED;
            }

            GC.Collect();
        }
        public void _on_ScrollGameUI_BackgroundChanged(Texture textureTile, int backgroundNumber, Vector2 offset)
        {
            //_gamestate = GAMESTATE.IDLE;
            _backgroundTile.Texture = textureTile;
            _backgroundTile.RectGlobalPosition = offset;
            _backgroundNumber = backgroundNumber;
        }
        public void _on_ScrollGameUI_FocusingTweenStarted()
        {
            _gamestate = GAMESTATE.PAUSED;
        }
        public void _on_ScrollGameUI_FocusingTweenCompleted()
        {
            if (!_settingsPanel.Visible)
                _gamestate = GAMESTATE.IDLE;
        }
        public void _on_RandomizeButton_RandomizePressed()
        {
            RandomizeObjects();

            _adsHandler.Call("loadBanner");
        }
        public void _on_ColorButton_ColorPressed(bool colorOn)
        {
            ColorOn = colorOn;
            ColorObjects();
        }
        public void _on_SettingsButton_SettingsPressed()
        {
            _settingsPanel.Visible = !_settingsPanel.Visible;

            if (_settingsPanel.Visible)
            {
                _gamestate = GAMESTATE.PAUSED;
                //_adsHandler.Call("loadBanner");
                return;
            }

            _gamestate = GAMESTATE.IDLE;
        }
        public void _SettingsPanel_ButtonPressed(Color defaultColor)
        {
            Globals.Colors.DefaultColor = defaultColor;
            if (!ColorOn)
            {
                ColorObjects();
            }

            foreach (TextureRect icon in GetTree().GetNodesInGroup("ObjectIcon"))
            {
                icon.SelfModulate = defaultColor;
            }

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