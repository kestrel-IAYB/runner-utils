using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using RunnerUtils.Components;
using RunnerUtils.Components.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RunnerUtils;

[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
public class Mod : BaseUnityPlugin
{
    public const string pluginGuid = "kestrel.iamyourbeast.runnerutils";
    public const string pluginName = "Runner Utils";
    public const string pluginVersion = "2.4.2";

    public static Mod Instance { get; private set; }
    private static RUInputManager InputManager { get; set; } = new();
    public static InGameLog Igl { get; private set; } = new InGameLog($"{pluginName}~Ingame Log (v{pluginVersion})");
    internal static new ManualLogSource Logger;

    private static string loadBearingColonThree = ":3";

    public void Awake() {
        if (loadBearingColonThree != ":3") Application.Quit();

        gameObject.hideFlags = HideFlags.HideAndDontSave; //fuck you unity
        Instance = this;
        Logger = base.Logger;

        RunnerUtilsSettings.InitialiseConfig();
        new Harmony(pluginGuid).PatchAll();
        Logger.LogInfo("Hiiiiiiiiiiii :3");
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update() {
        if (GameManager.instance.player is not null) {
            FairPlay.Update();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Additive) ShowTriggers.ExtendRegistry();
        ViewCones.OnSceneLoad();
    }

    [HarmonyPatch(typeof(Player))]
    public class PatchPlayer
    {
        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        public static void PlayerInitPostfix() {
            ShowTriggers.RegisterAll();
            Igl.Setup();
            ThrowCam.Reset();
            FairPlay.Init();
            HardFallOverlay.Instance.SetupIndicator();
            MovementDebug.Instance.Init();
        }
    }

    [HarmonyPatch(typeof(PlayerMovement))]
    public class PatchPlayerMovement
    {
        [HarmonyPatch("Initialize")]
        [HarmonyPostfix]
        public static void MovementInitPostfix(CharacterController ___controller) {
            if (RunnerUtilsSettings.WalkabilityOverlayEnabled) {
                foreach (var t in Terrain.activeTerrains) {
                    var terrainData = t.terrainData;
                    WalkabilityOverlay.MakeWalkabilityTex(ref  terrainData, ___controller.slopeLimit);
                    t.terrainData =  terrainData;
                }
            }
        }
    }

    [HarmonyPatch(typeof(HUDLevelTimer), "Update")]
    public class The
    {
        [HarmonyPostfix]
        public static void Postfix(ref TMP_Text ___gradeText) {
            ___gradeText.text = $" {___gradeText.text}";
        }
    }
}