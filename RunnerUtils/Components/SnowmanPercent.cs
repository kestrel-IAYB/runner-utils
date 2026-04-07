using HarmonyLib;
using RunnerUtils.Components.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunnerUtils.Components;

public static class SnowmanPercent
{
    [HarmonyPatch(typeof(Snowman), "Kill")]
    public class PatchSnowman
    {
        [HarmonyPostfix]
        public static void Postfix() {
            if (!RunnerUtilsSettings.SnowmanPercentEnabled) return;
            float time = GameManager.instance.levelController.GetCombatTimer().GetTime();
            GameManager.instance.player.GetHUD().GetNotificationPopUp().TriggerPopUp($"Snowman%: {time:0.00}", HUDNotificationPopUp.ThreatLevel.High);
        }
    }
}