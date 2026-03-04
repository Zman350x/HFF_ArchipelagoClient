namespace HffArchipelagoClient
{
    using HarmonyLib;

    public static class InputLimiter
    {
        public static bool canGrabLeft = false;
        public static bool canGrabRight = false;
        public static bool canJump = false;
        public static bool canPlayDead = false;
        public static bool canLookLeft = false;
        public static bool canLookRight = false;
        public static bool canLookUp = false;
        public static bool canLookDown = false;
        public static bool canShootFireworks = false;

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
