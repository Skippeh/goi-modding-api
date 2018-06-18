using ModAPI.Types;

namespace ModAPI.Plugins.Events
{
    public delegate void SceneChangedEventHandler(SceneChangedEventArgs args);
    
    public class SceneChangedEventArgs : System.EventArgs
    {
        public SceneType OldSceneType { get; }
        public SceneType SceneType { get; }
        
        public SceneChangedEventArgs(SceneType oldSceneType, SceneType sceneType)
        {
            OldSceneType = oldSceneType;
            SceneType = sceneType;
        }
    }
}