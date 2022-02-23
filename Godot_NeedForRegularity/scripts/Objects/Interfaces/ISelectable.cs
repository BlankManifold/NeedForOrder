using Godot;

namespace GameObjects
{
    public interface ISelectable
    {
        void Select();
        void HandleSelectionInput(InputEvent @event);
        
    }

}