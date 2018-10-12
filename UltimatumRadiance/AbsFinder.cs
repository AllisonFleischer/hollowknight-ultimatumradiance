using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon.Util;
using Logger = Modding.Logger;

namespace UltimatumRadiance
{
    internal class AbsFinder : MonoBehaviour
    {
        private GameObject _abs;
        private bool _assigned;

        private void Start()
        {
            Logger.Log("[Ultimatum Radiance] Added AbsFinder MonoBehaviour");
        }

        private void Update()
        {
            if (_abs == null) {
                _assigned = false;
                _abs = GameObject.Find("Absolute Radiance");
            }

            else if (!_assigned)
            {
                _assigned = true;
                Logger.Log("[Ultimatum Radiance] Found the Radiance!");
                _abs.AddComponent<Abs>();
            }
        }
    }
}