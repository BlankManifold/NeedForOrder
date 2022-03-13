using Godot;

namespace GameUI
{
    public class ScrollIconGameUI : TextureRect
    {
        // private Color _focusedMo;
        private Color _notFocusedModulate;
        public Color NotFocusedModulate
        {
            get { return _notFocusedModulate; }
        }
        private Color _focusedModulate;
        public Color FocusedModulate
        {
            get { return _focusedModulate; }
        }

        private Vector2 _notFocusedScale;
        public Vector2 NotFocusedScale
        {
            get { return _notFocusedScale; }
        }
        private Vector2 _focusedScale;
        public Vector2 FocusedScale
        {
            get { return _focusedScale; }
        }

        private float _notFocusedScaleFactor = 0.5f;
        
        [Export]
        private Globals.OBJECTTYPE _objectType;
        public Globals.OBJECTTYPE ObjectType { get { return _objectType;}}

        public static bool s_someButtonPressed = false;

        // private int _orderNumber;
        // public int OrderNumber
        // {
        //     get { return _orderNumber; }
        //     set { _orderNumber = value; }
        // }
      

        public async override void _Ready()
        {
            _notFocusedModulate = new Color(Modulate.r, Modulate.g, Modulate.b, 0.5f);
            _focusedModulate = Modulate;
        
            RectPivotOffset = RectSize / 2;
            _focusedScale = RectScale;
            _notFocusedScale = _focusedScale * _notFocusedScaleFactor;

            await ToSignal(GetTree(),"idle_frame");
            SetInitialValue(focused:false);

            //GetNode<Label>("Label").Text += " " + GetIndex();
        }

        public void SetInitialValue(bool focused = false)
        {
            if (!focused)
            {
                RectScale = _notFocusedScale;
                Modulate = _notFocusedModulate;
                return;
            }

            RectScale = _focusedScale;
            Modulate = _focusedModulate;
        }

        

    }
}