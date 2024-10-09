using BepInEx;
using R2API;
using RoR2;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace UncrustedSandwich
{
    // This is an example plugin that can be put in
    // BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    // It's a small plugin that adds a relatively simple item to the game,
    // and gives you that item whenever you press F2.

    // This attribute specifies that we have a dependency on a given BepInEx Plugin,
    // We need the R2API ItemAPI dependency because we are using for adding our item to the game.
    // You don't need this if you're not using R2API in your plugin,
    // it's just to tell BepInEx to initialize R2API before this plugin so it's safe to use R2API.
    [BepInDependency(ItemAPI.PluginGUID)]

    // This one is because we use a .language file for language tokens
    // More info in https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Assets/Localization/
    [BepInDependency(LanguageAPI.PluginGUID)]

    // This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    // This is the main declaration of our plugin class.
    // BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    // BaseUnityPlugin itself inherits from MonoBehaviour,
    // so you can use this as a reference for what you can declare and use in your plugin class
    // More information in the Unity Docs: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class UncrustedSandwich : BaseUnityPlugin
    {
        // The Plugin GUID should be a unique ID for this plugin,
        // which is human readable (as it is used in places like the config).
        // If we see this PluginGUID as it is on thunderstore,
        // we will deprecate this mod.
        // Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Egglectro";
        public const string PluginName = "UncrustedSandwich";
        public const string PluginVersion = "1.0.6";

        // We need our item definition to persist through our functions, and therefore make it a class field.
        private static ItemDef uncrustedSandwich;

        // The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            // Init our logging class so that we can properly log for debugging
            Log.Init(Logger);
            UncrustedSandwichConfig.SetUpConfig(this);

            // Define item
            uncrustedSandwich = ScriptableObject.CreateInstance<ItemDef>();

            // Language Tokens
            uncrustedSandwich.name = "SANDWICH_NAME";
            uncrustedSandwich.nameToken = "SANDWICH_NAME";
            uncrustedSandwich.pickupToken = "SANDWICH_PICKUP";
            uncrustedSandwich.descriptionToken = "SANDWICH_DESC";
            uncrustedSandwich.loreToken = "SANDWICH_LORE";

            if (UncrustedSandwichConfig.ReworkType.Value == 0)
            {

                // The tier determines what rarity the item is:
                // Tier1=white, Tier2=green, Tier3=red, Lunar=Lunar, Boss=yellow,
                // and finally NoTier is generally used for helper items, like the tonic affliction
#pragma warning disable Publicizer001 // Accessing a member that was not originally public. Here we ignore this warning because with how this example is setup we are forced to do this
                uncrustedSandwich._itemTierDef = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/LunarTierDef.asset").WaitForCompletion();
#pragma warning restore Publicizer001
                // Instead of loading the itemtierdef directly, you can also do this like below as a workaround
                // myItemDef.deprecatedTier = ItemTier.Tier2;

                // Add Assets
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UncrustedSandwich.uncrustedsandwich"))
                {
                    var bundle = AssetBundle.LoadFromStream(stream);

                    uncrustedSandwich.pickupModelPrefab = bundle.LoadAsset<GameObject>("Assets/Import/sandwich/sandwich.prefab");
                    uncrustedSandwich.pickupIconSprite = bundle.LoadAsset<Sprite>("Assets/Import/sandwich_icon/sandwich_icon.png");
                }

                ItemAPI.ApplyTagToItem("Any", uncrustedSandwich);

                // For some reason, this doesn't work
                uncrustedSandwich.canRemove = false;

                uncrustedSandwich.hidden = false;

                var displayRules = new ItemDisplayRuleDict(null);

                // Then finally add it to R2API
                ItemAPI.Add(new CustomItem(uncrustedSandwich, displayRules));
            }
            if (UncrustedSandwichConfig.ReworkType.Value == 1)
            {
                uncrustedSandwich = Addressables.LoadAssetAsync<ItemDef>("RoR2/Base/FlatHealth/FlatHealth.asset").WaitForCompletion();

                // Add Assets
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("UncrustedSandwich.uncrustedsandwich"))
                {
                    var bundle = AssetBundle.LoadFromStream(stream);

                    uncrustedSandwich.pickupModelPrefab = bundle.LoadAsset<GameObject>("Assets/Import/sandwich/sandwich.prefab");
                    uncrustedSandwich.pickupIconSprite = bundle.LoadAsset<Sprite>("Assets/Import/sandwich_icon/sandwich_icon.png");
                }
            }

            // Add Effects
            On.RoR2.CharacterBody.OnInventoryChanged += UncrustableEffect;
        }

        // Uncrustable Effect
        private void UncrustableEffect (On.RoR2.CharacterBody.orig_OnInventoryChanged orig, RoR2.CharacterBody self)
        {
            orig(self);  // Call the original method to retain default functionality
            if (NetworkServer.active){

                // Get the item count for the Uncrustable
                int itemCount = self.inventory ? self.inventory.GetItemCount(uncrustedSandwich) : 0;
                if (itemCount > 0)
                {
                    if (itemCount == 8)
                    {
                        if (!self.HasBuff(RoR2.RoR2Content.Buffs.Cripple))
                        {
                            self.AddBuff(RoR2.RoR2Content.Buffs.Cripple);
                            Logger.LogInfo($"Cripple Added");
                        }
                    }
                    else
                    {
                        if (self.HasBuff(RoR2.RoR2Content.Buffs.Cripple))
                        {
                            self.RemoveBuff(RoR2.RoR2Content.Buffs.Cripple);
                            Logger.LogInfo($"Cripple Removed");
                        }
                    }

                    Logger.LogInfo($"Uncrustable activated. Item count: {itemCount}");
                }
            }
            else
            {
                Logger.LogWarning("Not server, cannot apply Uncrustable rework.");
            }
        }
    }
}
