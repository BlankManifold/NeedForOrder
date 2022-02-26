using Godot;

namespace Globals

{
    public enum GAMESTATE
    {
        IDLE, MOVING, ROTATING, SCALING
    }
    public enum OBJECTSTATE
    {
        UNSELECTED, SELECTED, PRESSED, MOVING, ROTATING, SCALING
    }
    public enum OBJECTTYPE
    {
        DOT, SQUARE, LINE, CIRCLE
    }
    
    public struct ScreenInfo
    {
        public static Vector2 Size;
        public static Vector2 VisibleRectSize;
        public static Rect2 VisibleRect;
        public static Vector2 DisplaySize;
        public static Vector2 ScaleFactor;
        public static void UpdateScreenInfo(Viewport viewport)
        {
            Globals.ScreenInfo.Size = viewport.Size;
            Globals.ScreenInfo.VisibleRectSize = viewport.GetVisibleRect().Size;
            Globals.ScreenInfo.VisibleRect = viewport.GetVisibleRect();
            Globals.ScreenInfo.DisplaySize = new Vector2((int)ProjectSettings.GetSetting("display/window/size/width"), (int)ProjectSettings.GetSetting("display/window/size/height"));
            Globals.ScreenInfo.ScaleFactor = Globals.ScreenInfo.DisplaySize / Globals.ScreenInfo.VisibleRectSize;
        }
    }

    public struct MouseInfo
    {
        public static Vector2 Position;
    }
    public struct RandomManager
    {
        public static RandomNumberGenerator rng = new RandomNumberGenerator();
    }
}

