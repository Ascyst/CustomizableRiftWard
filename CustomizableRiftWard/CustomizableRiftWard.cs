using System; 
using Vintagestory.API.Common;  
using Vintagestory.API.Server;  
using Vintagestory.API.Client;  
using Vintagestory.API.Config;  
using Vintagestory.API.MathTools;   
using Vintagestory.GameContent;
using HarmonyLib;
using System.Reflection;




namespace CustomizableRiftWard
{

    public class RiftWardConfig : ModSystem
    {
        public static double riftBlockChance = 0.95;
        public static float maxRange = 150f;
    }
    // Create a class that implements the ModSystem interface
    public class CustomizableRiftWard : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
        }

    }

    // Harmony Patch to modify Vintagestory.GameContent.BlockEntityRiftWard's private void BlockEntityRiftWard_OnRiftSpawned(Rift rift)
    [HarmonyPatch(typeof(BlockEntityRiftWard), "BlockEntityRiftWard_OnRiftSpawned")]
    public class BlockEntityRiftWard_OnRiftSpawned_Patch
    {
        // Create a static method that will be called when the patch is applied
        static bool Prefix(BlockEntityRiftWard __instance, Rift rift)
        {
            var hasFuelProperty = AccessTools.Property(typeof(BlockEntityRiftWard), "HasFuel");
            bool hasFuel = (bool)hasFuelProperty.GetValue(__instance);
            var sapiField = AccessTools.Field(typeof(BlockEntityRiftWard), "sapi");
            ICoreServerAPI sapi = (ICoreServerAPI)sapiField.GetValue(__instance);

            var riftsBlockedField = AccessTools.Field(typeof(BlockEntityRiftWard), "riftsBlocked");
            int riftsBlocked = (int)riftsBlockedField.GetValue(__instance);
            // If rift is spawned and the riftward has fuel and the rift is within 30 blocks of the riftward
            if (hasFuel && sapi.World.Rand.NextDouble() <= RiftWardConfig.riftBlockChance && rift.Position.DistanceTo((double)__instance.Pos.X + 0.5, (double)(__instance.Pos.Y + 1), (double)__instance.Pos.Z + 0.5) < RiftWardConfig.maxRange)
            {
                rift.Size = 0f;
                riftsBlocked++;
                __instance.MarkDirty(false, null);
            }
            // return false to skip the original method
            return false;
        }
    }




}