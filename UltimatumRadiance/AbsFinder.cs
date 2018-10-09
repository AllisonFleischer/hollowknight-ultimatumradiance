using UnityEngine;
using Logger = Modding.Logger;

namespace UltimatumRadiance
{
    internal class AbsFinder : MonoBehaviour
    {
        private GameObject _abs;

        private void Start()
        {
            Logger.Log("[Ultimatum Radiance] Added AbsFinder MonoBehaviour");
        }

        private void Update()
        {
            if (_abs != null) return;
            _abs = GameObject.Find("Absolute Radiance");
            if (_abs == null) return;
            Logger.Log("[Ultimatum Radiance] Found the Radiance!");
            _abs.AddComponent<Abs>();
        }
    }
}