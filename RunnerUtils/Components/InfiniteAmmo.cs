using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace RunnerUtils.Components;

public class InfiniteAmmo : ComponentBase<InfiniteAmmo>
{
    public override string Identifier => "Ammo";
    public override bool ShowOnFairPlay => true;

    public override void Enable() {
        base.Enable();
        ReloadSlots();
    }

    public override void Disable() {
        base.Disable();
        ReloadSlots();
    }

    static void ReloadSlots() {
        try {
            GameManager.instance.player.GetHUD().GetAmmoIndicator()
                .LoadInSlots(GameManager.instance.player.GetArmScript().GetEquippedWeapon());
        }
        catch {
            // ignored
        }
    }

    private static void ColorAmmoSlots(List<HUDAmmoIndicatorSlot> slots, Color color) {
        foreach (var slot in slots) {
            slot.lowAmmoColor = color;
            slot.LowAmmo();
        }
    }

    [HarmonyPatch(typeof(HUDAmmoIndicator), nameof(HUDAmmoIndicator.LoadInSlots))]
    public class PatchColorAmmoSlots
    {
        [HarmonyPostfix]
        public static void Postfix(ref List<HUDAmmoIndicatorSlot> ___spawnedSlots) {
            if (Instance.enabled) {
                ColorAmmoSlots(___spawnedSlots, Color.red);
            }
            else {
                ColorAmmoSlots(___spawnedSlots, Color.white);
                foreach (var slot in ___spawnedSlots) {
                    slot.lowAmmoColor = Color.yellow;
                }
            }
        }
    }

    [HarmonyPatch(typeof(DebugManager), nameof(DebugManager.GetInfiniteAmmo))]
    public class PatchInfiniteAmmo
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result) {
            if (Instance.enabled) {
                __result = true;
                return false;
            }
            else {
                return true;
            }
        }
    }
}