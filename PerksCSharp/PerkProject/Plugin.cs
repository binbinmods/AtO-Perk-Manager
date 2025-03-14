﻿using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using Obeliskial_Essentials;
using static Obeliskial_Essentials.Essentials;

namespace PerkManager
{

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

        public static ConfigEntry<int> ZealCap { get; set; }
        public static ConfigEntry<bool> EnableDebugging { get; set; }

        public static string perkBase = "binbin_mainperk_";

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"{PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} has loaded!");

            // EnableMultipleReroll = Config.Bind(new ConfigDefinition("Debug", "Enable Multiple Rerolls"), true, new ConfigDescription("Enables the Multiple Reroll Mod. By default, only works for M3+"));
            // LimitRerollsForLowMadness = Config.Bind(new ConfigDefinition("Debug", "Limit Rerolls for Low Madness"), true, new ConfigDescription("Limits the rolls for low Madness (Below Base Madness 3)"));
            EnableDebugging = Config.Bind(new ConfigDefinition("TooManyPerks", "Enable Debugging"), true, new ConfigDescription("Enables debugging logs."));
            ZealCap = Config.Bind(new ConfigDefinition("TooManyPerks", "Cap on Zeal stacks"), 10, new ConfigDescription("Sets a Cap on the number of Zeal Stacks that heroes can have for the zeal1c perk. -1 should remove the cap."));
            string perkStem = "custom_binbin_mainperk_";

            // register with Obeliskial Essentials
            RegisterMod(
                _name: PluginInfo.PLUGIN_NAME,
                _author: "binbin",
                _description: "Custom Perks",
                _version: PluginInfo.PLUGIN_VERSION,
                _date: ModDate,
                _link: @"https://github.com/binbinmods/AtO-Perk-Manager",
                _contentFolder: "Too Many Perks",
                _type: ["content", "perk", "perkNode", "sprite"]
            );


            // Custom Text for Experience
            medsTexts[perkStem + "exp0"] = "Start with 10 Experience. Gain 10 Experience on level up.";
            medsTexts[perkStem + "exp1"] = "Start with 10 Experience. Gain 10 Experience on level up.";
            medsTexts[perkStem + "exp2"] = "Start with 10 Experience. Gain 10 Experience on level up.";
            medsTexts[perkStem + "exp3"] = "Start with 40 Experience. Gain 40 Experience on level up.";

            // Custom Text for Max HP
            // medsTexts[perkStem + "health6a"] = "+16 Max HP.";
            medsTexts[perkStem + "health6b"] = "Max HP -12. Gain 12 Max HP on level up.";
            medsTexts[perkStem + "health6c"] = "Max HP +36. Lose 14 Max HP on level up";
            medsTexts[perkStem + "health6d"] = "At the end of your turn, if you are at max HP, gain 2 Vitality.";

            // Custom Text for Resistances
            // medsTexts[perkStem + "resistance5a"] = "+8% Resistances.";
            medsTexts[perkStem + "resistance5b"] = "All Resistances -4%. Gain 4% to all Resistances on level up";
            medsTexts[perkStem + "resistance5c"] = "All Resistances +12%. Lose 4% to all Resistances on level up";
            medsTexts[perkStem + "resistance5d"] = "Maximum resistances for heroes and monsters are now 97%.";

            // Custom Text for Gold
            //medsTexts[perkStem + "currency6a"] = "+225 Gold.";
            medsTexts[perkStem + "currency6b"] = "Gain 125 gold on level up.";
            //medsTexts[perkStem + "currency6c"] = "Increases chance for Scarabs to spawn by 10%."; CANNOT BE DONE EASILY
            medsTexts[perkStem + "currency6c"] = "For every 2,000 gold you have, gain +10% damage.";
            medsTexts[perkStem + "currency6d"] = "Rerolling the shop costs 25% less.";
            medsTexts[perkStem + "currency6e"] = "Divinations cost 15% less.";

            // Custom Text for Shards
            //medsTexts[perkStem + "shards5a"] = "+225 Shards";
            medsTexts[perkStem + "shards5b"] = "Gain 125 shards on level up.";
            medsTexts[perkStem + "shards5c"] = "Increases chance for cards to be corrupted by 2%.";

            // Custom Text for Disarm
            medsTexts[perkStem + "disarm1a"] = "This hero is immune to Disarm.";
            medsTexts[perkStem + "disarm1b"] = "Disarm on this hero cannot be dispelled unless specified, but increases all resists by 10%.";

            // Custom Text for Silence
            medsTexts[perkStem + "silence1a"] = "This hero is immune to Silence.";
            medsTexts[perkStem + "silence1b"] = "Silence on this hero cannot be dispelled unless specified, but increases all damage by 7.";

            // Custom Text for Sight
            medsTexts[perkStem + "sight1d"] = "At the start of your turn, gain 1 Evasion for every enemy with 100 or more Sight charges.";
            medsTexts[perkStem + "sight1e"] = "When a monster reaches 100 charges of Sight, Dispel Sight and Purge 3.";

            // Custom Text for Fast
            medsTexts[perkStem + "fast0b"] = "Fast on this hero can stack, but loses all charges at the start of turn.";
            medsTexts[perkStem + "fast0c"] = "Fast on this hero falls off at the end of turn.";

            // Custom Text for Slow
            medsTexts[perkStem + "slow0b"] = "Slow on monsters can stack up to 10, but only reduces Speed by 1 per charge";
            medsTexts[perkStem + "slow0c"] = "Slow on heroes can stack up to 10, but only reduces Speed by 1 per charge";

            // Custom Text for Mark
            medsTexts[perkStem + "mark1d"] = "Once per combat, when an enemy reaches 10 Mark, apply 2 Taunt.";
            medsTexts[perkStem + "mark1e"] = "Every 2 Mark charges on enemies increases Piercing Damage by 3. Mark does not increase any other damage type.";
            medsTexts[perkStem + "mark1f"] = "+1 Mark Charge applied.";
            medsTexts[perkStem + "mark1g"] = "Mark on heroes and enemies decreases Speed by 1 per charge, but it increases damage by 50% of what it normally would.";

            // Custom Text for Evasion
            medsTexts[perkStem + "evasion0b"] = "Evasion on all heroes can stack but loses all charges at the start of turn.";

            // Custom Text for Energy
            medsTexts[perkStem + "energy2d"] = "Significantly increases the damage dealt by some high cost cards. Reduces damage dealt by some low cost cards. Works best with single damage instances. (Damage shown in preview might not be accurate)";
            // medsTexts[perkStem + "energy2e"] = "-1 Energy Regeneration. When this hero plays a card that costs 4 or more Energy, refund 1 Energy.";
            medsTexts["energy2c"] = "On round 4, this hero gains 3 Energy at the start of the turn.";

            // Custom Text for Shackle
            medsTexts[perkStem + "shackle1a"] = "+1 Charge applied";
            medsTexts[perkStem + "shackle1b"] = "This hero is immune to Shackle.";
            medsTexts[perkStem + "shackle1c"] = "Shackle cannot be prevented by Immunity or Buffer.";
            medsTexts[perkStem + "shackle1d"] = "At start of your turn, gain Fortify equal to your twice your Shackles.";
            medsTexts[perkStem + "shackle1e"] = "Shackle on this hero increases Dark charges applied by 1 per charge of Shackle.";
            medsTexts[perkStem + "shackle1f"] = "Shackle on monsters increases all damage received by 1 per base Speed.";

            // Custom Text for Mitigate
            medsTexts[perkStem + "mitigate1a"] = "At the start of your turn, gain 2 Mitigate (this only increases the amount of Mitigate you have by 1), but only stacks to 5.";
            medsTexts[perkStem + "mitigate1b"] = "Mitigate on this hero does not lose charges at start of turn and stacks to 12.";
            medsTexts[perkStem + "mitigate1c"] = "At the start of your turn, gain 7 Block per Mitigate charge.";
            medsTexts[perkStem + "mitigate1d"] = "Mitigate on this hero reduces incoming damage by 2 per charge, but loses all charges at the start of your turn.";
            medsTexts[perkStem + "mitigate1e"] = "Mitigate on heroes and monsters increases damage done by 10% per charge.";

            // Custom Text for Poison
            medsTexts[perkStem + "poison2d"] = "If Restricted Power is enabled, increases Max Charges to 300.";
            medsTexts[perkStem + "poison2e"] = "Poison on heroes and monsters reduces Slashing resistance by 0.20% per charge.";
            medsTexts[perkStem + "poison2f"] = "Poison on monsters reduces all resistances by 5% for every 100 charges.";
            medsTexts[perkStem + "poison2g"] = "Once per turn, when a monster with Poison dies, transfer 50% of their Poison charges to a random monster.";
            medsTexts[perkStem + "poison2h"] = "-1 Poison. When this hero applies Poison, deal indirect Mind damage to the target equal to 30% of their Poison charges.";
            medsTexts["poison2c"] = "Poison deals 50% bonus damage and deals damage at the start of turn but loses all charges at the start of turn.";

            // Custom Text for Bleed
            medsTexts[perkStem + "bleed2d"] = "If Restricted Power is enabled, increases Max Charges to 300.";
            medsTexts[perkStem + "bleed2e"] = "When this hero hits an enemy with Bleed, they heal for 25% of the target's Bleed charges.";
            medsTexts[perkStem + "bleed2f"] = "Bleed on monsters reduces Piercing resist by 0.25% per charge.";
            medsTexts[perkStem + "bleed2g"] = "When a monster dies with Bleed, all monsters lose HP equal to 25% of the killed target's Bleed charges.";
            medsTexts["bleed2b"] = "Bleed on this hero deals damage at the end of turn but cannot be dispelled unless specified. Fury on this hero is capped at 50 charges.";
            medsTexts["bleed2c"] = "Bleed Charges +2. Bleed on enemies deals damage at the end of turn but cannot be prevented or dispelled unless specified.";

            // Custom Text for Thorns
            medsTexts[perkStem + "thorns1d"] = "Cannot be purged unless specified.";
            // medsTexts[perkStem + "thorns1d"] = "Thorns on heroes can stack, but loses all charges at the end of turn.";
            medsTexts[perkStem + "thorns1e"] = "When a monster with Thorns dies, transfer their Thorns charges to a random hero.";
            medsTexts[perkStem + "thorns1f"] = "Bless on heroes increases Thorns damage by 5% per Bless charge.";

            // Custom Text for Reinforce
            medsTexts[perkStem + "reinforce1d"] = "Reinforce on this hero increases Block charges received by 1 per charge of Reinforce.";

            // Custom Text for Block
            medsTexts[perkStem + "block5b"] = "If Restricted Power is enabled, increases Max Charges to 600.";
            medsTexts[perkStem + "block5c"] = "At start of combat, apply 2 Block to all heroes.";
            medsTexts[perkStem + "block5d"] = "When this hero gains Block, they deal 1 Blunt to themselves and a random monster.";
            //medsTexts[perkStem + "block5e"] = "Block only functions if you are above 50% Max Health [Currently not working].";

            // Custom Text for Taunt
            medsTexts[perkStem + "taunt1e"] = "Taunt on this hero can stack and increases damage by 1 per charge.";

            // Custom Text for Fortify
            medsTexts[perkStem + "fortify1d"] = "At the end of this hero's turn, gain 1 Reinforce for every 2 Fortify charges.";
            medsTexts[perkStem + "fortify1e"] = "Fortify on all heroes can stack up to 5, but reduces damage taken by 1 per charge.";

            // Custom Text for Sharp
            medsTexts[perkStem + "sharp1e"] = "If Sharp on a hero would increase a damage type, it increases it by 1.5 damage per charge. Sharp on heroes only stacks to 25.";
            medsTexts[perkStem + "sharp1f"] = "For every 8 Sharp on this hero, increase Bleed charges applied by 1. Sharp on this hero does not increase damage.";

            // Custom Text for Fury
            medsTexts[perkStem + "fury1d"] = "For all heroes, at the end of turn, spread 70% of Fury to adjacent heroes. Fury on heroes loses all charges at end of turn.";

            // Custom Text for Crack
            medsTexts[perkStem + "crack2d"] = "Crack on monsters reduces Speed by 1 for every 5 charges.";
            medsTexts[perkStem + "crack2e"] = "Crack on monsters reduces Lightning resist by 0.3% per charge.";
            medsTexts[perkStem + "crack2f"] = "Crack on monsters increases Fire damage received by 1 per charge in addition to Blunt damage.[Not compatible with the Mind version.]";
            medsTexts[perkStem + "crack2g"] = "Crack on monsters increases Mind damage received by 1 per charge in addition to Blunt damage. [Overrides the Burn version.]";
            medsTexts[perkStem + "crack2h"] = "Crack on monsters reduces Slashing resist by 0.3% per charge.";
            medsTexts[perkStem + "crack2i"] = "Crack on monsters increases max Vulnerable charges by 1 per 25 charges of Crack.";

            // Custom Text for Burn
            medsTexts[perkStem + "burn2e"] = "Burn increases the damage dealt by Dark explosions by 0.5% per charge.";
            medsTexts["burn2d"] = "Burn deals double damage on enemies with 3 or less curses (burn included).";


            // Custom Text for Chill
            medsTexts[perkStem + "chill2e"] = "Chill on monsters reduces Cold and Mind resistance by 0.5% per charge.";
            medsTexts[perkStem + "chill2f"] = "At the start of your turn, suffer 3 Chill. Chill on this hero reduces Speed by 1 for every 10 charges";
            medsTexts[perkStem + "chill2g"] = "Chill on this hero reduces Speed by 1 for every 3 charges but does not reduce Cold resistance.";


            // Custom Text for Sparks
            medsTexts[perkStem + "spark2d"] = "Spark on this hero increases Lighting damage by 0.2 per charge.";
            medsTexts[perkStem + "spark2e"] = "Spark deal Fire damage. Spark decreases Fire resistance by 0.5% per charge and Lightning resistance by 0.5% per charge.";
            medsTexts[perkStem + "spark2f"] = "When you hit an enemy with Sparks, deal Lightning damage equal to 20% of their Sparks to their sides.";
            medsTexts[perkStem + "spark2g"] = "When this hero applies Spark, apply 1 Crack.";
            medsTexts["spark2c"] = "Spark also applies 1 Slow per 14 charges of Spark at end of each round and turn.";


            // Custom Text for Insulate
            medsTexts[perkStem + "insulate1d"] = "Insulate on this hero prevents their Speed from being lowered by Chill.";
            medsTexts[perkStem + "insulate1e"] = "Insulate on this hero increases Elemental damage by 10% per stack, but only increases Elemental resistances by 15%. Insulate on this hero stacks to 15";

            // Custom Text for Shield
            // medsTexts[perkStem + "shield5a"] = "+ Charges";
            medsTexts[perkStem + "shield5b"] = "If Restricted Power is enabled, increases Max Charges to 300.";
            medsTexts[perkStem + "shield5c"] = "At start of combat, apply 4 Shield to all heroes.";

            // Custom Text for Wet
            medsTexts[perkStem + "wet1d"] = "Wet does not Dispel or Prevent Burn.";

            // Custom Text for Inspire
            medsTexts[perkStem + "inspire0d"] = "If this hero ends their turn with 4 or more cards, gain 1 Inspire";

            // Custom Text for Energize
            medsTexts[perkStem + "energize1a"] = "At start of your first turn, gain 1 Energize.";
            medsTexts[perkStem + "energize1b"] = "Energize gives 2 energy per charge, but you can only have a maximum of 1 Energize.";
            medsTexts[perkStem + "energize1c"] = "Energize increases all damage 1 per charge.";
            medsTexts[perkStem + "energize1d"] = "If you end your turn with 4 or more energy, gain 1 Energize.";


            // Custom Text for Spellsword
            medsTexts[perkStem + "spellsword1a"] = "Max stacks +2";
            medsTexts[perkStem + "spellsword1b"] = "Spellsword on heroes reduces incoming damage by 2, but does not increase damage";
            medsTexts[perkStem + "spellsword1c"] = "At the start of your turn, all heroes and monsters gain 1 Spellsword";
            medsTexts[perkStem + "spellsword1d"] = "When this hero cast a Spell or Attack that costs 4 or more, gain 1 Spellsword";

            // Custom Text for Powerful
            medsTexts[perkStem + "powerful1d"] = "If this hero gains Powerful when it is at max charges, gain 1 Vitality.";

            // Custom Text for Paralyze
            // medsTexts[perkStem + "paralyze1a"] = "+1 Charge.";
            medsTexts[perkStem + "paralyze1b"] = "At the end of your turn, dispel Paralyze from all heroes.";
            medsTexts[perkStem + "paralyze1c"] = "Once per enemy per combat, when an enemy reaches 100 Spark, apply 1 Paralyze.";


            // Custom Text for Rust
            // medsTexts[perkStem + "rust1a"] = "+1 Charge.";
            medsTexts[perkStem + "rust1b"] = "Rather than decreasing the effectiveness of Wet, Rust increases the effectiveness of Wet by 50%.";
            medsTexts[perkStem + "rust1c"] = "Rather than increasing Poison Damage by 50%, Rust increases Poison Damage by 10% per stack (up to a max of 200%). Only affects Poison Damage.";
            medsTexts[perkStem + "rust1d"] = "Rust on this hero does not Prevent or Dispel Reinforce. At the start of your turn, suffer 2 Rust.";


            // Custom text for Zeal        
            medsTexts[perkStem + "zeal1a"] = "Zeal +1.";
            medsTexts[perkStem + "zeal1b"] = "Zeal on this hero loses 3 charges per turn rather than all charges.";
            medsTexts[perkStem + "zeal1c"] = "Zeal on all heroes can stack up to " + ZealCap.Value + ", but reduces Speed and all Damage by 2 per charge.";
            medsTexts[perkStem + "zeal1d"] = "Zeal on heroes and monsters increases all resistances by 0.5% per Wet charge.";
            medsTexts[perkStem + "zeal1e"] = "When this hero loses Zeal at end of turn, deal indirect Holy and Fire damage to all monsters equal to 4x the number of charges lost.";
            medsTexts[perkStem + "zeal1f"] = "Zeal on this hero can stack, but only increases Fire resistance by 2.5% per charge. At the end of turn, suffer 5 Burn per charge.";
            // medsTexts[perkStem + "zeal1f"] = "If this hero dies with Zeal, deal indirect Fire damage equal to 5x their insane to every monster.";

            // Custom text for Scourge
            medsTexts[perkStem + "scourge1a"] = "Scourge +1.";
            medsTexts[perkStem + "scourge1b"] = "Scourge on heroes and monsters loses 3 charges per turn rather than all charges."; // TODO: Replace with -1/dark explosion
            medsTexts[perkStem + "scourge1c"] = "Scourge on monsters can stack but increases all resists by 3% per charge.";
            medsTexts[perkStem + "scourge1d"] = "Instead of Chill, Scourge deals 2 Shadow damage per Sight charge.";
            medsTexts[perkStem + "scourge1e"] = "Scourge on monsters increases burn damage by 15%/charge";

            // Custom text for Weak
            medsTexts[perkStem + "weak1a"] = "Weak +1.";
            //medsTexts[perkStem + "weak1b"] = "Weak on heroes and monsters can stack but reduces damages by 15% per charge. Maximum damage reduction is 70%";
            medsTexts[perkStem + "weak1b"] = "Weak on monsters reduces the application of Auras and Curses by 20%.";
            medsTexts[perkStem + "weak1c"] = "Weak cannot be prevented by Immunity or Buffer, but reduces damage and healing by 25% instead of 50%.";
            medsTexts[perkStem + "weak1d"] = "This hero is immune to Weak.";

            // Custom Text for Healing
            medsTexts[perkStem + "heal5b"] = "When this hero heals a character at Max HP, apply 2 Powerful. [Powerful application cannot be increased by modifiers]";
            medsTexts[perkStem + "heal5c"] = "+35% Heal received.";

            // Custom Text for Insane
            medsTexts[perkStem + "insane2d"] = "Crack on monsters increases Blunt damage by an additional 1 for every 40 charges of Insane on that monster.";
            medsTexts[perkStem + "insane2e"] = "Insane on this hero increases the effectiveness of Sharp by 1% per charge.";
            medsTexts[perkStem + "insane2f"] = "At the start of their turn, heroes and monsters gain 1 Scourge for every 30 Insane charges on them.";

            // Custom Text for Dark
            medsTexts[perkStem + "dark2e"] = "Dark explosions deal Fire damage. Dark reduces Fire resistance by 0.5% per charge in addition to reducing Shadow resistance.";
            medsTexts["dark2b"] = "Dark on this hero explodes at 38 charges and cannot be dispelled unless specified.";


            // Custom Text for Sanctify
            medsTexts[perkStem + "sanctify2d"] = "Every 5 stacks of Sanctify increase the number of Dark charges needed for an explosion by 1.";
            medsTexts[perkStem + "sanctify2e"] = "At start of their turn, heroes gain 1 Zeal for every 20 Sanctify charges on them.";

            // Custom Text for Decay
            medsTexts[perkStem + "decay1d"] = "Decay purges Reinforce on heroes and monsters.";
            medsTexts[perkStem + "decay1e"] = "Every stack of decay increases the damage dealt by poison by 20%.";

            // Custom Text for Courage
            medsTexts[perkStem + "courage1d"] = "Courage increases Shield gained by this hero by 1 per charge.";

            // Custom Text for Vitality
            medsTexts[perkStem + "vitality1d"] = "Vitality on this hero dispels Poison.";

            // Custom Text for Bless
            medsTexts[perkStem + "bless1d"] = "Bless on all heroes increases Slashing, Fire, and Holy damage by 3% per charge but does not increase damage by 1.";

            // Custom Text for Stanza
            medsTexts[perkStem + "stanza0d"] = "On their first turn, this hero gains Stanza I. ";
            medsTexts[perkStem + "stanza0e"] = "This hero has Stanza II for their first turn. This hero cannot gain Stanza I or Stanza III.";

            // Custom Text for Regeneration
            medsTexts[perkStem + "regeneration1d"] = "Regeneration on this hero prevents 1 Vulnerable per charge";

            medsTexts["binbintestasdf"] = "asdfqwerafsdb";

            // apply patches
            harmony.PatchAll();
        }

        internal static void LogDebug(string msg)
        {
            if (EnableDebugging.Value)
            {
                Log.LogDebug(debugBase + msg);
            }
        }
        internal static void LogInfo(string msg)
        {
            if (EnableDebugging.Value)
            {
                Log.LogInfo(debugBase + msg);
            }
        }
        internal static void LogError(string msg)
        {
            if (EnableDebugging.Value)
            {
                Log.LogError(debugBase + msg);
            }
        }

    }
}