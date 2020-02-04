using ModAPI.API;
using UnityEngine;

namespace ModAPI.UI
{
    internal class InGameLogic : UILogic
    {
        private bool paused;
        private GameObject player;
        private GameObject narrator;
        private float lastTimeScale = 1;

        private void Start()
        {
            player = GameObject.Find("Player");
            narrator = FindObjectOfType<Narrator>().gameObject;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                paused = !paused;

                if (paused)
                {
                    lastTimeScale = Time.timeScale;
                    Time.timeScale = 0;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    player.SendMessage("Pause");
                    narrator.SendMessage("Pause");
                }
                else
                {
                    Time.timeScale = lastTimeScale;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    player.SendMessage("UnPause");
                    narrator.SendMessage("UnPause");
                }
            }
        }
    }
}