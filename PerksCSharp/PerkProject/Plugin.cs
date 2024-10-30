using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Obeliskial_Essentials;
using static Obeliskial_Essentials.Essentials;

namespace PerkManager{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.stiffmeds.obeliskialessentials")]
    [BepInDependency("com.stiffmeds.obeliskialcontent")]
    [BepInProcess("AcrossTheObelisk.exe")]

    public class Plugin : BaseUnityPlugin
    {
        // public static ConfigEntry<bool> EnableMultipleReroll { get; set; }
        // public static ConfigEntry<bool> LimitRerollsForLowMadness { get; set; }
        // public static ConfigEntry<int> NumberOfRerolls { get; set; }

        internal const int ModDate = 20241024;
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        internal static ManualLogSource Log;
        public static string debugBase = "Binbin - Testing Perks - ";

        public static string perkBase = "binbin_mainperk_";

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} has loaded!");

            // EnableMultipleReroll = Config.Bind(new ConfigDefinition("Debug", "Enable Multiple Rerolls"), true, new ConfigDescription("Enables the Multiple Reroll Mod. By default, only works for M3+"));
            // LimitRerollsForLowMadness = Config.Bind(new ConfigDefinition("Debug", "Limit Rerolls for Low Madness"), true, new ConfigDescription("Limits the rolls for low Madness (Below Base Madness 3)"));
            // NumberOfRerolls = Config.Bind(new ConfigDefinition("Debug", "Number of Rerolls"), 3, new ConfigDescription("Sets the number of rerolls. This will increase the number of rerolls for high madness and decrease it for low madnesss (if enabled)."));
            string perkStem = "custom_binbin_";

            // register with Obeliskial Essentials
            RegisterMod(
                _name: PluginInfo.PLUGIN_NAME,
                _author: "binbin",
                _description: "Custom Perks",
                _version: PluginInfo.PLUGIN_VERSION,
                _date: ModDate,
                _link: @"https://github.com/binbinmods/Perks"
            );
            // Custom text for Zeal
            medsTexts[perkStem + "zeal1a"] = "Zeal +1.";
            medsTexts[perkStem + "zeal1b"] = "Zeal on this hero loses 3 charges per turn rather than all charges.";
            medsTexts[perkStem + "zeal1c"] = "Zeal on all heroes can stack, but reduces Speed by 2 per charge.";
            medsTexts[perkStem + "zeal1d"] = "Zeal on heroes and monsters increases all resistances by 0.5% per Wet charge.";
            medsTexts[perkStem + "zeal1e"] = "When this hero loses Zeal, deal indirect Holy and Fire damage equal to 4x the number of stacks lost.";
            // medsTexts[perkStem + "zealf"] = "If this hero dies with Zeal, deal indirect Fire damage equal to 5x their insane to every enemy.";

            // Custom text for Scourge
            medsTexts[perkStem + "scourge1a"] = "Scourge +1.";
            medsTexts[perkStem + "scourge1b"] = "Scourge on heroes and monsters loses 3 charges per turn rather than all charges.";
            medsTexts[perkStem + "scourge1c"] = "Scourge on monsters can Stack but increases all resists by 3% per stack.";
            medsTexts[perkStem + "scourge1d"] = "Scourge deals damage based on Burn rather than Chill.";
            medsTexts[perkStem + "scourge1e"] = "Scourge on monsters increases burn damage by 15%/stack";

            // Custom text for Weakness
            medsTexts[perkStem + "weak1a"] = "Weak +1.";
            medsTexts[perkStem + "weak1b"] = "Weak on heroes and monsters can stack but reduces damages by 20% per stack.";
            medsTexts[perkStem + "weak1c"] = "Monsters cannot be immune to Weak, but no longer have their damage reduced by Insane.";
            medsTexts[perkStem + "weak1d"] = "This hero is immune to Weak.";
            // apply patches
            harmony.PatchAll();
        }


    }
}