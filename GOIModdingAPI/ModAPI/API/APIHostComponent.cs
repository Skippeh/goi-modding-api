using System;
using UnityEngine;

namespace ModAPI.API
{
    internal class APIHostComponent : MonoBehaviour
    {
        private void Start()
        {
            APIHost.InitializePlugins();
        }

        private void Update()
        {
            APIHost.Update();
        }

        private void OnApplicationQuit()
        {
            APIHost.OnApplicationQuit();
        }
    }
}