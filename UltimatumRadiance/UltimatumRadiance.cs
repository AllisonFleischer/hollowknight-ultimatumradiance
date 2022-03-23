using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using USceneManager = UnityEngine.SceneManagement.SceneManager;

namespace UltimatumRadiance
{
    [UsedImplicitly]
    public class UltimatumRadiance : Mod, ITogglableMod
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once NotAccessedField.Global
        public static UltimatumRadiance Instance;

        public UltimatumRadiance() : base("Ultimatum Radiance") { }

        public override void Initialize()
        {
            Instance = this;

            Log("Initalizing.");
            USceneManager.activeSceneChanged += CheckForRadiance;
            ModHooks.LanguageGetHook += LangGet;
        }


        public override string GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(UltimatumRadiance)).Location).FileVersion+"(1.5)";
        }

        public void Unload()
        {
            USceneManager.activeSceneChanged -= CheckForRadiance;
            ModHooks.LanguageGetHook -= LangGet;
        }

        private static string LangGet(string key, string sheettitle,string orig)
        {
            switch (key)
            {
                case "ABSOLUTE_RADIANCE_SUPER": return "Ultimatum";
                case "GG_S_RADIANCE": return "God of light, sworn to crush any rebellion";
                case "GODSEEKER_RADIANCE_STATUE":
                    return "Incredible! For a mere Speck to take up arms and defy the brilliant deity's ultimatum is to be consigned to oblivion, and yet thou survive!\n\n" +
                        "But couldst thou ever hope to overcome that mighty God tuned at the core of dream and mind, when met in perfect state, at peak of all others? We think not!\n\n" +
                        "Seriously, thy time is probably better spent elsewhere.";
                default: return orig;
            }
        }

        private static void CheckForRadiance(Scene from, Scene to)
        {
            if (to.name != "GG_Radiance")
            {
                return;
            }

            // ReSharper disable once ObjectCreationAsStatement
            new GameObject("AbsFinder", typeof(AbsFinder));
        }
    }
}