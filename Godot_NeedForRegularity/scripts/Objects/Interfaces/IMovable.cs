using Godot;

namespace GameObjects
{

    public interface IMovable
    {
        Vector2 targetPosition {get;}
        void Move(float delta);

    }
}