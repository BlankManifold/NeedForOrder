using Godot;

namespace GameUI
{
    public class ColorButton : TextureButton
    {
        private bool _colorOn = false;
        private ShaderMaterial _shader;

        [Signal]
        delegate void ColorPressed(bool colorOn);


        public override void _Ready()
        {
            base._Ready();

            _shader = (ShaderMaterial)Material;
            _shader.SetShaderParam("active", !_colorOn);

            Main.Main mainNode = (Main.Main)GetTree().GetNodesInGroup("main")[0];
            Connect(nameof(ColorPressed), mainNode, "_on_ColorButton_ColorPressed");
        }

        public void UpdateColorOn(bool colorOn)
        {
            _colorOn = colorOn;
            _shader.SetShaderParam("active", !_colorOn);
        }

        public void _on_ColorButton_pressed()
        {
            _colorOn = !_colorOn;
            _shader.SetShaderParam("active", !_colorOn);

            EmitSignal(nameof(ColorPressed), _colorOn);
        }
    }

}