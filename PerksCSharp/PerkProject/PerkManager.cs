using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using static Obeliskial_Essentials.Essentials;
using System;
using System.Linq;
using static UnityEngine.Mathf;
using static PerkManager.Plugin;
using static PerkManager.CustomFunctions;
using TMPro.Examples;
using System.Collections.Generic;
using UnityEngine.TextCore.LowLevel;
using System.Text.RegularExpressions;
using System.ComponentModel;
using UnityEngine;
using System.Diagnostics.Tracing;
using System.Dynamic;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;
using System.Data;
using static AuraCurseData;


namespace PerkManager
{
    [HarmonyPatch]
    public class PerkPatches
    {
        public static int[] paralyzeCounters = [0, 0, 0, 0];

        public static bool blockShieldFlag = false;

        public static int blockCount = 0;
        public static int shieldCount = 0;

        public static bool mark1dFlag = true;
        public static bool poison2gFlag = true;
        public static bool bleed2gFlag = true;
        public static bool thorns1eFlag = true;
        public static bool shackle1fFlag = false;
        public static bool insulate1dFlag = false;

        public static bool isDamagePreviewActive = false;

        public static bool isCalculateDamageActive = false;
        public static int infiniteProctection = 0;
        public static int infiniteProctectionPowerful = 0;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.BeginAdventure))]
        public static void BeginAdventurePostfix(ref AtOManager __instance)
        {
            // TODO
            // currency6c: Increases chance for Scarabs to spawn by 10%." - CANNOT BE DONE;
            // shards5c: Increases chance for cards to be corrupted by 2%." - Handled in RewrittenFunctions;
            // resistance5d: Maximum resistances for heroes and monsters are now 97%. - Rewritten Functions";

            // XP Perks
            // Hero[] teamHero = PlayerManager.Instance.GetTeamHero();
            Hero[] teamHero = Traverse.Create(__instance).Field("teamAtO").GetValue<Hero[]>();
            // List<Hero> heroes = PlayerManager.Instance.GetTeamHero
            for (int i = 0; i < teamHero.Length; i++)
            {
                Hero _hero = teamHero[i];
                HandleExpPerks(_hero);
            }

        }

        public static void HandleExpPerks(Hero _hero)
        {
            int expToGrant = 0;
            int expBase = 10;
            int expAdvanced = 40;
            if (CharacterObjectHavePerk(_hero, "exp0"))
                expToGrant += expBase;
            if (CharacterObjectHavePerk(_hero, "exp1"))
                expToGrant += expBase;
            if (CharacterObjectHavePerk(_hero, "exp2"))
                expToGrant += expBase;
            if (CharacterObjectHavePerk(_hero, "exp3"))
                expToGrant += expAdvanced;
            _hero.GrantExperience(expToGrant);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "HeroLevelUp")]
        public static void HeroLevelUpPrefix(ref AtOManager __instance, int heroIndex, string traitId)
        {
            // TODO:
            Hero[] teamAtO = Traverse.Create(__instance).Field("teamAtO").GetValue<Hero[]>();

            Hero hero = teamAtO[heroIndex];

            if (CharacterObjectHavePerk(hero, "health6b"))
            {
                int AmountToIncreasePerLevel = 12;
                teamAtO[heroIndex].ModifyMaxHP(AmountToIncreasePerLevel);
            }
            if (CharacterObjectHavePerk(hero, "health6c"))
            {
                int AmountToIncreasePerLevel = -14;
                teamAtO[heroIndex].ModifyMaxHP(AmountToIncreasePerLevel);
            }
            if (CharacterObjectHavePerk(hero, "shards5b"))
            {
                // shards5b: Gain 125 shards on level up."; - multiplayer?
                int AmountToIncreasePerLevel = 125;
                AtOManager.Instance.GivePlayer(1, AmountToIncreasePerLevel, anim: true);

            }
            if (CharacterObjectHavePerk(hero, "currency6b"))
            {
                // currency6b: Gain 125 gold on level up."; - NEEDS MULTIPLAYER?
                int AmountToIncreasePerLevel = 125;
                AtOManager.Instance.GivePlayer(0, AmountToIncreasePerLevel, anim: true);

            }


            if (CharacterObjectHavePerk(hero, "resistance5b"))
            {
                // resistance5b: -4% Resistances. Gain 4% to all Resistances on level up";
                LogDebug("Attempting to add resistances");
                int AmountToIncreasePerLevel = 4;
                string perkName = perkBase + "resistance5b";
                PerkData perk = Globals.Instance.GetPerkData(perkName);
                perk.ResistModified = Enums.DamageType.All;
                perk.ResistModifiedValue += AmountToIncreasePerLevel;

                Dictionary<string, PerkData> perkDictionary = Traverse.Create(Globals.Instance).Field("_PerksSource").GetValue<Dictionary<string, PerkData>>();
                if (perkDictionary.ContainsKey(perkName))
                {
                    LogDebug("Setting perk dictionary");
                    perkDictionary[perkName] = perk;
                }
                Traverse.Create(Globals.Instance).Field("_PerksSource").SetValue(perkDictionary);
            }

            if (CharacterObjectHavePerk(hero, "resistance5c"))
            {
                // resistance5c: +12% Resistances. Lose 4% to all Resistances on level up";            
                LogDebug("Attempting to add resistances");
                int AmountToIncreasePerLevel = -4;
                string perkName = perkBase + "resistance5b";
                PerkData perk = Globals.Instance.GetPerkData(perkName);
                perk.ResistModified = Enums.DamageType.All;
                perk.ResistModifiedValue += AmountToIncreasePerLevel;

                Dictionary<string, PerkData> perkDictionary = Traverse.Create(Globals.Instance).Field("_PerksSource").GetValue<Dictionary<string, PerkData>>();
                if (perkDictionary.ContainsKey(perkName))
                {
                    LogDebug("Setting perk dictionary");
                    perkDictionary[perkName] = perk;
                }
                Traverse.Create(Globals.Instance).Field("_PerksSource").SetValue(perkDictionary);
            }

            HandleExpPerks(hero);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), "GenerateHeroes")]
        public static void GenerateHeroesPostfix(ref MatchManager __instance)
        {
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            for (int index = 0; index < teamHero.Length; ++index)
            {
                if (teamHero[index] == null)
                    continue;

                Hero _hero = teamHero[index];
                // LogDebug(" PerkList for " + _hero.SubclassName + ": " + string.Join(",", _hero.PerkList.ToArray()));
                // if (CharacterObjectHavePerk(_hero, "mitigate1d"))
                //     LogDebug("Hero has mitigate1d");


                // LogDebug("Adding Immunities weak1d and more");

                if (CharacterObjectHavePerk(_hero, "weak1d"))
                {
                    // LogDebug("Adding Immunities, has weak1d");
                    AddImmunityToHero("weak", ref _hero);

                }
                if (CharacterObjectHavePerk(_hero, "disarm1a"))
                {
                    // LogDebug("Adding Immunities, has disarm1a");
                    AddImmunityToHero("disarm", ref _hero);
                }
                if (CharacterObjectHavePerk(_hero, "silence1a"))
                {
                    AddImmunityToHero("silence", ref _hero);

                }
                if (CharacterObjectHavePerk(_hero, "shackle1b"))
                {
                    AddImmunityToHero("shackle", ref _hero);
                }
                if (CharacterObjectHavePerk(_hero, "stanza0e"))
                {
                    AddImmunityToHero("stanzai", ref _hero);
                    AddImmunityToHero("stanzaiii", ref _hero);
                }
                if (CharacterObjectHavePerk(_hero, "energize1e"))
                {
                    AddImmunityToHero("fatigue", ref _hero);
                }

                if (CharacterObjectHavePerk(_hero, "vulnerable0d"))
                {
                    AddImmunityToHero("vulnerable", ref _hero);
                    AddImmunityToHero("reinforce", ref _hero);
                    AddImmunityToHero("insulate", ref _hero);
                    AddImmunityToHero("courage", ref _hero);
                }



            }
            Traverse.Create(MatchManager.Instance).Field("teamHero").SetValue(teamHero);

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), nameof(Globals.GetCostReroll))]
        public static void GetRerollCostPostfix(ref int __result)
        {
            // currency6d: Rerolling the shop costs 25% less.";

            if (TeamHasPerk("currency6d"))
            {
                __result = RoundToInt(0.75f * __result);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), nameof(Globals.GetDivinationCost))]
        public static void GetDivinationCost(ref Globals __instance, ref int __result)
        {
            // currency6e: Divinations cost 15% less.";

            if (TeamHasPerk("currency6e"))
            {
                __result = RoundToInt(0.85f * __result);
            }
            else
            {
                // return __result;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitHealReceivedPercentBonus))]
        public static void GetTraitHealReceivedPercentBonusPostfix(ref Character __instance, ref float __result)
        {
            if (CharacterObjectHavePerk(__instance, "heal5c") && __instance.IsHero && __instance.Alive && __instance != null)
            {
                __result += 35f;
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetEvent))]
        public static void SetEventPrefix(ref Character __instance,
                                            Enums.EventActivation theEvent,
                                            ref int auxInt,
                                            Character target = null,
                                            string auxString = "")
        {
            // Not sure why I made this a prefix

            // Not sure on this, but: Killed is "this character was killed" --> triggers things like resurrect
            // CharacterKilled is "a character was killed" --> triggers things like Yogger's Innate -- target is the character that was killed -- __instance might be the killer
            // For some reason CharacterKilled events trigger twice, no clue why

            // __instance is the "source" character, target is the target for AuraCurseSet

            // .Hitted has the __instance as the character that was hit and target as the caster

            // TODO:
            // //block5d: Block only functions if you are above 50% Max Health [Currently not working].";

            if (theEvent == Enums.EventActivation.CharacterAssign || theEvent == Enums.EventActivation.DestroyItem || __instance == null || theEvent == Enums.EventActivation.None)
            {
                return;
            }

            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();
            string eventString = Enum.GetName(typeof(Enums.EventActivation), theEvent);//((Enums.EventActivation)theEvent).ToString();
            // LogDebug("SETEVENTPREFIX - START - " + eventString);

            if (theEvent == Enums.EventActivation.BeginCombat)
            {
                infiniteProctection = 0;
                paralyzeCounters = [0, 0, 0, 0];
                mark1dFlag = true;
            }
            if (theEvent == Enums.EventActivation.AuraCurseRemoved && auxString == "burn" && TeamHasPerk("burn2f") && IsLivingNPC(__instance) && target != null && IsLivingHero(target))
            {
                // burn2f: When Burn is Dispelled from a Monster, they suffer Fire damage to the target equal to twice the Burn that was purged.";
                // LogDebug("burn2f");
                int n = target.GetAuraCharges("burn");
                int damageToDeal = 2 * n;
                __instance.IndirectDamage(Enums.DamageType.Fire, damageToDeal);
            }



            // if (theEvent == Enums.EventActivation.Killed && __instance.IsHero && __instance != null && CharacterObjectHavePerk(__instance, "zeal0f") && __instance.HasEffect("zeal"))
            // {
            //     // zeal0f: If this hero dies with Zeal, deal indirect Mind damage to all enemies equal to 5x their Burn/Insane stacks.

            //     LogDebug("zeal0f");
            //     LogDebug("Target Charges = " + target.GetAuraCharges("burn"));
            //     int n_stacks = __instance.GetAuraCharges("burn");
            //     LogDebug("Instance Charges = " + n_stacks);
            //     int damageToDeal = Functions.FuncRoundToInt(8 * n_stacks);
            //     __instance.IndirectDamage(Enums.DamageType.Mind, damageToDeal);
            // }


            if (theEvent == Enums.EventActivation.AuraCurseSet && IsLivingNPC(__instance) && TeamHasPerk("mark1d") && __instance.HasEffect("mark") && auxString == "mark")
            {
                if (__instance.GetAuraCharges("mark") >= 10 && mark1dFlag)
                {
                    // LogDebug("mark1d");
                    __instance.SetAura(__instance, GetAuraCurseData("taunt"), 2, useCharacterMods: false);
                    mark1dFlag = false;
                }
            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && IsLivingNPC(target) && IsLivingHero(__instance) && CharacterObjectHavePerk(__instance, "spark2g") && auxString == "spark")
            {
                // LogDebug("spark2g");
                target.SetAura(null, GetAuraCurseData("crack"), 2);
            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && auxString == "powerful" && IsLivingHero(__instance) && TeamHasPerk("powerful1d") && __instance.HasEffect("powerful"))
            {
                // If this hero gains Powerful when it is at max charges, gain 1 Vitality.

                AuraCurseData powerful = GetAuraCurseData("powerful");
                if ((__instance.GetAuraCharges("powerful") == powerful.MaxCharges || __instance.GetAuraCharges("powerful") == powerful.MaxCharges + 7) && infiniteProctectionPowerful < 10)
                {
                    // LogDebug("powerful1d");
                    __instance.SetAuraTrait(__instance, "vitality", 1);
                    infiniteProctectionPowerful++;
                }

            }
            if (theEvent == Enums.EventActivation.AuraCurseSet && auxString == "shackle" && IsLivingHero(__instance) && CharacterObjectHavePerk(__instance, "shackle1e") && __instance.HasEffect("shackle"))
            {
                // shackle1e: Shackle on this hero increases Dark charges you apply by 1 per charge of Shackle.";

                // LogDebug("shackle1e");
                int n_shackle = __instance.GetAuraCharges("shackle");
                __instance.ModifyAuraCurseQuantity("dark", n_shackle);
                // if (auraCurseModifiers.ContainsKey(traitData.AuracurseBonus1.Id))
                //     auraCurseModifiers[traitData.AuracurseBonus1.Id] += traitData.AuracurseBonusValue1;
                // else
                //     auraCurseModifiers[traitData.AuracurseBonus1.Id] = traitData.AuracurseBonusValue1;

            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && IsLivingHero(__instance) && IsLivingNPC(target) && CharacterObjectHavePerk(__instance, "poison2h") && auxString == "poison")
            {
                // poison2h: -1 Poison. When this hero applies poison, deal Mind damage to the target equal to 30% of their Poison charges.";
                // LogDebug("poison2h");

                int n = target.GetAuraCharges("poison");
                float multiplier = target.HasEffect("rust") ? 0.3f : 0.2f;
                int damageToDeal = RoundToInt(n * multiplier);
                target.IndirectDamage(Enums.DamageType.Mind, damageToDeal);
            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && IsLivingHero(target) && target.HasEffect("courage") && CharacterObjectHavePerk(target, "courage1d") && auxString == "shield")
            {
                // courage1d: Courage increases Shield gained by this hero by 1 per charge.";
                int n = target.GetAuraCharges("courage");
                // LogDebug("courage1d");
                // LogDebug("courage1d  n  before: " + n);

                // needs to run every other time this is called
                shieldCount++;
                if (shieldCount % 2 == 1 && shieldCount < 100)
                {
                    // LogDebug("courage1d");
                    target.SetAura(__instance, GetAuraCurseData("shield"), n, useCharacterMods: false);
                }
                // LogDebug("courage1d  AuxInt  after: " + auxInt);

            }
            if (theEvent == Enums.EventActivation.AuraCurseSet && IsLivingHero(target) && target.HasEffect("reinforce") && CharacterObjectHavePerk(target, "reinforce1d") && auxString == "block")
            {
                // reinforce1d: Reinforce on this hero increases Block received by 1 per charge of Reinforce.";
                int n = __instance.GetAuraCharges("reinforce");
                blockCount++;
                if (blockCount % 2 == 1 && blockCount < 100)
                {
                    // LogDebug("reinforce1d");
                    target.SetAura(__instance, GetAuraCurseData("block"), n, useCharacterMods: false);
                }
                // LogDebug("courage1d  AuxInt  after: " + auxInt);

            }
            if (theEvent == Enums.EventActivation.AuraCurseSet && IsLivingNPC(target) && TeamHasPerk("sight1e") && auxString == "sight")
            {
                // "sight1e: Once an enemy reaches 100 charges of Sight, Dispel Sight and Purge 3.";
                if (target.HasEffect("sight") && target.GetAuraCharges("sight") >= 100)
                {
                    // LogDebug("sight1e");
                    target.HealCursesName(singleCurse: "sight");
                    target.DispelAuras(3);
                }
            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && __instance != null && IsLivingNPC(target) && TeamHasPerk("paralyze1c") && auxString == "spark")
            {
                // paralyze1c: Once per enemy per combat, when an enemy reaches 100 Spark, apply 1 Paralyze.";
                int n = target.GetAuraCharges("spark");
                if (n >= 100 && paralyzeCounters[target.NPCIndex] <= 0)
                {
                    // LogDebug("paralyze1c");
                    AuraCurseData paralyze = GetAuraCurseData("paralyze");
                    paralyzeCounters[target.NPCIndex]++;
                    target.SetAura(__instance, paralyze, 1);
                }
            }
            if (theEvent == Enums.EventActivation.AuraCurseSet && auxString == "block" && IsLivingHero(target) && CharacterObjectHavePerk(target, "block5d"))
            {
                // block5e: When this hero gains Block, they deal 1 Blunt to themselves and a random monster.";
                // LogDebug("block5d");
                int damageToDeal = 1;
                Enums.DamageType damageType = Enums.DamageType.Blunt;
                int modifiedDamage = target.DamageWithCharacterBonus(damageToDeal, damageType, Enums.CardClass.None);
                Character targetCharacter = GetRandomCharacter(teamNpc);
                if (targetCharacter.Alive && targetCharacter != null && target.Alive && target != null)
                {
                    targetCharacter.IndirectDamage(damageType, modifiedDamage);
                    target.IndirectDamage(damageType, modifiedDamage);
                }
            }

            // rust0h: When you apply Rust to an enemy, Purge 1.
            if (theEvent == Enums.EventActivation.AuraCurseSet && auxString == "rust" && IsLivingNPC(__instance) && CharacterObjectHavePerk(target, "rust0h"))
            {
                // LogDebug("rust0h");
                __instance.DispelAuras(1);
            }
            // decay1g"] = "When you apply Decay, apply and suffer 1 Rust
            if (theEvent == Enums.EventActivation.AuraCurseSet && auxString == "decay" && IsLivingNPC(__instance) && IsLivingHero(target) && CharacterObjectHavePerk(target, "decay1g"))
            {
                // LogDebug("decay1g");
                target.SetAura(__instance, GetAuraCurseData("rust"), 1, useCharacterMods: true);
                target.SetAura(target, GetAuraCurseData("rust"), 1, useCharacterMods: true);
            }

            if (theEvent == Enums.EventActivation.BeginCombat && IsLivingHero(__instance) && CharacterObjectHavePerk(__instance, "block5c"))
            {
                // block5c: At start of combat, apply 2 Block to all heroes.";
                // LogDebug("block5c");
                bool allHeroes = true;
                bool allNpcs = false;
                ApplyAuraCurseTo("block", 2, allHeroes, allNpcs, false, false, ref __instance, ref teamHero, ref teamNpc, "", "");
            }
            if (theEvent == Enums.EventActivation.BeginCombat && IsLivingHero(__instance) && CharacterObjectHavePerk(__instance, "shield5c"))
            {
                // shield5c: At start of combat, apply 4 Shield to all heroes.";
                // LogDebug("shield5c");
                bool allHeroes = true;
                bool allNpcs = false;
                ApplyAuraCurseTo("shield", 4, allHeroes, allNpcs, false, false, ref __instance, ref teamHero, ref teamNpc, "", "");
            }
            // fortify1f: At the start of combat, apply 2 Fortify to all heroes. Fortify on all heroes has a maximum of 2.
            if (theEvent == Enums.EventActivation.BeginCombat && IsLivingHero(__instance) && CharacterObjectHavePerk(__instance, "fortify1f"))
            {
                // LogDebug("fortify1f");
                bool allHeroes = true;
                bool allNpcs = false;
                ApplyAuraCurseTo("fortify", 2, allHeroes, allNpcs, false, false, ref __instance, ref teamHero, ref teamNpc, "", "");
            }
            if (theEvent == Enums.EventActivation.BeginCombat && IsLivingHero(__instance) && CharacterObjectHavePerk(__instance, "rust0g"))
            {
                // rust0g:At the start of combat, apply 2 Rust to eveyone.";
                // LogDebug("rust0g");
                bool allHeroes = true;
                bool allNpcs = false;
                ApplyAuraCurseTo("rust", 2, allHeroes, allNpcs, false, false, ref __instance, ref teamHero, ref teamNpc, "", "");
            }

            if (theEvent == Enums.EventActivation.CharacterKilled && !target.IsHero && target != null && TeamHasPerk("poison2g") && target.HasEffect("poison"))
            {
                // poison2g: When a monster with Poison dies, transfer 50% of their Poison charges to a random monster.";
                // LogDebug("poison2g");
                if (poison2gFlag)
                {
                    poison2gFlag = false;
                    int n = target.GetAuraCharges("poison");
                    // LogDebug("poison2g - n charges: " + n);
                    int toApply = RoundToInt(n * 0.5f);
                    Character randomNPC = GetRandomCharacter(teamNpc);
                    // LogDebug("poison2g - random npc index: " + randomNPC.NPCIndex);
                    if (IsLivingNPC(randomNPC))
                        randomNPC.SetAura(__instance, GetAuraCurseData("poison"), toApply, useCharacterMods: false);

                }

            }
            if (theEvent == Enums.EventActivation.CharacterKilled && !target.IsHero && target != null && TeamHasPerk("thorns1e") && target.HasEffect("thorns"))
            {
                // thorns1e when a monster with Thorns dies, transfer their Thorns charges to a random hero.
                // LogDebug("thorns1e");
                if (thorns1eFlag)
                {
                    thorns1eFlag = false;
                    int n = target.GetAuraCharges("thorns");
                    int toApply = RoundToInt(n * 1.0f);
                    Character randomHero = GetRandomCharacter(teamHero);
                    if (IsLivingHero(randomHero))
                        randomHero.SetAura(__instance, GetAuraCurseData("thorns"), toApply, useCharacterMods: false);
                }
            }
            if (theEvent == Enums.EventActivation.CharacterKilled && !target.IsHero && target != null && TeamHasPerk("bleed2g") && target.HasEffect("bleed"))
            {
                // bleed2g: an enemy dies with Bleed, all monsters lose HP equal to 25% of the killed target's Bleed charges.";
                if (bleed2gFlag)
                {
                    bleed2gFlag = false;
                    // LogDebug("bleed2g");
                    int n = target.GetAuraCharges("bleed");
                    // LogDebug("bleed2g Bleed charges: " + n);
                    int toDeal = RoundToInt(n * 0.25f);
                    DealIndirectDamageToAllMonsters(Enums.DamageType.None, toDeal);

                }

            }

            if (theEvent == Enums.EventActivation.Hitted && IsLivingNPC(__instance) && IsLivingHero(target) && CharacterObjectHavePerk(target, "bleed2e"))
            {
                // bleed2e: When this hero hits an enemy with Bleed, they heal for 25% of the target's Bleed charges.";
                // LogDebug("bleed2e");
                int n = __instance.GetAuraCharges("bleed");
                int toHeal = RoundToInt(n * 0.25f);
                target.IndirectHeal(toHeal);
            }

            if (theEvent == Enums.EventActivation.Hitted && IsLivingNPC(__instance) && IsLivingHero(target) && CharacterObjectHavePerk(target, "spark2f"))
            {
                // spark2f: When you hit an enemy with Sparks, deal Lightning damage equal to 20% of their Sparks to their sides.";

                // LogDebug($"spark2f Hit by: {target.SourceName}");

                int n = __instance.GetAuraCharges("spark");
                int toDeal = RoundToInt(n * 0.2f);
                int npcIndex = __instance.NPCIndex;
                List<NPC> npcSides = MatchManager.Instance.GetNPCSides(npcIndex);
                for (int index = 0; index < npcSides.Count; ++index)
                {
                    if (npcSides[index].Alive && npcSides[index] != null)
                    {
                        // LogDebug($"spark2f dealing damage to: {npcSides[index].SourceName}");
                        npcSides[index].IndirectDamage(Enums.DamageType.Lightning, toDeal);
                    }
                }


            }
            if (theEvent == Enums.EventActivation.CastCard && IsLivingHero(__instance) && CharacterObjectHavePerk(__instance, "spellsword1d"))
            {
                // spellsword1d: When this hero cast a Spell or Attack that costs 4 or more, gain 1 Spellsword";
                CardData _castedCard = Traverse.Create(__instance).Field("cardCasted").GetValue<CardData>();

                // LogDebug("spellsword1d Card Types: " + string.Join(",", _castedCard.GetCardTypes().ToArray()));
                // LogDebug(debugBase+"spellsword1d IsSpell: "+_castedCard.HasCardType(Enums.CardType.Spell));

                if (_castedCard != null && MatchManager.Instance.energyJustWastedByHero >= 4 && (_castedCard.HasCardType(Enums.CardType.Attack) || _castedCard.HasCardType(Enums.CardType.Spell)))
                    __instance.SetAuraTrait(__instance, "spellsword", 1);
            }
            // energy2e: Energy Regeneration -1. When this hero plays a card that costs 3 or more Energy, refund 1 Energy.
            if (theEvent == Enums.EventActivation.CastCard && IsLivingHero(__instance) && CharacterObjectHavePerk(__instance, "energy2e"))
            {
                CardData _castedCard = Traverse.Create(__instance).Field("cardCasted").GetValue<CardData>();
                if (_castedCard != null && MatchManager.Instance.energyJustWastedByHero >= 4)
                    __instance.ModifyEnergy(1);
            }
            // LogDebug(debugBase+"somehow this breaks the game here?");
            // if (theEvent == Enums.EventActivation.CastCard && __instance.IsHero && __instance.Alive && __instance != null && CharacterObjectHavePerk(__instance, "heal5b"))
            // {
            //     LogDebug(debugBase+"somehow this breaks the game here 2?");
            //     LogDebug(debugBase+"powerful heal - instance: " + __instance.SourceName);
            //     LogDebug(debugBase+"powerful heal - target: " + target.SourceName);
            //     CardData _castedCard = Traverse.Create(__instance).Field("cardCasted").GetValue<CardData>();
            //     LogDebug(" HP remaining: " + target.GetHpLeftForMax());
            //     if (_castedCard == null)
            //     {

            //     }
            //     else
            //     {
            //         if (_castedCard != null && _castedCard.Heal > 0 && target.GetHpLeftForMax() <= 0)
            //         {
            //             LogDebug("heal5b");
            //             target.SetAuraTrait(__instance, "powerful", 1);

            //         }
            //     }
            // }

            // Scourge0i: Dark explosions deal 10% of their damage to the target's sides for each charge of Scourge.
            // if (theEvent == Enums.EventActivation && IsLivingNPC(__instance))
            // {
            //     LogDebug("scourge0f");
            //     float multiplier = 0.1f * __instance.GetAuraCharges("scourge");
            //     List<NPC> sides = MatchManager.Instance.GetNPCSides(__instance.NPCIndex);
            //     for (int index = 0; index < sides.Count; ++index)
            //     {
            //         if (sides[index].Alive && sides[index] != null && sides[index] != __instance)
            //         {
            //             int toDeal = RoundToInt(auxInt * multiplier);
            //             sides[index].IndirectDamage(Enums.DamageType.Shadow, toDeal);
            //         }
            //     }
            // }
            if (theEvent == Enums.EventActivation.Damaged && IsLivingHero(__instance) && CharacterObjectHavePerk(__instance, "taunt1g") && __instance.HasEffect("taunt"))
            {
                // taunt1g: Taunt on you is not lost at the start of turn, but 1 charge is consumed when you take damage
                // LogDebug("taunt1g");
                __instance.ConsumeEffectCharges("taunt", 1);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetEnergyTurn))]
        public static void GetEnergyTurnPostfix(Character __instance, ref int __result)
        {
            // energy2e: Energy Regeneration -1. 
            if (CharacterObjectHavePerk(__instance, "energy2e") && __instance.IsHero && __instance.Alive && __instance != null)
            {
                // LogDebug("energy2e");
                __result -= 1;
                if (__result < 0)
                    __result = 0;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.DamageReflected))]
        public static bool DamageReflectedPrefix(ref Character __instance, Hero theCasterHero, NPC theCasterNPC, int damageAmount = 0, int blockedAmount = 0)
        {
            // zeal0j: Zeal on heroes makes Thorns apply Burn rather than dealing damage

            if (__instance.IsHero && theCasterHero != null)
            {
                return true;
            }
            List<Aura> auraList = __instance.AuraList;
            for (int i = 0; i < auraList.Count; i++)
            {
                if (auraList[i] == null)
                {
                    continue;
                }
                AuraCurseData aCData = auraList[i].ACData;
                if (!(aCData != null) || aCData.DamageReflectedMultiplier <= 0 || __instance.GetAuraCharges(aCData.Id) < aCData.ChargesPreReqForDamageReflection)
                {
                    continue;
                }
                switch (aCData.DamageReflectedModifierType)
                {
                    case Enums.RefectedDamageModifierType.DamagePerAuraCharge:
                        {
                            if (aCData.Id == "thorns" && theCasterNPC != null && IfCharacterHas(__instance, CharacterHas.Perk, "zeal0j", AppliesTo.Heroes) && __instance.HasEffect("zeal"))
                            {
                                // LogDebug("zeal0j - applying burn instead of damage");
                                theCasterNPC.SetAura(__instance, Globals.Instance.GetAuraCurseData("burn"), Functions.FuncRoundToInt((float)auraList[i].AuraCharges * 0.5f));
                                if (aCData.DamageReflectedConsumeCharges > 0)
                                {
                                    __instance.ConsumeEffectCharges(aCData.Id, aCData.DamageReflectedConsumeCharges);
                                }
                                return false;
                            }
                        }
                        break;
                }
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.EndRound))]
        public static void EndRoundPostfix(ref Character __instance)
        {
            LogDebug("EndRoundPostfix");
            if (__instance == null)
            {
                return;
            }

            int auraCharges2 = __instance.GetAuraCharges("spark");
            if (AtOManager.Instance != null && AtOManager.Instance.TeamHavePerk("mainperkspark2c"))
            {
                __instance.SetAuraTrait((Character)null, "slow", Mathf.FloorToInt(0.0714285746f * (float)auraCharges2));
            }

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.DamageWithCharacterBonus))]
        public static void DamageWithCharacterBonusPostfix(
            ref Character __instance,
            ref int __result,
            int value,
            Enums.DamageType DT,
            Enums.CardClass CC,
            int energyCost = 0)
        {
            if (CC == Enums.CardClass.None || !IsLivingHero(__instance) || value == 0 || __result == 0 || DT == Enums.DamageType.None || AtOManager.Instance == null || MatchManager.Instance == null)
                return;
            if (!CharacterObjectHavePerk(__instance, "energy2d"))
                return;

            // LogDebug("Testing Energy Perk");

            float constantValue = 4.0f;
            constantValue += __instance.GetAuraCharges("bless") * 0.25f;
            if (DT == Enums.DamageType.Slashing || DT == Enums.DamageType.Piercing || (DT == Enums.DamageType.Shadow && AtOManager.Instance.CharacterHavePerk(__instance.SubclassName, "mainperkSharp1d")) || (DT == Enums.DamageType.Mind && AtOManager.Instance.TeamHaveTrait("shrilltone")))
                constantValue += __instance.GetAuraCharges("sharp") * 0.20f;
            if ((DT == Enums.DamageType.Fire || DT == Enums.DamageType.Blunt) && AtOManager.Instance.CharacterHavePerk(__instance.SubclassName, "mainperkfortify1a"))
                constantValue += __instance.GetAuraCharges("fortify") * 0.20f;

            Dictionary<int, float> multiplierDictionary = new()
            {
                {0, 0.4f},
                {1, 0.7f},
                {2, 1.0f},
                {3, 1.4f},
                {4, 1.8f},
                {5, 2.2f},
                {6, 2.7f},
                {7, 3.2f},
                {8, 3.9f},
                {9, 4.8f},
                {10, 6.0f},
            };
            int oldValue = __result;
            if (!multiplierDictionary.ContainsKey(energyCost))
                return;
            int newValue = RoundToInt((__result - constantValue * MathF.Sqrt(energyCost)) * multiplierDictionary[energyCost] + constantValue);

            if (energyCost < 2)
                __result = Math.Min(newValue, RoundToInt(oldValue * multiplierDictionary[energyCost]));
            else
                __result = Math.Max(newValue, oldValue);

            if (__result < 0)
                __result = 0;
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.SetAura))]
        public static void SetAuraPrefix(ref Character __instance,
                                            Character theCaster,
                                            AuraCurseData _acData,
                                            ref int charges,
                                            bool fromTrait = false,
                                            Enums.CardClass CC = Enums.CardClass.None,
                                            bool useCharacterMods = true,
                                            bool canBePreventable = true)
        {
            if (TeamHasPerk("weak1b") && __instance != null && __instance.Alive && IsLivingNPC(theCaster))
            {
                // LogDebug("weak1b");
                if (theCaster.HasEffect("weak"))
                    charges = Functions.FuncRoundToInt(0.8f * charges);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.BeginTurn))]
        public static void BeginTurnPrefix(ref Character __instance)
        {
            //shackle1d: At start of your turn, gain Fortify equal to your twice your Shackles
            // mitigate1a: At the start of your turn, gain 1 Mitigate, but only stack to 5. Does not charges at the start of your turn";
            // mitigate1c: At the start of your turn, gain 7 Block per Mitigate charge.";
            // health6c: At the start of your turn, if you are at max HP, gain 2 Vitality.";
            // chill2f: At the start of your turn, suffer 3 Chill. Chill on this hero reduces Speed by 1 for every 10 charges";
            // rust0f: Rust on this hero does not Prevent or Dispel Reinforce. At the start of your turn, suffer 2 Rust
            poison2gFlag = true;
            bleed2gFlag = true;
            thorns1eFlag = true;
            infiniteProctection = 0;
            infiniteProctectionPowerful = 0;

            if (!__instance.Alive || __instance == null || MatchManager.Instance == null)
                return;

            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNPC = MatchManager.Instance.GetTeamNPC();

            if (CharacterObjectHavePerk(__instance, "rust0f"))
            {
                int n_charges = 2;
                __instance.SetAuraTrait(__instance, "rust", n_charges);
            }

            if (CharacterObjectHavePerk(__instance, "shackle1d"))
            {
                int n_charges = __instance.GetAuraCharges("shackle");
                int multiplier = 2;
                __instance.SetAuraTrait(__instance, "fortify", n_charges * multiplier);
            }
            if (CharacterObjectHavePerk(__instance, "mitigate1a"))
            {
                int n_charges = 2;
                __instance.SetAuraTrait(__instance, "mitigate", n_charges);
            }
            if (CharacterObjectHavePerk(__instance, "mitigate1c"))
            {
                int n_charges = 7 * __instance.GetAuraCharges("mitigate");
                __instance.SetAuraTrait(__instance, "block", n_charges);
            }
            if (CharacterObjectHavePerk(__instance, "health6c"))
            {
                LogDebug("Health6c current: " + __instance.HpCurrent + " Max: " + __instance.GetMaxHP());
                if (__instance.GetMaxHP() <= __instance.HpCurrent || __instance.GetHpLeftForMax() <= 0)
                {
                    int n_charges = 2;
                    __instance.SetAuraTrait(__instance, "vitality", n_charges);
                }
            }
            if (CharacterObjectHavePerk(__instance, "chill2f"))
            {
                int n_charges = 3;
                __instance.SetAuraTrait(__instance, "chill", n_charges);

            }
            if (CharacterObjectHavePerk(__instance, "sight1d"))
            {//sight1d: At the start of your turn, gain 1 Evasion for every enemy with 100 or more Sight charges.
             // this is likely to break, but hopefully it works

                int n_charges = 0; //= teamNPC.Count(npc => npc.Alive && npc != null && npc.GetAuraCharges("sight") >= 100);
                foreach (NPC npc in teamNPC)
                {
                    if (IsLivingNPC(npc))
                    {
                        if (npc.GetAuraCharges("sight") >= 100)
                            n_charges++;
                    }

                }
                __instance.SetAuraTrait(__instance, "evasion", n_charges);

            }

            if (CharacterObjectHavePerk(__instance, "energize1a"))
            {
                // energize1a: At start of your first turn, gain 1 Energize.";
                if (MatchManager.Instance.GetCurrentRound() == 1)
                    __instance.SetAuraTrait(__instance, "energize", 1);
            }

            if (CharacterObjectHavePerk(__instance, "spellsword1c"))
            {
                // spellsword1c: At the start of your turn, all heroes and monsters gain 1 Spellsword";
                foreach (Hero hero in teamHero)
                {
                    if (IsLivingHero(hero))
                    {
                        if (hero.SubclassName == "queen" && hero.GetAuraCharges("spellsword") == 4)
                        {

                        }
                        else
                        {
                            hero.SetAuraTrait(__instance, "spellsword", 1);
                        }


                    }
                }
                foreach (NPC npc in teamNPC)
                {
                    if (IsLivingNPC(npc))
                        npc.SetAuraTrait(__instance, "spellsword", 1);
                }
            }

            // sanctify2e: At start of their turn, heroes gain 1 Zeal for every 20 Sanctify charges on them.";
            if (TeamHasPerk("sanctify2e") && __instance.IsHero && __instance.HasEffect("sanctify"))
            {
                int n_sanctify = __instance.GetAuraCharges("sanctify");
                int n_zeal = FloorToInt(n_sanctify * 0.05f);
                AuraCurseData zeal = GetAuraCurseData("zeal");
                __instance.SetAura(__instance, zeal, n_zeal, useCharacterMods: false);
            }
            if (TeamHasPerk("insane2f") && __instance.HasEffect("insane"))
            {
                int n_charges = FloorToInt(__instance.GetAuraCharges("insane") / 30);
                __instance.SetAuraTrait(__instance, "scourge", n_charges);
            }

            if (CharacterObjectHavePerk(__instance, "stealh1d") && __instance.IsHero && !__instance.HasEffect("stealth"))
            {
                __instance.SetAura(__instance, GetAuraCurseData("stealth"), 1, useCharacterMods: false);
            }
            if (CharacterObjectHavePerk(__instance, "stanza0d") && __instance.IsHero && MatchManager.Instance.GetCurrentRound() == 1)
            {
                __instance.SetAura(__instance, GetAuraCurseData("stanzai"), 1, useCharacterMods: false);
            }

            if (CharacterObjectHavePerk(__instance, "stanza0e") && __instance.IsHero && MatchManager.Instance.GetCurrentRound() == 1)
            {
                __instance.SetAura(__instance, GetAuraCurseData("stanzaii"), 1, useCharacterMods: false);
            }
            // if (CharacterObjectHavePerk(__instance, "mark1f") && __instance.IsHero && MatchManager.Instance.GetCurrentRound() == 2)
            // {
            //     __instance.AuraCurseModification["mark"] += 1;
            // }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitDamagePercentModifiers))]
        public static void GetTraitDamagePercentModifiersPostfix(ref Character __instance, ref float __result)
        {
            if (CharacterObjectHavePerk(__instance, "currency6c"))
            {
                // currency6c: For every 2,000 gold you have, gain +10% damage.";

                int currencyAmount = AtOManager.Instance.GetPlayerGold();
                int percentToIncrease = FloorToInt(10 * currencyAmount / 2000);
                __result += percentToIncrease;
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.EndTurn))]
        public static void MatchManagerEndTurnPrefix(MatchManager __instance)
        {
            // inspire0d: If this hero ends their turn with 4 or more cards, gain 1 Inspire";
            if (__instance == null)
                return;

            Character character = __instance.GetCharacterActive();

            if (!IsLivingHero(character))
                return;

            if (CharacterObjectHavePerk(character, "inspire0d"))
            {
                // LogDebug("Handsize at End of Turn: " + __instance.CountHeroHand());
                if (__instance.CountHeroHand() >= 4)
                {
                    character.SetAuraTrait(character, "inspire", 1);
                }
            }

            if (CharacterObjectHavePerk(character, "health6d") && character.GetHpLeftForMax() <= 0)
            {
                // LogDebug("health6d");
                character.SetAuraTrait(character, "vitality", 2);
            }

            if (CharacterObjectHavePerk(character, "leech0e"))
            {
                // Leech explodes at the end of turn
                // LogDebug("leech0e");
                AtOManager.Instance.DoLeachExplosion(character);
            }


            if (TeamHasPerk("fury1d") && character.HasEffect("fury"))
            {
                // LogDebug("fury1d");
                float fractionSpread = 0.7f;
                if ((UnityEngine.Object)character.HeroItem != (UnityEngine.Object)null)
                {
                    List<Hero> heroSides = MatchManager.Instance.GetHeroSides(character.Position);
                    for (int index = 0; index < heroSides.Count; ++index)
                        heroSides[index].SetAura((Character)null, Globals.Instance.GetAuraCurseData("fury"), Functions.FuncRoundToInt((float)character.GetAuraCharges("fury") * fractionSpread));
                }
                // __result.ConsumeAll = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPrefix()
        {
            isDamagePreviewActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.SetDamagePreview))]
        public static void SetDamagePreviewPostfix()
        {
            isDamagePreviewActive = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPrefix()
        {
            isCalculateDamageActive = true;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.CalculateDamagePrePostForThisCharacter))]
        public static void CalculateDamagePrePostForThisCharacterPostfix()
        {
            isCalculateDamageActive = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.IndirectDamage))]
        public static void IndirectDamagePostfix(
            ref Character __instance,
            Enums.DamageType damageType,
            ref int damage,
            AudioClip sound = null,
            string effect = "",
            string sourceCharacterName = "",
            string sourceCharacterId = "")
        {
            // Thorns1f: Bless increases thorns damage by 2% per charge.
            LogDebug("IndirectDamagePostfix");
            if (MatchManager.Instance == null)
            {
                return;
            }

            Character sourceCharacter = MatchManager.Instance.GetCharacterById(sourceCharacterId);

            if (TeamHasPerk("thorns1f") && IsLivingHero(sourceCharacter) && sourceCharacter.HasEffect("bless") && effect == "thorns")
            {
                int nRust = sourceCharacter.GetAuraCharges("rust");
                float multiplier = 1 + 0.05f * nRust;
                damage = RoundToInt(damage * multiplier);
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.HealReceivedFinal))]
        public static void HealReceivedFinalPostfix(Character __instance, int __result, int heal, bool isIndirect = false)
        {
            if (infiniteProctection > 100)
                return;
            if (isDamagePreviewActive || isCalculateDamageActive)
                return;
            if (MatchManager.Instance == null)
                return;
            if (!IsLivingHero(__instance) || MatchManager.Instance.GetHeroHeroActive() == null)
                return;

            infiniteProctection++;

            // MatchManager.Instance.cast
            Hero activeHero = MatchManager.Instance.GetHeroHeroActive();
            // LogDebug("Inf " + infiniteProctection);
            // LogDebug("Active Hero: " + activeHero.SubclassName);
            // LogDebug("Targeted/Instanced Hero: " + __instance.SubclassName);
            if (__result >= 0 && __instance.GetHpLeftForMax() <= 0 && CharacterObjectHavePerk(activeHero, "heal5b") && IsLivingHero(__instance) && IsLivingHero(activeHero) && heal > 0 && !isIndirect)
            {
                __instance.SetAura(__instance, GetAuraCurseData("powerful"), 2, useCharacterMods: false);
            }
        }

        // public static void ModifyHpPrefix(Character __instance, int _hp, bool _includeInStats = true, bool _refreshHP = true)
        // {
        //     if (MatchManager.Instance==null)
        //         return;
        //     if (!IsLivingHero(__instance)||MatchManager.Instance.GetHeroHeroActive()==null)
        //         return;

        //     // MatchManager.Instance.cast
        //     Hero activeHero = MatchManager.Instance.GetHeroHeroActive();            
        //     LogDebug("Active Hero: " + activeHero.SubclassName);
        //     LogDebug("Instance Hero: " + __instance.SubclassName);
        //     if (_hp >= 0 &&__instance.GetHpLeftForMax()>=0 &&CharacterObjectHavePerk(activeHero,"heal5b")&&IsLivingHero(__instance)&&IsLivingHero(activeHero))
        //     {
        //         __instance.SetAura(__instance,GetAuraCurseData("powerful"),2,useCharacterMods:false);
        //     }
        // }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.EndTurn))]
        public static void CharacterEndTurnPrefix(Character __instance)
        {
            if (!__instance.Alive || __instance == null || MatchManager.Instance == null)
                return;

            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            // zeal0h: When this hero loses Zeal, deal indirect Holy and Fire damage equal to 4x the number of stacks lost to all monsters.
            if (CharacterHasPerkForConsume("zeal0h", __instance.IsHero, AtOManager.Instance, __instance))
            {
                LogDebug("zeal0h");
                int zealCharges = __instance.GetAuraCharges("zeal");
                AuraCurseData zealData = GetAuraCurseData("zeal");
                int chargesRemoved;
                if (zealData.ConsumeAll)
                {
                    chargesRemoved = zealCharges;
                }
                else
                {
                    chargesRemoved = zealData.AuraConsumed;
                }
                int damageToDeal = 4 * chargesRemoved;

                int damageToDealHoly = __instance.DamageWithCharacterBonus(damageToDeal, Enums.DamageType.Holy, Enums.CardClass.None);
                // int damageToDealFire = __instance.DamageWithCharacterBonus(damageToDeal, Enums.DamageType.Fire, Enums.CardClass.None);

                DealIndirectDamageToAllMonsters(Enums.DamageType.Holy, damageToDealHoly);
                // DealIndirectDamageToAllMonsters(Enums.DamageType.Fire, damageToDealFire);
            }

            // paralyze1b: At the end of your turn, dispel Paralyze from all heroes.";
            if (CharacterObjectHavePerk(__instance, "paralyze1b"))
            {
                AuraCurseData paralyze = GetAuraCurseData("paralyze");
                foreach (Hero hero in teamHero)
                {
                    if (hero.Alive && hero != null)
                        hero.HealAuraCurse(paralyze);
                }

            }

            // energize1b: If you end your turn with 4 or more energy, gain 1 Energize.";
            if (CharacterObjectHavePerk(__instance, "energize1d"))
            {
                if (__instance.EnergyCurrent >= 4)
                {
                    __instance.SetAuraTrait(__instance, "energize", 1);
                }
            }

            if (CharacterObjectHavePerk(__instance, "fortify1d"))
            {
                // foritfy1d: At end of turn, gain 1 Reinforce for every 2 Fortify you have?
                int nFortify = __instance.GetAuraCharges("fortify");
                int reinforceToApply = RoundToInt(nFortify * 0.5f);
                __instance.SetAuraTrait(__instance, "reinforce", reinforceToApply);
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GainEnergyTurn))]
        public static void GainEnergyTurn(ref Character __instance)
        {
            // energy2c: At the start of round 4, gain 1 Energy.";
            // energy2f: Gain an extra energy on rounds 3+
            if (MatchManager.Instance.GetCurrentRound() > 3 && IfCharacterHas(__instance, CharacterHas.Perk, "energy2f", AppliesTo.ThisHero))
            {
                LogDebug("Updated Energy2f perk");
                __instance.ModifyEnergy(1, true);
            }
            if (MatchManager.Instance.GetCurrentRound() != 4 || !AtOManager.Instance.CharacterHavePerk(__instance.SubclassName, "mainperkenergy2c"))
                return;
            LogDebug("Updated Energy2c perk");
            __instance.ModifyEnergy(1, true);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.GlobalAuraCurseModificationByTraitsAndItems))]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {

            // LogDebug("Executing GACM Modifications - General");


            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            Character notCharacterOfInterest = _type == "set" ? _characterCaster : _characterTarget;
            if (characterOfInterest == null) { return; }

            bool hasRust = false;
            if (characterOfInterest != null)
            {
                hasRust = characterOfInterest.HasEffect("rust");
            }

            switch (_acId)
            {


                // leech0d: Charges applied +1. Decrease healing done by Leech by 50%.
                // leech0e: Leech explodes at the end of turn.
                // leech0f: Increase curses applied by Leech by 100%. Leech no longer reduces enemy resistances.
                // leech0g: Rather than healing, when Leech explodes, it deals damage to all enemies equal to the target's Bleed.
                case "leech":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "leech0d", AppliesTo.Heroes))
                    {
                        // LogDebug("leech0d");
                        __result.HealPerChargeOnExplode *= 0.5f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "leech0g", AppliesTo.Heroes))
                    {
                        // LogDebug("leech0g");
                        __result.HealPerChargeOnExplode = 0;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "leech0d", AppliesTo.Heroes))
                    {
                        // LogDebug("leech0d");
                        __result.ACChargesPerStackChargeOnExplode *= 2;
                        __result.ResistModified = Enums.DamageType.None;
                        __result.ResistModifiedValue = 0;
                    }
                    break;

                // infuse0d: Charges applied +1. Infuse on all heroes loses 3 charges per turn.
                // infuse0e: Max. Infuse charges +4. Infuse no longer increases resistances.
                // infuse0f: Infuse on heroes increases damage by 0.5 per Reinforce/Insulate/Courage charge rather than 1/Infuse charge.
                // infuse0g: Infuse on heroes increases the effectiveness of Reinforce/Insulate/Courage by 15% per charge rather than 50%.
                // infuse0h: Infuse on this hero loses one less charge.

                case "infuse":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "infuse0d", AppliesTo.Heroes))
                    {
                        // LogDebug("infuse0d");
                        __result.AuraConsumed = 3;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "infuse0e", AppliesTo.Heroes))
                    {
                        // LogDebug("infuse0e");
                        __result.MaxCharges += 4;
                        __result.MaxMadnessCharges += 4;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "infuse0f", AppliesTo.Heroes))
                    {
                        // LogDebug("infuse0e");
                        __result.AuraDamageConditionalBonuses = [];
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "infuse0h", AppliesTo.ThisHero))
                    {
                        // LogDebug("infuse0h");
                        __result.AuraConsumed -= 1;
                    }
                    break;
                case "evasion":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "evasion0b", AppliesTo.Heroes))
                    {
                        // LogDebug("evasion0b");
                        __result.ConsumeAll = true;
                        __result.GainCharges = true;
                        __result.ConsumedAtTurnBegin = true;
                    }
                    break;
                case "mark":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mark1e", AppliesTo.Monsters))
                    {
                        // LogDebug("mark1e");
                        // mark1e: Every 2 mark charges increases piercing damage by 3.
                        __result.IncreasedDamageReceivedType = Enums.DamageType.Piercing;
                        // __result.IncreasedDirectDamageChargesMultiplierNeededForOne = 2;
                        __result.IncreasedDirectDamageReceivedPerStack = 1.5f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mark1g", AppliesTo.Global))
                    {
                        // LogDebug("mark1g");
                        // mark1g: Halfs the bonus damage from Mark. Mark decreases speed by 1 per charge.
                        __result.IncreasedDirectDamageReceivedPerStack *= 0.5f;
                        __result.CharacterStatModified = Enums.CharacterStat.Speed;
                        __result.CharacterStatModifiedValuePerStack = -1;
                    }
                    break;
                case "disarm":
                    //disarm1b - cannot be dispelled unless specified, increases resists by 10%
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "disarm1b", AppliesTo.ThisHero))
                    {
                        // LogDebug("disarm1b");
                        __result.Removable = false;
                        __result.ResistModified = Enums.DamageType.All;
                        __result.ResistModifiedValue = 10;
                    }
                    break;

                case "silence":
                    //silence1b - cannot be dispelled unless specified, increases damage by 7
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "silence1b", AppliesTo.ThisHero))
                    {
                        // LogDebug("silence1b");
                        __result.Removable = false;
                        __result.AuraDamageType = Enums.DamageType.All;
                        __result.AuraDamageIncreasedTotal = 7;
                    }
                    break;

                case "stealth":
                    //  Unused
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "stealth1d", AppliesTo.Heroes))
                    {
                        // LogDebug("stealth1d");
                        __result.AuraDamageIncreasedPercentPerStack = 0.0f;
                    }
                    break;

                case "fast":
                    // fast0b: Fast on this hero can stack, but loses all charges at the start of turn.";
                    // fast0c: Fast on this hero falls off at the end of turn.";

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "fast0b", AppliesTo.Heroes))
                    {
                        // LogDebug("fast0b");
                        __result.GainCharges = true;
                        __result.ConsumeAll = true;

                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "fast0c", AppliesTo.Heroes))
                    {
                        // LogDebug("fast0c");
                        __result.ConsumedAtTurn = true;
                        __result.ConsumedAtTurnBegin = false;

                    }
                    break;

                case "slow":
                    // slow0b: Slow on monsters can stack up to 10, but only reduces Speed by 1 per charge";
                    // slow0c: Slow on heroes can stack up to 10, but only reduces Speed by 1 per charge";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "slow0b", AppliesTo.Monsters))
                    {
                        // LogDebug("slow0b");
                        __result.GainCharges = true;
                        __result.MaxCharges = 10;
                        __result.MaxMadnessCharges = 10;
                        __result.CharacterStatModifiedValuePerStack = -1;
                        __result.ChargesAuxNeedForOne1 = hasRust ? 2 : 1;
                        __result.CharacterStatChargesMultiplierNeededForOne = hasRust ? 2 : 1;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "slow0c", AppliesTo.Monsters))
                    {
                        // LogDebug("slow0c");
                        __result.GainCharges = true;
                        __result.MaxCharges = 10;
                        __result.MaxMadnessCharges = 10;
                        __result.CharacterStatModifiedValuePerStack = -1;
                        __result.ChargesAuxNeedForOne1 = hasRust ? 2 : 1;
                        __result.CharacterStatChargesMultiplierNeededForOne = hasRust ? 2 : 1;
                    }
                    break;

                // fortify1e: Fortify on all heroes is capped at 5 but reduces damage done by 1 per charge.  
                // fortify1f: At the start of combat, apply 2 Fortify to all heroes. Fortify on all heroes has a maximum of 2.
                case "fortify":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "fortify1e", AppliesTo.Heroes))
                    {
                        // LogDebug("fortify1e");
                        __result.GainCharges = true;
                        __result.MaxCharges = __result.MaxMadnessCharges = 5;
                        __result = AtOManager.Instance.GlobalAuraCurseModifyDamage(__result, Enums.DamageType.All, 0, -1, 0);
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "fortify1f", AppliesTo.Heroes))
                    {
                        // LogDebug("fortify1e");
                        __result.MaxCharges = __result.MaxMadnessCharges = 2;
                    }
                    break;
                case "fury":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "fury1d", AppliesTo.Heroes))
                    {
                        // LogDebug("fury1d");
                        __result.ConsumeAll = true;
                    }
                    break;

                case "sharp":
                    // sharp1d: shadow damaage for all heroes
                    // sharp1e: If Sharp on a hero would increase a damage type, it increases it by 1.5 damage per charge. Sharp on heroes only stacks to 25.";
                    // insane2e: Insane on this hero increases the effectiveness of sharp by 1% per charge.";
                    // zeal0e: While this hero has Zeal, Sharp increases their Holy damage by 1 per charge.
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperksharp1d", AppliesTo.Heroes))
                    {
                        // LogDebug("sharp1d");
                        __result = AtOManager.Instance.GlobalAuraCurseModifyDamage(__result, Enums.DamageType.Shadow, 0, 1, 0);
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "zeal0e", AppliesTo.ThisHero) && characterOfInterest.HasEffect("zeal"))
                    {
                        __result = AtOManager.Instance.GlobalAuraCurseModifyDamage(__result, Enums.DamageType.Holy, 0, 1, 0);
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "sharp1g", AppliesTo.ThisHero))
                    {
                        // LogDebug("sharp1d");
                        float amountToModify = AtOManager.Instance.TeamHaveTrait("shrilltone") ? 1.5f : 1;
                        if (hasRust)
                            amountToModify *= 0.5f;
                        if (AtOManager.Instance.TeamHaveTrait("shrilltone"))
                        {
                            if (__result.AuraDamageType == Enums.DamageType.Mind)
                            {
                                __result.AuraDamageIncreasedPerStack = amountToModify;
                            }
                            if (__result.AuraDamageType2 == Enums.DamageType.Mind)
                            {
                                __result.AuraDamageIncreasedPerStack2 = amountToModify;
                            }
                            if (__result.AuraDamageType3 == Enums.DamageType.Mind)
                            {
                                __result.AuraDamageIncreasedPerStack3 = amountToModify;
                            }
                            if (__result.AuraDamageType4 == Enums.DamageType.Mind)
                            {
                                __result.AuraDamageIncreasedPerStack4 = amountToModify;
                            }
                        }
                        else
                        {
                            __result.AuraDamageType4 = Enums.DamageType.Mind;
                            __result.AuraDamageIncreasedPerStack4 = amountToModify;
                        }


                    }


                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "sharp1e", AppliesTo.Heroes))
                    {
                        // LogDebug("sharp1e");
                        __result = AtOManager.Instance.GlobalAuraCurseModifyDamage(__result, Enums.DamageType.Shadow, 0, 1, 0);
                        __result.MaxCharges = 25;
                        __result.MaxMadnessCharges = 25;
                        __result.AuraDamageIncreasedPerStack = hasRust ? 0.75f : 1.5f;
                        __result.AuraDamageIncreasedPerStack2 = hasRust ? 0.75f : 1.5f;
                        __result.AuraDamageIncreasedPerStack3 = hasRust ? 0.75f : 1.5f;
                        __result.AuraDamageIncreasedPerStack4 = hasRust ? 0.75f : 1.5f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "insane2e", AppliesTo.ThisHero))
                    {
                        // LogDebug("insane2e");
                        // Doesn't need rust to be applied to it since it is a multiplier
                        __result = AtOManager.Instance.GlobalAuraCurseModifyDamage(__result, Enums.DamageType.Shadow, 0, 1, 0);
                        int n = characterOfInterest.GetAuraCharges("insane");
                        __result.AuraDamageIncreasedPerStack *= 1 + 0.01f * n;
                        __result.AuraDamageIncreasedPerStack2 *= 1 + 0.01f * n;
                        __result.AuraDamageIncreasedPerStack3 *= 1 + 0.01f * n;
                        __result.AuraDamageIncreasedPerStack4 *= 1 + 0.01f * n;
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "sharp1f", AppliesTo.ThisHero))
                    {
                        // LogDebug("sharp1f");
                        __result.AuraDamageIncreasedPerStack = 0;
                        __result.AuraDamageIncreasedPerStack2 = 0;
                        __result.AuraDamageIncreasedPerStack3 = 0;
                        __result.AuraDamageIncreasedPerStack4 = 0;
                        __result = AtOManager.Instance.GlobalAuraCurseModifyDamage(__result, Enums.DamageType.Shadow, 0, 1, 0);
                    }
                    break;

                case "crack":
                    // insane2d: Crack on monsters increases Blunt damage by an addition 1 for every 50 charges of Insane on that monster.";
                    // crack2d: Crack on monsters reduces Speed by 1 for every 5 charges.";
                    // crack2e: Crack on monsters reduces Lightning resistance by 0.3% per charge.
                    // crack2f: Crack increases fire damage too
                    // crack2g: Crack increases mind damage too
                    // crack2h: Crack on monsters reduces Slashing resistance by 0.3% per charge.
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "insane2d", AppliesTo.Monsters))
                    {
                        // LogDebug("insane2d");
                        int n = characterOfInterest.GetAuraCharges("insane");
                        __result.IncreasedDirectDamageReceivedPerStack += hasRust ? FloorToInt(0.03f * n) : FloorToInt(0.02f * n);
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "crack2d", AppliesTo.Monsters))
                    {
                        // LogDebug("crack2d");
                        __result.CharacterStatModified = Enums.CharacterStat.Speed;
                        __result.CharacterStatModifiedValue = -1;
                        __result.CharacterStatChargesMultiplierNeededForOne = hasRust ? 10 : 5;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "crack2e", AppliesTo.Monsters))
                    {
                        // LogDebug("crack2e");
                        float amountToModify = hasRust ? -0.45f : -0.3f;
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Lightning, 0, amountToModify);
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "crack2f", AppliesTo.Monsters))
                    {
                        // LogDebug("crack2f");
                        __result.IncreasedDamageReceivedType2 = Enums.DamageType.Fire;
                        __result.IncreasedDirectDamageReceivedPerStack2 = hasRust ? 1.125f : 0.75f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "crack2g", AppliesTo.Global))
                    {
                        // LogDebug("crack2g");
                        __result.IncreasedDamageReceivedType2 = Enums.DamageType.Mind;
                        __result.IncreasedDirectDamageReceivedPerStack2 = hasRust ? 1.125f : 0.75f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "crack2h", AppliesTo.Global))
                    {
                        // LogDebug("crack2h");
                        float amountToModify = hasRust ? -0.45f : -0.3f;
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Slashing, 0, amountToModify);
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Piercing, 0, amountToModify);
                    }
                    break;

                case "shackle":
                    // shackle1b: This hero is immune to Shackle.";
                    // shackle1c: Shackle cannot be prevented.";
                    // shackle1d: At start of your turn, gain Fortify equal to your twice your Shackles.";
                    // shackle1e: Shackle increases Dark charges you apply by 1 per charge of Shackle.";
                    // shackle1f: Shackles on monsters increases all damage received by 1 per base Speed.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "shackle1f", AppliesTo.Monsters))
                    {
                        // LogDebug("shackle1f");
                        shackle1fFlag = !shackle1fFlag;
                        if (!shackle1fFlag)
                        {
                            break;
                        }
                        int baseSpeed = characterOfInterest.GetSpeed()[1];
                        __result.IncreasedDamageReceivedType = Enums.DamageType.All;
                        float multiplier = 1.0f;
                        // int n_shackle = Math.Max(1,characterOfInterest.GetAuraCharges("shackle"));
                        __result.IncreasedDirectDamageReceivedPerStack = RoundToInt(baseSpeed * multiplier);
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "shackle1c", AppliesTo.Global))
                    {
                        // LogDebug("shackle1c");
                        __result.Preventable = false;
                    }

                    break;

                case "mitigate":
                    // mitigate1a: At the start of your turn, gain 2 Mitigate, but only stack to 5.";
                    // mitigate1b: Mitigate on this hero does not lose charges at start of turn and stacks to 12.";
                    // mitigate1c: At the start of your turn, gain 7 Block per Mitigate charge.";
                    // mitigate1d: Mitigate reduces incoming damage by 2 per charge, but loses all charges at the start of your turn.";
                    // mitigate1e: Mitigate on heroes and monsters increases damage done by 10% per charge.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mitigate1a", AppliesTo.ThisHero))
                    {
                        // LogDebug("mitigate1a");
                        __result.MaxCharges = 5;
                        __result.MaxMadnessCharges = 5;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mitigate1b", AppliesTo.ThisHero))
                    {
                        // LogDebug("mitigate1b");
                        __result.ConsumedAtTurnBegin = false;
                        __result.MaxCharges = 12;
                        __result.MaxMadnessCharges = 12;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mitigate1d", AppliesTo.ThisHero))
                    {
                        // LogDebug("mitigate1d");
                        __result.ConsumeAll = true;
                        __result.IncreasedDirectDamageReceivedPerStack = -2;
                        __result.ChargesMultiplierDescription = 2;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mitigate1e", AppliesTo.Global))
                    {
                        // LogDebug("mitigate1e");
                        __result.AuraDamageType = Enums.DamageType.All;
                        __result.AuraDamageIncreasedPercentPerStack = 10;
                    }
                    break;

                case "poison":
                    // poison2d: If Restricted Power is enabled, increases Max Charges to 300.";
                    // poison2e: Poison on heroes and monsters reduces Slashing resistance by 0.25% per charge.";
                    // poison2f: Poison on monsters deals shadow damage
                    // poison2g: When a monster with Poison dies, transfer 50% of their Poison charges to a random monster.";
                    // poison2h: -1 Poison. When this hero applies poison, deal Mind damage to the target equal to 20% of their Poison charges.";
                    // decay1e: Every stack of decay increases the damage dealt by poison by 20%.";


                    // rust0c: "Rather than increasing Poison Damage by 50%, Rust increases Poison Damage by 10% per stack (up to a max of 200%). Only affects Poison Damage.";

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperkpoison2c", AppliesTo.Monsters))
                    {
                        // LogDebug("poison2c");
                        __result.ConsumedAtTurnBegin = true;
                        __result.ConsumedAtTurn = false;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "poison2d", AppliesTo.Global))
                    {
                        // LogDebug("poison2d");
                        __result.MaxMadnessCharges = hasRust ? 450 : 300;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "poison2e", AppliesTo.Global))
                    {
                        // LogDebug("poison2e");
                        __result.ResistModified3 = Enums.DamageType.Slashing;
                        __result.ResistModifiedPercentagePerStack3 = hasRust ? -0.3f : -0.2f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "poison2f", AppliesTo.Monsters))
                    {
                        // LogDebug("poison2f");
                        __result.DamageTypeWhenConsumed = Enums.DamageType.Shadow;

                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "decay1e", AppliesTo.Global))
                    {
                        // multiplier so no need for rust
                        // LogDebug("decay1e");
                        int n_decay = characterOfInterest.GetAuraCharges("decay");
                        float multiplier = 1 + 0.2f * n_decay;
                        __result.DamageWhenConsumedPerCharge *= multiplier;
                    }

                    // if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "rust0c", AppliesTo.Global))
                    // {
                    //     // LogDebug("rust0c");
                    //     float undoRust = 1.0f / 1.5f;
                    //     __result.DamageWhenConsumedPerCharge *= undoRust;
                    //     int nRust = characterOfInterest.GetAuraCharges("rust");
                    //     float newRustMultiplier = Min(1 + 0.1f * nRust, 3.0f); // caps at +200%
                    //     __result.DamageWhenConsumedPerCharge *= newRustMultiplier;
                    // }

                    break;

                case "bleed":
                    // bleed2b: If Restricted Power is enabled, increases Max Charges to 300.";
                    // bleed2c: Can no longer be dispelled unless specified
                    // bleed2d: If Restricted Power is enabled, increases Max Charges to 300.";
                    // bleed2e: When this hero hits an enemy with Bleed, they heal for 25% of the target's Bleed charges.";
                    // bleed2f: Bleed on monsters reduces Piercing resist by 0.25% per charge.";
                    // bleed2g: When this hero kills an enemy with Bleed, all monsters lose HP equal to 25% of the killed target's Bleed charges.";
                    // decay1f: Every stack of decay increases the damage dealt by Bleed by 20%.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperkbleed2b", AppliesTo.ThisHero))
                    {
                        // LogDebug("bleed2b");
                        __result.MaxCharges = 50;
                        __result.MaxMadnessCharges = 50;
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperkbleed2c", AppliesTo.Monsters))
                    {
                        // LogDebug("bleed2c");
                        __result.Removable = false;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "bleed2d", AppliesTo.Global))
                    {
                        // LogDebug("bleed2d");
                        __result.MaxMadnessCharges = 300;

                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "bleed2f", AppliesTo.Monsters))
                    {
                        // LogDebug("bleed2f");
                        __result.ResistModified3 = Enums.DamageType.Piercing;
                        __result.ResistModifiedPercentagePerStack3 = -0.25f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "decay1f", AppliesTo.Global))
                    {
                        // multiplier so no need for rust
                        // LogDebug("decay1e");
                        int n_decay = characterOfInterest.GetAuraCharges("decay");
                        float multiplier = 1 + 0.2f * n_decay;
                        __result.DamageWhenConsumedPerCharge *= multiplier;
                    }
                    break;

                case "thorns":
                    // thorns1d: Cannot be purged unless specified.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "thorns1d", AppliesTo.ThisHero))
                    {
                        // LogDebug("thorns1d");
                        __result.Removable = false;
                    }
                    break;

                case "reinforce":
                    // reinforce1b: Increased to 40%;
                    // reinforce1d: Reinforce increases Block charges by 2 per charge of Reinforce.";
                    // reinforce1e: Reinforce on this hero now increases Piercing, Lightning, and Mind Resistance
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperkreinforce1b", AppliesTo.Heroes))
                    {
                        // LogDebug("reinforce1b");
                        __result.ResistModifiedValue = 40;
                        __result.ResistModifiedValue2 = 40;
                        __result.ResistModifiedValue3 = 40;
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "infuse0f", AppliesTo.Heroes) && characterOfInterest.HasEffect("infuse"))
                    {
                        __result.AuraDamageType = Enums.DamageType.Slashing;
                        __result.AuraDamageType = Enums.DamageType.Piercing;
                        __result.AuraDamageType = Enums.DamageType.Blunt;
                        __result.AuraDamageIncreasedPerStack = __result.AuraDamageIncreasedPerStack2 = __result.AuraDamageIncreasedPerStack3 = 1;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "reinforce1e", AppliesTo.ThisHero))
                    {
                        __result.AuraDamageType = Enums.DamageType.Mind;
                        __result.AuraDamageType = Enums.DamageType.Piercing;
                        __result.AuraDamageType = Enums.DamageType.Lightning;
                    }

                    break;

                case "block":
                    // block5b: If Restricted Power is enabled, increases Max Charges to 600.";
                    // block5c: At start of combat, apply 2 Block to all heroes.";
                    // //block5d: Block only functions if you are above 50% Max Health [Currently not working].";
                    // block5e: When this hero gains Block, they deal 1 Blunt to themselves and a random monster.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "block5b", AppliesTo.Heroes))
                    {
                        // LogDebug("block5b");
                        __result.MaxMadnessCharges = 600;
                    }
                    break;
                case "vulnerable":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "crack2i", AppliesTo.Monsters))
                    {
                        // LogDebug("crack2i");
                        int toIncrease = FloorToInt(0.04f * characterOfInterest.GetAuraCharges("crack"));
                        __result.MaxCharges += toIncrease;
                        __result.MaxMadnessCharges += toIncrease;
                    }

                    break;
                case "taunt":
                    // taunt1e: Taunt on this hero can stack and increases damage by 1 per charge.";
                    // taunt1h: Taunt on monsters decreases All Resistances by 5% per charge.
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "taunt1h", AppliesTo.Monsters))
                    {
                        // LogDebug("taunt1e");
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.All, 0, -5f);
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "taunt1e", AppliesTo.ThisHero))
                    {
                        // LogDebug("taunt1e");
                        __result.GainCharges = true;
                        __result.AuraDamageType = Enums.DamageType.All;
                        __result.AuraDamageIncreasedPerStack = 1;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "taunt1g", AppliesTo.ThisHero))
                    {
                        // LogDebug("taunt1e");
                        __result.ConsumedAtTurn = false;
                    }
                    break;

                // rust0f: Rust on this hero does not Prevent or Dispel Reinforce. At the start of your turn, suffer 2 Rust.";
                // rust0e: Rust on enemies does not Prevent or Dispel Reinforce. Rust on enemies reduces Physical resistance by 5% per charge.
                case "rust":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "rust0f", AppliesTo.ThisHero))
                    {
                        // LogDebug("rust0f");
                        AuraCurseData noneAC = GetAuraCurseData("None");
                        __result.PreventedAuraCurse = noneAC;
                        __result.PreventedAuraCurseStackPerStack = 0;
                        __result.RemoveAuraCurse = noneAC;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "rust0e", AppliesTo.ThisHero))
                    {
                        // LogDebug("rust0f");
                        AuraCurseData noneAC = GetAuraCurseData("None");
                        __result.PreventedAuraCurse = noneAC;
                        __result.PreventedAuraCurseStackPerStack = 0;
                        __result.RemoveAuraCurse = noneAC;
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Slashing, 0, -5f);
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Piercing, 0, -5f);
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Blunt, 0, -5f);
                    }
                    break;
                // taunt1f: Taunt on heroes increases maximum Powerful by 1 per charge.
                // powerful1e: Powerful on this hero has no cap, but increases damage done by 2% per charge.
                case "powerful":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "taunt1f", AppliesTo.Heroes))
                    {
                        // LogDebug("taunt1f");
                        int nTaunt = characterOfInterest.GetAuraCharges("taunt");
                        __result.MaxCharges += nTaunt;
                        __result.MaxMadnessCharges += nTaunt;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "powerful1e", AppliesTo.ThisHero))
                    {
                        // LogDebug("powerful1e");
                        __result.AuraDamageIncreasedPercentPerStack = 2f;
                        __result.MaxCharges = __result.MaxMadnessCharges = -1;
                    }
                    break;
                // inspire0e: Inspire on this hero is lost at the end of turn and increases Holy and Mind damage by 0.5 per charge
                case "inspire":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "inspire0e", AppliesTo.ThisHero))
                    {
                        // LogDebug("taunt1f");
                        __result.AuraDamageType = Enums.DamageType.Holy;
                        __result.AuraDamageType2 = Enums.DamageType.Mind;
                        __result.AuraDamageIncreasedPerStack = 0.5f;
                        __result.AuraDamageIncreasedPerStack2 = 0.5f;
                    }
                    break;
                case "insulate":
                    // insulate1d: Insulate on this hero prevents their Speed from being lowered by Chill.  
                    // insulate1e: Insulate on this hero increases Elemental damage by 5% per stack, but only increases Elemental resistances by 15%. Insulate on this hero stacks to 8.
                    // insulate1f: Insulate on this hero now increases Blunt, Chill, and Shadow Resistance

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "infuse0f", AppliesTo.Heroes) && characterOfInterest.HasEffect("infuse"))
                    {
                        __result.AuraDamageType = Enums.DamageType.Fire;
                        __result.AuraDamageType = Enums.DamageType.Lightning;
                        __result.AuraDamageType = Enums.DamageType.Cold;
                        __result.AuraDamageIncreasedPerStack = __result.AuraDamageIncreasedPerStack2 = __result.AuraDamageIncreasedPerStack3 = 1;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperkinsulate1b", AppliesTo.Heroes))
                    {
                        __result.ResistModifiedValue = 40;
                        __result.ResistModifiedValue2 = 40;
                        __result.ResistModifiedValue3 = 40;
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "insulate1e", AppliesTo.ThisHero))
                    {
                        // LogDebug("Insulate1e");
                        __result.AuraDamageType = Enums.DamageType.Fire;
                        __result.AuraDamageType2 = Enums.DamageType.Cold;
                        __result.AuraDamageType3 = Enums.DamageType.Lightning;
                        __result.AuraDamageIncreasedPercentPerStack = 5.0f;
                        __result.AuraDamageIncreasedPercentPerStack2 = 5.0f;
                        __result.AuraDamageIncreasedPercentPerStack3 = 5.0f;
                        __result.ResistModifiedValue = 15;
                        __result.ResistModifiedValue2 = 15;
                        __result.ResistModifiedValue3 = 15;
                        __result.GainCharges = true;
                        __result.MaxCharges = 8;
                        __result.MaxMadnessCharges = 8;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "insulate1f", AppliesTo.ThisHero))
                    {
                        // LogDebug("courage1d");
                        __result.ResistModified = Enums.DamageType.Blunt;
                        __result.ResistModified2 = Enums.DamageType.Cold;
                        __result.ResistModified3 = Enums.DamageType.Shadow;
                    }


                    break;
                case "spellsword":
                    // spellsword1a: Max stacks +2";
                    // spellsword1b: Spellsword on heroes reduces incoming damage by 2, but does not increase damage";

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "spellsword1a", AppliesTo.Heroes))
                    {
                        // LogDebug("spellsword1a");
                        __result.MaxCharges += 2;
                        if (__result.MaxMadnessCharges != -1)
                            __result.MaxMadnessCharges += 2;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "spellsword1b", AppliesTo.Heroes))
                    {
                        // LogDebug("spellsword1b");
                        __result.AuraDamageType = Enums.DamageType.None;
                        __result.AuraDamageIncreasedPerStack = 0;
                        __result.IncreasedDirectDamageReceivedPerStack = -2;
                        __result.ChargesMultiplierDescription = 2;
                    }

                    break;
                case "energize":
                    // energize1b: Energize gives 2 energy per charge, but you can only have a maximum of 1 Energize.";
                    // energize1c: Energize increases all damage 1 per charge.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "energize1b", AppliesTo.ThisHero))
                    {
                        __result.MaxCharges = 1;
                        __result.MaxMadnessCharges = 1;
                        __result.CharacterStatModifiedValuePerStack = 2;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "energize1c", AppliesTo.ThisHero))
                    {
                        __result.AuraDamageType = Enums.DamageType.All;
                        __result.AuraDamageIncreasedPerStack = 1;
                    }

                    break;
                case "burn":
                    // mainperkburn2d: changed to be less than 4 rather than less than 3 curses
                    // scourge0h: Scourge on monsters increases burn damage by 15%/stack";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperkburn2d", AppliesTo.Monsters))
                    {
                        __result.DoubleDamageIfCursesLessThan = 4;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "scourge0h", AppliesTo.Heroes))
                    {
                        int scourge_charges = characterOfInterest.GetAuraCharges("scourge");
                        float multiplier = 0.15f * scourge_charges + 1;
                        __result.DamageWhenConsumedPerCharge *= multiplier;
                    }
                    break;
                case "chill":
                    // chill2e: Chill reduces Cold and Mind resistance by 0.5% per charge.";
                    // chill2f: At the start of your turn, suffer 3 Chill. Chill on this hero reduces Speed by 1 for every 10 charges";
                    // chill2g: Chill on this hero reduces Speed by 1 for every 3 charges but does not reduce Cold resistance.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "chill2e", AppliesTo.Global))
                    {
                        // LogDebug("chill2e");
                        __result.ResistModified = Enums.DamageType.Cold;
                        __result.ResistModified2 = Enums.DamageType.Mind;
                        __result.ResistModifiedPercentagePerStack = -0.5f;
                        __result.ResistModifiedPercentagePerStack2 = -0.5f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "chill2f", AppliesTo.ThisHero))
                    {
                        // LogDebug("chill2f");
                        __result.CharacterStatChargesMultiplierNeededForOne = 10;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "chill2g", AppliesTo.ThisHero))
                    {
                        // LogDebug("chill2g");
                        __result.CharacterStatChargesMultiplierNeededForOne = 3;
                        __result.ResistModified = Enums.DamageType.None;
                        __result.ResistModifiedPercentagePerStack = 0.0f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "insulate1d", AppliesTo.ThisHero))
                    {
                        // LogDebug("insulate1d");
                        __result.CharacterStatModified = Enums.CharacterStat.None;
                        __result.CharacterStatAbsoluteValuePerStack = 0;
                    }
                    break;
                case "wet":
                    // wet1d: Wet does not Dispel or Prevent Burn.";
                    // zeal0g: While any hero and monster has Zeal, Wet increases all resistances by 0.5%
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "zeal0g", AppliesTo.Global) && characterOfInterest.HasEffect("zeal"))
                    {
                        // LogDebug("zeal0g");
                        __result.ResistModified3 = Enums.DamageType.All;
                        // __result.ResistModifiedPercentagePerStack3 = 0.5f;
                        __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.All, 0, 0.5f);
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "wet1d", AppliesTo.Global))
                    {
                        // Not sure if this is working
                        AuraCurseData noneAC = GetAuraCurseData("None");
                        __result.PreventedAuraCurse = noneAC;
                        __result.PreventedAuraCurseStackPerStack = 0;
                        __result.RemoveAuraCurse = noneAC;
                    }
                    bool hasRust0d = IfCharacterHas(characterOfInterest, CharacterHas.Perk, "rust0bd", AppliesTo.Monsters);
                    if (hasRust0d)
                    {
                        __result.IncreasedDirectDamageReceivedPerStack *= 2.25f;
                        __result.IncreasedDirectDamageReceivedPerStack2 *= 2.25f;
                        __result.ResistModifiedPercentagePerStack *= 2.25f;
                        __result.ResistModifiedPercentagePerStack2 *= 2.25f;
                        __result.ResistModifiedPercentagePerStack3 *= 2.25f;
                    }
                    // if (hasRust0d)
                    // {
                    //     __result.IncreasedDirectDamageReceivedPerStack = 1.5f;
                    // }
                    // if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperkwet1a", AppliesTo.Monsters) && hasRust0d)
                    // {
                    //     __result.IncreasedDamageReceivedType2 = Enums.DamageType.Cold;
                    //     __result.IncreasedDirectDamageReceivedPerStack2 = 1.5f;
                    // }
                    // if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperkwet1b", AppliesTo.Monsters) && hasRust0d)
                    // {
                    //     __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.Lightning, 0, -1.5f);
                    //     __result.AuraConsumed = 0;
                    // }
                    break;

                case "spark":
                    // spark2d: Gain +1 Lightning Damage for every 5 stacks of Spark on this hero."
                    // spark2e: Spark deal Fire damage. Spark decreases Fire resistance by 0.5% per charge and Lightning resistance by 0.5% per charge.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "spark2d", AppliesTo.ThisHero))
                    {
                        // LogDebug("spark2d");
                        __result.AuraDamageType = Enums.DamageType.Lightning;
                        __result.AuraDamageIncreasedPerStack = 1;
                        __result.ChargesAuxNeedForOne1 = 5;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "spark2e", AppliesTo.Monsters))
                    {
                        // LogDebug("spark2e");
                        __result.DamageTypeWhenConsumed = Enums.DamageType.Fire;
                        __result.ResistModified = Enums.DamageType.Lightning;
                        __result.ResistModified2 = Enums.DamageType.Fire;
                        __result.ResistModifiedPercentagePerStack = -0.5f;
                        __result.ResistModifiedPercentagePerStack2 = -0.5f;
                    }
                    break;

                case "shield":
                    // shield5b: If Restricted Power is enabled, increases Max Charges to 300.";
                    // shield5d: Shield on you increases Holy damage by 0.2 per charge
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "shield5b", AppliesTo.Global))
                    {
                        // LogDebug("shield5b");
                        __result.MaxMadnessCharges = 300;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "shield5d", AppliesTo.Heroes))
                    {
                        // __result = AtOManager.Instance.GlobalAuraCurseModifyDamage(__result, Enums.DamageType.Holy, 0, 0.2f, 0);
                        __result.AuraDamageType = Enums.DamageType.Holy;
                        __result.AuraDamageIncreasedPerStack = 0.2f;
                    }
                    break;

                case "regeneration":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "regeneration1d", AppliesTo.Heroes))
                    {
                        __result.PreventedAuraCurse = GetAuraCurseData("vulnerable");
                        __result.PreventedAuraCurseStackPerStack = 1;
                    }

                    break;
                case "dark":
                    // scourge0e: Dark no longer explodes. Every charge of Scourge increases damage due to other curses by 5%.";
                    // dark2e: Dark explosions deal Fire damage. Dark reduces Fire resistance by 0.25% per charge in addition to reducing Shadow resistance..";
                    // burn1e: Burn increases the damage dealt by Dark explosions by 0.5% per charge.";
                    // sanctify2d: Every 5 stacks of Sanctify increase the number of Dark charges needed for an explosion by 1.";

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperkdark2b", AppliesTo.ThisHero))
                    {
                        __result.Removable = false;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "dark2e", AppliesTo.Global))
                    {
                        __result.DamageTypeWhenConsumed = Enums.DamageType.Fire;
                        __result.ResistModified2 = Enums.DamageType.Fire;
                        __result.ResistModifiedPercentagePerStack2 = -0.5f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "sanctify2d", AppliesTo.Global))
                    {
                        int n = characterOfInterest.GetAuraCharges("sanctify");
                        __result.ExplodeAtStacks += FloorToInt(0.2f * n);
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "burn2e", AppliesTo.Global))
                    {
                        int n_charges = characterOfInterest.GetAuraCharges("burn");
                        float multiplier = 1 + 0.05f * n_charges;
                        __result.DamageWhenConsumedPerCharge *= multiplier;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "scourge0e", AppliesTo.Global))
                    {
                        __result.ExplodeAtStacks = -1;
                    }
                    break;

                case "decay":
                    // decay1d: Decay purges Insulate.";

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "decay1d", AppliesTo.Global))
                    {
                        __result.RemoveAuraCurse = GetAuraCurseData("insulate");
                    }


                    break;

                case "courage":
                    // courage1d: Courage increases Shield gained by this hero by 1 per charge.
                    // courage1e: Courge on this hero now increases Slashing, Fire, and Holy resistance.

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "infuse0f", AppliesTo.Heroes) && characterOfInterest.HasEffect("infuse"))
                    {
                        __result.AuraDamageType = Enums.DamageType.Holy;
                        __result.AuraDamageType = Enums.DamageType.Shadow;
                        __result.AuraDamageType = Enums.DamageType.Mind;
                        __result.AuraDamageIncreasedPerStack = __result.AuraDamageIncreasedPerStack2 = __result.AuraDamageIncreasedPerStack3 = 1;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "mainperkcourage1b", AppliesTo.Heroes))
                    {
                        __result.ResistModifiedValue = 40;
                        __result.ResistModifiedValue2 = 40;
                        __result.ResistModifiedValue3 = 40;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "courage1f", AppliesTo.ThisHero))
                    {
                        // LogDebug("courage1d");
                        __result.ResistModified = Enums.DamageType.Slashing;
                        __result.ResistModified2 = Enums.DamageType.Fire;
                        __result.ResistModified3 = Enums.DamageType.Holy;
                    }
                    break;

                case "zeal":
                    // zeal0d: Zeal on this hero increases All Damage done by 1.5% per Bleed charge on this hero
                    // zeal0e: While this hero has Zeal, Sharp increases their Holy damage by 1 per charge.
                    // zeal0f: Zeal on all heroes increases Speed by 2 per charge.
                    // zeal0g: When this hero loses Zeal, deal indirect Holy and Fire damage equal to 4x the number of stacks lost to all monsters.
                    // zeal0h: When this hero loses Zeal at end of turn, deal indirect Holy and Fire damage to all monsters equal to 4x the number of charges lost..
                    // zeal0i: Zeal on this hero can stack, but no longer increases Resistances. At the end of turn, suffer 5 Burn per charge..
                    // zeal0j: Zeal on heroes makes Thorns apply half their damage as Burn rather than dealing damage (untested).
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "zeal0d", AppliesTo.ThisHero))
                    {
                        // LogDebug("zeal0d");
                        __result.AuraDamageType = Enums.DamageType.All;
                        __result.AuraDamageIncreasedPercent = Mathf.RoundToInt(1.5f * characterOfInterest.GetAuraCharges("bleed"));
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "zeal0i", AppliesTo.ThisHero))
                    {
                        __result.GainCharges = true;
                        __result.ResistModified = Enums.DamageType.None;
                        __result.ResistModifiedPercentagePerStack = 0.0f;
                        __result.GainAuraCurseConsumption = GetAuraCurseData("burn");
                        __result.GainAuraCurseConsumptionPerCharge = 5;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "zeal0f", AppliesTo.Heroes))
                    {
                        // LogDebug("zeal0f");
                        __result.CharacterStatModified = Enums.CharacterStat.Speed;
                        __result.CharacterStatModifiedValuePerStack = 2;
                    }
                    break;

                case "scourge":
                    // scourge0d: Scourge on monsters also deals 1 Shadow damage per Sight charge (Not working) TODO
                    // scourge0e: Dark no longer explodes. Every charge of Scourge increases damage due to other curses by 5%.
                    // scourge0f: Scourge on monsters can Stack but increases all resists by 3% per stack.
                    // scourge0g: Scourge deals damage based on Sight rather than Chill.
                    // scourge0h: Scourge on monsters increases burn damage by 15%/stack
                    // scourge0i: Dark explosions deal 10% of their damage to the target's sides for each charge of Scourge
                    // scourge0j: If an enemy has two or less curses, Scourge deals 4x damage

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "scourge0d", AppliesTo.Monsters))
                    {
                        __result.DamageTypeWhenConsumed = Enums.DamageType.Shadow;
                        __result.DamageWhenConsumed += characterOfInterest.GetAuraCharges("sight");
                    }

                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "scourge0f", AppliesTo.Monsters))
                    {
                        __result.GainCharges = true;
                        __result.ResistModified2 = Enums.DamageType.All;
                        __result.ResistModifiedPercentagePerStack2 = 3.0f;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "scourge0g", AppliesTo.Global))
                    {
                        __result.ConsumedDamageChargesBasedOnACCharges = GetAuraCurseData("sight");
                        __result.DamageWhenConsumedPerCharge = 2;
                    }
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "scourge0j", AppliesTo.Global))
                    {
                        if (characterOfInterest.GetCurseList().Count <= 2)
                        {
                            __result.DamageWhenConsumedPerCharge *= 4;
                        }
                    }
                    break;

                case "weak":
                    // weak1c: Monsters cannot be immune to Weak, but no longer have their damage reduced by Insane.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "weak1c", AppliesTo.Monsters))
                    {
                        __result.Preventable = false;
                        __result.AuraDamageIncreasedPercent = -20;
                        __result.HealDonePercent = -20;
                    }
                    break;

                case "vitality":
                    // vitality1d: Vitality on this hero dispels Poison.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "vitality1d", AppliesTo.ThisHero))
                    {
                        __result.RemoveAuraCurse = GetAuraCurseData("poison");
                    }
                    break;

                case "bless":
                    // bless1d: Bless on all heroes increases Slashing, Fire, and Holy damage by 3% per charge but does not increase damage by 1.";
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "bless1d", AppliesTo.Heroes))
                    {
                        __result.AuraDamageType = Enums.DamageType.Slashing;
                        __result.AuraDamageType2 = Enums.DamageType.Fire;
                        __result.AuraDamageType3 = Enums.DamageType.Holy;
                        __result.AuraDamageIncreasedPercentPerStack = 3.0f;
                        __result.AuraDamageIncreasedPercentPerStack2 = 3.0f;
                        __result.AuraDamageIncreasedPercentPerStack3 = 3.0f;
                        __result.AuraDamageIncreasedPerStack = 0.0f;
                    }
                    break;
            }
            if (IfCharacterHas(characterOfInterest, CharacterHas.Perk, "scourge0e", AppliesTo.Monsters))
            {
                __result.DamageWhenConsumed = Mathf.RoundToInt(characterOfInterest.GetAuraCharges("scourge") * 0.05f + 1) * __result.DamageWhenConsumed;
                __result.DamageWhenConsumedPerCharge *= 1 + characterOfInterest.GetAuraCharges("scourge") * 0.05f;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitAuraCurseModifiers))]
        public static void GetTraitAuraCurseModifiersPostfix(ref Character __instance, ref Dictionary<string, int> __result)
        {
            // Sharp1f: For every 8 Sharp, gain +1 Bleed

            string perkOfInterest = perkBase + "sharp1f";
            if (IsLivingHero(__instance) && AtOManager.Instance.CharacterHavePerk(__instance.SubclassName, perkOfInterest))
            {
                LogDebug($"Executing Perk {perkOfInterest}");
                int nSharp = __instance.EffectCharges("sharp");
                int bonusBleedCharges = FloorToInt(nSharp * 0.125f);

                if (bonusBleedCharges != 0) { __result["bleed"] = bonusBleedCharges; }
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "UpdateResistanceModifiersBasedOnInfuse")]
        private static bool UpdateResistanceModifiersBasedOnInfusePrefix(string _type, Character _characterCaster, Character _characterTarget, ref AuraCurseData AC)
        {
            int infuseCharges = 0;
            if (_characterCaster != null && _type == "consume")
            {
                infuseCharges = _characterCaster.GetAuraCharges("infuse");
            }
            else if (_characterTarget != null)
            {
                infuseCharges = _characterTarget.GetAuraCharges("infuse");
            }
            if (infuseCharges > 0)
            {

                if (_characterCaster != null && CharacterObjectHavePerk(_characterCaster, "infuse0g"))
                {
                    float resistMultiplier = 1f + 0.10f * infuseCharges;
                    AC.ResistModifiedValue *= resistMultiplier;
                    AC.ResistModifiedValue2 *= resistMultiplier;
                    AC.ResistModifiedValue3 *= resistMultiplier;
                    AC.ResistModifiedPercentagePerStack *= resistMultiplier;
                    AC.ResistModifiedPercentagePerStack2 *= resistMultiplier;
                    AC.ResistModifiedPercentagePerStack3 *= resistMultiplier;
                    return false;
                }
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "DoLeachExplosion")]
        public static void DoLeachExplosionPostfix(Character target)
        {
            // leech0g: Rather than healing, when Leech explodes, it deals damage to all enemies equal to the target's Bleed.
            if (TeamHasPerk("leech0g") && target.NpcData != null)
            {
                int nBleed = target.GetAuraCharges("bleed");
                DealIndirectDamageToAllMonsters(Enums.DamageType.None, nBleed);
            }
        }
    }
}
