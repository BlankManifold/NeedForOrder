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

    public static class Utilities
    {

        public static string ObjectTypeToString(OBJECTTYPE type)
        {
            switch (type)
            {
                case OBJECTTYPE.SQUARE:
                    return "SquareObject";

                case OBJECTTYPE.DOT:
                    return "DotObject";

                case OBJECTTYPE.CIRCLE:
                    return "CircleObject";

                case OBJECTTYPE.LINE:
                    return "LineObject";

                default:
                    return "SquareObject";
            }
        }

        public static string GetObjectScenePath(OBJECTTYPE type)
        {
            return $"res://scenes/SpecificObjects/{ObjectTypeToString(type)}.tscn";
        }
    }
    public enum OBJECTTYPE
    {
        DOT, SQUARE, LINE, CIRCLE, NOTAOBJECT
    }
    public struct ObjectsInterfaces
    {
        public bool IMovable;
        public bool IRotatable;
        public bool IScalable;

        public ObjectsInterfaces(bool movable = true, bool rotatable = false, bool scalable = false)
        {
            IMovable = movable;
            IRotatable = rotatable;
            IScalable = scalable;
        }

        public ObjectsInterfaces(OBJECTTYPE type)
        {
            switch (type)
            {
                case OBJECTTYPE.SQUARE:
                case OBJECTTYPE.LINE:

                    IMovable = true;
                    IRotatable = true;
                    IScalable = false;
                    break;

                case OBJECTTYPE.DOT:
                case OBJECTTYPE.CIRCLE:
                    IMovable = true;
                    IRotatable = false;
                    IScalable = false;
                    break;

                default:
                    IMovable = true;
                    IRotatable = false;
                    IScalable = false;
                    break;
            }
        }
    }

    public struct ScreenInfo
    {
        public static Vector2 Size;
        public static Vector2 VisibleRectSize;
        public static Rect2 VisibleRect;
        public static Vector2 DisplaySize;
        public static Vector2 ScaleFactor;
        public static Vector2 PlayableSize;
        public static void UpdateScreenInfo(Viewport viewport)
        {
            ScreenInfo.Size = viewport.Size;
            ScreenInfo.VisibleRectSize = viewport.GetVisibleRect().Size;
            ScreenInfo.VisibleRect = viewport.GetVisibleRect();
            ScreenInfo.DisplaySize = new Vector2((int)ProjectSettings.GetSetting("display/window/size/width"), (int)ProjectSettings.GetSetting("display/window/size/height"));
            ScreenInfo.ScaleFactor = ScreenInfo.DisplaySize / ScreenInfo.VisibleRectSize;
        }
        public static void UpdatePlayableSize(Vector2 limits)
        {
            ScreenInfo.PlayableSize = ScreenInfo.VisibleRectSize - limits;
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

