using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using HarmonyLib;
using System.Reflection;
using Vintagestory.Client.NoObf;
using Newtonsoft.Json;
using CustomizableRiftWard;

namespace CustomizableRiftWard
{

    public static class RiftWardConfig
    {
        public static RiftWardConfigData Loaded = new RiftWardConfigData();

        public class RiftWardConfigData
        {
            public double riftBlockChance = 0.95;
            public float maxRange = 30f;
        }


    }


    public class CustomizableRiftWard : ModSystem
    {
        public bool patched;
        public override void StartPre(ICoreAPI api)
        {
            if (!patched)
            {
                new Harmony("com.ascyst.customizableriftward").PatchAll();
                patched = true;
            }
            string cfgFileName = "CustomizableRiftWard.json";
            try
            {
                RiftWardConfig.Loaded = api.LoadModConfig<RiftWardConfig.RiftWardConfigData>(cfgFileName);
                if (RiftWardConfig.Loaded == null)
                {
                    RiftWardConfig.Loaded = new RiftWardConfig.RiftWardConfigData();
                    api.StoreModConfig(RiftWardConfig.Loaded, cfgFileName);
                }
            }
            catch (System.Exception ex)
            {
                api.Logger.Error("Error loading or saving " + cfgFileName + ": " + ex.Message);
            }
        }
    }
}


    // Harmony Patch to modify Vintagestory.GameContent.BlockEntityRiftWard's private void BlockEntityRiftWard_OnRiftSpawned(Rift rift)
    [HarmonyPatch(typeof(BlockEntityRiftWard), "BlockEntityRiftWard_OnRiftSpawned")]
    public class BlockEntityRiftWard_OnRiftSpawned_Patch
    {

        // Prefix because I'm way too lazy to figure out transpiling right now
        static bool Prefix(BlockEntityRiftWard __instance, Rift rift)
        {


            var hasFuelProperty = AccessTools.Property(typeof(BlockEntityRiftWard), "HasFuel");
            bool hasFuel = (bool)hasFuelProperty.GetValue(__instance);
            var sapiField = AccessTools.Field(typeof(BlockEntityRiftWard), "sapi");
            ICoreServerAPI sapi = (ICoreServerAPI)sapiField.GetValue(__instance);

            var riftsBlockedField = AccessTools.Field(typeof(BlockEntityRiftWard), "riftsBlocked");
            int riftsBlocked = (int)riftsBlockedField.GetValue(__instance);

            double riftBlockChance = RiftWardConfig.Loaded.riftBlockChance;
            float maxRange = RiftWardConfig.Loaded.maxRange;

            if (hasFuel && sapi.World.Rand.NextDouble() <= riftBlockChance && rift.Position.DistanceTo((double)__instance.Pos.X + 0.5, (double)(__instance.Pos.Y + 1), (double)__instance.Pos.Z + 0.5) < maxRange)
            {
                rift.Size = 0f;
                riftsBlocked++;
                __instance.MarkDirty(false, null);
            }
            // return false to skip the original method
            return false;
        }
    }
