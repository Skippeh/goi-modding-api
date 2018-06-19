using UnityEngine;

namespace ModAPI.API
{
    public class APIHostComponent : MonoBehaviour
    {
        private void Update()
        {
            APIHost.Plugins.Tick();
        }
    }
}