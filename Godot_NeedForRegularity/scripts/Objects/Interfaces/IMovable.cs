using Godot;

namespace GameObjects
{

    public interface IMovable
    {
        Vector2 RelevantPosition {get;}
        void MoveObject(float delta);

    }
}