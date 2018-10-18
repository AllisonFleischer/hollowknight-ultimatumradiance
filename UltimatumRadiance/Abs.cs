using System.Collections;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon.Util;
using JetBrains.Annotations;
using UnityEngine;
using Logger = Modding.Logger;

namespace UltimatumRadiance
{
    internal class Abs : MonoBehaviour
    {
        private GameObject _spikeMaster;
        private GameObject _spikeTemplate;

        private GameObject _spikeClone;
        private GameObject _spikeClone2;
        private GameObject _spikeClone3;
        private GameObject _spikeClone4;
        private GameObject _spikeClone5;

        private GameObject _beamsweeper;
        private GameObject _beamsweeper2;
        private GameObject _knight;

        private HealthManager _hm;

        private PlayMakerFSM _attackChoices;
        private PlayMakerFSM _attackCommands;
        private PlayMakerFSM _control;
        private PlayMakerFSM _phaseControl;
        private PlayMakerFSM _spikeMasterControl;
        private PlayMakerFSM _beamsweepercontrol;
        private PlayMakerFSM _beamsweeper2control;
        private PlayMakerFSM _spellControl;

        private int CWRepeats = 0;
        private bool fullSpikesSet = false;
        private bool arena2Set = false;
        private bool onePlatSet = false;
        private bool platSpikesSet = false;

        private const int fullSpikesHealth = 250;
        private const int onePlatHealth = 100;
        private const int platSpikesHealth = 150;

        /*        radiant spike x: 48.490002
                                y: 21.180000
                                z: -0.001010

        radiant plat small (10) x: 67.779999
                                y: 151.820007
                                z: 0.000000

                      left plat x: 58.040001*/

        private void Awake()
        {
            Log("Added AbsRad MonoBehaviour");

            _hm = gameObject.GetComponent<HealthManager>();

            _attackChoices = gameObject.LocateMyFSM("Attack Choices");
            _attackCommands = gameObject.LocateMyFSM("Attack Commands");
            _control = gameObject.LocateMyFSM("Control");
            _phaseControl = gameObject.LocateMyFSM("Phase Control");

            _spikeMaster = GameObject.Find("Spike Control");
            _spikeMasterControl = _spikeMaster.LocateMyFSM("Control");
            _spikeTemplate = GameObject.Find("Radiant Spike");

            _beamsweeper = GameObject.Find("Beam Sweeper");
            _beamsweeper2 = Instantiate(_beamsweeper);
            _beamsweeper2.AddComponent<BeamSweeperClone>();
            _beamsweepercontrol = _beamsweeper.LocateMyFSM("Control");
            _beamsweeper2control = _beamsweeper2.LocateMyFSM("Control");

            _knight = GameObject.Find("Knight");
            _spellControl = _knight.LocateMyFSM("Spell Control");
        }

        private void Start()
        {
            Log("Changing fight variables...");

            //HEALTH
            _hm.hp += fullSpikesHealth + onePlatHealth + platSpikesHealth; //We're adding new phases, so create more health to accomodate them
            _phaseControl.FsmVariables.GetFsmInt("P2 Spike Waves").Value += fullSpikesHealth + onePlatHealth + platSpikesHealth; //Increase phase health threshholds
            _phaseControl.FsmVariables.GetFsmInt("P3 A1 Rage").Value += onePlatHealth + platSpikesHealth;
            _phaseControl.FsmVariables.GetFsmInt("P4 Stun1").Value += onePlatHealth + platSpikesHealth;
            _phaseControl.FsmVariables.GetFsmInt("P5 Acend").Value += onePlatHealth + platSpikesHealth;
            _control.GetAction<SetHP>("Scream", 7).hp = 1000 + onePlatHealth + platSpikesHealth; //Increase health for final phase

            //PLATFORM SPIKES
            //Create spikes on top platform
            _spikeClone = Instantiate(_spikeTemplate);
            _spikeClone.transform.SetPositionX(58f);
            _spikeClone.transform.SetPositionY(153.8f);

            _spikeClone2 = Instantiate(_spikeTemplate);
            _spikeClone2.transform.SetPositionX(57.5f);
            _spikeClone2.transform.SetPositionY(153.8f);

            _spikeClone3 = Instantiate(_spikeTemplate);
            _spikeClone3.transform.SetPositionX(57f);
            _spikeClone3.transform.SetPositionY(153.8f);

            _spikeClone4 = Instantiate(_spikeTemplate);
            _spikeClone4.transform.SetPositionX(58.5f);
            _spikeClone4.transform.SetPositionY(153.8f);

            _spikeClone5 = Instantiate(_spikeTemplate);
            _spikeClone5.transform.SetPositionX(59f);
            _spikeClone5.transform.SetPositionY(153.8f);

            _spikeClone.LocateMyFSM("Control").SendEvent("DOWN");
            _spikeClone2.LocateMyFSM("Control").SendEvent("DOWN");
            _spikeClone3.LocateMyFSM("Control").SendEvent("DOWN");
            _spikeClone4.LocateMyFSM("Control").SendEvent("DOWN");
            _spikeClone5.LocateMyFSM("Control").SendEvent("DOWN");

            //ORB BARRAGE
            _attackCommands.GetAction<Wait>("Orb Antic", 0).time = .75f; //INCREASE wait time at start of orb barrage, to increase chance player isn't in a nail wall or something
            _attackCommands.GetAction<SetIntValue>("Orb Antic", 1).intValue = 7; //Spawn more orbs
            _attackCommands.GetAction<RandomInt>("Orb Antic", 2).min = 6;
            _attackCommands.GetAction<RandomInt>("Orb Antic", 2).max = 8;
            _attackCommands.GetAction<Wait>("Orb Summon", 2).time = 0.40f; //Decrease telegraph time to spawn orb
            _attackCommands.GetAction<Wait>("Orb Pause", 0).time = 0.01f; //Remove time to start spawning new orb
            _attackChoices.GetAction<Wait>("Orb Recover", 0).time = 1.25f; //Increase downtime at the end of the barrage, just like at the start

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
            _attackChoices.GetAction<SendEventByName>("Nail Top Sweep", 1).delay = 0.35f;
            _attackChoices.GetAction<SendEventByName>("Nail Top Sweep", 2).delay = 0.7f;
            _attackChoices.GetAction<SendEventByName>("Nail Top Sweep", 3).delay = 1.05f;
            _attackChoices.GetAction<Wait>("Nail Top Sweep", 4).time = 2.3f;
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
                _spikeMasterControl.GetAction<SendEventByName>("Spikes Left", 0).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Spikes Left", 1).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Spikes Left", 2).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Spikes Left", 3).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Spikes Left", 4).sendEvent = "UP";

                _spikeMasterControl.GetAction<SendEventByName>("Spikes Right", 0).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Spikes Right", 1).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Spikes Right", 2).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Spikes Right", 3).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Spikes Right", 4).sendEvent = "UP";

                _spikeMasterControl.GetAction<SendEventByName>("Wave L", 2).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Wave L", 3).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Wave L", 4).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Wave L", 5).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Wave L", 6).sendEvent = "UP";
                _spikeMasterControl.GetAction<WaitRandom>("Wave L", 7).timeMin = 0.1f;
                _spikeMasterControl.GetAction<WaitRandom>("Wave L", 7).timeMax = 0.1f;

                _spikeMasterControl.GetAction<SendEventByName>("Wave R", 2).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Wave R", 3).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Wave R", 4).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Wave R", 5).sendEvent = "UP";
                _spikeMasterControl.GetAction<SendEventByName>("Wave R", 6).sendEvent = "UP";
                _spikeMasterControl.GetAction<WaitRandom>("Wave R", 7).timeMin = 0.1f;
                _spikeMasterControl.GetAction<WaitRandom>("Wave R", 7).timeMax = 0.1f;

                _spikeMasterControl.SetState("Spike Waves");

                //Prevent ddark cheese; if you try to dive onto spikes you take damage
                AddDivePunishment();

                //More generous orbs
                _attackCommands.GetAction<Wait>("Orb Summon", 2).time = 1.5f;
                _attackCommands.GetAction<SetIntValue>("Orb Antic", 1).intValue = 2;
                _attackCommands.GetAction<RandomInt>("Orb Antic", 2).min = 1;
                _attackCommands.GetAction<RandomInt>("Orb Antic", 2).max = 3;

                //Slower radial bursts
                _attackCommands.GetAction<AudioPlayerOneShotSingle>("EB 1", 2).delay = .75f;
                _attackCommands.GetAction<SendEventByName>("EB 1", 3).delay = .75f;
                _attackCommands.GetAction<SendEventByName>("EB 1", 8).delay = .75f;
                _attackCommands.GetAction<SendEventByName>("EB 1", 9).delay = .85f;
                _attackCommands.GetAction<Wait>("EB 1", 10).time = 1.92f;

                _attackCommands.GetAction<AudioPlayerOneShotSingle>("EB 2", 3).delay = .75f;
                _attackCommands.GetAction<SendEventByName>("EB 2", 4).delay = .75f;
                _attackCommands.GetAction<SendEventByName>("EB 2", 8).delay = .75f;
                _attackCommands.GetAction<SendEventByName>("EB 2", 9).delay = .85f;
                _attackCommands.GetAction<Wait>("EB 2", 10).time = 1.2f;

                _attackCommands.GetAction<AudioPlayerOneShotSingle>("EB 3", 3).delay = .75f;
                _attackCommands.GetAction<SendEventByName>("EB 3", 4).delay = .75f;
                _attackCommands.GetAction<SendEventByName>("EB 3", 8).delay = .75f;
                _attackCommands.GetAction<SendEventByName>("EB 3", 9).delay = .85f;
                _attackCommands.GetAction<Wait>("EB 3", 10).time = 1.2f;

                //Nail sweeps are disabled, too bullshit with spikes everywhere
                _attackChoices.ChangeTransition("A1 Choice", "NAIL L SWEEP", "Beam Sweep L");
                _attackChoices.ChangeTransition("A1 Choice", "NAIL R SWEEP", "Beam Sweep L");
                _attackChoices.ChangeTransition("A1 Choice", "NAIL FAN", "Eye Beam Wait");
                _attackChoices.ChangeTransition("A1 Choice", "NAIL TOP SWEEP", "Orb Wait");
            }

            if ((_attackChoices.FsmVariables.GetFsmInt("Arena").Value == 2) && !arena2Set) //Platform phase!
            {
                Logger.Log("[Ultimatum Radiance] Starting Phase 2");
                arena2Set = true;

                _spellControl.RemoveAction("Q2 Land", 0); //Revert ddark to normal behavior

                _attackCommands.GetAction<SetIntValue>("Orb Antic", 1).intValue = 6; //Reset orbs
                _attackCommands.GetAction<RandomInt>("Orb Antic", 2).min = 5;
                _attackCommands.GetAction<RandomInt>("Orb Antic", 2).max = 7;
                _attackCommands.GetAction<Wait>("Orb Summon", 2).time = 0.60f;

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

            if (_hm.hp < _phaseControl.FsmVariables.GetFsmInt("P5 Acend").Value - onePlatHealth)
            {
                GameObject.Find("Radiant Plat Small (10)").LocateMyFSM("radiant_plat").ChangeState(GetFsmEventByName(GameObject.Find("Radiant Plat Small (10)").LocateMyFSM("radiant_plat"), "SLOW VANISH"));
                if (!onePlatSet)
                {
                    onePlatSet = true;
                    Log("Removing upper right platform");
                    _attackCommands.GetAction<Wait>("Orb Summon", 2).time = 0.80f;
                }
            }
            if (_hm.hp < _phaseControl.FsmVariables.GetFsmInt("P5 Acend").Value - onePlatHealth - platSpikesHealth)
            {
                _spikeClone.LocateMyFSM("Control").SendEvent("UP");
                _spikeClone2.LocateMyFSM("Control").SendEvent("UP");
                _spikeClone3.LocateMyFSM("Control").SendEvent("UP");
                _spikeClone4.LocateMyFSM("Control").SendEvent("UP");
                _spikeClone5.LocateMyFSM("Control").SendEvent("UP");
                if (!platSpikesSet)
                {
                    platSpikesSet = true;
                    GameObject.Find("Radiant Plat Small (10)").LocateMyFSM("radiant_plat").ChangeState(GetFsmEventByName(GameObject.Find("Radiant Plat Small (10)").LocateMyFSM("radiant_plat"), "SLOW VANISH"));
                    AddDivePunishment();
                }
            }
        }

        private IEnumerator AddDivePunishment()
        {
            yield return new WaitForSeconds(2); //Wait for spikes to go up
            _spellControl.InsertAction("Q2 Land", new CallMethod
            {
                behaviour = this,
                methodName = "DivePunishment", //Add a method to take damage to the player's dive FSM
                parameters = new FsmVar[0],
                everyFrame = false
            }, 0);
            yield break;
        }

        [UsedImplicitly]
        public void DivePunishment()
        {
            Log("YOU WON'T CHEESE SPIKES IN THIS TOWN AGAIN");
            HeroController.instance.TakeDamage(gameObject, GlobalEnums.CollisionSide.bottom, 1, 0); //Knight takes a hit
            EventRegister.SendEvent("HERO DAMAGED"); //Tells the UI to refresh
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