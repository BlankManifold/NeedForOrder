using Godot;

namespace GameUI
{
    public class SettingsButton : TextureButton
    {
        [Signal]
        delegate void SettingsPressed();


        public override void _Ready()
        {
            base._Ready();
        
            Main.Main mainNode = (Main.Main)GetTree().GetNodesInGroup("main")[0];
            Connect(nameof(SettingsPressed), mainNode, "_on_SettingsButton_SettingsPressed");
        }

        public void _on_SettingsButton_pressed()
        {
            EmitSignal(nameof(SettingsPressed));
        }
    }

}