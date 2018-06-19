using System;
using ModAPI.Logging;
using ModAPI.Plugins;
using ModAPI.Types;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModAPI.API
{
    public static class APIHost
    {
        public static Logging.Logger Logger { get; private set; }
        public static PluginManager Plugins { get; private set; }

        private static APIHostComponent apiComponent;
        
        internal static void Initialize()
        {
            Logger = new Logging.Logger(new LogOptions
            {
                LogDirectory = "Logs",
                LogToConsole = true
            });
            
            Application.logMessageReceived += OnLogMessageReceived;
            SceneManager.activeSceneChanged += OnNewScene;

            Application.runInBackground = true; // Without this plugin hotloading could be unpredictable in certain cases.

            var apiObject = new GameObject("ModAPI");
            apiComponent = apiObject.AddComponent<APIHostComponent>();
            GameObject.DontDestroyOnLoad(apiObject);
            
            Plugins = new PluginManager();
            Plugins.LoadPlugins();
            Plugins.StartListening();
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
            //Interface.CallHook("OnSceneChanged", GetSceneType(newScene), newScene);
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
    }
}