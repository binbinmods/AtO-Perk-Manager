using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
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
        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} has loaded!");

            // EnableMultipleReroll = Config.Bind(new ConfigDefinition("Debug", "Enable Multiple Rerolls"), true, new ConfigDescription("Enables the Multiple Reroll Mod. By default, only works for M3+"));
            // LimitRerollsForLowMadness = Config.Bind(new ConfigDefinition("Debug", "Limit Rerolls for Low Madness"), true, new ConfigDescription("Limits the rolls for low Madness (Below Base Madness 3)"));
            // NumberOfRerolls = Config.Bind(new ConfigDefinition("Debug", "Number of Rerolls"), 3, new ConfigDescription("Sets the number of rerolls. This will increase the number of rerolls for high madness and decrease it for low madnesss (if enabled)."));
            

            // register with Obeliskial Essentials
            RegisterMod(
                _name: PluginInfo.PLUGIN_NAME,
                _author: "binbin",
                _description: "Custom Perks",
                _version: PluginInfo.PLUGIN_VERSION,
                _date: ModDate,
                _link: @"https://github.com/binbinmods/Perks"
            );
            // apply patches
            harmony.PatchAll();
        }

    }
}