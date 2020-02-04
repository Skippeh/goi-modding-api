using ModAPI.UI.CEF;
using UnityEngine;

namespace ModAPI.UI
{
    internal class BrowserInstanceComponent : MonoBehaviour
    {
        public IBrowserInstance BrowserInstance;
        public OffScreenClient OffScreenClient;
        public Texture2D TextureTarget;

        private bool isFullscreen;
        
        private void Start()
        {
            if (BrowserInstance is FullscreenBrowserInstance)
            {
                isFullscreen = true;
                gameObject.AddComponent<FullscreenBrowserInputManager>().Client = OffScreenClient;
            }

            DontDestroyOnLoad(gameObject);
        }

        private void OnGUI()
        {
            if (isFullscreen && Event.current.type == EventType.Repaint)
            {
                OffScreenClient.LoadToTexture(TextureTarget);
                GL.PushMatrix();
                GL.LoadOrtho();
                Graphics.DrawTexture(new Rect(0, 0, 1, 1), TextureTarget);
                GL.PopMatrix();
            }
        }

        private void OnDestroy()
        {
            BrowserInstance.Dispose();
        }
    }
}