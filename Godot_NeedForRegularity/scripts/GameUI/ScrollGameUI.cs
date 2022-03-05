using Godot;
using System.Collections.Generic;

namespace GameUI
{
    public class ScrollGameUI : CenterContainer
    {
        private List<ScrollButtonGameUI> _buttons = new List<ScrollButtonGameUI>() { };
        protected ScrollButtonGameUI _focusedButton;
        private HBoxContainer _hbox;

        private bool _pressed = false;
        private int _maxIndex;

        protected Tween _focusingTween;
        private float _tweenSpeed = 0.3f;

        [Signal]
        delegate void FocusingTweenCompleted();
       

        public async override void _Ready()
        {

            _focusingTween = GetNode<Tween>("Tween");

            _hbox = GetNode<HBoxContainer>("CenterContainer/HBoxContainer");

            foreach (ScrollButtonGameUI button in _hbox.GetChildren())
            {
                _buttons.Add(button);
            }

            _maxIndex = _buttons.Count - 1;
            _focusedButton = _buttons[1];

            await ToSignal(GetTree(), "idle_frame");
            _focusedButton.SetInitialValue(focused: true);
        }

        private async void UpdateFocus(ScrollButtonGameUI newFocusedButton)
        {
            SetUpFocusingTweenButtons(newFocusedButton);
            SetUpFocusingTweenHBox(newFocusedButton);

            _focusingTween.Start();
            _focusedButton = newFocusedButton;

            await ToSignal(this, nameof(FocusingTweenCompleted));
        }

        private void SetUpFocusingTweenButtons(ScrollButtonGameUI newFocusedButton)
        {
            _focusingTween.InterpolateProperty(_focusedButton, "modulate", _focusedButton.FocusedModulate, _focusedButton.NotFocusedModulate, _tweenSpeed);
            _focusingTween.InterpolateProperty(_focusedButton, "rect_scale", _focusedButton.FocusedScale, _focusedButton.NotFocusedScale, _tweenSpeed);

            _focusingTween.InterpolateProperty(newFocusedButton, "modulate", newFocusedButton.NotFocusedModulate, newFocusedButton.FocusedModulate, _tweenSpeed);
            _focusingTween.InterpolateProperty(newFocusedButton, "rect_scale", newFocusedButton.NotFocusedScale, newFocusedButton.FocusedScale, _tweenSpeed);

        }
        private void SetUpFocusingTweenHBox(ScrollButtonGameUI newFocusedButton)
        {
            float deltaX = newFocusedButton.RectPosition.x - _focusedButton.RectPosition.x;
            float oldXPos = _hbox.RectPosition.x;
            float newXPos = oldXPos - deltaX;
            _focusingTween.InterpolateProperty(_hbox, "rect_position:x", oldXPos, newXPos, _tweenSpeed);

        }

        public void _on_Area2D_input_event(Node _, InputEvent @event, int __)
        {
            if (@event is InputEventMouseButton)
            {

                if (@event.IsActionPressed("select"))
                {
                    _pressed = true;

                    @event.Dispose();
                    return;
                }

                if (@event.IsActionReleased("select"))
                {
                    _pressed = false;
                    @event.Dispose();
                    return;
                }
            }

            if (@event is InputEventMouseMotion mouseMotion && _pressed)
            {
                if (Mathf.Abs(mouseMotion.Relative.x) > 5)
                {
                    int currentIndex = _focusedButton.GetIndex();

                    if (mouseMotion.Relative.x < 0 && currentIndex < _maxIndex)
                    {
                        _pressed = false;
                        UpdateFocus(_buttons[currentIndex + 1]);

                    }
                    else if (mouseMotion.Relative.x > 0 && currentIndex > 0)
                    {
                        _pressed = false;
                        UpdateFocus(_buttons[currentIndex - 1]);

                    }
                }

                mouseMotion.Dispose();
                return;
            }

            @event.Dispose();
            return;


        }

        public virtual void _on_Tween_tween_all_completed()
        {
            EmitSignal(nameof(FocusingTweenCompleted));     
        }
    }
}