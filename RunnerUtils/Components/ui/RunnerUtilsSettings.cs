using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Fleece;
using UnityEngine.UI;
using BepInEx;
using BepInEx.Configuration;
using TMPro;

namespace RunnerUtils.Components.UI;
internal class RunnerUtilsSettings
{

    private static ConfigEntry<bool> skipSplashCards;
    private static ConfigEntry<float> throwCam_rangeScalar;
    private static ConfigEntry<bool> throwCam_unlockCamera;
    private static ConfigEntry<bool> throwCam_autoSwitch;
    private static ConfigEntry<bool> walkabilityOverlay;

    private static ConfigEntry<bool> saveLocation_verbose;

    private static ConfigEntry<bool> snowmanPercent;

    public static void InitialiseConfig()
    {
        var config = Mod.Instance.Config;
        skipSplashCards = config.Bind("Options", "Skip Splash Cards", false, "Skip the splash cards on boot");

        throwCam_unlockCamera = config.Bind("Throw Cam", "Unlock Camera", false, "Unlock the camera when in throw cam");
        throwCam_rangeScalar = config.Bind("Throw Cam", "Camera Range", 0.2f, new ConfigDescription("Follow range of the throw cam", new AcceptableValueRange<float>(0.01f, 3.0f)));
        throwCam_autoSwitch = config.Bind("Throw Cam", "Auto Switch", false, "Automatically switch to throw cam when a projectile is thrown");
        walkabilityOverlay = config.Bind("Walkability Overlay", "Enable", false, "Makes all walkable surfaces appear snowy, and all unwalkable surfaces appear black");

        saveLocation_verbose = config.Bind("Location Save", "Verbose", false, "Log the exact location and rotation when a save or load is performed");

        snowmanPercent = config.Bind("Snowman%", "Enable", true, "Displays your time upon destroying a snowman, to time the (silly) category snowman%");
    }

    public static bool SkipSplashCardsEnabled { get { return skipSplashCards.Value; } }
    public static bool ThrowCamUnlockCameraEnabled { get { return throwCam_unlockCamera.Value; } }
    public static float ThrowCamCameraRange { get { return throwCam_rangeScalar.Value; } }
    public static bool ThrowCamAutoSwitchEnabled { get { return throwCam_autoSwitch.Value; } }
    public static bool WalkabilityOverlayEnabled { get { return walkabilityOverlay.Value; } }
    public static bool SaveLocationVerboseEnabled { get { return saveLocation_verbose.Value; } }
    public static bool SnowmanPercentEnabled { get { return snowmanPercent.Value; } }

    // Custom settings window to change these settings
    public class UISettingsSubMenuCustom : UISettingsSubMenu
    {
        private UISettingsOptionToggle skipSplashCardsToggle;
        private UISettingsOptionToggle walkabilityOverlayToggle;
        private UISettingsOptionToggle verboseLocationSaveToggle;
        private UISettingsOptionToggle snowmanPercentToggle;
        private UISettingsOptionToggle throwCamUnlockCameraToggle;
        private UISettingsOptionToggle throwCamAutoSwitchToggle;

        private static Jumper splashCardSkipText = Text.MakeJumper("Skip the splash cards");
        private static Jumper walkabilityOverlayText = Text.MakeJumper("Walkability Overlay");
        private static Jumper verboseLocationSaveText = Text.MakeJumper("Log exact location on save/load");
        private static Jumper snowmanPercentText = Text.MakeJumper("Enable Snowman% Timer");
        private static Jumper throwCamUnlockCameraText = Text.MakeJumper("Unlock Camera");
        private static Jumper throwCamAutoSwitchText = Text.MakeJumper("Auto Switch");

        public override void Start()
        {
            base.Start();

            // TODO: once this gets too big for the window
            // (if it's smaller than the window it's buggy for some reason, TODO fix that)
            var content = Base.MakeScrollable(gameObject);

            // setup our settings
            var heading = Base.MakeHeading(
                content.transform,
                "RunnerUtils Settings",
                "Some of these options are banned from regular play.\nPlease show the top right corner during all recordings with this mod."
            );
            // heading's default padding is for if it's in the middle of a page
            heading.GetComponent<VerticalLayoutGroup>().padding.top = 0;
            heading.GetComponent<VerticalLayoutGroup>().padding.bottom = 10;

            skipSplashCardsToggle = Base.MakeToggleOption(content.transform, splashCardSkipText, skipSplashCards.Value);
            walkabilityOverlayToggle = Base.MakeToggleOption(content.transform, walkabilityOverlayText, walkabilityOverlay.Value);
            verboseLocationSaveToggle = Base.MakeToggleOption(content.transform, verboseLocationSaveText, saveLocation_verbose.Value);
            snowmanPercentToggle = Base.MakeToggleOption(content.transform, snowmanPercentText, snowmanPercent.Value);

            var throwCamHeading = Base.MakeHeading(content.transform, "Throw Cam");
            throwCamHeading.GetComponent<VerticalLayoutGroup>().padding.top = 10;
            throwCamHeading.GetComponent<VerticalLayoutGroup>().padding.bottom = 0;

            throwCamUnlockCameraToggle = Base.MakeToggleOption(content.transform, throwCamUnlockCameraText, throwCam_unlockCamera.Value);
            throwCamAutoSwitchToggle = Base.MakeToggleOption(content.transform, throwCamAutoSwitchText, throwCam_autoSwitch.Value);
            // TODO: slider for throw cam camera range
        }

        public override void SaveSettings()
        {
            base.SaveSettings();

            skipSplashCards.Value = skipSplashCardsToggle.GetToggled();
            walkabilityOverlay.Value = walkabilityOverlayToggle.GetToggled();
            saveLocation_verbose.Value = verboseLocationSaveToggle.GetToggled();
            snowmanPercent.Value = snowmanPercentToggle.GetToggled();

            throwCam_unlockCamera.Value = throwCamUnlockCameraToggle.GetToggled();
            throwCam_autoSwitch.Value = throwCamAutoSwitchToggle.GetToggled();

            Mod.Instance.Config.Save();
        }
    }
}
