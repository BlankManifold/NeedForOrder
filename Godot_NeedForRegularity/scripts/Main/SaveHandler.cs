using Godot;
using Globals;

namespace SaveSystem
{
    public static class SaveObjectsHandler
    {
        public static void SaveObjects(Globals.OBJECTTYPE objectType, uint numberOfObjects, Godot.Collections.Array objects)//, uint backgroundNumber)
        {
            File objectsDataFile = new File();
            objectsDataFile.Open($"user://{Utilities.ObjectTypeToString(objectType)}{numberOfObjects}.save", File.ModeFlags.Write);

            foreach (GameObjects.BaseObject objectNode in objects)
            {
                Godot.Collections.Dictionary<string, object> data = objectNode.CreateDict();

                objectsDataFile.StoreLine(JSON.Print(data));
            }

            // Godot.Collections.Dictionary<string, uint> backgroundData = new Godot.Collections.Dictionary<string, uint> ();
            // backgroundData.Add("BackgroundNumber", backgroundNumber);
            // objectsDataFile.StoreLine(JSON.Print(backgroundData));

            objectsDataFile.Close();
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

                PackedScene objectScene = ResourceLoader.Load<PackedScene>(Utilities.GetObjectScenePath(objectType));
                GameObjects.BaseObject objectNode = objectScene.Instance<GameObjects.BaseObject>();
                
                objectNode.LoadData(nodeData);
                objectsContainer.AddChild(objectNode);
            }

            fileToLoad.Close();
            return true;
        }
    }


}