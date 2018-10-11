using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Modding;
using UnityEngine;
using Logger = Modding.Logger;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace UltimatumRadiance
{
    internal class Abs : MonoBehaviour
    {
        private PlayMakerFSM _attackChoices;
        private PlayMakerFSM _attackCommands;
        private PlayMakerFSM _control;

        private int CWRepeats = 0;

        private void Awake()
        {
            Log("Added AbsRad MonoBehaviour");
            _attackChoices = gameObject.LocateMyFSM("Attack Choices");
            _attackCommands = gameObject.LocateMyFSM("Attack Commands");
            _control = gameObject.LocateMyFSM("Control");
        }

        private void Start()
        {
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
            _attackChoices.GetAction<Wait>("Beam Sweep L", 0).time = 4f; //Wait longer because this attack is a lot more demanding now
            _attackChoices.GetAction<Wait>("Beam Sweep R", 0).time = 4f;
            _attackChoices.ChangeTransition("A1 Choice", "BEAM SWEEP R", "Beam Sweep L");
            _attackChoices.ChangeTransition("A2 Choice", "BEAM SWEEP R", "Beam Sweep L 2");
            _attackChoices.GetAction<Wait>("Beam Sweep L 2", 0).time = 4f;
            _attackChoices.GetAction<Wait>("Beam Sweep R 2", 0).time = 4f;
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
        }

        private static void Log(object obj)
        {
            Logger.Log("[Ultimatum Radiance] " + obj);
        }
    }
}