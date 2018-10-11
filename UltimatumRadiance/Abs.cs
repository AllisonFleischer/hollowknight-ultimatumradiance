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

            //RADIAL BEAM BARRAGE
            _attackCommands.GetAction<SendEventByName>("EB 1", 9).delay = 0.525f; //Reduce the time the beam is active for
            _attackCommands.GetAction<Wait>("EB 1", 10).time = 0.55f; //And drastically reduce the wait afterward
            _attackCommands.GetAction<SendEventByName>("EB 2", 9).delay = 0.5f;
            _attackCommands.GetAction<Wait>("EB 2", 10).time = 0.525f;
            _attackCommands.GetAction<SendEventByName>("EB 3", 9).delay = 0.5f;
            _attackCommands.GetAction<Wait>("EB 3", 10).time = 0.525f;
            _attackCommands.GetAction<SendEventByName>("EB 4", 5).delay = 0.6f;
            _attackCommands.GetAction<Wait>("EB 4", 6).time = 0.6f;
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
            _attackCommands.GetAction<Wait>("Eb Extra Wait", 0).time = 0.05f;

            /*// Decrease idles
            _control.GetAction<WaitRandom>("Idle", 5).timeMax = 0.01f;
            _control.GetAction<WaitRandom>("Idle", 5).timeMin = 0.001f;
            
            // 2x Damage
            _control.GetAction<SetDamageHeroAmount>("Roar End", 3).damageDealt.Value = 2;

            // Increase Jump X
            _control.GetAction<FloatMultiply>("Aim Dstab", 3).multiplyBy = 5;
            _control.GetAction<FloatMultiply>("Aim Jump", 3).multiplyBy = 2.2f;

            // Decrease walk idles.
            RandomFloat walk = _control.GetAction<RandomFloat>("Idle", 3);
            walk.min = 0.001f;
            walk.max = 0.01f;

            // Speed up
            _control.GetAction<Wait>("Jump", 5).time = 0.01f;
            _control.GetAction<Wait>("Dash Antic 2", 2).time = 0.27f;

            // Fall faster.
            _control.GetAction<SetVelocity2d>("Dstab Fall", 4).y = -200; // -130; // -90
            _control.GetAction<SetVelocity2d>("Dstab Fall", 4).everyFrame = true;

            // Combo Dash into Upslash followed by Dstab's Projectiles..
            _control.CopyState("Dstab Land", "Spawners");
            _control.CopyState("Ohead Slashing", "Ohead Combo");
            _control.CopyState("Dstab Recover", "Dstab Recover 2");

            _control.ChangeTransition("Dash Recover", "FINISHED", "Ohead Combo");

            _control.RemoveAnim("Dash Recover", 3);
            _control.RemoveAnim("Spawners", 3);

            _control.ChangeTransition("Ohead Combo", "FINISHED", "Spawners");
            _control.ChangeTransition("Spawners", "FINISHED", "Dstab Recover 2");
            _control.GetAction<Wait>("Dstab Recover 2", 0).time = 0f;

            List<FsmStateAction> a = _control.GetState("Dstab Fall").Actions.ToList();
            a.AddRange(_control.GetState("Spawners").Actions);

            _control.GetState("Dstab Fall").Actions = a.ToArray();

            // Spawners before Overhead Slashing.
            _control.CopyState("Spawners", "Spawn Ohead");
            _control.ChangeTransition("Ohead Antic", "FINISHED", "Spawn Ohead");
            _control.ChangeTransition("Spawn Ohead", "FINISHED", "Ohead Slashing");
            _control.FsmVariables.GetFsmFloat("Evade Range").Value *= 2;

            // Dstab => Upslash
            _control.CopyState("Ohead Slashing", "Ohead Combo 2");
            _control.ChangeTransition("Dstab Land", "FINISHED", "Ohead Combo 2");
            _control.ChangeTransition("Ohead Combo 2", "FINISHED", "Dstab Recover");

            // Aerial Dash => Dstab
            _control.ChangeTransition("Dash Recover", "FALL", "Dstab Antic");

            // bingo bongo ur dash is now lightspeed
            _control.FsmVariables.GetFsmFloat("Dash Speed").Value *= 2;
            _control.FsmVariables.GetFsmFloat("Dash Reverse").Value *= 2;

            // Fixes the cheese where you can sit on the wall
            // right above where he can jump and then just spam ddark
            _control.CopyState("Jump", "Cheese Jump");
            _control.GetAction<Wait>("Cheese Jump", 5).time.Value *= 5;
            _control.RemoveAction("Cheese Jump", 4);
            _control.InsertAction("Cheese Jump", new FireAtTarget
            {
               gameObject = new FsmOwnerDefault
               {
                   GameObject = gameObject
               },
               target = HeroController.instance.gameObject,
               speed = 100f,
               everyFrame = false,
               spread = 0f,
               position = new Vector3(0, 0)
            }, 4);

            CallMethod cm = new CallMethod
            {
                behaviour = this,
                methodName = "StopCheese",
                parameters = new FsmVar[0],
                everyFrame = false
            };

            foreach (string i in new[] {"Damage Response", "Attack Choice"})
            {
                _control.InsertAction(i, cm, 0);
            }*/

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

        /*[UsedImplicitly]
        public void StopCheese()
        {
            float hx = HeroController.instance.gameObject.transform.GetPositionX();
            float hy = HeroController.instance.gameObject.transform.GetPositionY();

            if (hy > 35 && (15 < hx && hx < 16.6 || 36.55 < hx && hx < 37.8))
            {
                _control.SetState("Cheese Jump");
            }
        }
        
        private static GameObject Projectile(GameObject go)
        {
            if (go.name != "IK Projectile DS(Clone)" && go.name != "Parasite Balloon Spawner(Clone)") return go;

            foreach (DamageHero i in go.GetComponentsInChildren<DamageHero>(true))
            {
                i.damageDealt = 2;
            }

            return go;
        }

        private void OnDestroy()
        {
            ModHooks.Instance.ObjectPoolSpawnHook -= Projectile;
        }*/

        private static void Log(object obj)
        {
            Logger.Log("[Ultimatum Radiance] " + obj);
        }
    }
}