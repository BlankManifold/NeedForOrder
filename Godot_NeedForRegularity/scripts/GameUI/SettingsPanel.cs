using Godot;

namespace GameUI
{
    public class SettingsPanel : Control
    {

        private TextureButton _currentButton;
        private Tween _lineTween;
        private TextureRect _line;
        private ColorRect _unselectionArea;

        [Signal]
        delegate void ButtonPressed(Color colorSelected);

        [Signal]
        delegate void Unselect();




        public override void _Ready()
        {
            Main.Main mainNode = (Main.Main)GetTree().GetNodesInGroup("main")[0];
            Connect(nameof(ButtonPressed), mainNode, "_SettingsPanel_ButtonPressed");
            Connect(nameof(Unselect), mainNode, "_on_SettingsButton_SettingsPressed");



            Godot.Collections.Array<TextureButton> buttons = new Godot.Collections.Array<TextureButton>(GetTree().GetNodesInGroup("SettingsButton"));
            foreach (TextureButton button in buttons)
            {
                button.Connect("pressed", this, "_on_someButton_pressed", new Godot.Collections.Array { button });
            }

            _line = (TextureRect)FindNode("SelectionLine");
            _currentButton = (TextureButton)FindNode("BlueButton");
            _unselectionArea = GetNode<ColorRect>("Control/UnselectionRect");
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (Visible)
            {

                if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == 1 && mouseButtonEvent.IsPressed())
                {
                    if (!_unselectionArea.GetRect().HasPoint(mouseButtonEvent.GlobalPosition))
                    {
                        EmitSignal(nameof(Unselect));
                    }
                    mouseButtonEvent.Dispose();
                    return;
                }
            }

            @event.Dispose();
            return;
        }
        private void _on_someButton_pressed(TextureButton button)
        {
            Vector2 pos = _line.RectPosition;

            _currentButton.RemoveChild(_line);
            button.AddChild(_line);


            _line.RectPosition = pos;
            _currentButton = button;

            EmitSignal(nameof(ButtonPressed), button.Modulate);
        }

    }
}