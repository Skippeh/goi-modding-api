using System;
using UnityEngine;

namespace ModAPI.API
{
    internal class APIHostComponent : MonoBehaviour
    {
        private void Update()
        {
            APIHost.Plugins.Tick();
        }

        private void OnApplicationQuit()
        {
            APIHost.OnApplicationQuit();
        }
    }
}