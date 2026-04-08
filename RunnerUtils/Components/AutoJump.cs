using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace RunnerUtils.Components;

public class AutoJump : ComponentBase<AutoJump>
{
    public override string Identifier => "Auto Jump";
    public override bool ShowOnFairPlay => true;
    
    [HarmonyPatch(typeof(PlayerMovement), nameof(PlayerMovement.UpdateJumpCheck))]
    public static class PlayerMovementPatch
    {
        public static bool InputCheckDetour(InputCheck inputCheck) {
            if (Instance.enabled) {
                return inputCheck.Held();
            }

            return inputCheck.Pressed();
        }
        
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AutoJumpIfActionHeld(IEnumerable<CodeInstruction> instructions) {
            return new CodeMatcher(instructions)
                .MatchForward(false, 
                    new CodeMatch(OpCodes.Ldc_I4_0),    
                    new CodeMatch(
                        OpCodes.Callvirt,
                        AccessTools.Method(typeof(InputCheck), nameof(InputCheck.Pressed))
                    )
                )
                .RemoveInstruction()
                .Set(OpCodes.Call, AccessTools.Method(typeof(PlayerMovementPatch), nameof(InputCheckDetour)))
                .InstructionEnumeration();
        }
    }
}