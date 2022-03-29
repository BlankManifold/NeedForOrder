using Godot;
using System.Collections.Generic;


namespace Globals

{
    public enum GAMESTATE
    {
        IDLE, MOVING, ROTATING, SCALING, PAUSED
    }
    public enum OBJECTTYPE
    {
        DOT, SQUARE, LINE, CIRCLE, NOTAOBJECT
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
    public static class Colors
    {
        public static Color DefaultColor = new Color(0f, 0.058824f, 0.333333f);
        static Dictionary<string, Color> _dict = new Dictionary<string, Color>()
        {
            {"BLUE", new Color("000f55")},
            {"BLACK", new Color("f0000000")},
            {"RED", new Color("ac3235")},
            {"GREEN", new Color("11887b")}
        };
        static List<Color> _list = new List<Color>()
        {
            new Color(0f, 0.058824f, 0.333333f),
            new Color(0f, 0f, 0f),
            new Color(0.67451f, 0.196078f, 0.207843f),
            //new Color(0.066667f, 0.533333f, 0.482353f),
            new Color(0.015686f, 0.490196f, 0.329412f),
            //new Color(0.078431f, 0.078431f, 0.078431f)
        };

        // public static Color GetRandomColor()
        // {
        //     var colors = new List<Color>(_dict.Values);
        //     int index = Globals.RandomManager.rng.RandiRange(0, colors.Count - 1);
        //     return colors[index];
        // }
        public static Color GetRandomColor()
        {
            int index = Globals.RandomManager.rng.RandiRange(0, _list.Count - 1);
            return _list[index];
        }


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

        public static bool CheckIfValidPosition(Vector2 position)
        {
            return (0 <= position.x && position.x <= PlayableSize.x && 0 <= position.y && position.y <= PlayableSize.y);
        }
    }
    public struct RandomManager
    {
        public static RandomNumberGenerator rng = new RandomNumberGenerator();
    }

    // public static class BaseObjectDict
    // {
    //     public static Godot.Collections.Dictionary<string, object> s_dict = new Godot.Collections.Dictionary<string, object>() { };
    //     public static void Clear()
    //     {
    //         s_dict.Clear();
    //     }

    //     public static void AddBaseObjectData(GameObjects.BaseObject baseObject)
    //     {
    //         s_dict.Add("PositionX", baseObject.Position.x);
    //         s_dict.Add("PositionY", baseObject.Position.y);
    //         s_dict.Add("ColorR", baseObject.ObjectColor.r);
    //         s_dict.Add("ColorB", baseObject.ObjectColor.b);
    //         s_dict.Add("ColorG", baseObject.ObjectColor.g);
    //         s_dict.Add("ColorA", baseObject.ObjectColor.a);
    //     }
    //     public static void AddDataFromJSON(File fileToLoad)
    //     {
    //         s_dict = new Godot.Collections.Dictionary<string,object>((Godot.Collections.Dictionary)JSON.Parse(fileToLoad.GetLine()).Result);
    //     }
    // }

}

