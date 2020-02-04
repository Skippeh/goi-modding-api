using ModAPI.API;
using ModAPI.API.Events;
using ModAPI.Types;
using ModAPI.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModAPI.UI
{
    internal class GameUIComponent : MonoBehaviour
    {
        public FullscreenBrowserInstance Browser { get; private set; }
        
        private GameObject originalUiObject;
        private MainMenuLogic mainMenuLogic;
        private InGameLogic inGameLogic;

        private void Awake()
        {
            Browser = UIHost.CreateFullscreenBrowser("modapi://gameui");
            APIHost.Events.SceneChanged += OnSceneChanged;
            OnNewScene(SceneType.Menu);
        }

        private void OnEnable()
        {
            if (originalUiObject)
                originalUiObject.SetActive(false);
            
            Browser.Enabled = true;
        }

        private void OnDisable()
        {
            Browser.Enabled = false;

            if (originalUiObject)
                originalUiObject.SetActive(true);
        }

        private void OnSceneChanged(SceneChangedEventArgs args)
        {
            OnNewScene(args.SceneType);
        }

        private void OnNewScene(SceneType sceneType)
        {
            Destroy(mainMenuLogic);
            Destroy(inGameLogic);
            originalUiObject = null;
            inGameLogic = null;
            mainMenuLogic = null;

            // Disable original ui and update fields
            if (sceneType == SceneType.Menu)
            {
                inGameLogic = null;

                originalUiObject = GameObject.Find("Canvas");
                originalUiObject.SetActive(false);
                mainMenuLogic = gameObject.AddComponent<MainMenuLogic>();
                mainMenuLogic.GameUI = this;
            }
            else if (sceneType == SceneType.Game)
            {
                mainMenuLogic = null;

                originalUiObject = GameObject.Find("Canvas");
                originalUiObject.SetActive(false);
                inGameLogic = gameObject.AddComponent<InGameLogic>();
                inGameLogic.GameUI = this;
            }
        }
    }
}