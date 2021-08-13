using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class GameStorageManager : Godot.Reference
    {
        int _saveId=0;
        string _gameSavesDirectory = "user://";
        string _gameSaveFilePath = "game_save_{0}.json";
        string _statsFilePath = "stats.json";
        string _settingsFilePath = "settings.json";

        // getter properties
        public string GameSaveFilePath { get { return _gameSavesDirectory + String.Format(_gameSaveFilePath, _saveId); } }
        public string StatsFilePath { get { return _gameSavesDirectory + _statsFilePath; } }
        public string SettingsFilePath { get { return _gameSavesDirectory + _settingsFilePath; } }

        // dictionaries
        public Godot.Collections.Dictionary<string, object> Stats = new Godot.Collections.Dictionary<string, object>(){
            {"last_game_save_id", -1},
            {"game_saves", new Godot.Collections.Array<string>()},
        };

        public Godot.Collections.Dictionary<string, object> DefaultSettings = new Godot.Collections.Dictionary<string, object>(){
            {"locale", "en"},
            {"volume", 0.0f},
            {"mouse_sensivity", 100.0f},
            {"fullscreen", false},
            {"meat_is_lasagna", false},
            {"debug", false},
            {"fxaa", false},
        };
        public Godot.Collections.Dictionary<string, object> Settings;

        public Godot.Collections.Dictionary<string, object> GameSaveEmpty = new Godot.Collections.Dictionary<string, object>(){
            {"max_health", 100},
            {"health", 60},
            {"inventory", new Godot.Collections.Array()},
            {"active_item_id", -1},
            {"spawn_point_id", 0},
            {"player_transform", Transform.Identity},
            {"scene", "res://scenes/Begining.tscn"},
            {"saved_scenes", new Godot.Collections.Dictionary()},
            {"objectives", new Godot.Collections.Dictionary()},
        };
        public Godot.Collections.Dictionary<string, object> GameSave;

        public static System.Collections.Generic.HashSet<string> persistScenes = new System.Collections.Generic.HashSet<string> {
        "res://scenes/Begining.tscn", "res://scenes/Proxy.tscn"};

        bool listenersInitialized = false;
        
        public GameStorageManager()
        {
            Settings = DefaultSettings.Duplicate();
            GameSaveEmpty = LoadDictionaryFromJson("res://data/default_game_save.json", GameSaveEmpty);
            GameSave = GameSaveEmpty.Duplicate();

            LoadStats();
            LoadSettings();

            InitializeListeners();
        }
        public void ApplySettings()
        {
            OS.WindowFullscreen = (bool)Settings["fullscreen"];
            Global.SetLocale((string)Settings["locale"]);

            Global.Instance.GetAudioManager().SetVolume(Settings.GetFloat("volume"));

            if (Global.Instance.CurrentSceneInstance == null) return;
            Global.Instance.GetViewport().Msaa = Settings.Get<bool>("fxaa") ? Viewport.MSAA.Disabled : Viewport.MSAA.Msaa4x;
            Global.Instance.GetViewport().Fxaa = Settings.Get<bool>("fxaa");
            if (Global.Instance.GetPlayerCamera() != null)
            {
                Global.Instance.GetPlayerCamera().SetMouseSensivity(Settings.Get<float>("mouse_sensivity"));
            }
        }

        public void InitializeListeners()
        {
            if (!listenersInitialized)
            {
                listenersInitialized = true;
                Global.Instance.OnSceneReady += () =>
                {
                    ApplySettings();
                };
                Global.Instance.OnPlayerRegistered += () =>
                {
                    Global.Instance.GetObjectivesManager().ImportObjectives(GameSave.Get<Godot.Collections.Dictionary>("objectives"));

                    Global.Instance.GetPlayer().SetMaxHealth(GameSave.GetFloat("max_health"));
                    Global.Instance.GetPlayer().SetHealth(GameSave.GetFloat("health"));
                    if (GameSave.GetInt("spawn_point_id") == -1)
                    {
                        Global.Instance.GetPlayer().GlobalTransform = ((Transform)GD.Str2Var(GameSave.Get<string>("player_transform")));
                    }

                    Global.Instance.GetPlayer().OnHealthChangeEvent += (float healthChange) =>
                    {
                        //GameSave["health"] = Global.Instance.GetPlayer().GetHealth();
                    };
                };
                Global.Instance.OnInventoryRegistered += () =>
                {
                    Global.Instance.GetInventory().ImportItems((Godot.Collections.Array)GameSave["inventory"]);
                    Global.Instance.GetInventory().CallDeferred("SetActiveItemIndex", Mathf.Max(0, Convert.ToInt32(GameSave["active_item_id"])));
                    Global.Instance.GetInventory().OnItemsArrayChanged += () =>
                    {
                        
                    };
                };

                listenersInitialized = true;
            }
        }

        void SaveDictionaryToJson(string filepath, Godot.Collections.Dictionary<string, object> dict)
        {
            File saveFile = new File();
            saveFile.Open(filepath, File.ModeFlags.Write);
            saveFile.StoreString(JSON.Print(dict, indent: "  "));
            saveFile.Close();
        }
        Godot.Collections.Dictionary<string, object> LoadDictionaryFromJson(string filepath, Godot.Collections.Dictionary<string, object> defaultDict)
        {
            File file = new File();
            if (!file.FileExists(filepath))
            {
                SaveDictionaryToJson(filepath, defaultDict);
                return defaultDict;
            }
            file.Open(filepath, File.ModeFlags.Read);
            Godot.Collections.Dictionary<string, object> dict = 
                defaultDict.Assign(new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary)JSON.Parse(file.GetAsText()).Result));
            file.Close();
            return dict;
        }
        void SaveDictionaryToBinary(string filepath, Godot.Collections.Dictionary<string, object> dict)
        {
            File saveFile = new File();
            saveFile.Open(filepath, File.ModeFlags.Write);
            saveFile.StoreVar(dict, true);
            saveFile.Close();
        }
        Godot.Collections.Dictionary<string, object> LoadDictionaryFromBinary(string filepath, Godot.Collections.Dictionary<string, object> defaultDict)
        {
            File file = new File();
            if (!file.FileExists(filepath))
            {
                SaveDictionaryToBinary(filepath, defaultDict);
                return defaultDict;
            }
            file.Open(filepath, File.ModeFlags.Read);
            Godot.Collections.Dictionary<string, object> dict = new Godot.Collections.Dictionary<string, object>((Godot.Collections.Dictionary)file.GetVar());
            file.Close();
            dict = defaultDict.Assign(dict);
            return dict;
        }


        public void LoadStats()
        {
            Stats = LoadDictionaryFromJson(StatsFilePath, Stats);
        }
        public void LoadSettings()
        {
            Settings = LoadDictionaryFromJson(SettingsFilePath, Settings);
        }
        public void LoadGame(int saveId)
        {
            Global.Instance.GetAudioManager().StopMusic(0.0f);
            Global.Instance.DEBUG = false || Settings.Get<bool>("debug");
            _saveId = saveId;
            GameSave = LoadDictionaryFromJson(GameSaveFilePath, GameSave);
            Global.Instance.LoadScene((string)GameSave["scene"], Convert.ToInt32(GameSave["spawn_point_id"]), false);
        }
        public void LoadGameByName(string name)
        {
            LoadGame(((Godot.Collections.Array)Stats["game_saves"]).IndexOf(name));
        }
        public void StartNewGame()
        {
            GameSaveEmpty = LoadDictionaryFromJson("res://data/new_game_save.json", GameSaveEmpty);
            GameSave = GameSaveEmpty.Duplicate();
            _saveId = ((Godot.Collections.Array)Stats["game_saves"]).Count;
            ((Godot.Collections.Array)Stats["game_saves"]).Add((_saveId+1).ToString());
            Stats["last_game_save_id"] = _saveId;
            SaveStats();
            LoadGame(_saveId);
        }
        public void ContinueGame()
        {
            int saveId = Convert.ToInt32(Stats["last_game_save_id"]);
            if (saveId != -1)
            {
                _saveId = saveId;
                LoadGame(_saveId);
            }
        }

        public Godot.Collections.Array<Godot.Collections.Dictionary<string, object>> ExportPersistNodes()
        {
            Godot.Collections.Array<Godot.Collections.Dictionary<string, object>> exportedNodes = new Godot.Collections.Array<Godot.Collections.Dictionary<string, object>>();

            Godot.Collections.Array persistNodes = Global.Instance.GetPersistNodes();
            foreach(Node node in persistNodes)
            {
                if (!node.Filename.Empty() && node is Spatial)
                {
                    Godot.Collections.Dictionary<string, object> dict = new Godot.Collections.Dictionary<string, object>();
                    dict["scene"] = node.Filename;
                    dict["transform"] = GD.Var2Str(((Spatial)node).GlobalTransform);

                    dict["parent"] = "."+node.GetParent().GetPath().ToString().Substring(Global.Instance.CurrentSceneInstance.GetPath().ToString().Length);

                    object item = node.Get("item"); ;
                    if (node.Get("Item")!=null)
                    {
                        item = node.Get("Item");
                    }
                    if (item != null)
                    {
                        if(item is Inventory.Item)
                        {
                            Inventory.Item invItem = (Inventory.Item)item;
                            dict["item"] = invItem.ExportToDict();
                        }
                    }
                    exportedNodes.Add(dict);
                }
            }
            return exportedNodes;
        }

        public void SaveScene()
        {
            if (Global.Instance.CurrentSceneInstance!=null)
            {
                // will not save nodes if scene is not in persist scenes hash set
                if (!persistScenes.Contains(Global.Instance.CurrentSceneInstance.Filename)) return;

                Godot.Collections.Array<Godot.Collections.Dictionary<string, object>> persistNodes = ExportPersistNodes();
                if (persistNodes.Count > 0)
                {
                    Godot.Collections.Dictionary savedScenes = (Godot.Collections.Dictionary)((Godot.Collections.Dictionary)GameSave["saved_scenes"]);
                    savedScenes[Global.Instance.CurrentSceneInstance.Filename] = persistNodes;
                }
            }
        }
        public void LoadScene()
        {
            if (Global.Instance.CurrentSceneInstance != null)
            {
                Godot.Collections.Dictionary savedScenes = (Godot.Collections.Dictionary)((Godot.Collections.Dictionary)GameSave["saved_scenes"]);
                if (savedScenes.Contains(Global.Instance.CurrentSceneInstance.Filename))
                {
                    Godot.Collections.Array persistNodes = 
                        (Godot.Collections.Array)savedScenes[Global.Instance.CurrentSceneInstance.Filename];

                    foreach(object nodeObject in persistNodes)
                    {
                        Godot.Collections.Dictionary<string, object> nodeDict = new Godot.Collections.Dictionary<string, object>(((Godot.Collections.Dictionary)nodeObject));
                        if (!nodeDict.ContainsKey("transform") || !nodeDict.ContainsKey("scene") || !nodeDict.ContainsKey("parent"))
                        {
                            GD.PrintErr("saved node cant be instanced");
                            continue;
                        }

                        Spatial instance = GD.Load<PackedScene>((string)nodeDict["scene"]).Instance<Spatial>();
                        Spatial parent = Global.Instance.CurrentSceneInstance.GetNode<Spatial>((string)nodeDict["parent"]);
                        parent.AddChild(instance);
                        string strVar = (string)nodeDict["transform"];
                        instance.GlobalTransform = ((Transform)GD.Str2Var(strVar));
                    }
                }
            }
        }

        public void UpdateGameSave()
        {
            if (Global.Instance.GetPlayer() != null && Global.Instance.GetInventory() != null)
            {
                GameSave["inventory"] = Global.Instance.GetInventory().ExportItems();
                GameSave["objectives"] = Global.Instance.GetObjectivesManager().ExportObjectives();
                GameSave["max_health"] = Global.Instance.GetPlayer().MaxHealth;
                GameSave["health"] = Global.Instance.GetPlayer().GetHealth();
                GameSave["scene"] = Global.Instance.CurrentSceneInstance.Filename;
                GameSave["spawn_point_id"] = -1;
                GameSave["player_transform"] = GD.Var2Str(((Spatial)Global.Instance.GetPlayer()).GlobalTransform);
                GameSave["active_item_id"] = Global.Instance.GetInventory().GetActiveItemIndex();
            }
            SaveScene();
        }

        public void SaveGame()
        {
            UpdateGameSave();

            Stats["last_game_save_id"] = _saveId;
            NormilizeStats();
            SaveStats();

            SaveDictionaryToJson(GameSaveFilePath, GameSave);

            Global.Instance.PopupInfo(Global.Translate("info_popup.GAME_SAVED"));
        }
        public void NormilizeStats()
        {
            Godot.Collections.Array game_saves = (Godot.Collections.Array)Stats["game_saves"];
            while (game_saves.Count < _saveId + 1) game_saves.Add((game_saves.Count + 1).ToString());
        }
        public void SaveStats()
        {
            string filepath = _gameSavesDirectory + _statsFilePath;
            SaveDictionaryToJson(filepath, Stats);
        }
        public void SaveSettings()
        {
            string filepath = _gameSavesDirectory + _settingsFilePath;
            SaveDictionaryToJson(filepath, Settings);
        }
    }
}
