using Godot;

namespace Globals

{
    public enum GAMESTATE
    {
        IDLE, MOVING, ROTATING, SCALING
    }
    public enum OBJECTSTATE
    {
        UNSELECETED, SELECTED, PRESSED, MOVING, PRECISION_MOVING, ROTATING, SCALING
    }
    public enum OBJECTTYPE
    {
        DOT, SQUARE, LINE, CIRCLE
    }
    
    public struct ScreenInfo
    {
        public static Vector2 size;
        public static Rect2 screenRect;
    };
}

