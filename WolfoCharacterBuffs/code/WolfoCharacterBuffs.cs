using BepInEx;
using R2API;
using RoR2;

namespace WolfoCharacterBuffs
{
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin("com.Wolfo.WolfoCharacterBuffs", "WolfoCharacterBuffs", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        public static bool mod_riskyMod = false;
        public static bool mod_flatItems = false;
        public void Awake()
        {
            WConfig.InitConfig();

            mod_riskyMod = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.RiskyLives.RiskyMod");
            mod_flatItems = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.kking117.FlatItemBuff");

            Acrid.Start();



            GameModeCatalog.availability.CallWhenAvailable(CallLate);
        }

        public static void CallLate()
        {
            //Is language loaded here?
        }
    }
}