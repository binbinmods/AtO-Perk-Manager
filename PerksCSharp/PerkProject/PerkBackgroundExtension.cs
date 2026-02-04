using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using static Obeliskial_Essentials.Essentials;
using System;
using UnityEngine;


namespace PerkManager
{
    [HarmonyPatch]
    public class PerkBackgroundExtension
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerkNodeAmplify), "SetForNodes")]
        public static bool SetForNodesPrefix(PerkNodeAmplify __instance, int _numNodes)
        {
            // LogDebug(Plugin.debugBase + "Extending Clickbox for Perks");
            PolygonCollider2D component = __instance.GetComponent<PolygonCollider2D>();
            float horizontalShift = -0.7f * (_numNodes - 1);
            if (_numNodes == 2)
            {
                __instance.bg2.gameObject.SetActive(true);
                __instance.bg3.gameObject.SetActive(false);
                __instance.bg4.gameObject.SetActive(false);
                __instance.amplifyNodes.localPosition = new Vector3(horizontalShift, __instance.amplifyNodes.localPosition.y, __instance.amplifyNodes.localPosition.z);
                component.points = __instance.bg2.GetComponent<PolygonCollider2D>().points;
            }
            else if (_numNodes == 3)
            {
                __instance.bg2.gameObject.SetActive(false);
                __instance.bg3.gameObject.SetActive(true);
                __instance.bg4.gameObject.SetActive(false);
                __instance.amplifyNodes.localPosition = new Vector3(horizontalShift, __instance.amplifyNodes.localPosition.y, __instance.amplifyNodes.localPosition.z);
                component.points = __instance.bg3.GetComponent<PolygonCollider2D>().points;
            }
            else
            {
                __instance.bg2.gameObject.SetActive(false);
                __instance.bg3.gameObject.SetActive(false);
                __instance.bg4.gameObject.SetActive(true);
                __instance.amplifyNodes.localPosition = new Vector3(horizontalShift, __instance.amplifyNodes.localPosition.y, __instance.amplifyNodes.localPosition.z);
                float scaleFactor = _numNodes / 4.0f;
                __instance.bg4.transform.localScale = new Vector3(scaleFactor, 1.0f, 1.0f);
                PolygonCollider2D backgroundComponent = __instance.bg4.GetComponent<PolygonCollider2D>();
                Vector2[] originalPoints = backgroundComponent.points;
                for (int i = 0; i < originalPoints.Length; i++)
                {
                    originalPoints[i] = new Vector2(originalPoints[i].x * scaleFactor, originalPoints[i].y);
                }
                component.points = originalPoints;
            }

            return false;

        }

        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(PerkTree), "DrawTree")]
        // public static void DrawTreePrefix()
        // {
        //     // No idea why but this debug log seems to be necessary for the other prefix to work. No clue what's going on.
        //     Plugin.LogDebug(Plugin.debugBase + "Draw Tree");
        // }

    }
}