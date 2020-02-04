using System.Collections;
using System.Linq;
using System.Text;
using ModAPI.API;
using ModAPI.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModAPI.Utility
{
    public static class UnityUtility
    {
        public static void PrintSceneObjects(int maxDepth = 1, LogLevel logLevel = LogLevel.Debug)
        {
            APIHost.Logger.Log("-- Start of scene objects --", logLevel);
            
            foreach (var gameObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                PrintObjects(gameObject, 0, maxDepth, logLevel);
            }

            APIHost.Logger.Log("-- End of scene objects --", logLevel);
        }

        public static void PrintObject(GameObject gameObject, int maxDepth, LogLevel logLevel = LogLevel.Debug)
        {
            APIHost.Logger.Log($"-- Start of {gameObject.name} --", logLevel);
            
            foreach (Transform childTransform in gameObject.transform)
            {
                PrintObjects(childTransform.gameObject, 0, maxDepth, logLevel);
            }

            APIHost.Logger.Log($"-- End of {gameObject.name} --", logLevel);
        }

        public static void PrintObjects(GameObject gameObject, int depth, int maxDepth, LogLevel logLevel)
        {
            LogObject(gameObject, depth, logLevel);
            
            if (depth + 1 <= maxDepth)
            {
                foreach (Transform childTransform in gameObject.transform)
                {
                    PrintObjects(childTransform.gameObject, depth + 1, maxDepth, logLevel);
                }
            }
        }

        private static void LogObject(GameObject gameObject, int depth, LogLevel logLevel)
        {
            StringBuilder builder = new StringBuilder();

            if (depth > 0)
            {
                builder.Append(string.Join("", Enumerable.Repeat("  ", depth).ToArray()));
            }

            builder.Append(gameObject.name);
            builder.Append($" ({GetComponentString(gameObject)})");

            APIHost.Logger.Log(builder.ToString(), logLevel);
        }

        private static string GetComponentString(GameObject gameObject)
        {
            Component[] components = gameObject.GetComponents<Component>();
            return string.Join(", ", components.Select(comp => comp.GetType().Name).ToArray());
        }
    }
}