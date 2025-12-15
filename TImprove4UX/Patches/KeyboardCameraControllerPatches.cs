namespace TImprove4UX.Patches;

[HarmonyPatch(typeof(KeyboardCameraController))]
public static class KeyboardCameraControllerPatches
{

    [HarmonyPrefix, HarmonyPatch(nameof(KeyboardCameraController.KeyboardJumpRotationAngle))]
    public static bool JumpToCardinalDirection(KeyboardCameraController __instance, ref float __result)
    {
        var input = __instance._inputService;

        bool? moveLeft = input.IsKeyDown("RotateCordinalLeft")
            ? true
            : (input.IsKeyDown("RotateCordinalRight")
                ? false
                : null);

        if (moveLeft is null) { return true; }

        __result = CalculateCardinalDelta(!moveLeft.Value, __instance);
        return false;
    }

    static float CalculateCardinalDelta(bool moveRight, KeyboardCameraController c)
    {
        float curr = c._cameraService.HorizontalAngle;

        const float step = 90f;
        const float tolerance = 1e-4f; // Adjustable tolerance for "approximately zero"

        // Compute lower cardinal and distance within the sector [0, 90)
        float lowerCardinal = Mathf.Floor(curr / step) * step;
        float distanceToLower = curr - lowerCardinal;

        // Detect if we are effectively on a cardinal angle
        bool isOnCardinal = Mathf.Abs(distanceToLower) < tolerance ||
                            Mathf.Abs(distanceToLower - step) < tolerance; // Handles rare overflow cases

        float directionSign = moveRight ? +1f : -1f;

        float delta;

        if (isOnCardinal)
        {
            delta = directionSign * step;
        }
        else
        {
            if (moveRight)
            {
                delta = directionSign * distanceToLower;
            }
            else
            {
                delta = directionSign * (step - distanceToLower);
            }
        }

        return delta;
    }

}
