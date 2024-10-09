using BepInEx;
using BepInEx.Configuration;

namespace UncrustedSandwich
{
    public static class UncrustedSandwichConfig
    {
        public static ConfigEntry<int> ReworkType { get; set; }

        public static void SetUpConfig(BaseUnityPlugin plugin)
        {
            ReworkType = plugin.Config.Bind(
                "Uncrusted Sandwich",
                "Toggle Rework", 1,
                new ConfigDescription(
                    "[ 0 = Lunar | 1 = Replace Bison Steak ]",
                    new AcceptableValueRange<int>(0, 1)
                )
            );
        }
    }
}