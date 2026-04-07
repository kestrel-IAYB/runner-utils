using UnityEngine;
using RunnerUtils.Components;
using RunnerUtils.Components.UI;

namespace RunnerUtils;

public static class FairPlay
{
    private static InGameLog igl = new InGameLog($" ", 10);
    public static bool triggersModified = false;

    public static void Init() {
        igl.anchoredPos = new Vector2(800, 567);
        igl.sizeDelta = new Vector2(750, 250);
        igl.textAlignment = TMPro.TextAlignmentOptions.TopRight;
        igl.Setup();
    }

    public static void Update() {

        if (!igl.TextComponent) return;
        igl.Clear();
        foreach (var comp in ComponentBase.components) {
            if (!comp.ShowOnFairPlay || !comp.enabled) continue;
            igl.LogLine(comp.Identifier);
        }
        if (triggersModified) igl.LogLine("Triggers");
        if (LocationSave.savedPosition.HasValue || LocationSave.savedRotation.HasValue) igl.LogLine($"Location Saved{(RunnerUtilsSettings.SaveLocationVerboseEnabled ? $" ({LocationSave.StringLoc})":"")}");

    }
}