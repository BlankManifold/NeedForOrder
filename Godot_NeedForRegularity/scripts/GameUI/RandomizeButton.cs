using Godot;

namespace GameUI
{
    public class RandomizeButton : TextureButton
    {
        [Signal]
        delegate void RandomizePressed();


        public override void _Ready()
        {
            base._Ready();
        
            Main.Main mainNode = (Main.Main)GetTree().GetNodesInGroup("main")[0];
            Connect(nameof(RandomizePressed), mainNode, "_on_RandomizeButton_RandomizePressed");
        }

        public void _on_RandomizeButton_pressed()
        {
            EmitSignal(nameof(RandomizePressed));
        }
    }

}