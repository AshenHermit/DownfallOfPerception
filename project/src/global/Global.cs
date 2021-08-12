using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class Global : Node
    {
        public static Global Instance { get { return _instance; } }
        private static Global _instance;

        string Version = "0.7 beta";

        public event Action OnSceneReady;
        public Node CurrentSceneInstance;
        public string CurrentSceneName;

        public int SpawnPointId = -1;

        public bool DEBUG = false;
        public bool GameBooted = false;

        public Godot.Collections.Array<string> Locales = new Godot.Collections.Array<string>{ "en", "ru" };

        public delegate void InfoPopupHandler(string message);
        public event InfoPopupHandler OnInfoPopup;

        public static string mainMenuScenePath = "res://scenes/MainMenu.tscn";

        public Global()
        {
            GD.Seed(OS.GetUnixTime());

            _instance = this;

            TranslationServer.AddTranslation(GD.Load<Translation>("res://localization/translation.en.translation"));
            TranslationServer.AddTranslation(GD.Load<Translation>("res://localization/translation.ru.translation"));
            SetLocale("en");

            _gameStorageManager = new GameStorageManager();
            _audioManager = new AudioManager();
            _audioManager._Ready();
            _objectivesManager = new ObjectivesManager();
        }

        public void PopupInfo(string message)
        {
            if (OnInfoPopup != null) OnInfoPopup.Invoke(message);
        }

        public void CollectGarbage()
        {
            GC.Collect();
        }

        public override void _Ready()
        {
            SetMainSceneInstance(GetNode<Node>("/root/scene"));
            FreeOldEnvironment();
            CallDeferred("DefferedReady");
        }
        void DefferedReady()
        {
            GameBooted = true;
        }
        public override void _Process(float delta)
        {
            if (_audioManager != null)
            {
                _audioManager.Process(delta);
            }
        }
        public void SetMainSceneInstance(Node node)
        {
            if (node.Filename != "")
            {
                CurrentSceneName = node.Filename.GetFileNameFromPath();
            }
            else
            {
                CurrentSceneName = "";
            }
            CurrentSceneInstance = node;
            if (OnSceneReady != null) OnSceneReady.Invoke();
        }
        void FreeOldEnvironment()
        {
            if (CurrentSceneInstance.HasNode("WorldEnvironment"))
            {
                WorldEnvironment oldEnv = CurrentSceneInstance.GetNode<WorldEnvironment>("WorldEnvironment");
                if (oldEnv != null) { 
                    oldEnv.QueueFree();
                    CurrentSceneInstance.RemoveChild(oldEnv);
                }
            }
        }
        async public void WaitAndRemove(Node nodeToRemove, float time)
        {
            await ToSignal(GetTree().CreateTimer(time), "timeout");
            nodeToRemove.QueueFree();
        }
        public void SetEnvironment(Godot.Environment environment)
        {
            if (GetViewport().GetCamera() != null)
            {
                GetViewport().GetCamera().Environment = environment;
            }
        }
        public void SetGamePauseState(bool pauseState)
        {
            GetTree().Paused = pauseState;
        }
        public void ExitGame()
        {
            OS.WindowFullscreen = false;
            GetTree().Quit();
        }
        public Godot.Collections.Array GetPersistNodes()
        {
            return GetTree().GetNodesInGroup("Persist");
        }
        void RemoveSceneComponents()
        {
            _player = null;
            _playerCamera = null;
            _inventory = null;
            _npcManager = null;
            _generationManager = null;
            _uiManager = null;
            _vfxManager = null;
        }

        void ClearEventHandlers()
        {
            if (OnInfoPopup == null) return;
            foreach (InfoPopupHandler handler in OnInfoPopup.GetInvocationList())
            {
                OnInfoPopup -= handler;
            }
        }

        //TODO: all scene controlling things in Global should be in some another place, in another manager
        public void LoadScene(string scenePath, int spawnPointId = 0, bool updateSave=true)
        {
            if (scenePath == mainMenuScenePath) Input.SetMouseMode(Input.MouseMode.Visible);

            ClearEventHandlers();

            AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("world"), -100.0f);

            SpawnPointId = spawnPointId;
            SetGamePauseState(false);
            if(updateSave) GetGameStorage().UpdateGameSave();
            RemoveSceneComponents();
            CurrentSceneInstance.QueueFree();
            SetMainSceneInstance(GD.Load<PackedScene>(scenePath).Instance<Node>());
            GetNode<Node>("/root").AddChild(CurrentSceneInstance);
            FreeOldEnvironment();
            GetGameStorage().LoadScene();
        }

        public PhysicsDirectSpaceState GetDirectSpaceState()
        {
            return GetPlayer().GetWorld().DirectSpaceState;
        }


        //TODO: better turn "Get..." methods into "get" properties

        // player
        private Player _player;
        public event Action OnPlayerRegistered;
        public void RegisterPlayer(Player player) { 
            _player = player;
            if (OnPlayerRegistered != null) OnPlayerRegistered.Invoke();
        }
        public Player GetPlayer() { return _player; }


        // player camera
        private PlayerCamera _playerCamera;
        public void RegisterPlayerCamera(PlayerCamera playerCamera) { _playerCamera = playerCamera; }
        public PlayerCamera GetPlayerCamera() { return _playerCamera; }

        // ui manager
        private UI.UIManager _uiManager;
        public void RegisterUIManager(UI.UIManager uiManager) { _uiManager = uiManager; }
        public UI.UIManager GetUIManager() { return _uiManager; }

        // inventory
        private Inventory _inventory;
        public event Action OnInventoryRegistered;
        public void RegisterInventory(Inventory inventory) { 
            _inventory = inventory;
            if (OnInventoryRegistered != null) OnInventoryRegistered.Invoke();
        }
        public Inventory GetInventory() { return _inventory; }

        // npc manager
        private NpcManager _npcManager;
        public void RegisterNpcManager(NpcManager npcManager) { _npcManager = npcManager; }
        public NpcManager GetNpcManager() { return _npcManager; }

        // vfx manager
        private VfxManager _vfxManager;
        public void RegisterVfxManager(VfxManager vfxManager) { _vfxManager = vfxManager; }
        public VfxManager GetVfxManager() { return _vfxManager; }

        // generation manager
        private GenerationManager _generationManager;
        public void RegisterGenerationManager(GenerationManager generationManager) { _generationManager = generationManager; }
        public GenerationManager GetGenerationManager() { return _generationManager; }
        
        // game storage
        private GameStorageManager _gameStorageManager;
        public GameStorageManager GetGameStorage() { return _gameStorageManager; }

        // audioManager
        private AudioManager _audioManager;
        public AudioManager GetAudioManager() { return _audioManager; }

        // objectivesManager
        private ObjectivesManager _objectivesManager;
        public ObjectivesManager GetObjectivesManager() { return _objectivesManager; }

        // objectivesManager
        private LoadingScreen _loadingScreen;
        public void RegisterLoadingScreen(LoadingScreen loadingScreen) { _loadingScreen = loadingScreen; }
        public LoadingScreen GetLoadingScreen() { return _loadingScreen; }




        // localization
        /// <summary>
        /// Sets the locale of game localization.
        /// </summary>
        /// <param name="locale">translation locale. for example: "en", "ru" ...</param>
        public static void SetLocale(string locale)
        {
            TranslationServer.SetLocale(locale);
        }
        /// <summary>
        /// Returns the current locale's translation for the given key.
        /// </summary>
        /// <param name="key">key of message in translation table</param>
        /// <returns></returns>
        public static string Translate(string key)
        {
            return TranslationServer.Translate(key);
        }
        /// <summary>
        /// Translates message with random variation index.
        /// </summary>
        /// <param name="messagePrefix">If prefix is "loading_screen_text.", then random message can be "loading_screen_text.0" or "loading_screen_text.3".</param>
        /// <param name="maxMessagesCount">Max variation indices count.</param>
        /// <returns></returns>
        public static string TranslateRandomMessageVariation(string messagePrefix, int maxMessagesCount)
        {
            return Translate(messagePrefix + (Mathf.Floor(GD.Randf() * maxMessagesCount)).ToString());
        }
        public string TranslateWithInstance(string key)
        {
            return Global.Translate(key);
        }
    }
}