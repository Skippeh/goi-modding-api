using System;
using UnityEngine.SceneManagement;

namespace ModAPI.UI
{
    internal class MainMenuLogic : UILogic
    {
        private void Start()
        {
            SceneManager.LoadScene("Mian", LoadSceneMode.Single);
            return;
        }
    }
}