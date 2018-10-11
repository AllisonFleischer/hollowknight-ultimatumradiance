using UnityEngine;
using HutongGames.PlayMaker;
using Logger = Modding.Logger;

namespace UltimatumRadiance
{
    internal class AbsFinder : MonoBehaviour
    {
        private GameObject _abs;
        private GameObject _beamsweeper;
        private GameObject _beamsweeper2;

        private PlayMakerFSM _beamsweepercontrol;
        private PlayMakerFSM _beamsweeper2control;

        private bool _cloned;
        private bool _assigned;
        private bool _returned;

        private void Start()
        {
            Logger.Log("[Ultimatum Radiance] Added AbsFinder MonoBehaviour");
        }

        private void Update()
        {
            if (_abs == null) {
                _cloned = false;
                _assigned = false;
                _returned = false;
                _abs = GameObject.Find("Absolute Radiance");
            }

            else if (!_assigned)
            {
                _beamsweeper = GameObject.Find("Beam Sweeper");
                if (!_returned)
                {
                    _returned = true;
                    return;
                }

                if (!_cloned)
                {
                    _beamsweeper2 = Instantiate(_beamsweeper);
                    _cloned = true;
                    Logger.Log("[Ultimatum Radiance] Instantiating beamsweeper clone");
                }

                _assigned = true;
                Logger.Log("[Ultimatum Radiance] Found the Radiance!");
                _abs.AddComponent<Abs>();
                _beamsweeper2.AddComponent<BeamSweeperClone>();
                _beamsweepercontrol = _beamsweeper.LocateMyFSM("Control");
                _beamsweeper2control = _beamsweeper2.LocateMyFSM("Control");
            }

            else if (_beamsweeper2control != null)
            {
                if (_beamsweepercontrol.ActiveStateName == _beamsweeper2control.ActiveStateName)
                {
                    switch (_beamsweepercontrol.ActiveStateName)
                    {
                        case "Beam Sweep L":
                            _beamsweeper2control.ChangeState(GetFsmEventByName(_beamsweeper2control, "BEAM SWEEP R"));
                            break;
                        case "Beam Sweep R":
                            _beamsweeper2control.ChangeState(GetFsmEventByName(_beamsweeper2control, "BEAM SWEEP L"));
                            break;
                    }

                }
            }
        }

        private static FsmEvent GetFsmEventByName(PlayMakerFSM fsm, string eventName)
        {
            foreach (FsmEvent t in fsm.FsmEvents)
            {
                if (t.Name == eventName) return t;
            }
            return null;
        }
    }
}