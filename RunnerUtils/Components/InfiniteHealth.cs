using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace RunnerUtils.Components;

public class InfiniteHealth : ComponentBase<InfiniteHealth>
{
    public override string Identifier => "Infinite Health";
    public override bool ShowOnFairPlay => true;

    [HarmonyPatch(typeof(DebugManager), nameof(DebugManager.GetInfiniteHealth))]
    public class DebugManagerPatch
    {
        [HarmonyPrefix]
        public static bool SetInfiniteHealth(ref bool __result) {
            if (Instance.enabled) {
                __result = true;
                return false;
            }

            return true;
        }
    }
}