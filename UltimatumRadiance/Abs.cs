﻿using System.Collections;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using UnityEngine;
using Logger = Modding.Logger;

namespace UltimatumRadiance
{
    internal class Abs : MonoBehaviour
    {
        private GameObject _spikeMaster;
        private GameObject _spikeTemplate;

        private GameObject[] _spikes;

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
        private bool disableBeamSet = false;
        private bool arena2Set = false;
        private bool onePlatSet = false;
        private bool platSpikesSet = false;

        private const int fullSpikesHealth = 250;
        private const int onePlatHealth = 100;
        private const int platSpikesHealth = 150;

        private const float nailWallDelay = .8f;

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
            _spikes = new GameObject[5];

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
            for(int i = 0; i < _spikes.Length; i++)
            {
                Vector2 pos = new Vector2(57f + i/2f,153.8f);

                _spikes[i] = Instantiate(_spikeTemplate);
                _spikes[i].transform.SetPosition2D(pos);
                _spikes[i].LocateMyFSM("Control").SendEvent("DOWN");
            }

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
            _attackCommands.GetAction<Wait>("Aim", 11).time = 0.75f;
            _attackCommands.GetAction<Wait>("Eb Extra Wait", 0).time = 0.05f;

            //VERTICAL NAIL COMB
            _attackChoices.GetAction<SendEventByName>("Nail Top Sweep", 1).delay = 0.35f;
            _attackChoices.GetAction<SendEventByName>("Nail Top Sweep", 2).delay = 0.7f;
            _attackChoices.GetAction<SendEventByName>("Nail Top Sweep", 3).delay = 1.05f;
            _attackChoices.GetAction<Wait>("Nail Top Sweep", 4).time = 2.3f;
            _control.GetAction<Wait>("Rage Comb", 0).time = 0.6f;

            //HORIZONTAL NAIL COMB
            _attackChoices.GetAction<SendEventByName>("Nail L Sweep", 1).delay = .5f;
            _attackChoices.GetAction<SendEventByName>("Nail L Sweep", 1).delay = .5f + (nailWallDelay * 2);
            _attackChoices.GetAction<SendEventByName>("Nail L Sweep", 2).delay = .5f + (nailWallDelay * 4);
            _attackChoices.GetAction<Wait>("Nail L Sweep", 3).time = 1f + (nailWallDelay * 5);
            _attackChoices.GetAction<SendEventByName>("Nail R Sweep", 1).delay = .5f;
            _attackChoices.GetAction<SendEventByName>("Nail R Sweep", 1).delay = .5f + (nailWallDelay * 2);
            _attackChoices.GetAction<SendEventByName>("Nail R Sweep", 2).delay = .5f + (nailWallDelay * 4);
            _attackChoices.GetAction<Wait>("Nail R Sweep", 3).time = 1f + (nailWallDelay * 5);
            AddNailWall("Nail L Sweep", "COMB R", .5f + nailWallDelay, 1);
            AddNailWall("Nail R Sweep", "COMB L", .5f + nailWallDelay, 1);
            AddNailWall("Nail L Sweep", "COMB R", .5f + (nailWallDelay * 3), 1);
            AddNailWall("Nail R Sweep", "COMB L", .5f + (nailWallDelay * 3), 1);
            AddNailWall("Nail L Sweep 2", "COMB R2", 1, 1);
            AddNailWall("Nail R Sweep 2", "COMB L2", 1, 1);

            Log("fin.");

        }

        private void Update()
        {
            //Silly hack to get three waves of radial nails instead of two
            //This is a dumb way to do this but I don't care
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
                for (int i = 0; i < 5; i++)
                {
                    _spikeMasterControl.GetAction<SendEventByName>("Spikes Left", i).sendEvent = "UP";
                    _spikeMasterControl.GetAction<SendEventByName>("Spikes Right", i).sendEvent = "UP";
                    _spikeMasterControl.GetAction<SendEventByName>("Wave L", i + 2).sendEvent = "UP";
                    _spikeMasterControl.GetAction<SendEventByName>("Wave R", i + 2).sendEvent = "UP";
                }
                _spikeMasterControl.GetAction<WaitRandom>("Wave L", 7).timeMin = 0.1f;
                _spikeMasterControl.GetAction<WaitRandom>("Wave L", 7).timeMax = 0.1f;
                _spikeMasterControl.GetAction<WaitRandom>("Wave R", 7).timeMin = 0.1f;
                _spikeMasterControl.GetAction<WaitRandom>("Wave R", 7).timeMax = 0.1f;

                _spikeMasterControl.SetState("Spike Waves");

                //Prevent ddark cheese; if you try to dive onto spikes you take damage
                StartCoroutine(AddDivePunishment());

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

            if (_hm.hp < _phaseControl.FsmVariables.GetFsmInt("P3 A1 Rage").Value + 30 && !disableBeamSet)
            {
                disableBeamSet = true;

                //Disable beam sweeps when nail rain/rage phase is about to start, so the player isn't forced to ddark
                _attackChoices.ChangeTransition("A1 Choice", "BEAM SWEEP L", "Orb Wait");
                _attackChoices.ChangeTransition("A1 Choice", "BEAM SWEEP R", "Eye Beam Wait");
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

            if (gameObject.transform.position.y >= 150f) //Indicates the final phase has started
            {
                if (_hm.hp < _phaseControl.FsmVariables.GetFsmInt("P5 Acend").Value - onePlatHealth)
                {
                    //When the player deals some damage, remove the right platform
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
                    //When the player deals some more damage, spikes on the left platform go up
                    foreach(GameObject spike in _spikes)
                    {
                        spike.LocateMyFSM("Control").SendEvent("UP");
                    }

                    if (!platSpikesSet)
                    {
                        platSpikesSet = true;
                        GameObject.Find("Radiant Plat Small (10)").LocateMyFSM("radiant_plat").ChangeState(GetFsmEventByName(GameObject.Find("Radiant Plat Small (10)").LocateMyFSM("radiant_plat"), "SLOW VANISH"));
                        StartCoroutine(AddDivePunishment()); //Dive cheese prevention here too
                    }
                }
            }
        }

        /// <summary>
        /// Add behavior to the player's dive state ("Q2 Land") that deals damage. Behavior is added after a 2 second delay.
        /// </summary>
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

        /// <summary>
        /// Behavior added by AddDivePunishment that actually deals the damage.
        /// </summary>
        [UsedImplicitly]
        public void DivePunishment()
        {
            Log("YOU WON'T CHEESE SPIKES IN THIS TOWN AGAIN");
            HeroController.instance.TakeDamage(gameObject, GlobalEnums.CollisionSide.bottom, 1, 0); //Knight takes a hit
            EventRegister.SendEvent("HERO DAMAGED"); //Tells the UI to refresh
        }

        /// <summary>
        /// Adds an action to a state that generates a nail wall. Only adds to states in Attack Choices.
        /// </summary>
        /// <param name="eventName">Either "COMB R" or "COMB L". Use "COMB R2" or "COMB L2" for platform phase.</param>
        private void AddNailWall(string stateName, string eventName, float delay, int index)
        {
            _attackChoices.InsertAction(stateName, new SendEventByName
            {
                eventTarget = _attackChoices.GetAction<SendEventByName>("Nail L Sweep", 0).eventTarget, //Getting the event target from the fsm is hard, so let's just do it at runtime instead
                sendEvent = eventName,
                delay = delay,
                everyFrame = false
            }, index);
        }

        /// <summary>
        /// Convert string to FsmEvent
        /// </summary>
        /// <param name="fsm">FSM to scan</param>
        /// <param name="eventName">Name to search for</param>
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