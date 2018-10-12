using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon.Util;
using JetBrains.Annotations;
using Modding;
using UnityEngine;
using Logger = Modding.Logger;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace UltimatumRadiance
{
    internal class Abs : MonoBehaviour
    {
        private GameObject _spikes;
        private GameObject _beamsweeper;
        private GameObject _beamsweeper2;

        private HealthManager _hm;

        private PlayMakerFSM _attackChoices;
        private PlayMakerFSM _attackCommands;
        private PlayMakerFSM _control;
        private PlayMakerFSM _phaseControl;
        private PlayMakerFSM _spikeControl;
        private PlayMakerFSM _beamsweepercontrol;
        private PlayMakerFSM _beamsweeper2control;

        private int CWRepeats = 0;
        private readonly int fullSpikesHealth = 350;
        private bool fullSpikesSet = false;
        private bool arena2Set = false;

        private void Awake()
        {
            Log("Added AbsRad MonoBehaviour");

            _hm = gameObject.GetComponent<HealthManager>();

            _attackChoices = gameObject.LocateMyFSM("Attack Choices");
            _attackCommands = gameObject.LocateMyFSM("Attack Commands");
            _control = gameObject.LocateMyFSM("Control");
            _phaseControl = gameObject.LocateMyFSM("Phase Control");

            _spikes = GameObject.Find("Spike Control");
            _spikeControl = _spikes.LocateMyFSM("Control");

            _beamsweeper = GameObject.Find("Beam Sweeper");
            _beamsweeper2 = Instantiate(_beamsweeper);
            _beamsweeper2.AddComponent<BeamSweeperClone>();
            _beamsweepercontrol = _beamsweeper.LocateMyFSM("Control");
            _beamsweeper2control = _beamsweeper2.LocateMyFSM("Control");
        }

        private void Start()
        {
            //HEALTH
            _hm.hp += fullSpikesHealth; //We're adding a new phase, so create more health to accomodate it
            _phaseControl.FsmVariables.GetFsmInt("P2 Spike Waves").Value += fullSpikesHealth; //P2 spikes is before the new phase, so increase health threshhold for that too

            //ORB BARRAGE
            _attackCommands.GetAction<Wait>("Orb Antic", 0).time = .75f; //INCREASE wait time at start of orb barrage, to increase chance player isn't in a nail wall or something
            _attackCommands.GetAction<SetIntValue>("Orb Antic", 1).intValue = 7; //Spawn more orbs
            _attackCommands.GetAction<RandomInt>("Orb Antic", 2).min = 6;
            _attackCommands.GetAction<RandomInt>("Orb Antic", 2).max = 8;
            _attackCommands.GetAction<Wait>("Orb Summon", 2).time = 0.40f; //Decrease telegraph time to spawn orb
            _attackCommands.GetAction<Wait>("Orb Pause", 0).time = 0.01f; //Remove time to start spawning new orb
            _attackChoices.GetAction<Wait>("Orb Recover", 0).time = 0.75f; //Increase downtime at the end of the barrage, just like at the start

            //RADIAL NAIL BARRAGE
            //Note that there's stuff in Update() below for this attack too
            _attackCommands.GetAction<Wait>("CW Repeat", 0).time = 0f; //The little "spin" animation is faster
            _attackCommands.GetAction<Wait>("CCW Repeat", 0).time = 0f;
            _attackCommands.GetAction<FloatAdd>("CW Restart", 2).add = -10; //Change angle by thirds instead of halves
            _attackCommands.GetAction<FloatAdd>("CCW Restart", 2).add = 10;
            _attackCommands.RemoveAction("CW Restart", 1); //Go straight into the next wave of nails, no delay
            _attackCommands.RemoveAction("CCW Restart", 1);

            //GIANT BEAM SWEEP
            //Note: Logic for spawning the second beam is found in the AbsFinder class.
            //The attached MonoBehaviour for reversing its direction is in BeamSweeperClone.
            _attackChoices.GetAction<Wait>("Beam Sweep L", 0).time = 4.05f; //Wait longer because this attack is a lot more demanding now
            _attackChoices.GetAction<Wait>("Beam Sweep R", 0).time = 4.05f;
            _attackChoices.ChangeTransition("A1 Choice", "BEAM SWEEP R", "Beam Sweep L");
            _attackChoices.ChangeTransition("A2 Choice", "BEAM SWEEP R", "Beam Sweep L 2");
            _attackChoices.GetAction<Wait>("Beam Sweep L 2", 0).time = 5.05f;
            _attackChoices.GetAction<Wait>("Beam Sweep R 2", 0).time = 5.05f;
            _attackChoices.GetAction<SendEventByName>("Beam Sweep L 2", 1).sendEvent = "BEAM SWEEP L";
            _attackChoices.GetAction<SendEventByName>("Beam Sweep R 2", 1).sendEvent = "BEAM SWEEP R";


            //RADIAL BEAM BARRAGE
            _attackCommands.GetAction<SendEventByName>("EB 1", 9).delay = 0.525f; //Reduce the time the beam is active for
            _attackCommands.GetAction<Wait>("EB 1", 10).time = 0.55f; //And drastically reduce the wait afterward
            _attackCommands.GetAction<SendEventByName>("EB 2", 9).delay = 0.5f;
            _attackCommands.GetAction<Wait>("EB 2", 10).time = 0.525f;
            _attackCommands.GetAction<SendEventByName>("EB 3", 9).delay = 0.5f;
            _attackCommands.GetAction<Wait>("EB 3", 10).time = 0.525f;
            _attackCommands.GetAction<SendEventByName>("EB 4", 4).delay = 0.6f;
            _attackCommands.GetAction<Wait>("EB 4", 5).time = 0.6f;
            _attackCommands.GetAction<SendEventByName>("EB 5", 5).delay = 0.6f;
            _attackCommands.GetAction<Wait>("EB 5", 6).time = 0.6f;
            _attackCommands.GetAction<SendEventByName>("EB 6", 5).delay = 0.6f;
            _attackCommands.GetAction<Wait>("EB 6", 6).time = 0.6f;
            _attackCommands.GetAction<SendEventByName>("EB 7", 8).delay = 0.6f;
            _attackCommands.GetAction<Wait>("EB 7", 9).time = 0.625f;
            _attackCommands.GetAction<SendEventByName>("EB 8", 8).delay = 0.6f;
            _attackCommands.GetAction<Wait>("EB 8", 9).time = 0.625f;
            _attackCommands.GetAction<SendEventByName>("EB 9", 8).delay = 0.6f;
            _attackCommands.GetAction<Wait>("EB 9", 9).time = 0.625f;
            _attackCommands.GetAction<SendEventByName>("Aim", 10).delay = 0.6f;
            _attackCommands.GetAction<Wait>("Aim", 11).time = 0.625f;
            _attackCommands.GetAction<Wait>("Eb Extra Wait", 0).time = 0.05f;

            //VERTICAL NAIL COMB
            _attackChoices.GetAction<SendEventByName>("Nail Top Sweep", 1).delay = 0.25f;
            _attackChoices.GetAction<SendEventByName>("Nail Top Sweep", 2).delay = 0.5f;
            _attackChoices.GetAction<SendEventByName>("Nail Top Sweep", 3).delay = 0.75f;
            _attackChoices.GetAction<Wait>("Nail Top Sweep", 4).time = 2f;
            _control.GetAction<Wait>("Rage Comb", 0).time = 0.6f;

            Log("fin.");

        }

        private void Update()
        {
            //Silly hack to get three waves of radial nails instead of two
            //This feels really inefficient and I basically just threw numbers at the wall until it worked but uh, whatever
            if (_attackCommands.FsmVariables.GetFsmBool("Repeated").Value) {
                switch (CWRepeats)
                {
                    case 0:
                        CWRepeats = 1;
                        _attackCommands.FsmVariables.GetFsmBool("Repeated").Value = false;
                        break;
                    case 1:
                        CWRepeats = 2;
                        break;
                }
            }
            else if (CWRepeats == 2) CWRepeats = 0;

            //Force beam sweepers to always go in opposing directions. There were some special cases where they wouldn't that I was too lazy to investigate
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

            if (_hm.hp < _phaseControl.FsmVariables.GetFsmInt("P2 Spike Waves").Value - fullSpikesHealth && !fullSpikesSet) //NEW PHASE
            {
                fullSpikesSet = true;

                //Spikes cover the whole arena!
                _spikeControl.GetAction<SendEventByName>("Spikes Left", 0).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Spikes Left", 1).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Spikes Left", 2).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Spikes Left", 3).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Spikes Left", 4).sendEvent = "UP";

                _spikeControl.GetAction<SendEventByName>("Spikes Right", 0).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Spikes Right", 1).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Spikes Right", 2).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Spikes Right", 3).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Spikes Right", 4).sendEvent = "UP";

                _spikeControl.GetAction<SendEventByName>("Wave L", 2).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Wave L", 3).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Wave L", 4).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Wave L", 5).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Wave L", 6).sendEvent = "UP";
                _spikeControl.GetAction<WaitRandom>("Wave L", 7).timeMin = 0.1f;
                _spikeControl.GetAction<WaitRandom>("Wave L", 7).timeMax = 0.1f;

                _spikeControl.GetAction<SendEventByName>("Wave R", 2).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Wave R", 3).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Wave R", 4).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Wave R", 5).sendEvent = "UP";
                _spikeControl.GetAction<SendEventByName>("Wave R", 6).sendEvent = "UP";
                _spikeControl.GetAction<WaitRandom>("Wave R", 7).timeMin = 0.1f;
                _spikeControl.GetAction<WaitRandom>("Wave R", 7).timeMax = 0.1f;

                _attackCommands.GetAction<Wait>("Orb Summon", 2).time = 1.5f; //More generous orbs
                _attackCommands.GetAction<SetIntValue>("Orb Antic", 1).intValue = 2;
                _attackCommands.GetAction<RandomInt>("Orb Antic", 2).min = 1;
                _attackCommands.GetAction<RandomInt>("Orb Antic", 2).max = 3;

                _attackCommands.GetAction<Wait>("EB 1", 10).time = 0.925f; //Slower radial bursts
                _attackCommands.GetAction<Wait>("EB 2", 10).time = 0.9f;
                _attackCommands.GetAction<Wait>("EB 3", 10).time = 0.9f;

                _attackChoices.ChangeTransition("A1 Choice", "NAIL L SWEEP", "Beam Sweep L"); //Nail sweeps are disabled, too bullshit with spikes everywhere
                _attackChoices.ChangeTransition("A1 Choice", "NAIL R SWEEP", "Beam Sweep L");
                _attackChoices.ChangeTransition("A1 Choice", "NAIL FAN", "Eye Beam Wait");
                _attackChoices.ChangeTransition("A1 Choice", "NAIL TOP SWEEP", "Orb Wait");
            }

            if ((_attackChoices.FsmVariables.GetFsmInt("Arena").Value == 2) && !arena2Set) //Platform phase!
            {
                Logger.Log("[Ultimatum Radiance] Starting Phase 2");
                arena2Set = true;

                _attackCommands.GetAction<SetIntValue>("Orb Antic", 1).intValue = 7; //Reset orbs
                _attackCommands.GetAction<RandomInt>("Orb Antic", 2).min = 6;
                _attackCommands.GetAction<RandomInt>("Orb Antic", 2).max = 8;
                _attackCommands.GetAction<Wait>("Orb Summon", 2).time = 0.40f;

                //Beam sweepers cover a larger area
                /*Normally the FSM handles this, but I'm modifying the numbers through code instead
                 *because the L2/R2 states don't have the events I need to get opposing beams working */
                _beamsweepercontrol.GetAction<SetPosition>("Beam Sweep L", 3).x = 89;
                _beamsweepercontrol.GetAction<iTweenMoveBy>("Beam Sweep L", 5).vector = new Vector3(-50, 0, 0);
                _beamsweepercontrol.GetAction<iTweenMoveBy>("Beam Sweep L", 5).time = 5;
                _beamsweepercontrol.GetAction<SetPosition>("Beam Sweep R", 4).x = 32.6f;
                _beamsweepercontrol.GetAction<iTweenMoveBy>("Beam Sweep R", 6).vector = new Vector3(50, 0, 0);
                _beamsweepercontrol.GetAction<iTweenMoveBy>("Beam Sweep R", 6).time = 5;
                _beamsweeper2control.GetAction<SetPosition>("Beam Sweep L", 2).x = 89;
                _beamsweeper2control.GetAction<iTweenMoveBy>("Beam Sweep L", 4).vector = new Vector3(-50, 0, 0);
                _beamsweeper2control.GetAction<iTweenMoveBy>("Beam Sweep L", 4).time = 5;
                _beamsweeper2control.GetAction<SetPosition>("Beam Sweep R", 3).x = 32.6f;
                _beamsweeper2control.GetAction<iTweenMoveBy>("Beam Sweep R", 5).vector = new Vector3(50, 0, 0);
                _beamsweeper2control.GetAction<iTweenMoveBy>("Beam Sweep R", 5).time = 5;
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

        private static void Log(object obj)
        {
            Logger.Log("[Ultimatum Radiance] " + obj);
        }
    }
}