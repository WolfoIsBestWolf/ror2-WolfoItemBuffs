using BepInEx;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace WolfoItemBuffs
{
    public class WConfig
    {
        public static ConfigFile ConfigFileUNSORTED = new ConfigFile(Paths.ConfigPath + "\\Wolfo.Wolfo_Item_Buffs.cfg", true);

        public static ConfigEntry<bool> cfg_White_Warbanner;
 
        public static ConfigEntry<bool> cfg_Green_LeechSeed;
        public static ConfigEntry<bool> cfg_Green_Harpoon;
        public static ConfigEntry<float> cfg_Green_Harpoon_VAL;
        //public static ConfigEntry<bool> cfg_Green_Whip;
        public static ConfigEntry<bool> cfg_Green_Squid;
        public static ConfigEntry<bool> cfg_Green_Stealthkit;
        public static ConfigEntry<bool> cfg_Green_WarCry;
        public static ConfigEntry<bool> cfg_Green_RegenScrap;
        public static ConfigEntry<bool> cfg_Green_IgnitionTank;

        public static ConfigEntry<bool> cfg_Red_Aegis;
        public static ConfigEntry<float> cfg_Red_Aegis_VAL;
        public static ConfigEntry<ConfigChoice> cfg_Red_LaserScope;
        public static ConfigEntry<bool> cfg_Red_ICBM;
        public static ConfigEntry<bool> cfg_Red_Nkuhana;

        public static ConfigEntry<bool> cfg_Yellow_Knurl;
        public static ConfigEntry<bool> cfg_Yellow_DefenseNuc;

        public static ConfigEntry<bool> cfg_Pink_Ring;

        public static ConfigEntry<bool> cfg_Blue_Eulogy;
        public static ConfigEntry<bool> cfg_Blue_Focus;

        public static ConfigEntry<bool> cfg_Orange_EliteInherit;
        public static ConfigEntry<bool> cfg_Orange_OcularHud;
        public static ConfigEntry<bool> cfg_Orange_Molotov;

        public enum ConfigChoice
        {
            Off,
            Buff,
            Rework
        }
        public enum ICBM
        {
            Off,
            AllExplosives,
        }
        public enum Harpoon
        {
            Off,
            ReturnsTweaked,
            ReturnsExact
        }
        public static void InitConfig()
        {
            cfg_White_Warbanner = ConfigFileUNSORTED.Bind(
                "White",
                "Warbanner",
                true,
                "Spawns on stage start and special boss fights like Mithrix. Stacks more Radius. Special Boss automatically disabled with RiskyMod as it does the same thing."
            );
             
            cfg_Green_RegenScrap = ConfigFileUNSORTED.Bind(
                "Green",
                "Regenerating Scrap Rework Off",
                false,
                "Works on any printer. Consumed all at once for chance of multiple items. Basically Sale Star for printers with different stacking."
            );
            cfg_Green_Harpoon = ConfigFileUNSORTED.Bind(
                "Green",
                "Hunters Harpoon",
                true,
                "Hunters Harpoon works like in Returns."
            );
            cfg_Green_Harpoon_VAL = ConfigFileUNSORTED.Bind(
                 "Green",
                 "Hunters Harpoon - Speed Buff",
                 100f,
                 "How much speed should it give you in %"
             );
            cfg_Green_WarCry = ConfigFileUNSORTED.Bind(
                "Green",
                "Berzerkers Pauldron",
                true,
                "Berzerker Pauldron longer consecutive kill window for easier activation."
            );

            cfg_Green_Squid = ConfigFileUNSORTED.Bind(
                "Green",
                "Squid Polyp : Mechanical",
                false,
                "Squids count as mechanical, making them immune to Void Infestors and benefit from Microbots and Drone Parts."
            );
            cfg_Green_IgnitionTank = ConfigFileUNSORTED.Bind(
                 "Green",
                 "Ignition Tank Off",
                 false,
                 "Fire items also ignite. (Kjaros, WilloWisp). Small chance to fire an ignition tank to ignite enemies in a small area."
             );
            cfg_Green_Stealthkit = ConfigFileUNSORTED.Bind(
                "Green",
                "Old War Stealthkit",
                true,
                "Activate at 50% instead of 25%"
            );
            cfg_Green_LeechSeed = ConfigFileUNSORTED.Bind(
                "Green",
                "Leech Seed",
                true,
                "Heals on damage over time effects like burn or bleed."
            );
            cfg_Red_Aegis = ConfigFileUNSORTED.Bind(
                "Red",
                "Aegis",
                true,
                "Slow Barrier Decay Rate by a lot on first stack."
            );
            cfg_Red_ICBM = ConfigFileUNSORTED.Bind(
                "Red",
                "Pocket I.C.B.M Off",
                false,
                "Activates on all explosive projectiles."
            );
            cfg_Red_Aegis_VAL = ConfigFileUNSORTED.Bind(
                "Red",
                "Aegis - Barrier Decay",
                0.2f,
                "Barrier Decay Multiplier"
            );
            cfg_Red_Nkuhana = ConfigFileUNSORTED.Bind(
                "Red",
                "Nkuhanas Opinion",
                true,
                "Proc Coeff buff and overall QoL"
            );
            //
            cfg_Red_LaserScope = ConfigFileUNSORTED.Bind(
                "Red",
                "Laser Scope Buff",
                ConfigChoice.Buff,
                "Rework : A lot of crit, no crit damage mult, crit damage above 100% turns into crit damage. \n\nBuff : +15% crit"
            );
            cfg_Yellow_Knurl = ConfigFileUNSORTED.Bind(
                "Yellow",
                "Titanic Knurl",
                true,
                "12 armor per stack. Similiar to RoR:Returns."
            );
            cfg_Yellow_DefenseNuc = ConfigFileUNSORTED.Bind(
                "Yellow",
                "Defense Nucleus",
                true,
                "Now stacks stats instead of spawn limit. Give them 200 Wake of Vultures."
            );
            cfg_Pink_Ring = ConfigFileUNSORTED.Bind(
                "Void",
                "Singularity Band",
                true,
                "150% Total Damage instead of 100%"
            );
            cfg_Blue_Eulogy = ConfigFileUNSORTED.Bind(
                "Lunar",
                "Eulogy Zero",
                true,
                "Elites have a chance to spawn as perfected."
            );
            cfg_Blue_Focus = ConfigFileUNSORTED.Bind(
                "Lunar",
                "Focused Convergence",
                true,
                "Exponential stacking instead linear divided. Removed item cap of 3. Shrinkage capped"
            );
            cfg_Orange_EliteInherit = ConfigFileUNSORTED.Bind(
                "Equipment",
                "Elite Equipment Inheritance",
                true,
                "Minions spawn as elite when you have a Elite Equipment"
            );
            /*cfg_Orange_OcularHud = ConfigFileUNSORTED.Bind(
                "Equipment",
                "Ocular Hud",
                true,
                "Turns excessive Crit into Crit Damage."
            );*/
            cfg_Orange_Molotov = ConfigFileUNSORTED.Bind(
                "Equipment",
                "Molotov Pack",
                true,
                "Molotov 8 Pack instead of 6."
            );
            RiskConfig();
        }

        public static void RiskConfig()
        {
 
            ModSettingsManager.SetModIcon(Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/MoveSpeedOnKill/texGrappleHookIcon.png").WaitForCompletion());
            ModSettingsManager.SetModDescription("Various item buffs.");

            
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Red_Aegis, true));
            ModSettingsManager.AddOption(new FloatFieldOption(cfg_Red_Aegis_VAL, true));
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Red_Nkuhana, true));

            
           
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Green_Harpoon, true));
            ModSettingsManager.AddOption(new FloatFieldOption(cfg_Green_Harpoon_VAL, true));
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Green_LeechSeed, true));
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Green_Squid, true));
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Green_Stealthkit, true));
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Green_WarCry, true));

            ModSettingsManager.AddOption(new CheckBoxOption(cfg_White_Warbanner, true));
             
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Yellow_Knurl, true));
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Yellow_DefenseNuc, true));

            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Pink_Ring, true));

            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Blue_Eulogy, true));
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Blue_Focus, true));

            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Orange_EliteInherit, true));
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Orange_Molotov, true));



            
            
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Red_ICBM, true));
            ModSettingsManager.AddOption(new ChoiceOption(cfg_Red_LaserScope, true));
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Green_IgnitionTank, true));
            ModSettingsManager.AddOption(new CheckBoxOption(cfg_Green_RegenScrap, true));

        }
    }
}