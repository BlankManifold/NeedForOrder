using Godot;

namespace GameUI
{
    public class ScrollNumber : ScrollGameUI
    {

        [Signal]
        delegate void NumberChanged(uint number);


        public override void _Ready()
        {
            base._Ready();

            Main.Main mainNode = (Main.Main)GetTree().GetNodesInGroup("main")[0];
            Connect(nameof(NumberChanged), mainNode, "_on_ScrollGameUI_NumberChanged");
        }

        public override void _on_Tween_tween_all_completed()
        {
            base._on_Tween_tween_all_completed();
            
            ScrollNumberIconGameUI selectedIcon = (ScrollNumberIconGameUI)_focusedButton;
            EmitSignal(nameof(NumberChanged), selectedIcon.Number);
        }
    }
}
