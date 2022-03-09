using Godot;

namespace GameUI
{
    public class ScrollObjectType : ScrollGameUI
    {
       
        [Signal]
        delegate void ObjectTypeChanged(Globals.OBJECTTYPE type);


        public override void _Ready()
        {
            base._Ready();

            Main.Main mainNode = (Main.Main)GetTree().GetNodesInGroup("main")[0];
            Connect(nameof(ObjectTypeChanged), mainNode, "_on_ScrollGameUI_ObjectTypeChanged");

        }

        public override void _on_Tween_tween_all_completed()
        {
           base._on_Tween_tween_all_completed();
            EmitSignal(nameof(ObjectTypeChanged), _focusedButton.ObjectType);
        }
    }
}