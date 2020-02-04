using System;
using System.Linq;
using ModAPI.Logging;
using ModAPI.Plugins;
using ModAPI.SemanticVersioning;
using ModAPI.Types;
using ModAPI.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModAPI.API
{
    public static class APIHost
    {
        public static Logging.Logger Logger { get; private set; }
        public static PluginManager Plugins { get; private set; }
        public static GameEvents Events { get; private set; }

        private static APIHostComponent apiComponent;
        private static GameUIComponent uiComponent;
        
        internal static void Initialize()
        {
            Logger = new Logging.Logger(new LogOptions
            {
                LogDirectory = "Logs",
                LogToConsole = true,
                MinLogLevel = Environment.GetCommandLineArgs().Any(line => line.ToLowerInvariant() == "--debug") ? LogLevel.Debug : LogLevel.Info
            });

            Logger.LogDebug($"Unity Version: {Application.unityVersion}");
            Logger.LogInfo($"Game Version: {Application.version}");
            
            Events = new GameEvents();

            Application.logMessageReceived += OnLogMessageReceived;
            SceneManager.activeSceneChanged += OnNewScene;

            Application.runInBackground = true; // Without this, plugin hotloading could be unpredictable in certain cases.

            var apiObject = new GameObject("ModAPI");
            apiComponent = apiObject.AddComponent<APIHostComponent>();
            GameObject.DontDestroyOnLoad(apiObject);
            
            UIHost.Initialize();

            Plugins = new PluginManager(new PluginOptions
            {
                PluginsDirectory = "Plugins"
            });
            Plugins.LoadPlugins();
            Plugins.StartListening();
        }

        /// <summary>
        /// Called from a unity component's Start method.
        /// </summary>
        internal static void InitializePlugins()
        {
            var gameUIObject = new GameObject("UI");
            gameUIObject.transform.SetParent(apiComponent.transform);
            uiComponent = gameUIObject.AddComponent<GameUIComponent>();
        }

        internal static void OnApplicationQuit()
        {
            APIHost.Logger.LogDebug("Unloading plugins and shutting down CEF...");
            
            Plugins.UnloadAllPlugins();
            UIHost.Destroy();

            APIHost.Logger.LogDebug("Done");
        }
        
        private static void OnLogMessageReceived(string condition, string stacktrace, LogType type)
        {
            string logString = condition + (stacktrace != null ? "\n" + stacktrace : "");
            logString = logString.Trim();
            
            switch (type)
            {
                case LogType.Log:
                    Logger.LogInfo(logString);
                    break;
                case LogType.Warning:
                    Logger.LogWarning(logString);
                    break;
                case LogType.Error:
                case LogType.Assert:
                case LogType.Exception:
                    Logger.LogError(logString);
                    break;
            }
        }
        
        private static void OnNewScene(Scene oldScene, Scene newScene)
        {
            Plugins.OnNewScene(oldScene != default(Scene) ? GetSceneType(oldScene) : (SceneType?) null, GetSceneType(newScene));
        }

        private static SceneType GetSceneType(Scene newScene)
        {
            switch (newScene.name)
            {
                case "Loader":
                    return SceneType.Menu;
                case "Mian":
                    return SceneType.Game;
                case "Credits":
                    return SceneType.Credits;
                default:
                    Logger.LogError($"Unknown scene loaded: \"{newScene.name}\"");
                    return SceneType.Invalid;
            }
        }

        public static void Update()
        {
            Plugins.Tick();
            UIHost.Update();
        }
    }
}