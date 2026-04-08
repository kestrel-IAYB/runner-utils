using HarmonyLib;

namespace RunnerUtils.Extensions;

public static class CodeMatcherExtensions
{
    public static CodeMatcher Dump(this CodeMatcher matcher) {
        foreach (var instruction in matcher.InstructionEnumeration()) {
            Mod.Logger.LogInfo(instruction);
        }

        return matcher;
    }
}