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


namespace PerkManager
{
    [HarmonyPatch]
    public class PerkPatches
    {
        public static int[] paralyzeCounters = [0, 0, 0, 0];

        public static bool blockShieldFlag = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.BeginAdventure))]
        public static void BeginAdventurePostfix(ref AtOManager __instance)
        {
            // TODO
            // currency6c"] = "Increases chance for Scarabs to spawn by 10%." - CANNOT BE DONE;
            // shards5c"] = "Increases chance for cards to be corrupted by 2%." - Handled in RewrittenFunctions;
            // medsTexts[perkStem + "resistance5d"] = "Maximum resistances for heroes and monsters are now 97%. - Rewritten Functions";

            // XP Perks
            Hero[] teamHero = PlayerManager.Instance.GetTeamHero();
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
                // medsTexts[perkStem + "shards5b"] = "Gain 125 shards on level up."; - multiplayer?
                int AmountToIncreasePerLevel = 125;
                AtOManager.Instance.GivePlayer(1, AmountToIncreasePerLevel, anim: true);

            }
            if (CharacterObjectHavePerk(hero, "currency6b"))
            {
                // medsTexts[perkStem + "currency6b"] = "Gain 125 gold on level up."; - NEEDS MULTIPLAYER?
                int AmountToIncreasePerLevel = 125;
                AtOManager.Instance.GivePlayer(0, AmountToIncreasePerLevel, anim: true);

            }


            if (CharacterObjectHavePerk(hero, "resistance5b"))
            {
                // medsTexts[perkStem + "resistance5b"] = "-4% Resistances. Gain 4% to all Resistances on level up";
                Plugin.Log.LogDebug(debugBase + "Attempting to add resistances");
                int AmountToIncreasePerLevel = 4;
                string perkName = perkBase + "resistance5b";
                PerkData perk = Globals.Instance.GetPerkData(perkName);
                perk.ResistModified = Enums.DamageType.All;
                perk.ResistModifiedValue += AmountToIncreasePerLevel;

                Dictionary<string, PerkData> perkDictionary = Traverse.Create(Globals.Instance).Field("_PerksSource").GetValue<Dictionary<string, PerkData>>();
                if (perkDictionary.ContainsKey(perkName))
                {
                    Plugin.Log.LogDebug(debugBase + "Setting perk dictionary");
                    perkDictionary[perkName] = perk;
                }
                Traverse.Create(Globals.Instance).Field("_PerksSource").SetValue(perkDictionary);
            }

            if (CharacterObjectHavePerk(hero, "resistance5c"))
            {
                // medsTexts[perkStem + "resistance5c"] = "+12% Resistances. Lose 4% to all Resistances on level up";            
                Plugin.Log.LogDebug(debugBase + "Attempting to add resistances");
                int AmountToIncreasePerLevel = -4;
                string perkName = perkBase + "resistance5b";
                PerkData perk = Globals.Instance.GetPerkData(perkName);
                perk.ResistModified = Enums.DamageType.All;
                perk.ResistModifiedValue += AmountToIncreasePerLevel;

                Dictionary<string, PerkData> perkDictionary = Traverse.Create(Globals.Instance).Field("_PerksSource").GetValue<Dictionary<string, PerkData>>();
                if (perkDictionary.ContainsKey(perkName))
                {
                    Plugin.Log.LogDebug(debugBase + "Setting perk dictionary");
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
                // Plugin.Log.LogDebug(debugBase + " PerkList for " + _hero.SubclassName + ": " + string.Join(",", _hero.PerkList.ToArray()));
                if (CharacterObjectHavePerk(_hero, "mitigate1d"))
                    Plugin.Log.LogDebug(debugBase + "Hero has mitigate1d");


                Plugin.Log.LogDebug(debugBase + "Adding Immunities weak1d and more");

                if (CharacterObjectHavePerk(_hero, "weak1d"))
                {
                    Plugin.Log.LogDebug(debugBase + "Adding Immunities, has weak1d");
                    AddImmunityToHero("weak", ref _hero);

                }
                if (CharacterObjectHavePerk(_hero, "disarm1a"))
                {
                    Plugin.Log.LogDebug(debugBase + "Adding Immunities, has disarm1a");
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



            }
            Traverse.Create(MatchManager.Instance).Field("teamHero").SetValue(teamHero);

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), nameof(Globals.GetCostReroll))]
        public static void GetRerollCostPostfix(ref int __result)
        {
            // medsTexts[perkStem + "currency6d"] = "Rerolling the shop costs 25% less.";

            if (TeamHasPerk("currency6d"))
            {
                __result = RoundToInt(0.75f * __result);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), nameof(Globals.GetDivinationCost))]
        public static void GetDivinationCost(ref Globals __instance, ref int __result)
        {
            // medsTexts[perkStem + "currency6e"] = "Divinations cost 15% less.";

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
            // Not sure on this, but: Killed is "this character was killed" --> triggers things like resurrect
            // CharacterKilled is "a character was killed" --> triggers things like Yogger's Innate

            // __instance is the "source" character, target is the target

            // .Hitted has the __instance as the character that was hit and target as the caster

            // TODO:
            // //block5d: Block only functions if you are above 50% Max Health [Currently not working].";

            if (theEvent == Enums.EventActivation.CharacterAssign || theEvent == Enums.EventActivation.DestroyItem)
            {
                return;
            }

            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            Plugin.Log.LogDebug(debugBase + "SETEVENTPREFIX");

            if (theEvent == Enums.EventActivation.BeginCombat)
                paralyzeCounters = [0, 0, 0, 0];


            if (theEvent == Enums.EventActivation.Killed && __instance.IsHero && __instance != null && CharacterObjectHavePerk(__instance, "zeal1f") && __instance.HasEffect("zeal"))
            {
                // zeal1f: If this hero dies with Zeal, deal indirect Mind damage to all enemies equal to 5x their Burn/Insane stacks.

                Plugin.Log.LogDebug(debugBase + "zeal1f");
                Plugin.Log.LogDebug(debugBase + "Target Charges = " + target.GetAuraCharges("burn"));
                int n_stacks = __instance.GetAuraCharges("burn");
                Plugin.Log.LogDebug(debugBase + "Instance Charges = " + n_stacks);
                int damageToDeal = Functions.FuncRoundToInt(8 * n_stacks);
                __instance.IndirectDamage(Enums.DamageType.Mind, damageToDeal);
            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && !__instance.IsHero && __instance != null && AtOManager.Instance.TeamHavePerk(debugBase + "weak1b") && __instance.HasEffect("weak"))
            {
                // weak1b: "Weak on monsters reduces the application of Auras and Curses by 20%.";

                Plugin.Log.LogDebug(debugBase + "weak1b");
                auxInt = Functions.FuncRoundToInt(0.8f * auxInt);

            }
            if (theEvent == Enums.EventActivation.AuraCurseSet && __instance.IsHero && __instance != null && TeamHasPerk("weak1b") && __instance.HasEffect("shackle"))
            {
                // shackle1e"] = "Shackle on this hero increases Dark charges you apply by 1 per charge of Shackle.";

                Plugin.Log.LogDebug(debugBase + "shackle1e");
                int n_shackle = __instance.GetAuraCharges("shackle");
                __instance.ModifyAuraCurseQuantity("dark", n_shackle);
                // if (auraCurseModifiers.ContainsKey(traitData.AuracurseBonus1.Id))
                //     auraCurseModifiers[traitData.AuracurseBonus1.Id] += traitData.AuracurseBonusValue1;
                // else
                //     auraCurseModifiers[traitData.AuracurseBonus1.Id] = traitData.AuracurseBonusValue1;

            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && __instance.IsHero && __instance != null && !target.IsHero && target.Alive && target != null && CharacterObjectHavePerk(__instance, "poison2h"))
            {
                // poison2h: -1 Poison. When this hero applies poison, deal Mind damage to the target equal to 30% of their Poison charges.";
                Plugin.Log.LogDebug(debugBase + "poison2h");

                int n = target.GetAuraCharges("poison");
                int damageToDeal = RoundToInt(n * 0.3f);
                target.IndirectDamage(Enums.DamageType.Mind, damageToDeal);
            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && target.IsHero && target != null && target.Alive && target.HasEffect("courage") && CharacterObjectHavePerk(target, "courage1d") && auxString == "shield")
            {
                // medsTexts[perkStem + "courage1d"] = "Courage increases Shield gained by this hero by 1 per charge.";
                Plugin.Log.LogDebug(debugBase + "courage1d");
                int n = target.GetAuraCharges("courage");
                // Plugin.Log.LogDebug(debugBase + "courage1d  AuxInt  before: " + auxInt);
                // Plugin.Log.LogDebug(debugBase + "courage1d  n  before: " + n);
                blockShieldFlag = !blockShieldFlag;
                if (blockShieldFlag)
                    target.SetAura(__instance, GetAuraCurseData("shield"), n, useCharacterMods: false);
                // Plugin.Log.LogDebug(debugBase + "courage1d  AuxInt  after: " + auxInt);

            }
            if (theEvent == Enums.EventActivation.AuraCurseSet && target.IsHero && target != null && target.Alive && target.HasEffect("reinforce") && CharacterObjectHavePerk(target, "reinforce1d") && auxString == "block")
            {
                // reinforce1d: Reinforce on this hero increases Block received by 1 per charge of Reinforce.";
                int n = __instance.GetAuraCharges("reinforce");
                blockShieldFlag = !blockShieldFlag;
                if (blockShieldFlag)
                    target.SetAura(__instance, GetAuraCurseData("block"), n, useCharacterMods: false);
                // Plugin.Log.LogDebug(debugBase + "courage1d  AuxInt  after: " + auxInt);

            }
            if (theEvent == Enums.EventActivation.AuraCurseSet && !target.IsHero && target != null && TeamHasPerk("sight1e") && auxString == "sight")
            {
                // "sight1e"] = "Once an enemy reaches 100 charges of Sight, Dispel Sight and Purge 3.";
                if (target.HasEffect("sight") && target.GetAuraCharges("sight") >= 100)
                    target.HealCursesName(singleCurse: "sight");
                target.DispelAuras(3);
            }

            if (theEvent == Enums.EventActivation.AuraCurseSet && __instance != null && !target.IsHero && target.Alive && target != null && TeamHasPerk("paralyze1c") && auxString == "spark")
            {
                // medsTexts[perkStem + "paralyze1c"] = "Once per enemy per combat, when an enemy reaches 100 Spark, apply 1 Paralyze.";
                int n = target.GetAuraCharges("spark");
                if (n >= 100)
                {
                    AuraCurseData paralyze = GetAuraCurseData("paralyze");
                    paralyzeCounters[target.NPCIndex]++;
                    target.SetAura(__instance, paralyze, 1);
                }
            }

            if (theEvent == Enums.EventActivation.BeginCombat && __instance.IsHero && __instance != null && CharacterObjectHavePerk(__instance, "block5c"))
            {
                // block5c: At start of combat, apply 2 Block to all heroes.";
                // Plugin.Log.LogDebug(debugBase + "block5c");
                bool allHeroes = true;
                bool allNpcs = false;
                ApplyAuraCurseTo("block", 2, allHeroes, allNpcs, false, false, ref __instance, ref teamHero, ref teamNpc, "", "");
            }
            if (theEvent == Enums.EventActivation.BeginCombat && __instance.IsHero && __instance != null && CharacterObjectHavePerk(__instance, "shield5c"))
            {
                // medsTexts[perkStem + "shield5c"] = "At start of combat, apply 4 Shield to all heroes.";
                // Plugin.Log.LogDebug(debugBase + "shield5c");
                bool allHeroes = true;
                bool allNpcs = false;
                ApplyAuraCurseTo("shield", 4, allHeroes, allNpcs, false, false, ref __instance, ref teamHero, ref teamNpc, "", "");
            }

            if (theEvent == Enums.EventActivation.CharacterKilled && !__instance.IsHero && __instance != null && TeamHasPerk("poison2g") && __instance.HasEffect("poison"))
            {
                // poison2g: When a monster with Poison dies, transfer 50% of their Poison charges to a random monster.";
                Plugin.Log.LogDebug(debugBase + "poison2g");

                int n = __instance.GetAuraCharges("poison");
                int toApply = RoundToInt(n * 0.5f);
                Character randomNPC = GetRandomCharacter(teamNpc);
                randomNPC.SetAura(__instance, GetAuraCurseData("poison"), toApply);
            }
            if (theEvent == Enums.EventActivation.CharacterKilled && !__instance.IsHero && __instance != null && TeamHasPerk("thorns1e") && __instance.HasEffect("thorns"))
            {
                // thorns1e when a monster with Thorns dies, transfer their Thorns charges to a random hero.
                Plugin.Log.LogDebug(debugBase + "thors1e");

                int n = __instance.GetAuraCharges("thorns");
                int toApply = RoundToInt(n * 1.0f);
                Character randomHero = GetRandomCharacter(teamHero);
                randomHero.SetAura(__instance, GetAuraCurseData("thorns"), toApply);
            }
            if (theEvent == Enums.EventActivation.CharacterKilled && !__instance.IsHero && __instance != null && TeamHasPerk("bleed2g") && __instance.HasEffect("bleed"))
            {
                // bleed2g: an enemy dies with Bleed, all monsters lose HP equal to 25% of the killed target's Bleed charges.";
                Plugin.Log.LogDebug(debugBase + "bleed2g");
                int n = __instance.GetAuraCharges("bleed");
                Plugin.Log.LogDebug(debugBase + "bleed2g Bleed charges: " + n);
                int toDeal = RoundToInt(n * 0.25f);
                DealIndirectDamageToAllMonsters(Enums.DamageType.None, toDeal);

            }

            if (theEvent == Enums.EventActivation.Hitted && !__instance.IsHero && __instance.Alive && target.IsHero && target.Alive && __instance != null && target != null && CharacterObjectHavePerk(target, "bleed2e"))
            {
                // bleed2e: When this hero hits an enemy with Bleed, they heal for 25% of the target's Bleed charges.";
                int n = __instance.GetAuraCharges("bleed");
                int toHeal = RoundToInt(n * 0.25f);
                target.IndirectHeal(toHeal);
            }
            if (theEvent == Enums.EventActivation.Hitted && !__instance.IsHero && __instance.Alive && target.IsHero && target.Alive && __instance != null && target != null && CharacterObjectHavePerk(target, "spark2f"))
            {
                // medsTexts[perkStem + "spark2f"] = "When you hit an enemy with Sparks, deal Lightning damage equal to 20% of their Sparks to their sides.";
                
                Plugin.Log.LogDebug(debugBase+"spark2f Hitted: "+target.SubclassName);
                int n = __instance.GetAuraCharges("spark");
                int toDeal = RoundToInt(n * 0.2f);
                int npcIndex = __instance.NPCIndex;
                int[] sides = [npcIndex - 1, npcIndex + 1];
                foreach (int side in sides)
                {
                    if (side >= 0 && side < teamNpc.Length)
                    {
                        NPC sideTarget = teamNpc[side];
                        if (sideTarget.Alive && sideTarget != null)
                            sideTarget.IndirectDamage(Enums.DamageType.Lightning, toDeal);
                    }
                }
            }
            if (theEvent == Enums.EventActivation.CastCard && __instance.IsHero && __instance.Alive && __instance != null && CharacterObjectHavePerk(__instance, "spellsword1d"))
            {
                // medsTexts[perkStem + "spellsword1d"] = "When this hero cast a Spell or Attack that costs 4 or more, gain 1 Spellsword";
                CardData _castedCard = Traverse.Create(__instance).Field("cardCasted").GetValue<CardData>();

                // Plugin.Log.LogDebug(debugBase+"spellsword1d Card Types: "+string.Join(",", _castedCard.GetCardTypes().ToArray()));
                // Plugin.Log.LogDebug(debugBase+"spellsword1d IsSpell: "+_castedCard.HasCardType(Enums.CardType.Spell));

                if (_castedCard != null && _castedCard.EnergyCost >= 4 && (_castedCard.HasCardType(Enums.CardType.Attack) || _castedCard.HasCardType(Enums.CardType.Spell)))
                    __instance.SetAuraTrait(__instance, "spellsword", 1);
            }

            // if (theEvent == Enums.EventActivation.CastCard && __instance.IsHero && __instance.Alive && __instance != null && target.IsHero && target.Alive && target != null && CharacterObjectHavePerk(__instance, "heal5b"))
            // {
            //     CardData _castedCard = Traverse.Create(__instance).Field("cardCasted").GetValue<CardData>();
            //     Plugin.Log.LogDebug(debugBase + " HP remaining: " + target.GetHpLeftForMax());
            //     if (_castedCard == null)
            //     {

            //     }
            //     else
            //     {
            //         if (_castedCard != null && _castedCard.Heal > 0 && target.GetHpLeftForMax() <= 0)
            //         {
            //             Plugin.Log.LogDebug(debugBase + "heal5b");
            //             target.SetAuraTrait(__instance, "powerful", 1);

            //         }
            //     }
            // }
        }



        // if (theEvent == Enums.EventActivation.AuraCurseSet && __instance.IsHero &&__instance != null && CharacterObjectHavePerk(target,debugBase+"block5d"))
        // { 
        //     // block5e: When this hero gains Block, they deal 1 Blunt to themselves and a random monster.";
        //     Plugin.Log.LogDebug(debugBase+"block5d");
        //     int damageToDeal = 1;
        //     Enums.DamageType damageType = Enums.DamageType.Blunt;
        //     int modifiedDamage = __instance.DamageWithCharacterBonus(damageToDeal,damageType,Enums.CardClass.None);
        //     Character targetCharacter = GetRandomCharacter(teamNpc);
        //     targetCharacter.IndirectDamage(damageType, modifiedDamage);
        //     __instance.IndirectDamage(damageType, modifiedDamage);
        // }            


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
            if (TeamHasPerk("weak1b"))
            {
                Plugin.Log.LogDebug(debugBase + "weak1b");
                if (!theCaster.IsHero && theCaster.HasEffect("weak"))
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
            // health6c"] = "At the start of your turn, if you are at max HP, gain 2 Vitality.";
            // medsTexts[perkStem + "chill2f"] = "At the start of your turn, suffer 3 Chill. Chill on this hero reduces Speed by 1 for every 10 charges";
            if (!__instance.Alive || __instance == null || MatchManager.Instance == null)
                return;

            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNPC = MatchManager.Instance.GetTeamNPC();

            if (CharacterObjectHavePerk(__instance, "shackle1d"))
            {
                int n_charges = __instance.GetAuraCharges("shackle");
                __instance.SetAuraTrait(__instance, "fortify", n_charges);
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
                if (__instance.GetMaxHP() <= __instance.HpCurrent)
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

                int n_charges = teamNPC.Count(npc => npc.Alive && npc != null && npc.GetAuraCharges("sight") >= 100);
                __instance.SetAuraTrait(__instance, "evasion", n_charges);

            }

            if (CharacterObjectHavePerk(__instance, "energize1a"))
            {
                // medsTexts[perkStem + "energize1a"] = "At start of your first turn, gain 1 Energize.";
                if (MatchManager.Instance.GetCurrentRound() == 1)
                    __instance.SetAuraTrait(__instance, "energize", 1);
            }

            if (CharacterObjectHavePerk(__instance, "spellsword1c"))
            {
                // medsTexts[perkStem + "spellsword1c"] = "At the start of your turn, all heroes and monsters gain 1 Spellsword";
                foreach (Hero hero in teamHero)
                {
                    if (hero.Alive && hero != null)
                        hero.SetAuraTrait(__instance, "spellsword", 1);
                }
                foreach (NPC npc in teamNPC)
                {
                    if (npc.Alive && npc != null)
                        npc.SetAuraTrait(__instance, "spellsword", 1);
                }
            }

            // medsTexts[perkStem + "sanctify2e"] = "At start of their turn, heroes gain 1 Zeal for every 20 Sanctify charges on them.";
            if (TeamHasPerk("sanctify2e") && __instance.IsHero && __instance != null && __instance.Alive && __instance.HasEffect("sanctify"))
            {
                int n_sanctify = __instance.GetAuraCharges("sanctify");
                int n_zeal = FloorToInt(n_sanctify * 0.05f);
                AuraCurseData zeal = GetAuraCurseData("zeal");
                __instance.SetAura(__instance, zeal, n_zeal, useCharacterMods: false);
            }
            if (TeamHasPerk("insane2f") && __instance != null && __instance.Alive && __instance.HasEffect("insane"))
            {
                int n_charges = FloorToInt(__instance.GetAuraCharges("insane") / 30);
                __instance.SetAuraTrait(__instance, "scourge", n_charges);
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), nameof(Character.GetTraitDamagePercentModifiers))]
        public static void GetTraitDamagePercentModifiersPostfix(ref Character __instance, ref float __result)
        {
            if (CharacterObjectHavePerk(__instance, "currency6d"))
            {
                // currency6d"] = "For every 2,000 gold you have, gain +10% damage.";

                int currencyAmount = AtOManager.Instance.GetPlayerGold();
                int percentToIncrease = FloorToInt(10 * currencyAmount / 2000);
                __result += percentToIncrease;
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.EndTurn))]
        public static void MatchManagerEndTurnPrefix(Character __instance)
        {

        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), nameof(Character.EndTurn))]
        public static void CharacterEndTurnPrefix(Character __instance)
        {
            if (!__instance.Alive || __instance == null || MatchManager.Instance == null)
                return;

            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            // zeal1e: When this hero loses Zeal, deal indirect Holy and Fire damage equal to 4x the number of stacks lost to all monsters.
            if (CharacterHasPerkForConsume("zeal1e", __instance.IsHero, AtOManager.Instance, __instance))
            {
                Plugin.Log.LogDebug(debugBase + "zeal1e");
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

                DealIndirectDamageToAllMonsters(Enums.DamageType.Holy, damageToDeal);
                DealIndirectDamageToAllMonsters(Enums.DamageType.Fire, damageToDeal);
            }

            // medsTexts[perkStem + "paralyze1b"] = "At the end of your turn, dispel Paralyze from all heroes.";
            if (CharacterObjectHavePerk(__instance, "paralyze1b"))
            {
                AuraCurseData paralyze = GetAuraCurseData("paralyze");
                foreach (Hero hero in teamHero)
                {
                    if (hero.Alive && hero != null)
                        hero.HealAuraCurse(paralyze);
                }

            }
            // medsTexts[perkStem + "inspire0d"] = "If this hero ends their turn with 4 or more cards, gain 1 Inspire";
            if (CharacterObjectHavePerk(__instance, "inspire0d"))
            {
                Plugin.Log.LogDebug(debugBase + "Handsize at End of Turn: " + MatchManager.Instance.CountHeroHand());
                if (MatchManager.Instance.CountHeroHand() >= 4)
                {
                    __instance.SetAuraTrait(__instance, "inspire", 1);
                }
            }

            // medsTexts[perkStem + "energize1b"] = "If you end your turn with 4 or more energy, gain 1 Energize.";
            if (CharacterObjectHavePerk(__instance, "energize1d"))
            {
                if (__instance.EnergyCurrent >= 4)
                {
                    __instance.SetAuraTrait(__instance, "energize", 1);
                }
            }


        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.GlobalAuraCurseModificationByTraitsAndItems))]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfixGeneral(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {

            Plugin.Log.LogDebug(debugBase + "Executing AC Modifications - General");

            bool ConsumeAppliesToHeroes = false; //flag1
            bool SetAppliesToHeroes = false; //flag2
            bool ConsumeAppliesToMonsters = false;
            bool SetAppliesToMonsters = false;
            bool AppliesGlobally = true;

            if (_characterCaster != null)
            {
                ConsumeAppliesToHeroes = _characterCaster.IsHero;
                ConsumeAppliesToMonsters = !_characterCaster.IsHero;
            }
            if (_characterTarget != null)
            {
                SetAppliesToHeroes = _characterTarget.IsHero;
                SetAppliesToMonsters = !_characterTarget.IsHero;
            }

            switch (_acId)
            {
                case "disarm":
                    if (_type == "set")
                    { //disarm1b - cannot be dispelled unless specified, increases resists by 10%
                        if (CharacterHasPerkForSet("disarm1b", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.Removable = false;
                            __result.ResistModified = Enums.DamageType.All;
                            __result.ResistModifiedValue = 10;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("disarm1b", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.ResistModified = Enums.DamageType.All;
                            __result.ResistModifiedValue = 10;
                        }
                    }
                    break;

                case "silence":
                    if (_type == "set")
                    { //silence1b - cannot be dispelled unless specified, increases damage by 7
                        if (CharacterHasPerkForSet("silence1b", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.Removable = false;
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.AuraDamageIncreasedTotal = 7;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("silence1b", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.AuraDamageIncreasedTotal = 7;
                        }
                    }
                    break;
                case "fast":
                    if (_type == "set")
                    {
                        // medsTexts[perkStem + "fast0b"] = "Fast on this hero can stack, but loses all charges at the start of turn.";
                        // medsTexts[perkStem + "fast0c"] = "Fast on this hero falls off at the end of turn.";

                        if (CharacterHasPerkForSet("fast0b", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.GainCharges = true;
                            __result.ConsumeAll = true;
                        }
                        if (CharacterHasPerkForSet("fast0c", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.ConsumedAtTurn = true;
                            __result.ConsumedAtTurnBegin = false;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("fast0b", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.GainCharges = true;
                            __result.ConsumeAll = true;
                        }
                        if (CharacterHasPerkForConsume("fast0c", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.ConsumedAtTurn = true;
                            __result.ConsumedAtTurnBegin = false;
                        }
                    }
                    break;

                case "slow":
                    if (_type == "set")
                    {
                        // medsTexts[perkStem + "slow0b"] = "Slow on monsters can stack up to 10, but only reduces Speed by 1 per charge";
                        // medsTexts[perkStem + "slow0c"] = "Slow on heroes can stack up to 10, but only reduces Speed by 1 per charge";

                        if (TeamHasPerkForSet("slow0b", SetAppliesToMonsters, __instance, _characterTarget))
                        {
                            __result.GainCharges = true;
                            __result.MaxCharges = 10;
                            __result.MaxMadnessCharges = 10;
                            __result.CharacterStatModifiedValuePerStack = -1;
                        }
                        if (TeamHasPerkForSet("slow0c", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.GainCharges = true;
                            __result.MaxCharges = 10;
                            __result.MaxMadnessCharges = 10;
                            __result.CharacterStatModifiedValuePerStack = -1;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("slow0b", ConsumeAppliesToMonsters, __instance, _characterCaster))
                        {
                            __result.GainCharges = true;
                            __result.MaxCharges = 10;
                            __result.MaxMadnessCharges = 10;
                            __result.CharacterStatModifiedValuePerStack = -1;

                        }
                        if (TeamHasPerkForConsume("slow0b", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.GainCharges = true;
                            __result.MaxCharges = 10;
                            __result.MaxMadnessCharges = 10;
                            __result.CharacterStatModifiedValuePerStack = -1;

                        }
                    }
                    break;
                case "ac":
                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("", AppliesGlobally, __instance, _characterTarget))
                        {

                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("", AppliesGlobally, __instance, _characterCaster))
                        {

                        }
                    }
                    break;
            }



        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.GlobalAuraCurseModificationByTraitsAndItems))]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfixPhysical(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {

            Plugin.Log.LogDebug(debugBase + "Executing AC Modifications - Physical");

            bool ConsumeAppliesToHeroes = false; //flag1
            bool SetAppliesToHeroes = false; //flag2
            bool ConsumeAppliesToMonsters = false;
            bool SetAppliesToMonsters = false;
            bool AppliesGlobally = true;

            if (_characterCaster != null)
            {
                ConsumeAppliesToHeroes = _characterCaster.IsHero;
                ConsumeAppliesToMonsters = !_characterCaster.IsHero;
            }
            if (_characterTarget != null)
            {
                SetAppliesToHeroes = _characterTarget.IsHero;
                SetAppliesToMonsters = !_characterTarget.IsHero;
            }

            switch (_acId)
            {
                case "ac":
                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("", AppliesGlobally, __instance, _characterTarget))
                        {

                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("", AppliesGlobally, __instance, _characterCaster))
                        {

                        }
                    }
                    break;
                case "sharp":
                    // medsTexts[perkStem + "insane2e"] = "Insane on this hero increases the effectiveness of sharp by 1% per charge.";

                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("insane2e", AppliesGlobally, __instance, _characterTarget) && _characterTarget.HasEffect("insane"))
                        {
                            int n = _characterTarget.GetAuraCharges("insane");
                            __result.AuraDamageIncreasedPerStack *= 1 + 0.01f * n;
                            __result.AuraDamageIncreasedPerStack2 *= 1 + 0.01f * n;
                            __result.AuraDamageIncreasedPerStack3 *= 1 + 0.01f * n;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("insane2e", AppliesGlobally, __instance, _characterCaster) && _characterCaster.HasEffect("insane"))
                        {
                            int n = _characterCaster.GetAuraCharges("insane");
                            __result.AuraDamageIncreasedPerStack *= 1 + 0.01f * n;
                            __result.AuraDamageIncreasedPerStack2 *= 1 + 0.01f * n;
                            __result.AuraDamageIncreasedPerStack3 *= 1 + 0.01f * n;
                        }
                    }
                    break;

                case "crack":
                    // medsTexts[perkStem + "insane2d"] = "Crack on monsters increases Blunt damage by an addition 1 for every 40 charges of Insane on that monster.";

                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("insane2d", SetAppliesToMonsters, __instance, _characterTarget) && _characterTarget.HasEffect("insane"))
                        {
                            int n = _characterTarget.GetAuraCharges("insane");
                            __result.IncreasedDirectDamageReceivedPerStack += FloorToInt(0.025f * n);
                        }
                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("insane2d", ConsumeAppliesToMonsters, __instance, _characterCaster) && _characterCaster.HasEffect("insane"))
                        {
                            int n = _characterCaster.GetAuraCharges("insane");
                            __result.IncreasedDirectDamageReceivedPerStack += FloorToInt(0.025f * n);
                        }
                    }
                    break;

                case "shackle":
                    if (_type == "set")
                    {
                        // shackle1b: This hero is immune to Shackle.";
                        // shackle1c: Shackle cannot be prevented.";
                        // shackle1d: At start of your turn, gain Fortify equal to your twice your Shackles.";
                        // shackle1e: Shackle increases Dark charges you apply by 1 per charge of Shackle.";
                        // shackle1f: Shackles on monsters increases all damage received by 1 per base Speed.";

                        if (TeamHasPerkForSet("shackle1c", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.Preventable = false;
                        }
                        // if (CharacterHasPerkForSet("shackle1e", SetAppliesToHeroes, __instance, _characterTarget))
                        // {
                        //     int n_charges = _characterTarget.GetAuraCharges("shackle");
                        //     _characterTarget.ModifyAuraCurseQuantity("dark", n_charges);
                        // }
                        if (TeamHasPerkForSet("shackle1f", SetAppliesToMonsters, __instance, _characterTarget))
                        {
                            int baseSpeed = _characterTarget.GetSpeed()[1];
                            int n_charges = _characterTarget.GetAuraCharges("shackle");
                            __result.IncreasedDamageReceivedType = Enums.DamageType.All;
                            __result.IncreasedDirectDamageReceivedPerStack = RoundToInt(baseSpeed / n_charges / 2);

                        }
                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("shackle1c", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.Preventable = false;
                        }
                        // if (CharacterHasPerkForConsume("shackle1e", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        // {
                        //     int n_charges = _characterCaster.GetAuraCharges("shackle");
                        //     _characterCaster.ModifyAuraCurseQuantity("dark", n_charges);
                        // }

                        if (TeamHasPerkForConsume("shackle1f", ConsumeAppliesToMonsters, __instance, _characterCaster))
                        {
                            int baseSpeed = _characterCaster.GetSpeed()[1];
                            int n_charges = _characterCaster.GetAuraCharges("shackle");
                            __result.IncreasedDamageReceivedType = Enums.DamageType.All;
                            __result.IncreasedDirectDamageReceivedPerStack = RoundToInt(baseSpeed / n_charges / 2);
                        }
                    }
                    break;

                case "mitigate":
                    if (_type == "set")
                    {
                        // mitigate1a: At the start of your turn, gain 2 Mitigate, but only stack to 5.";
                        // mitigate1b: Mitigate on this hero does not lose charges at start of turn and stacks to 12.";
                        // mitigate1c: At the start of your turn, gain 7 Block per Mitigate charge.";
                        // mitigate1d: Mitigate reduces incoming damage by 2 per charge, but loses all charges at the start of your turn.";
                        // mitigate1e: Mitigate on heroes and monsters increases damage done by 10% per charge.";
                        if (CharacterHasPerkForSet("mitigate1a", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.MaxCharges = 5;
                            __result.MaxMadnessCharges = 5;

                        }
                        if (CharacterHasPerkForSet("mitigate1b", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.ConsumedAtTurnBegin = false;
                            __result.MaxCharges = 12;
                            __result.MaxMadnessCharges = 12;
                        }
                        if (CharacterHasPerkForSet("mitigate1d", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.ConsumeAll = true;
                            __result.IncreasedDirectDamageReceivedPerStack = -2;
                            // Plugin.Log.LogDebug(debugBase + "Mitigate1d: set");
                            __result.ChargesMultiplierDescription = 2;


                        }
                        if (TeamHasPerkForSet("mitigate1e", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.AuraDamageIncreasedPercentPerStack = 10;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("mitigate1a", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.MaxCharges = 5;
                            __result.MaxMadnessCharges = 5;
                        }

                        if (CharacterHasPerkForConsume("mitigate1b", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.ConsumedAtTurnBegin = false;
                            __result.MaxCharges = 12;
                            __result.MaxMadnessCharges = 12;

                        }
                        if (CharacterHasPerkForConsume("mitigate1d", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.IncreasedDirectDamageReceivedPerStack = -2;
                            __result.ConsumeAll = true;
                            __result.ChargesMultiplierDescription = 2;
                            // Plugin.Log.LogDebug(debugBase + "Mitigate1d: Consume");

                        }
                        if (TeamHasPerkForConsume("mitigate1e", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.AuraDamageIncreasedPercentPerStack = 10;
                        }
                    }
                    break;

                case "poison":
                    if (_type == "set")
                    {
                        // poison2d: If Restricted Power is enabled, increases Max Charges to 300.";
                        // poison2e: Poison on heroes and monsters reduces Slashing resistance by 0.25% per charge.";
                        // poison2f: Poison on monsters reduces all resistances by 5% for every 100 charges.";
                        // poison2g: When a monster with Poison dies, transfer 50% of their Poison charges to a random monster.";
                        // poison2h: -1 Poison. When this hero applies poison, deal Mind damage to the target equal to 30% of their Poison charges.";
                        // medsTexts[perkStem + "decay1e"] = "Every stack of decay increases the damage dealt by poison by 20%.";

                        if (TeamHasPerkForSet("poison2d", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.MaxMadnessCharges = 300;
                        }
                        if (TeamHasPerkForSet("poison2e", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.ResistModified3 = Enums.DamageType.Slashing;
                            __result.ResistModifiedPercentagePerStack3 = -0.25f;
                        }
                        if (TeamHasPerkForSet("poison2f", SetAppliesToMonsters, __instance, _characterTarget))
                        {
                            __result.ResistModified2 = Enums.DamageType.All;
                            int n_poison = _characterTarget.GetAuraCharges("poison");
                            __result.ResistModifiedValue2 = FloorToInt(0.01f * n_poison);
                        }
                        if (TeamHasPerkForSet("decay1e", AppliesGlobally, __instance, _characterTarget) && _characterTarget.HasEffect("decay"))
                        {
                            int n_decay = _characterTarget.GetAuraCharges("decay");
                            float multiplier = 1 + 0.2f * n_decay;
                            __result.DamageWhenConsumedPerCharge *= multiplier;
                        }


                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("poison2d", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.MaxMadnessCharges = 300;
                        }
                        if (TeamHasPerkForConsume("poison2e", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.ResistModified3 = Enums.DamageType.Slashing;
                            __result.ResistModifiedPercentagePerStack3 = -0.25f;
                        }
                        if (TeamHasPerkForConsume("poison2f", ConsumeAppliesToMonsters, __instance, _characterCaster))
                        {
                            __result.ResistModified2 = Enums.DamageType.All;
                            int n_poison = _characterCaster.GetAuraCharges("poison");
                            __result.ResistModifiedValue2 = FloorToInt(0.01f * n_poison);
                        }
                        if (TeamHasPerkForConsume("decay1e", AppliesGlobally, __instance, _characterCaster) && _characterCaster.HasEffect("decay"))
                        {
                            int n_decay = _characterCaster.GetAuraCharges("decay");
                            float multiplier = 1 + 0.2f * n_decay;
                            __result.DamageWhenConsumedPerCharge *= multiplier;
                        }


                    }
                    break;

                case "bleed":
                    // bleed2d: If Restricted Power is enabled, increases Max Charges to 300.";
                    // bleed2e: When this hero hits an enemy with Bleed, they heal for 25% of the target's Bleed charges.";
                    // bleed2f: Bleed on monsters reduces Piercing resist by 0.25% per charge.";
                    // bleed2g: When this hero kills an enemy with Bleed, all monsters lose HP equal to 25% of the killed target's Bleed charges.";

                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("bleed2d", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.MaxMadnessCharges = 300;
                        }
                        if (TeamHasPerkForSet("bleed2f", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.ResistModified3 = Enums.DamageType.Piercing;
                            __result.ResistModifiedPercentagePerStack3 = -0.25f;
                        }

                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("bleed2d", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.MaxMadnessCharges = 300;
                        }
                        if (TeamHasPerkForConsume("bleed2f", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.ResistModified3 = Enums.DamageType.Piercing;
                            __result.ResistModifiedPercentagePerStack3 = -0.25f;
                        }

                    }
                    break;

                case "thorns":
                    // thorns1d: Cannot be purged unless specified.";
                    // thorns1e: When a monster with Thorns dies, transfer their Thorns charges to a random hero.";

                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("thorns1d", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.Removable = false;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.Removable = false;
                        }
                    }
                    break;

                case "reinforce":
                    // reinforce1d: Reinforce increases Block charges by 2 per charge of Reinforce.";
                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("", AppliesGlobally, __instance, _characterTarget))
                        {

                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("", AppliesGlobally, __instance, _characterCaster))
                        {

                        }
                    }
                    break;

                case "block":
                    // block5b: If Restricted Power is enabled, increases Max Charges to 600.";
                    // block5c: At start of combat, apply 2 Block to all heroes.";
                    // //block5d: Block only functions if you are above 50% Max Health [Currently not working].";
                    // block5e: When this hero gains Block, they deal 1 Blunt to themselves and a random monster.";
                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("block5b", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.MaxMadnessCharges = 600;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("block5b", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.MaxMadnessCharges = 600;
                        }
                    }
                    break;

                case "taunt":
                    // taunt1e: Taunt on this hero can stack and increases damage by 1 per charge.";
                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("taunt1e", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.GainCharges = true;
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.AuraDamageIncreasedPerStack = 1;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("taunt1e", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.GainCharges = true;
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.AuraDamageIncreasedPerStack = 1;
                        }
                    }
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.GlobalAuraCurseModificationByTraitsAndItems))]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfixElemental(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {

            Plugin.Log.LogDebug(debugBase + "Executing AC Modifications - Elemental");

            bool ConsumeAppliesToHeroes = false; //flag1
            bool SetAppliesToHeroes = false; //flag2
            bool ConsumeAppliesToMonsters = false;
            bool SetAppliesToMonsters = false;
            bool AppliesGlobally = true;

            if (_characterCaster != null)
            {
                ConsumeAppliesToHeroes = _characterCaster.IsHero;
                ConsumeAppliesToMonsters = !_characterCaster.IsHero;
            }
            if (_characterTarget != null)
            {
                SetAppliesToHeroes = _characterTarget.IsHero;
                SetAppliesToMonsters = !_characterTarget.IsHero;
            }

            switch (_acId)
            {
                case "ac":
                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("", AppliesGlobally, __instance, _characterTarget))
                        {

                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("", AppliesGlobally, __instance, _characterCaster))
                        {

                        }
                    }
                    break;
                case "spellsword":
                    // medsTexts[perkStem + "spellsword1a"] = "Max stacks +2";
                    // medsTexts[perkStem + "spellsword1b"] = "Spellsword on heroes reduces incoming damage by 2, but does not increase damage";

                    if (_type == "set")
                    {

                        if (CharacterHasPerkForSet("spellsword1a", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.MaxCharges += 2;
                            if (__result.MaxMadnessCharges != -1)
                                __result.MaxMadnessCharges += 2;
                        }
                        if (TeamHasPerkForSet("spellsword1b", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.AuraDamageType = Enums.DamageType.None;
                            __result.AuraDamageIncreasedPerStack = 0;
                            __result.IncreasedDirectDamageReceivedPerStack = -2;
                            __result.ChargesMultiplierDescription = 2;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("spellsword1a", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.MaxCharges += 2;
                            if (__result.MaxMadnessCharges != -1)
                                __result.MaxMadnessCharges += 2;

                        }
                        if (TeamHasPerkForConsume("spellsword1b", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.AuraDamageType = Enums.DamageType.None;
                            __result.AuraDamageIncreasedPerStack = 0;
                            __result.IncreasedDirectDamageReceivedPerStack = -2;
                            __result.ChargesMultiplierDescription = 2;
                        }

                    }
                    break;
                case "energize":
                    // medsTexts[perkStem + "energize1b"] = "Energize gives 2 energy per charge, but you can only have a maximum of 1 Energize.";
                    // medsTexts[perkStem + "energize1c"] = "Energize increases all damage 1 per charge.";

                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("energize1b", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.MaxCharges = 1;
                            __result.MaxMadnessCharges = 1;
                            __result.CharacterStatModifiedValuePerStack = 2;
                        }
                        if (CharacterHasPerkForSet("energize1c", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.AuraDamageIncreasedPerStack = 1;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("energize1b", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.MaxCharges = 1;
                            __result.MaxMadnessCharges = 1;
                            __result.CharacterStatModifiedValuePerStack = 2;
                        }
                        if (CharacterHasPerkForConsume("energize1c", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.AuraDamageType = Enums.DamageType.All;
                            __result.AuraDamageIncreasedPerStack = 1;
                        }
                    }
                    break;
                case "burn":
                    // scourge1e: Scourge on monsters increases burn damage by 15%/stack";

                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("scourge1e", SetAppliesToMonsters, __instance, _characterTarget) && _characterTarget.HasEffect("scourge"))
                        {
                            int scourge_charges = _characterTarget.GetAuraCharges("scourge");
                            float multiplier = 0.15f * scourge_charges + 1;
                            __result.DamageWhenConsumedPerCharge *= multiplier;
                        }

                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("scourge1e", ConsumeAppliesToMonsters, __instance, _characterCaster) && _characterCaster.HasEffect("scourge"))
                        {
                            int scourge_charges = _characterCaster.GetAuraCharges("scourge");
                            float multiplier = 1 + 0.15f * scourge_charges;
                            __result.DamageWhenConsumedPerCharge *= multiplier;
                        }
                    }
                    break;
                case "chill":
                    // medsTexts[perkStem + "chill2e"] = "Chill reduces Cold and Mind resistance by 0.5% per charge.";
                    // medsTexts[perkStem + "chill2f"] = "At the start of your turn, suffer 3 Chill. Chill on this hero reduces Speed by 1 for every 10 charges";
                    // medsTexts[perkStem + "chill2g"] = "Chill on this hero reduces Speed by 1 for every 3 charges but does not reduce Cold resistance.";

                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("chill2e", SetAppliesToMonsters, __instance, _characterTarget))
                        {
                            __result.ResistModified = Enums.DamageType.Cold;
                            __result.ResistModified2 = Enums.DamageType.Mind;
                            __result.ResistModifiedPercentagePerStack = -0.5f;
                            __result.ResistModifiedPercentagePerStack2 = -0.5f;
                        }
                        if (CharacterHasPerkForSet("chill2f", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.CharacterStatChargesMultiplierNeededForOne = 10;
                        }
                        if (CharacterHasPerkForSet("chill2g", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.CharacterStatChargesMultiplierNeededForOne = 3;
                            __result.ResistModified = Enums.DamageType.None;
                            __result.ResistModifiedPercentagePerStack = 0.0f;
                        }

                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("chill2e", ConsumeAppliesToMonsters, __instance, _characterCaster))
                        {
                            __result.ResistModified = Enums.DamageType.Cold;
                            __result.ResistModified2 = Enums.DamageType.Mind;
                            __result.ResistModifiedPercentagePerStack = 0.5f;
                            __result.ResistModifiedPercentagePerStack2 = 0.5f;
                        }
                        if (CharacterHasPerkForSet("chill2f", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.CharacterStatChargesMultiplierNeededForOne = 10;
                        }
                        if (CharacterHasPerkForConsume("chill2g", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.CharacterStatChargesMultiplierNeededForOne = 3;
                            __result.ResistModified = Enums.DamageType.None;
                            __result.ResistModifiedPercentagePerStack = 0.0f;
                        }


                    }
                    break;
                case "wet":
                    // zeald: Zeal on heroes and enemies increases all resistances by 0.5% per Wet charge
                    // medsTexts[perkStem + "wet1d"] = "Wet does not Dispel or Prevent Burn.";

                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("zeal1d", SetAppliesToHeroes, __instance, _characterTarget) && _characterTarget.HasEffect("zeal"))
                        {
                            Plugin.Log.LogDebug(debugBase + "zeal1d");
                            __result.ResistModified3 = Enums.DamageType.All;
                            // __result.ResistModifiedPercentagePerStack3 = 0.5f;
                            __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.All, 0, 0.5f);

                        }
                        if (TeamHasPerkForSet("wet1d", AppliesGlobally, __instance, _characterTarget))
                        {
                            // Not sure if this is working
                            AuraCurseData noneAC = GetAuraCurseData("None");
                            __result.PreventedAuraCurse = (AuraCurseData)null;
                            __result.PreventedAuraCurseStackPerStack = 0;
                            __result.RemoveAuraCurse = (AuraCurseData)null;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("zeal1d", ConsumeAppliesToHeroes, __instance, _characterCaster) && _characterCaster.HasEffect("zeal"))
                        {
                            //Plugin.Log.LogDebug(debugBase+"zeal1d");
                            __result.ResistModified3 = Enums.DamageType.All;
                            // __result.ResistModifiedPercentagePerStack3 = 0.5f;
                            __result = __instance.GlobalAuraCurseModifyResist(__result, Enums.DamageType.All, 0, 0.5f);
                        }
                        if (TeamHasPerkForConsume("wet1d", AppliesGlobally, __instance, _characterCaster))
                        {
                            // Not sure if this is working
                            AuraCurseData noneAC = GetAuraCurseData("None");
                            __result.PreventedAuraCurse = noneAC;
                            __result.PreventedAuraCurseStackPerStack = 0;
                            __result.RemoveAuraCurse = noneAC;
                        }

                    }
                    break;

                case "spark":
                    // medsTexts[perkStem + "spark2d"] = "Gain +1 Lightning Damage for every 5 stacks of Spark on this hero."
                    // medsTexts[perkStem + "spark2e"] = "Spark deal Fire damage. Spark decreases Fire resistance by 0.5% per charge and Lightning resistance by 0.5% per charge.";


                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("spark2d", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.AuraDamageType = Enums.DamageType.Lightning;
                            __result.AuraDamageIncreasedPerStack = 1;
                            __result.ChargesAuxNeedForOne1 = 5;
                        }
                        if (TeamHasPerkForSet("spark2e", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.DamageTypeWhenConsumed = Enums.DamageType.Fire;
                            __result.ResistModified = Enums.DamageType.Lightning;
                            __result.ResistModified2 = Enums.DamageType.Fire;
                            __result.ResistModifiedPercentagePerStack = -0.5f;
                            __result.ResistModifiedPercentagePerStack2 = -0.5f;

                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("spark2d", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.AuraDamageType = Enums.DamageType.Lightning;
                            __result.AuraDamageIncreasedPerStack = 0.2f;
                            // __result.ChargesAuxNeedForOne1 = 5;
                        }
                        if (TeamHasPerkForConsume("spark2e", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.DamageTypeWhenConsumed = Enums.DamageType.Fire;
                            __result.ResistModified = Enums.DamageType.Lightning;
                            __result.ResistModified2 = Enums.DamageType.Fire;
                            __result.ResistModifiedPercentagePerStack = -0.5f;
                            __result.ResistModifiedPercentagePerStack2 = -0.5f;
                        }
                    }
                    break;

                case "shield":
                    // medsTexts[perkStem + "shield5b"] = "If Restricted Power is enabled, increases Max Charges to 300.";
                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("shield5b", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.MaxCharges = 300;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("shield5b", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.MaxCharges = 300;
                        }
                    }
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.GlobalAuraCurseModificationByTraitsAndItems))]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfixMystical(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {

            Plugin.Log.LogDebug(debugBase + "Executing AC Modifications - Mystical");

            bool ConsumeAppliesToHeroes = false; //flag1
            bool SetAppliesToHeroes = false; //flag2
            bool ConsumeAppliesToMonsters = false; //!flag1
            bool SetAppliesToMonsters = false; //!flag1
            bool AppliesGlobally = true;

            if (_characterCaster != null)
            {
                ConsumeAppliesToHeroes = _characterCaster.IsHero;
                ConsumeAppliesToMonsters = !_characterCaster.IsHero;
            }
            if (_characterTarget != null)
            {
                SetAppliesToHeroes = _characterTarget.IsHero;
                SetAppliesToMonsters = !_characterTarget.IsHero;
            }

            switch (_acId)
            {
                case "ac":
                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("", AppliesGlobally, __instance, _characterTarget))
                        {

                        }
                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("", AppliesGlobally, __instance, _characterCaster))
                        {

                        }
                    }
                    break;
                case "dark":
                    // medsTexts[perkStem + "dark2e"] = "Dark explosions deal Fire damage. Dark reduces Fire resistance by 0.25% per charge in addition to reducing Shadow resistance..";
                    // medsTexts[perkStem + "burn1e"] = "Burn increases the damage dealt by Dark explosions by 0.5% per charge.";
                    // medsTexts[perkStem + "sanctify2d"] = "Every 5 stacks of Sanctify increase the number of Dark charges needed for an explosion by 1.";

                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("dark2e", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.DamageTypeWhenConsumed = Enums.DamageType.Fire;
                            __result.ResistModified2 = Enums.DamageType.Fire;
                            __result.ResistModifiedPercentagePerStack2 = -0.5f;
                        }
                        if (TeamHasPerkForSet("sanctify2d", AppliesGlobally, __instance, _characterTarget) && _characterTarget.HasEffect("sanctify"))
                        {
                            int n = _characterTarget.GetAuraCharges("sanctify");
                            __result.ExplodeAtStacks += FloorToInt(0.2f * n);

                        }
                        if (TeamHasPerkForSet("burn2e", AppliesGlobally, __instance, _characterTarget) && _characterTarget.HasEffect("burn"))
                        {
                            int n_charges = _characterTarget.GetAuraCharges("burn");
                            float multiplier = 1 + 0.05f * n_charges;
                            __result.DamageWhenConsumedPerCharge *= multiplier;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("burn2e", AppliesGlobally, __instance, _characterCaster))
                        {
                            int n_charges = _characterCaster.GetAuraCharges("burn");
                            float multiplier = 1 + 0.05f * n_charges;
                            __result.DamageWhenConsumedPerCharge *= multiplier;

                        }
                        if (TeamHasPerkForConsume("sanctify2d", AppliesGlobally, __instance, _characterCaster) && _characterCaster.HasEffect("sanctify"))
                        {
                            int n = _characterCaster.GetAuraCharges("sanctify");
                            __result.ExplodeAtStacks += FloorToInt(0.2f * n);

                        }
                        if (TeamHasPerkForConsume("dark2e", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.DamageTypeWhenConsumed = Enums.DamageType.Fire;
                            __result.ResistModified2 = Enums.DamageType.Fire;
                            __result.ResistModifiedPercentagePerStack2 = -0.5f;

                        }
                    }

                    break;



                case "decay":
                    // medsTexts[perkStem + "decay1d"] = "Decay purges Reinforce.";

                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("decay1d", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.RemoveAuraCurse = GetAuraCurseData("reinforce");
                        }

                    }
                    break;



                case "zeal":
                    // zealb: Zeal on this hero loses 3 charges per turn rather than all charges
                    // zealc: Zeal on all heroes can stack, but reduces Speed by 2 per charge
                    // zeald: Zeal on heroes and enemies increases all resistances by 0.5% per Wet charge
                    // zeale: When this hero loses Zeal, deal indirect Holy and Fire damage equal to 4x the number of stacks lost to all monsters.
                    // zealf: If this hero dies with Zeal, deal indirect Mind damage to all enemies equal to 5x their Burn/Insane stacks.
                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("zeal1b", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            Plugin.Log.LogDebug(debugBase + "zeal1b");
                            __result.ConsumeAll = false;
                            __result.AuraConsumed = 3;

                        }

                        if (TeamHasPerkForSet("zeal1c", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            Plugin.Log.LogDebug(debugBase + "zeal1c");
                            __result.GainCharges = true;
                            __result.CharacterStatModified = Enums.CharacterStat.Speed;
                            __result.CharacterStatModifiedValuePerStack = -2;

                        }

                    }
                    if (_type == "consume")
                    {
                        if (CharacterHasPerkForConsume("zeal1b", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            Plugin.Log.LogDebug(debugBase + "zeal1b");
                            __result.ConsumeAll = false;
                            __result.AuraConsumed = 3;

                        }
                        if (TeamHasPerkForConsume("zeal1c", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            Plugin.Log.LogDebug(debugBase + "zeal1c");
                            __result.GainCharges = true;
                            __result.CharacterStatModified = Enums.CharacterStat.Speed;
                            __result.CharacterStatModifiedValuePerStack = -2;

                        }

                    }
                    break;

                case "scourge":
                    // scourge1a: Scourge +1.";
                    // scourge1b: Scourge on heroes and monsters loses 3 charges per turn rather than all charges.";
                    // scourge1c: Scourge on monsters can Stack but increases all resists by 3% per stack.";
                    // scourge1d: Scourge deals damage based on Sight rather than Chill.";
                    // scourge1e: Scourge on monsters increases burn damage by 15%/stack";
                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("scourge1b", AppliesGlobally, __instance, _characterTarget))
                        {
                            __result.ConsumeAll = false;
                            __result.AuraConsumed = 3;
                        }
                        if (TeamHasPerkForSet("scourge1c", SetAppliesToMonsters, __instance, _characterTarget))
                        {
                            __result.GainCharges = true;
                            __result.ResistModified2 = Enums.DamageType.All;
                            __result.ResistModifiedPercentagePerStack2 = 3.0f;
                        }
                        if (TeamHasPerkForSet("scourge1d", AppliesGlobally, __instance, _characterTarget))
                        {
                            // __result.DamageTypeWhenConsumed = Enums.DamageType.Fire;
                            __result.ConsumedDamageChargesBasedOnACCharges = GetAuraCurseData("sight");
                            __result.DamageWhenConsumedPerCharge = 2;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("scourge1b", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.ConsumeAll = false;
                            __result.AuraConsumed = 3;
                        }

                        if (TeamHasPerkForConsume("scourge1c", ConsumeAppliesToMonsters, __instance, _characterCaster))
                        {
                            __result.GainCharges = true;
                            __result.ResistModified2 = Enums.DamageType.All;
                            __result.ResistModifiedPercentagePerStack2 = 3.0f;
                        }
                        if (TeamHasPerkForConsume("scourge1d", AppliesGlobally, __instance, _characterCaster))
                        {
                            __result.ConsumedDamageChargesBasedOnACCharges = GetAuraCurseData("sight");
                            __result.DamageWhenConsumedPerCharge = 2;
                        }

                    }
                    break;

                case "weak":
                    // weak1c: Monsters cannot be immune to Weak, but no longer have their damage reduced by Insane.";

                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("weak1c", SetAppliesToMonsters, __instance, _characterTarget))
                        {
                            __result.Preventable = false;
                            __result.AuraDamageIncreasedPercent = -25;
                            __result.HealDonePercent = -25;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("weak1c", ConsumeAppliesToMonsters, __instance, _characterCaster))
                        {
                            __result.Preventable = false;
                            __result.AuraDamageIncreasedPercent = -25;
                            __result.HealDonePercent = -25;
                        }


                    }
                    break;

                case "vitality":
                    // medsTexts[perkStem + "vitality1d"] = "Vitality on this hero dispels Poison.";

                    if (_type == "set")
                    {
                        if (CharacterHasPerkForSet("vitality1d", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.RemoveAuraCurse = GetAuraCurseData("poison");
                        }

                    }
                    break;

                case "bless":
                    // medsTexts[perkStem + "bless1d"] = "Bless on all heroes increases Slashing, Fire, and Holy damage by 3% per charge but does not increase damage by 1.";

                    if (_type == "set")
                    {
                        if (TeamHasPerkForSet("bless1d", SetAppliesToHeroes, __instance, _characterTarget))
                        {
                            __result.AuraDamageType = Enums.DamageType.Slashing;
                            __result.AuraDamageType2 = Enums.DamageType.Fire;
                            __result.AuraDamageType3 = Enums.DamageType.Holy;
                            __result.AuraDamageIncreasedPercentPerStack = 3.0f;
                            __result.AuraDamageIncreasedPercentPerStack2 = 3.0f;
                            __result.AuraDamageIncreasedPercentPerStack3 = 3.0f;
                            __result.AuraDamageIncreasedPerStack = 0.0f;
                        }
                    }
                    if (_type == "consume")
                    {
                        if (TeamHasPerkForConsume("bless1d", ConsumeAppliesToHeroes, __instance, _characterCaster))
                        {
                            __result.AuraDamageType = Enums.DamageType.Slashing;
                            __result.AuraDamageType2 = Enums.DamageType.Fire;
                            __result.AuraDamageType3 = Enums.DamageType.Holy;
                            __result.AuraDamageIncreasedPercentPerStack = 3.0f;
                            __result.AuraDamageIncreasedPercentPerStack2 = 3.0f;
                            __result.AuraDamageIncreasedPercentPerStack3 = 3.0f;
                            __result.AuraDamageIncreasedPerStack = 0.0f;
                        }
                    }
                    break;
            }
        }
    }
}
