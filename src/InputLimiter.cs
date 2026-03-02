namespace HffArchipelagoClient
{
    using HarmonyLib;

    public static class InputLimiter
    {
        public static bool canGrabLeft = true;
        public static bool canGrabRight = true;
        public static bool canJump = true;
        public static bool canPlayDead = true;
        public static bool canLookLeft = true;
        public static bool canLookRight = true;
        public static bool canLookUp = true;
        public static bool canLookDown = true;
        public static bool canShootFireworks = true;

        public static void Patch()
        {
            Harmony.CreateAndPatchAll(typeof(InputLimiter), "InputLimiter");
        }

        public static void Unpatch()
        {
            Harmony.UnpatchID("InputLimiter");
        }

        [HarmonyPatch(typeof(InControl.PlayerAction), "Update")]
        [HarmonyPrefix]
        public static void Update(InControl.PlayerAction __instance, ref bool __runOriginal)
        {
            __runOriginal = true;

            switch (__instance.Name)
            {
                case "Left Hand":
                    __runOriginal = canGrabLeft;
                    break;
                case "Right Hand":
                    __runOriginal = canGrabRight;
                    break;
                case "Jump":
                    __runOriginal = canJump;
                    break;
                case "Play Dead":
                    __runOriginal = canPlayDead;
                    break;
                case "Look Left":
                    __runOriginal = canLookLeft;
                    break;
                case "Look Right":
                    __runOriginal = canLookRight;
                    break;
                case "Look Up":
                    __runOriginal = canLookUp;
                    break;
                case "Look Down":
                    __runOriginal = canLookDown;
                    break;
                case "Shoot Fireworks":
                    __runOriginal = canShootFireworks;
                    break;
                default:
                    break;
            }
        }
    }
}
