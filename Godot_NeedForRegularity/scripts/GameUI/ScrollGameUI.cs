using Godot;
using System.Collections.Generic;

namespace GameUI
{
    public class ScrollGameUI : CenterContainer
    {
        private List<ScrollIconGameUI> _buttons = new List<ScrollIconGameUI>() { };
        protected ScrollIconGameUI _focusedButton;
        private HBoxContainer _hbox;

        private bool _pressed = false;
        private static bool _movable = true;
        private int _maxIndex;

        [Export]
        private int _initialIndex;

        protected Tween _focusingTween;
        private float _tweenSpeed = 0.2f;

        [Signal]
        delegate void FocusingTweenCompleted();
        [Signal]
        delegate void FocusingTweenStarted();


        public async override void _Ready()
        {

            _focusingTween = GetNode<Tween>("Tween");

            Main.Main mainNode = (Main.Main)GetTree().GetNodesInGroup("main")[0];
            Connect(nameof(FocusingTweenStarted), mainNode, "_on_ScrollGameUI_FocusingTweenStarted");
            Connect(nameof(FocusingTweenCompleted), mainNode, "_on_ScrollGameUI_FocusingTweenCompleted");

            _hbox = GetNode<HBoxContainer>("CenterContainer/HBoxContainer");

            foreach (ScrollIconGameUI button in _hbox.GetChildren())
            {
                _buttons.Add(button);
            }

            _maxIndex = _buttons.Count - 1;
            _focusedButton = _buttons[_initialIndex];

            await ToSignal(GetTree(), "idle_frame");
            _focusedButton.SetInitialValue(focused: true);
        }

        public async void UpdateFocus(ScrollIconGameUI newFocusedButton)
        {

            int currentIndex = _focusedButton.GetIndex();
            int newIndex = newFocusedButton.GetIndex();

            SetUpFocusingTweenButtons(newFocusedButton);
            if (CheckIfValidTranslation(currentIndex, newIndex))
            {
                int target = Mathf.Clamp(newIndex, 1, _maxIndex - 1);
                int initial = Mathf.Clamp(currentIndex, 1, _maxIndex - 1);

                // SetUpFocusingTweenHBox(_buttons[target]);
                SetUpFocusingTweenHBox(_buttons[target], _buttons[initial]);
            }

            _focusingTween.Start();
            _focusedButton = newFocusedButton;

            await ToSignal(this, nameof(FocusingTweenCompleted));
        }

        private void SetUpFocusingTweenButtons(ScrollIconGameUI newFocusedButton)
        {
            _focusingTween.InterpolateProperty(_focusedButton, "modulate", _focusedButton.FocusedModulate, _focusedButton.NotFocusedModulate, _tweenSpeed);
            _focusingTween.InterpolateProperty(_focusedButton, "rect_scale", _focusedButton.FocusedScale, _focusedButton.NotFocusedScale, _tweenSpeed);

            _focusingTween.InterpolateProperty(newFocusedButton, "modulate", newFocusedButton.NotFocusedModulate, newFocusedButton.FocusedModulate, _tweenSpeed);
            _focusingTween.InterpolateProperty(newFocusedButton, "rect_scale", newFocusedButton.NotFocusedScale, newFocusedButton.FocusedScale, _tweenSpeed);

        }
        private void SetUpFocusingTweenHBox(ScrollIconGameUI target, ScrollIconGameUI initial)
        {
            float deltaX = (target.RectPosition.x - initial.RectPosition.x) * _hbox.RectScale.x;
            float oldXPos = _hbox.RectPosition.x;
            float newXPos = oldXPos - deltaX;
            _focusingTween.InterpolateProperty(_hbox, "rect_position:x", oldXPos, newXPos, _tweenSpeed);
        }
        private bool CheckIfValidTranslation(int currentIndex, int newIndex)
        {
            return !(
                    (currentIndex == 0 && newIndex == 1) ||
                    (currentIndex == 1 && newIndex == 0) ||
                    (currentIndex == _maxIndex - 1 && newIndex == _maxIndex) ||
                    (currentIndex == _maxIndex && newIndex == _maxIndex - 1)
                    );
        }

        public void _on_Area2D_input_event(Node _, InputEvent @event, int __)
        {
            if (_movable)
            {

                if (@event is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == 1)
                {

                    if (mouseButtonEvent.IsPressed())
                    {
                        _pressed = true;
                        mouseButtonEvent.Dispose();
                        return;
                    }

                    if (!mouseButtonEvent.IsPressed())
                    {
                        _pressed = false;
                        mouseButtonEvent.Dispose();
                        return;
                    }

                    mouseButtonEvent.Dispose();
                    return;
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
                        _pressed = false;
                    }
                    mouseMotion.Dispose();
                    return;
                }

            }
            @event.Dispose();
           
            return;


        }

        public virtual void _on_Tween_tween_all_completed()
        {
            _movable = true;
            EmitSignal(nameof(FocusingTweenCompleted));
        }

        public virtual void _on_Tween_tween_started(object _, NodePath nodePath)
        {
            _movable = false;
            EmitSignal(nameof(FocusingTweenStarted));
        }
    
    
    }


    
}