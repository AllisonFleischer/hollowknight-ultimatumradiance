using System.Diagnostics;
using System.Reflection;
using JetBrains.Annotations;
using Modding;
using UObject = UnityEngine.Object;

namespace UltimatumRadiance
{
    [UsedImplicitly]
    public class UltimatumRadiance : Mod<UltSettings>, ITogglableMod
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once NotAccessedField.Global
        public static UltimatumRadiance Instance;

        public override string GetVersion()
        {
            return FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(UltimatumRadiance)).Location).FileVersion;
        }

        public override void Initialize()
        {
            Instance = this;

            Log("Initalizing.");
            ModHooks.Instance.AfterSavegameLoadHook += AfterSaveGameLoad;
            ModHooks.Instance.NewGameHook += AddComponent;
            ModHooks.Instance.LanguageGetHook += LangGet;
        }

        private static string LangGet(string key, string sheettitle)
        {
            return key == "INFECTED_KNIGHT_DREAM_MAIN" && PlayerData.instance.infectedKnightDreamDefeated
                ? "Lord"
                : Language.Language.GetInternal(key, sheettitle);
        }

        private static void AfterSaveGameLoad(SaveGameData data) => AddComponent();

        private static void AddComponent()
        {
            GameManager.instance.gameObject.AddComponent<AbsFinder>();
        }

        public void Unload()
        {
            ModHooks.Instance.AfterSavegameLoadHook -= AfterSaveGameLoad;
            ModHooks.Instance.NewGameHook -= AddComponent;
            ModHooks.Instance.LanguageGetHook -= LangGet;

            // ReSharper disable once Unity.NoNullPropogation
            AbsFinder x = GameManager.instance?.gameObject.GetComponent<AbsFinder>();
            if (x == null) return;
            UObject.Destroy(x);
        }
    }
}