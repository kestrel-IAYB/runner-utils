using HarmonyLib;
using TMPro;
using UnityEngine;

namespace RunnerUtils.Components;

public class HardFallOverlay : ComponentBase<HardFallOverlay>
{
    public override string Identifier => "HF Overlay";
    public override bool ShowOnFairPlay => true;

    private GameObject m_obj;
    private TMP_Text m_text;

    public void SetupIndicator() {
        var gps = GameManager.instance.player.GetHUD().GetGPS();
        m_obj = Object.Instantiate(new GameObject(), gps.transform.parent.gameObject.transform);
        var font = gps.transform.parent.GetComponentInChildren<TextMeshProUGUI>().font;

        m_obj.name = "RunnerUtils~HF Indicator";
        m_text = m_obj.AddComponent<TextMeshProUGUI>();
        m_text.fontSize = 35;
        m_text.font = font;
        m_text.alignment = TextAlignmentOptions.BottomLeft;

        var transform = m_obj.GetComponent<RectTransform>();
        if (!transform) transform = m_obj.AddComponent<RectTransform>();

        transform.anchoredPosition = new Vector2(-1085, -645);
    }

    [HarmonyPatch(typeof(PlayerMovement))]
    public class PlayerMovementPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void UpdateIndicator(PlayerMovement __instance) {
            Instance.m_obj?.SetActive(Instance.enabled);
            Instance.m_text.text = __instance.IsHardFalling()?"<color=#FF0000>HF": "<color=#ED424240>HF";
        }
    }
}