using Godot;
using GameObjects;
using Globals;

namespace Main
{

    public class Main : Node2D
    {
        private GAMESTATE _gamestate = GAMESTATE.IDLE;
        private OBJECTTYPE _objectType = OBJECTTYPE.SQUARE;
        private BaseObject _selectedObject;
        private uint _objectsNumber = 10;
        private Node _objectsContainer;

        public override void _Ready()
        {
            Globals.ScreenInfo.UpdateScreenInfo(GetViewport());

            PackedScene levelBarrierScene = (PackedScene)ResourceLoader.Load("res://scenes/LevelBarrier.tscn"); 
            BackgroundAndLevel.LevelBarrier levelBarrier = levelBarrierScene.Instance<BackgroundAndLevel.LevelBarrier>();
            AddChild(levelBarrier);
            
            GD.Randomize();
            _objectsContainer = GetNode<Node>("ObjectsContainer");
            SpawnObjects();
        }
        public override void _PhysicsProcess(float delta)
        {
            UpdateState();


            Label label = GetNode<Label>("Label");
            label.Text = "VisibleRect Size:" + GetViewport().GetVisibleRect().Size.ToString();
            label.Text += "\n Viewport Size:" + GetViewport().Size.ToString();
            label.Text += "\n Viewport OverrideSize:" + GetViewport().GetSizeOverride().ToString();

            switch (_gamestate)
            {
                case GAMESTATE.MOVING:
                    _selectedObject.Move(delta);
                    break;
                case GAMESTATE.IDLE:
                    break;

            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (_selectedObject != null)
            {
                _selectedObject.HandleOthersInput(@event);
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

                default:
                    return;
            }

            PackedScene objectScene = ResourceLoader.Load<PackedScene>($"res://scenes/{objectType}.tscn");

            for (int _ = 0; _ < _objectsNumber; _++)
            {
                uint positionX = GD.Randi() % (uint)Globals.ScreenInfo.Size[0];
                uint positionY = GD.Randi() % (uint)Globals.ScreenInfo.Size[1];

                BaseObject spawnedObject = objectScene.Instance<BaseObject>();
                spawnedObject.Init(new Vector2(positionX, positionY));
                _objectsContainer.AddChild(spawnedObject);
            }


        }
        private void UpdateState()
        {
            if (_selectedObject != null)
            {

                if (_selectedObject.State == OBJECTSTATE.MOVING)
                {
                    _gamestate = GAMESTATE.MOVING;
                    return;
                }
            }

            _gamestate = GAMESTATE.IDLE;
        }

        public void _on_GameObjects_UpdateSelection(BaseObject sender, bool eliminateOldSelection, bool unSelectAll)
        {
            if (eliminateOldSelection)
            {
                if (unSelectAll)
                {
                    _selectedObject = null;
                    return;
                }
            }

            _selectedObject = sender;
        }

    }
}