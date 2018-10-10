using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using Modding;
using UnityEngine;
using Logger = Modding.Logger;

namespace UltimatumRadiance
{
    class BeamSweeperClone : MonoBehaviour
    {
        private PlayMakerFSM _control;

        private void Awake()
        {
            Log("Added BeamSweeperClone MonoBehaviour");
            _control = gameObject.LocateMyFSM("Control");
        }

        private void Start()
        {
            _control.ChangeTransition("Idle", "BEAM SWEEP L", "Beam Sweep R"); //Cross the wires
            _control.ChangeTransition("Idle", "BEAM SWEEP R", "Beam Sweep L");
            _control.ChangeTransition("Idle", "BEAM SWEEP L 2", "Beam Sweep R 2");
            _control.ChangeTransition("Idle", "BEAM SWEEP R 2", "Beam Sweep L 2");

            Log("it's double beam time");
        }

        private static void Log(object obj)
        {
            Logger.Log("[Ultimatum Radiance] " + obj);
        }
    }
}
