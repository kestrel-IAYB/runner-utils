using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using Progress;
using RunnerUtils.Components.UI;

namespace RunnerUtils.Components;

public class IntroSplashCardSkip
{
    [HarmonyPatch(typeof(ProgressManager), nameof(ProgressManager.ShouldDisplayGameIntroOnStart))]
    public static class PatchSplashCard
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (!RunnerUtilsSettings.SkipSplashCardsEnabled)
            {
                return true;
            }

            Mod.Logger.LogInfo("Skipping the splash cards");
            
            __result = false;
            return false;
        }
    }
}