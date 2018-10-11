using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
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
        private PlayMakerFSM _absattackchoices;

        private bool _cloned;
        private bool _assigned;
        private bool _returned;
        private bool _arena2set;

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
                _arena2set = false;
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
                _absattackchoices = _abs.LocateMyFSM("Attack Choices");
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
                if ((_absattackchoices.FsmVariables.GetFsmInt("Arena").Value == 2) && !_arena2set)
                {
                    Logger.Log("[Ultimatum Radiance] Starting Phase 2");
                    _arena2set = true;

                    _beamsweepercontrol.GetAction<SetPosition>("Beam Sweep L", 3).x = 89;
                    _beamsweepercontrol.GetAction<iTweenMoveBy>("Beam Sweep L", 5).vector = new Vector3 (-50, 0, 0);
                    _beamsweepercontrol.GetAction<iTweenMoveBy>("Beam Sweep L", 5).time = 5;
                    _beamsweepercontrol.GetAction<SetPosition>("Beam Sweep R", 4).x = 32.6f;
                    _beamsweepercontrol.GetAction<iTweenMoveBy>("Beam Sweep R", 6).vector = new Vector3(50, 0, 0);
                    _beamsweepercontrol.GetAction<iTweenMoveBy>("Beam Sweep R", 6).time = 5;

                    _beamsweeper2control.GetAction<SetPosition>("Beam Sweep L", 2).x = 89;
                    _beamsweeper2control.GetAction<iTweenMoveBy>("Beam Sweep L", 4).vector = new Vector3(-50, 0, 0);
                    _beamsweeper2control.GetAction<iTweenMoveBy>("Beam Sweep L", 4).time = 10;
                    _beamsweeper2control.GetAction<SetPosition>("Beam Sweep R", 3).x = 32.6f;
                    _beamsweeper2control.GetAction<iTweenMoveBy>("Beam Sweep R", 5).vector = new Vector3(50, 0, 0);
                    _beamsweeper2control.GetAction<iTweenMoveBy>("Beam Sweep R", 5).time = 5;
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