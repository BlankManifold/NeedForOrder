using Godot;

namespace GameUI
{
    public class ScrollBackground : ScrollGameUI
    {

        [Signal]
        delegate void BackgroundChanged(Texture textureTile, int backgroundNumber, Vector2 offset);


        public override void _Ready()
        {
            base._Ready();

            Main.Main mainNode = (Main.Main)GetTree().GetNodesInGroup("main")[0];
            Connect(nameof(BackgroundChanged), mainNode, "_on_ScrollGameUI_BackgroundChanged");
        }

        public override void _on_Tween_tween_all_completed()
        {
            base._on_Tween_tween_all_completed();
            
            ScrollBackgroundIconGameUI selectedIcon = (ScrollBackgroundIconGameUI)_focusedButton;
            EmitSignal(nameof(BackgroundChanged), selectedIcon.TextureTile, selectedIcon.GetIndex(), selectedIcon.Offset);
        }
    }
}