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
        private HealthManager _hm;

        private tk2dSpriteAnimator _anim;

        private Recoil _recoil;

        private PlayMakerFSM _attackChoices;
        private PlayMakerFSM _balloons;
        private PlayMakerFSM _control;

        private void Awake()
        {
            Log("Added AbsRad MonoBehaviour");
            
            ModHooks.Instance.ObjectPoolSpawnHook += Projectile;
            
            _hm = gameObject.GetComponent<HealthManager>();
            _attackChoices = gameObject.LocateMyFSM("Attack Choices");
            //_balloons = gameObject.LocateMyFSM("Spawn Balloon");
            _anim = gameObject.GetComponent<tk2dSpriteAnimator>();
            //_control = gameObject.LocateMyFSM("IK Control");
            _recoil = gameObject.GetComponent<Recoil>();
        }

        private void Start()
        {
            //_attackChoices.GetAction<SendRandomEventV3>("A1 Choice", 1).weights = new FsmFloat[] { 0, 0, 0, 0, 1, 0, 0, 0, 0 };
            _attackChoices.GetAction<Wait>("Orb Recover", 0).time = 10;

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

        [UsedImplicitly]
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
        }

        private static void Log(object obj)
        {
            Logger.Log("[Ultimatum Radiance] " + obj);
        }
    }
}