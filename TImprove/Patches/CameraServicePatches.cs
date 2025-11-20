using System.Reflection.Emit;

namespace TImprove.Patches;

[HarmonyPatch(typeof(CameraService))]
public static class CameraServicePatches
{

    [HarmonyTranspiler, HarmonyPatch(nameof(CameraService.ModifyVerticalAngle))]
    public static IEnumerable<CodeInstruction> LockCameraAngle(IEnumerable<CodeInstruction> instructions)
    {
        var ins = instructions.ToList();

        var method = typeof(CameraService).PropertyGetter(nameof(CameraService.FreeMode));
        var index = ins.FindIndex(q => q.opcode == OpCodes.Call && q.operand is MethodInfo m && m == method);

        if (index == -1)
        {
            throw new InvalidOperationException("Could not find CameraService.FreeMode in ModifyVerticalAngle.");
        }

        ins[index] = new(OpCodes.Call, typeof(CameraServicePatches).Method(nameof(ShouldUnlockCameraAngle)));

        return ins;
    }

    public static bool ShouldUnlockCameraAngle(CameraService cameraService) 
        => cameraService.FreeMode && MSettings.Instance?.FreeCameraLockAngle.Value != true;

}
