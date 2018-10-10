using UnityEngine;
using Logger = Modding.Logger;

namespace UltimatumRadiance
{
    internal class AbsFinder : MonoBehaviour
    {
        private GameObject _abs;
        private GameObject _beamsweeper;
        private GameObject _beamsweeper2;
        private bool _cloned;

        private void Start()
        {
            Logger.Log("[Ultimatum Radiance] Added AbsFinder MonoBehaviour");
        }

        private void Update()
        {
            if (_abs != null) return;
            _abs = GameObject.Find("Absolute Radiance");
            _beamsweeper = GameObject.Find("Beam Sweeper");
            if (!_cloned)
            {
                _beamsweeper2 = Instantiate(_beamsweeper);
                _cloned = true;
                Logger.Log("[Ultimatum Radiance] Instantiating beamsweeper clone");
            }
            if (_abs == null) return;
            Logger.Log("[Ultimatum Radiance] Found the Radiance!");
            _abs.AddComponent<Abs>();
            _beamsweeper2.AddComponent<BeamSweeperClone>();
        }
    }
}