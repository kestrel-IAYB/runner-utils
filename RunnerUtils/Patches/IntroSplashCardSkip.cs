using HarmonyLib;
using Progress;

namespace RunnerUtils.Patches;

[HarmonyPatch(typeof(ProgressManager), nameof(ProgressManager.ShouldDisplayGameIntroOnStart))]
public static class IntroSplashCardSkip
{
    [HarmonyPrefix]
    public static void SkipSplashCards(ref bool __result) {
        if (Configs.SkipSplashCardsEnabled) {
            __result = false;
        }
    }
}