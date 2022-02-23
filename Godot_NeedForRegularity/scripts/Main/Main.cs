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
        private uint _objectsNumber = 1000;

        private Node _objectsContainer;

        public override void _Ready()
        {
            GD.Randomize();
            Globals.ScreenInfo.size = GetViewport().Size;
            Globals.ScreenInfo.screenRect = GetViewport().GetVisibleRect();
            _objectsContainer = GetNode<Node>("ObjectsContainer");

            SpawnObjects();
        }
        public override void _PhysicsProcess(float delta)
        {
            UpdateState();

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
               _selectedObject.HandleOutsideInput(@event);
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
                uint positionX = GD.Randi() % (uint)Globals.ScreenInfo.size[0];
                uint positionY = GD.Randi() % (uint)Globals.ScreenInfo.size[1];

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