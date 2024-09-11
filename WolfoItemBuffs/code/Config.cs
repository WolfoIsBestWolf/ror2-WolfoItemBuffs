using BepInEx;
using BepInEx.Configuration;

namespace WolfoItemBuffs
{
    public class WConfig
    {
        public static ConfigFile ConfigFileUNSORTED = new ConfigFile(Paths.ConfigPath + "\\Wolfo.Wolfo_Item_Buffs.cfg", true);
 
        public static ConfigEntry<bool> cfg_Green_LeechSeed;
        public static ConfigEntry<bool> cfg_Green_Harpoon;
        public static ConfigEntry<bool> cfg_Green_Whip;
        public static ConfigEntry<bool> cfg_Green_Squid;

        public static ConfigEntry<bool> cfg_Red_Aegis;
        public static ConfigEntry<bool> cfg_Red_LaserScope;

        public static ConfigEntry<bool> cfg_Yellow_Knurl;
        public static ConfigEntry<bool> cfg_Yellow_DefenseNuc;

        public static ConfigEntry<bool> cfg_Pink_Ring;

        public static ConfigEntry<bool> cfg_Blue_Eulogy;

        public static ConfigEntry<bool> cfg_Orange_EliteInherit;


        public static void InitConfig()
        {
            cfg_Green_Harpoon = ConfigFileUNSORTED.Bind(
                "Green",
                "Hunters Harpoon",
                true,
                "Hunters Harpoon Returns Buffs"
            );
            cfg_Green_Squid = ConfigFileUNSORTED.Bind(
                "Green",
                "Squid Polyp",
                true,
                "Squids count as mechanical."
            );
            cfg_Green_LeechSeed = ConfigFileUNSORTED.Bind(
                "Green",
                "Leech Seed",
                true,
                "Heals regardless of proc coeff."
            );
            cfg_Red_Aegis = ConfigFileUNSORTED.Bind(
                "Red",
                "Aegis",
                true,
                "Removes Barrier Decay."
            );
            //
            cfg_Red_LaserScope = ConfigFileUNSORTED.Bind(
                "Red",
                "Laser Scope",
                true,
                "10 crit"
            );
            cfg_Yellow_Knurl = ConfigFileUNSORTED.Bind(
                "Yellow",
                "Titanic Knurl",
                true,
                "15 armor per stack"
            );
            cfg_Yellow_DefenseNuc = ConfigFileUNSORTED.Bind(
                "Yellow",
                "Defense Nucleus",
                true,
                "Give them Wake of Vultures"
            );
            cfg_Pink_Ring = ConfigFileUNSORTED.Bind(
                "Void",
                "Singularity Band",
                true,
                "300 damage instead of 100"
            );
            cfg_Blue_Eulogy = ConfigFileUNSORTED.Bind(
                "Lunar",
                "Eulogy Zero",
                true,
                "Perfected Elites"
            );
            cfg_Orange_EliteInherit = ConfigFileUNSORTED.Bind(
                "Equipment",
                "Elite Equipment Inheritance",
                true,
                "Minions spawn as elite when you have a elite"
            );

        }

    }
}