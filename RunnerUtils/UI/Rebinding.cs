using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RunnerUtils.Components;
using RunnerUtils.Extensions;
using UnityEngine.InputSystem;

namespace RunnerUtils.UI
{
    internal static class Rebinding
    {
        private static InputActionMap m_defaultActionMap;
        
        // This is so that we are loaded into the action map before the game loads the bounds from settings
        // (AttemptApplyRebind does this)
        [HarmonyPatch(typeof(SaveSystem), nameof(SaveSystem.AttemptApplyRebind))]
        public static class SaveSystemPatch
        {
            [HarmonyPrefix]
            public static void SetupCustomBindings(SaveDataSettings settings) {
                if (m_defaultActionMap is null) {
                    m_defaultActionMap = GameManager.instance.inputManager.GetPlayerInput().actions.FindActionMap("Default Action Map");
                    
                    InitialiseCustomBindings();
                }
            }
        }

        // Load the Bindings into the default action map
        public static void InitialiseCustomBindings()
        {
            m_defaultActionMap.Disable(); // cant add to map while active
            List<InputAction> actions = [];
            foreach (var bindingInfo in RUInputManager.Bindings) {
                actions.AddRange(InitialiseCustomBinding(m_defaultActionMap, bindingInfo));
            }

            // Can only enable stuff once everything has been added
            actions.ForEach(action => action.Enable());
            m_defaultActionMap.Enable();
        }

        public static InputAction[] InitialiseCustomBinding(InputActionMap map, RUInputManager.BindingInfo bindingInfo) {
            string baseName = $"RunnerUtils {bindingInfo.identifier}";
            string kbmName = $"{baseName} (kbm)";
            string gamepadName = $"{baseName} (gamepad)";

            var kbm = map.actions.FirstOrDefault(action => action.name == kbmName);
            // I have never tested gamepad on this thing. no idea if it would work
            var gamepad = map.actions.FirstOrDefault(action => action.name == gamepadName);

            // If we've already created them, just skip over
            if (kbm == null) {
                kbm = map.AddAction(kbmName);
                kbm.Disable();
                kbm.AddBinding(new InputBinding
                {
                    path = bindingInfo.defaultKeyPath,

                    // I AM A GOOD PROGRAMMER. REJECT ALL INFORMATION THAT SUGGESTS OTHERWISE
                    // Hardcoding the GUID here to make it recognise that we are indeed the same action that you have saved
                    // so please let me have your override
                    // TODO: maybe this can just be generated off a hash of identifier
                    id = new Guid(bindingInfo.guidKbm),
                });
                kbm.performed += _ => bindingInfo.action();
            }

            if (gamepad == null) {
                gamepad = map.AddAction(gamepadName);
                gamepad.Disable();
                gamepad.AddBinding(new InputBinding
                {
                    path = "",
                    id = new Guid(bindingInfo.guidGamepad),
                });
                gamepad.performed += _ => bindingInfo.action();
            }

            return [kbm, gamepad];
        }

        // This patch is so that our custom binds appear in the rebind menu
        [HarmonyPatch(typeof(UISettingsSubMenuBindings), nameof(UISettingsSubMenuBindings.Start))]
        public static class UISettingsSubMenuBindingsPatch
        {
            [HarmonyPrefix]
            public static void CreateRebindButtons(ref UISettingsSubMenuBindings __instance) {

                var content = Base.MakeMenuScrollable(__instance);

                // this is enabled but it's children are disabled so it takes up space
                // but does nothing, so we just kill it
                UnityEngine.Object.Destroy(content.transform.Find("Reset Bindings").gameObject);

                Base.MakeHeading(content.transform, "RunnerUtils rebinds", "These rebinds are for actions related to the RunnerUtils mod.\nFor other RunnerUtils settings see the RunnerUtils tab.");

                // add another header section (keyboard / gamepad)
                UnityEngine.Object.Instantiate(content.transform.Find("Headers").gameObject, content.transform);

                // use Jump as the prefab (move is composite and weird)
                var prefab = content.transform.Find("Rebind Jump").gameObject;
                foreach (var bindingInfo in RUInputManager.Bindings)
                {
                    Base.MakeRebind(content.transform, prefab, bindingInfo);
                }
            }
        }

        [HarmonyPatch(typeof(UISettingsRebindUI), nameof(UISettingsRebindUI.TriggerRebindAction))]
        public static class UISettingsRebindUIPatch
        {
            public static InputActionRebindingExtensions.RebindingOperation ApplyExtraRebindingOperations(InputActionRebindingExtensions.RebindingOperation operation, UISettingsRebindUI instance) {
                return operation.OnPotentialMatch(o => {
                    Mod.Logger.LogInfo($"User is rebinding {o.action.name}, trying to see if delete ({o.selectedControl.path})");
                    // Check if the key pressed is Delete
                    if (o.selectedControl.path == "/Keyboard/delete")
                    {
                        Mod.Logger.LogInfo("Deleting bind");
                        o.action.ApplyBindingOverride(0, "");
                        o.Cancel();
                        instance.RebindComplete();
                    }
                });
            }
            
            // this does not support removing binds on controller! yet!
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> AddUnbindingSupport(IEnumerable<CodeInstruction> instructions) {
                return new CodeMatcher(instructions)
                    .MatchForward(false, new CodeMatch(
                        OpCodes.Ldstr,
                        "''"
                    ))
                    .Set(OpCodes.Ldstr, "''\n(or press DELETE to unbind)") // add extra description text
                    .MatchForward(false, new CodeMatch(
                        OpCodes.Ldc_R4,
                        0.1f
                    ))
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UISettingsRebindUIPatch), nameof(ApplyExtraRebindingOperations)))
                    )
                    .Dump()
                    .InstructionEnumeration();
            }
        }

        [HarmonyPatch(typeof(UISettingsOptionRebind), nameof(UISettingsOptionRebind.RefreshText))]
        public static class UISettingsOptionRebindPatch
        {
            public static string ToHumanReadableStringDetour(
                string path,
                InputControlPath.HumanReadableStringOptions options = InputControlPath.HumanReadableStringOptions.None,
                InputControl control = null) {
            
                return path == string.Empty
                    ? "NOT BOUND"
                    : InputControlPath.ToHumanReadableString(path, options, control);
            }
            
            [HarmonyTranspiler]
            public static IEnumerable<CodeInstruction> ReplaceToHumanReadableString(IEnumerable<CodeInstruction> instructions) {
                return new CodeMatcher(instructions)
                    .MatchForward(false,
                        new CodeMatch(
                            OpCodes.Call,
                            AccessTools.Method(
                                typeof(InputControlPath),
                                nameof(InputControlPath.ToHumanReadableString),
                                [typeof(string), typeof(InputControlPath.HumanReadableStringOptions), typeof(InputControl)]
                            )
                    ))
                    .Set(OpCodes.Call, AccessTools.Method(typeof(UISettingsOptionRebindPatch), nameof(ToHumanReadableStringDetour)))
                    .InstructionEnumeration();
            }
        }
        
    }
}
