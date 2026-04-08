using HarmonyLib;
using RunnerUtils.UI;

namespace RunnerUtils.Components;

[HarmonyPatch(typeof(Snowman), nameof(Snowman.Kill))]
public static class SnowmanPercent
{
    [HarmonyPostfix]
    public static void ShowPopUp() {
        if (!Configs.SnowmanPercentEnabled) return;
        float time = GameManager.instance.levelController.GetCombatTimer().GetTime();
        GameManager.instance.player.GetHUD().GetNotificationPopUp().TriggerPopUp($"Snowman%: {time:0.00}", HUDNotificationPopUp.ThreatLevel.High);
    }
}