using Godot;

namespace GameObjects
{
    public interface ISelectable
    {
        void Select(bool unSelectAll = false);
        void HandleInsideInput(InputEvent @event);
        void HandleOutsideInput(InputEvent @event);
    }

}