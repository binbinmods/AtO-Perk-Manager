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
            string perkStem = "custom_binbin_mainperk_";

            // register with Obeliskial Essentials
            RegisterMod(
                _name: PluginInfo.PLUGIN_NAME,
                _author: "binbin",
                _description: "Custom Perks",
                _version: PluginInfo.PLUGIN_VERSION,
                _date: ModDate,
                _link: @"https://github.com/binbinmods/Perks",
                _contentFolder: "Too Many Perks - Beta",
                _type: ["content", "perk", "perkNode","sprite"]  
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
            medsTexts[perkStem + "health6d"] = "At the start of your turn, if you are at max HP, gain 2 Vitality.";

            // Custom Text for Resistances
            // medsTexts[perkStem + "resistance5a"] = "+8% Resistances.";
            medsTexts[perkStem + "resistance5b"] = "-4% Resistances. Gain 4% to all Resistances on level up";
            medsTexts[perkStem + "resistance5c"] = "+12% Resistances. Lose 4% to all Resistances on level up";
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

            // Custom Text for Shackle
            medsTexts[perkStem + "shackle1a"] = "+1 Charge applied";
            medsTexts[perkStem + "shackle1b"] = "This hero is immune to Shackle.";
            medsTexts[perkStem + "shackle1c"] = "Shackle cannot be prevented by Immunity or Buffer.";
            medsTexts[perkStem + "shackle1d"] = "At start of your turn, gain Fortify equal to your twice your Shackles.";
            medsTexts[perkStem + "shackle1e"] = "Shackle on this hero increases Dark charges applied by 1 per charge of Shackle.";
            medsTexts[perkStem + "shackle1f"] = "Shackle on monsters increases all damage received by 0.5 per base Speed per charge of Shackle.";

            // Custom Text for Mitigate
            medsTexts[perkStem + "mitigate1a"] = "At the start of your turn, gain 2 Mitigate, but only stacks to 5.";
            medsTexts[perkStem + "mitigate1b"] = "Mitigate on this hero does not lose charges at start of turn and stacks to 12.";
            medsTexts[perkStem + "mitigate1c"] = "At the start of your turn, gain 7 Block per Mitigate charge.";
            medsTexts[perkStem + "mitigate1d"] = "Mitigate on this hero reduces incoming damage by 2 per charge, but loses all charges at the start of your turn.";
            medsTexts[perkStem + "mitigate1e"] = "Mitigate on heroes and monsters increases damage done by 10% per charge.";

            // Custom Text for Poison
            medsTexts[perkStem + "poison2d"] = "If Restricted Power is enabled, increases Max Charges to 300.";
            medsTexts[perkStem + "poison2e"] = "Poison on heroes and monsters reduces Slashing resistance by 0.25% per charge.";
            medsTexts[perkStem + "poison2f"] = "Poison on monsters reduces all resistances by 5% for every 100 charges.";
            medsTexts[perkStem + "poison2g"] = "Once per turn, when a monster with Poison dies, transfer 50% of their Poison charges to a random monster.";
            medsTexts[perkStem + "poison2h"] = "-1 Poison. When this hero applies Poison, deal indirect Mind damage to the target equal to 30% of their Poison charges.";


            // Custom Text for Bleed
            medsTexts[perkStem + "bleed2d"] = "If Restricted Power is enabled, increases Max Charges to 300.";
            medsTexts[perkStem + "bleed2e"] = "When this hero hits an enemy with Bleed, they heal for 25% of the target's Bleed charges.";
            medsTexts[perkStem + "bleed2f"] = "Bleed on monsters reduces Piercing resist by 0.25% per charge.";
            medsTexts[perkStem + "bleed2g"] = "When a monster dies with Bleed, all monsters lose HP equal to 25% of the killed target's Bleed charges.";
            
            // Custom Text for Thorns
            medsTexts[perkStem + "thorns1d"] = "Cannot be purged unless specified.";
            medsTexts[perkStem + "thorns1e"] = "When a monster with Thorns dies, transfer their Thorns charges to a random hero.";

            // Custom Text for Reinforce
            medsTexts[perkStem + "reinforce1d"] = "Reinforce on this hero increases Block charges received by 1 per charge of Reinforce.";

            // Custom Text for Block
            medsTexts[perkStem + "block5b"] = "If Restricted Power is enabled, increases Max Charges to 600.";
            medsTexts[perkStem + "block5c"] = "At start of combat, apply 2 Block to all heroes.";
            medsTexts[perkStem + "block5d"] = "When this hero gains Block, they deal 1 Blunt to themselves and a random monster. [BROKEN]   ";
            //medsTexts[perkStem + "block5e"] = "Block only functions if you are above 50% Max Health [Currently not working].";

            // Custom Text for Taunt
            medsTexts[perkStem + "taunt1e"] = "Taunt on this hero can stack and increases damage by 1 per charge.";
            
            // Custom Text for Fortify
            medsTexts[perkStem + "fortify1d"] = "At the end of this hero's turn, gain 1 Reinforce for every 2 Fortify charges.";

            // Custom Text for Sharp
            medsTexts[perkStem + "sharp1e"] = "If Sharp would increase a damage type, it increases it by 1.5 damage per charge. Sharp on heroes only stacks to 25.";

            // Custom Text for Fury
            medsTexts[perkStem + "fury1d"] = "For all heroes, at the end of turn, spread 70% of Fury to adjacent heroes. Fury ";
            
            // Custom Text for Burn
            medsTexts[perkStem + "burn2e"] = "Burn increases the damage dealt by Dark explosions by 0.5% per charge.";
            
            // Custom Text for Chill
            medsTexts[perkStem + "chill2e"] = "Chill on monsters reduces Cold and Mind resistance by 0.5% per charge.";
            medsTexts[perkStem + "chill2f"] = "At the start of your turn, suffer 3 Chill. Chill on this hero reduces Speed by 1 for every 10 charges";
            medsTexts[perkStem + "chill2g"] = "Chill on this hero reduces Speed by 1 for every 3 charges but does not reduce Cold resistance.";
            
            // Custom Text for Sparks
            medsTexts[perkStem + "spark2d"] = "Spark on this hero increases Lighting damage by 0.2 per charge.";
            medsTexts[perkStem + "spark2e"] = "Spark deal Fire damage. Spark decreases Fire resistance by 0.5% per charge and Lightning resistance by 0.5% per charge.";
            medsTexts[perkStem + "spark2f"] = "When you hit an enemy with Sparks, deal Lightning damage equal to 20% of their Sparks to their sides.";
            
            // Custom Text for Insulate
            medsTexts[perkStem + "insulate1d"] = "Insulate on this hero prevents their Speed from being lowered by Chill.";

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
            
            // Custom text for Zeal        
            medsTexts[perkStem + "zeal1a"] = "Zeal +1.";
            medsTexts[perkStem + "zeal1b"] = "Zeal on this hero loses 3 charges per turn rather than all charges.";
            medsTexts[perkStem + "zeal1c"] = "Zeal on all heroes can stack, but reduces Speed by 2 per charge.";
            medsTexts[perkStem + "zeal1d"] = "Zeal on heroes and monsters increases all resistances by 0.5% per Wet charge.";
            medsTexts[perkStem + "zeal1e"] = "When this hero loses Zeal at end of turn, deal indirect Holy and Fire damage to all monsters equal to 4x the number of charges lost.";
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
            medsTexts[perkStem + "heal5b"] = "When this hero heals a character at Max HP, apply 1 Powerful.";
            medsTexts[perkStem + "heal5c"] = "+35% Heal received.";

            // Custom Text for Insane
            medsTexts[perkStem + "insane2d"] = "Crack on monsters increases Blunt damage by an additional 1 for every 40 charges of Insane on that monster.";
            medsTexts[perkStem + "insane2e"] = "Insane on this hero increases the effectiveness of sharp by 1% per charge.";
            medsTexts[perkStem + "insane2f"] = "At the start of their turn, heroes and monsters gain 1 Scourge for every 30 Insane charges on them.";
            
            // Custom Text for Dark
            medsTexts[perkStem + "dark2e"] = "Dark explosions deal Fire damage. Dark reduces Fire resistance by 0.5% per charge in addition to reducing Shadow resistance..";

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
            medsTexts[perkStem + "stanza0e"] = "On their first turn, this hero gains Stanza II. This hero cannot gain Stanza I or Stanza III";

            // Custom Text for Regeneration
            medsTexts[perkStem + "regneration1d"] = "Regeneration on heroes prevents 1 Vulnerable per charge";

            // apply patches
            harmony.PatchAll();
        }

        

    }
}