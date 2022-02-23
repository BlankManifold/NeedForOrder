using Godot;

namespace GameObjects
{
    public interface ISelectable
    {
        void Select(bool unSelectAll = false);
        void HandleSelectionInput(InputEvent @event);
        
    }

}