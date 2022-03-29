using Godot;
using Globals;

namespace SaveSystem
{
    public static class SaveObjectsHandler
    {
        public static void SaveObjects(Globals.OBJECTTYPE objectType, uint numberOfObjects, Godot.Collections.Array objects, int backgroundNumber)
        {
            File objectsDataFile = new File();
            objectsDataFile.Open($"user://{Utilities.ObjectTypeToString(objectType)}{numberOfObjects}.save", File.ModeFlags.Write);

            foreach (GameObjects.BaseObject objectNode in objects)
            {
                Godot.Collections.Dictionary<string, object> data = objectNode.CreateDict();
                
                objectsDataFile.StoreLine(JSON.Print(data));
            }

            objectsDataFile.Close();
        }
        public static void SaveBackground(Globals.OBJECTTYPE objectType, uint numberOfObjects, int backgroundNumber, bool colorOn)
        {
            File backgroundDataFile = new File();
            string filePath = $"user://{Utilities.ObjectTypeToString(objectType)}{numberOfObjects}Background.save";
            backgroundDataFile.Open(filePath, File.ModeFlags.Write);

            Godot.Collections.Dictionary<string, object> backgroundData = new Godot.Collections.Dictionary<string, object> ();
            backgroundData.Add("BackgroundNumber", backgroundNumber);
            backgroundData.Add("ColorOn", colorOn);
            backgroundDataFile.StoreLine(JSON.Print(backgroundData));

            backgroundDataFile.Close();
        }
        public static bool LoadObjects(Globals.OBJECTTYPE objectType, uint numberOfObjects, Node objectsContainer)
        {
            File fileToLoad = new File();
            string filePath = $"user://{Utilities.ObjectTypeToString(objectType)}{numberOfObjects}.save";
           
            if (!fileToLoad.FileExists(filePath))
                return false;

            
            fileToLoad.Open(filePath, File.ModeFlags.Read);

        
            while (fileToLoad.GetPosition() < fileToLoad.GetLen())
            {
                var nodeData = new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary)JSON.Parse(fileToLoad.GetLine()).Result);
                
                //BaseObjectDict.Clear();
                //BaseObjectDict.AddDataFromJSON(fileToLoad);
                
                PackedScene objectScene = ResourceLoader.Load<PackedScene>(Utilities.GetObjectScenePath(objectType));
                GameObjects.BaseObject objectNode = objectScene.Instance<GameObjects.BaseObject>();
                
                objectNode.LoadData(nodeData);
                //objectNode.LoadData(BaseObjectDict.s_dict);
                objectsContainer.AddChild(objectNode);
            }

            fileToLoad.Close();
            return true;
        }
        public static bool LoadBackground(Globals.OBJECTTYPE objectType, uint numberOfObjects, ref int backgroundNumber, ref bool colorOn)
        {
            File fileToLoad = new File();
            string filePath = $"user://{Utilities.ObjectTypeToString(objectType)}{numberOfObjects}Background.save";
           
            if (!fileToLoad.FileExists(filePath))
                return false;

            fileToLoad.Open(filePath, File.ModeFlags.Read);

            var backgroundData = new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary)JSON.Parse(fileToLoad.GetLine()).Result);
            //backgroundNumber =  (int)backgroundData["BackgroundNumber"];
            colorOn = (bool)backgroundData["ColorOn"];

            fileToLoad.Close();
            
            return true;
        }

    }


}