using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MistbornMod.Content.Items;
using MistbornMod.Content.Items.HemalurgicSpikes;
using MistbornMod.Content.Items.CombinationVials;

namespace MistbornMod.Recipes
{
    public class AllMetalRecipes : ModSystem
    {
        public override void AddRecipes()
        {
            // Base requirements
            int oreAmount = 4;
            int barAmount = 1;
            
            // === EXISTING VIAL RECIPES ===
            
            // --- Iron Vial Recipe ---
            Recipe ironVialRecipe = Recipe.Create(ModContent.ItemType<IronVial>());
            ironVialRecipe.AddIngredient(ItemID.BottledWater); // Water as base
            ironVialRecipe.AddIngredient(ItemID.IronBar, barAmount); // Iron metal
            ironVialRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol component for mixture
            // No tile requirement - craftable by hand
            ironVialRecipe.Register();

            // --- Steel Vial Recipe ---
            Recipe steelVialRecipe = Recipe.Create(ModContent.ItemType<SteelVial>());
            steelVialRecipe.AddIngredient(ItemID.BottledWater);
            steelVialRecipe.AddIngredient(ItemID.IronBar, barAmount); // Iron base
            steelVialRecipe.AddIngredient(ItemID.Coal, 2); // Carbon to convert to steel
            steelVialRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol component
            steelVialRecipe.AddTile(TileID.Furnaces); // Requires furnace
            steelVialRecipe.Register();

            // --- Pewter Vial Recipe ---
            Recipe pewterVialRecipe = Recipe.Create(ModContent.ItemType<PewterVial>());
            pewterVialRecipe.AddIngredient(ItemID.BottledWater);
            pewterVialRecipe.AddIngredient(ItemID.TinBar, 1); // Tin component
            pewterVialRecipe.AddIngredient(ItemID.LeadBar, 1); // Lead component
            pewterVialRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol base
            pewterVialRecipe.AddTile(TileID.Furnaces); // Requires furnace for mixing
            pewterVialRecipe.Register();

            // --- Tin Vial Recipe ---
            Recipe tinVialRecipe = Recipe.Create(ModContent.ItemType<TinVial>());
            tinVialRecipe.AddIngredient(ItemID.BottledWater);
            tinVialRecipe.AddIngredient(ItemID.TinBar, barAmount);
            tinVialRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol component
            // No tile requirement
            tinVialRecipe.Register();

            // --- Brass Vial Recipe ---
            Recipe brassVialRecipe = Recipe.Create(ModContent.ItemType<BrassVial>());
            brassVialRecipe.AddIngredient(ItemID.BottledWater);
            brassVialRecipe.AddIngredient(ItemID.CopperBar, 1); // Copper component
            brassVialRecipe.AddIngredient(ModContent.ItemType<ZincOre>(), 2); // Zinc component
            brassVialRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol base
            brassVialRecipe.AddTile(TileID.Furnaces); // Requires furnace for alloy
            brassVialRecipe.Register();

            // --- Zinc Vial Recipe ---
            Recipe zincVialRecipe = Recipe.Create(ModContent.ItemType<ZincVial>());
            zincVialRecipe.AddIngredient(ItemID.BottledWater);
            zincVialRecipe.AddIngredient(ModContent.ItemType<ZincOre>(), oreAmount);
            zincVialRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol component
            // No tile requirement
            zincVialRecipe.Register();
            
            // --- Copper Vial Recipe ---
            Recipe copperVialRecipe = Recipe.Create(ModContent.ItemType<CopperVial>());
            copperVialRecipe.AddIngredient(ItemID.BottledWater);
            copperVialRecipe.AddIngredient(ItemID.CopperBar, barAmount);
            copperVialRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol component
            // No tile requirement
            copperVialRecipe.Register();

            // --- Bronze Vial Recipe ---
            Recipe bronzeVialRecipe = Recipe.Create(ModContent.ItemType<BronzeVial>());
            bronzeVialRecipe.AddIngredient(ItemID.BottledWater);
            bronzeVialRecipe.AddIngredient(ItemID.CopperBar, 1); // Copper component
            bronzeVialRecipe.AddIngredient(ItemID.TinBar, 1); // Tin component
            bronzeVialRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol component
            bronzeVialRecipe.AddTile(TileID.Furnaces); // Requires furnace for alloy
            bronzeVialRecipe.Register();
            
            // --- Atium Vial Recipe 1 (by hand from Atium Bead) ---
            Recipe atiumVialSimpleRecipe = Recipe.Create(ModContent.ItemType<AtiumVial>());
            atiumVialSimpleRecipe.AddIngredient(ItemID.BottledWater);
            atiumVialSimpleRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol base
            atiumVialSimpleRecipe.AddIngredient(ModContent.ItemType<AtiumBead>(), 1); // Use Atium bead as source
            // No tile requirement
            atiumVialSimpleRecipe.Register();
            
            // --- Atium Vial Recipe 2 (alternative complex recipe) ---
            Recipe atiumVialComplexRecipe = Recipe.Create(ModContent.ItemType<AtiumVial>());
            atiumVialComplexRecipe.AddIngredient(ItemID.BottledWater);
            atiumVialComplexRecipe.AddIngredient(ItemID.FallenStar, 3); // Celestial component
            atiumVialComplexRecipe.AddIngredient(ItemID.SoulofLight, 1); // Essence component
            atiumVialComplexRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol base
            atiumVialComplexRecipe.AddTile(TileID.DemonAltar); // Special altar still required for direct creation
            atiumVialComplexRecipe.Register();

            // --- Chromium Vial Recipe (special testing metal) ---
            Recipe chromiumVialRecipe = Recipe.Create(ModContent.ItemType<ChromiumVial>());
            chromiumVialRecipe.AddIngredient(ItemID.BottledWater);
            chromiumVialRecipe.AddIngredient(ItemID.PlatinumBar, 1); // High-tier metal as substitute
            chromiumVialRecipe.AddIngredient(ItemID.MeteoriteBar, 1); // Special component
            chromiumVialRecipe.AddIngredient(ItemID.Ale, 1); // Alcohol component
            chromiumVialRecipe.AddTile(TileID.MythrilAnvil); // Requires higher-tier crafting
            chromiumVialRecipe.Register();

            // --- Lerasium Bead Recipe ---
            Recipe lerasiumBeadRecipe = Recipe.Create(ModContent.ItemType<LerasiumBead>());
            // High-tier ingredients to make it challenging to craft
            lerasiumBeadRecipe.AddIngredient(ItemID.LifeCrystal, 1); // Life Crystal
            lerasiumBeadRecipe.AddIngredient(ItemID.FallenStar, 5);  // Fallen Stars
            lerasiumBeadRecipe.AddIngredient(ItemID.GoldBar, 5);     // Gold Bars
            lerasiumBeadRecipe.AddIngredient(ItemID.SoulofLight, 5); // Souls of Light 
            lerasiumBeadRecipe.AddIngredient(ItemID.SoulofNight, 5); // Souls of Night
            // Require a special crafting station
            lerasiumBeadRecipe.AddTile(TileID.DemonAltar); // At a Demon Altar
            lerasiumBeadRecipe.Register();

            // --- Atium Bead Recipe ---
            Recipe atiumBeadRecipe = Recipe.Create(ModContent.ItemType<AtiumBead>());
            // High-tier ingredients to make it challenging to craft
            atiumBeadRecipe.AddIngredient(ItemID.FallenStar, 5);  // Fallen Stars
            atiumBeadRecipe.AddIngredient(ItemID.SoulofLight, 1); // Souls of Light 
            atiumBeadRecipe.AddIngredient(ItemID.SoulofNight, 1); // Souls of Night
            // Require a special crafting station
            atiumBeadRecipe.AddTile(TileID.DemonAltar); // At a Demon Altar
            atiumBeadRecipe.Register();

            // --- Metal Tester Recipe ---
            Recipe recipe = Recipe.Create(ModContent.ItemType<MetalTester>());
            recipe.AddIngredient(ItemID.Glass, 3);          // Reduced from 5
            recipe.AddIngredient(ItemID.IronBar, 1);        // Reduced from 2
            recipe.AddIngredient(ItemID.CopperBar, 1);      // Reduced from 2
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
            
            // Make it an alternative requirement (iron OR lead)
            Recipe recipe2 = Recipe.Create(ModContent.ItemType<MetalTester>());
            recipe2.AddIngredient(ItemID.Glass, 3);
            recipe2.AddIngredient(ItemID.LeadBar, 1);
            recipe2.AddIngredient(ItemID.CopperBar, 1);
            recipe2.AddTile(TileID.Anvils);
            recipe2.Register();

            // === NEW: HEMALURGIC SPIKE RECIPES ===
            
            // --- Bone Spike Recipes (Early Game) ---
            Recipe boneIronSpikeRecipe = Recipe.Create(ModContent.ItemType<BoneIronSpike>());
            boneIronSpikeRecipe.AddIngredient(ItemID.Bone, 10);
            boneIronSpikeRecipe.AddIngredient(ItemID.IronBar, 2);
            boneIronSpikeRecipe.AddIngredient(ItemID.DarkShard, 1); // Dark essence
            boneIronSpikeRecipe.AddTile(TileID.Anvils);
            boneIronSpikeRecipe.Register();
            
            Recipe boneSteelSpikeRecipe = Recipe.Create(ModContent.ItemType<BoneSteelSpike>());
            boneSteelSpikeRecipe.AddIngredient(ItemID.Bone, 10);
            boneSteelSpikeRecipe.AddIngredient(ItemID.IronBar, 2);
            boneSteelSpikeRecipe.AddIngredient(ItemID.Coal, 3);
            boneSteelSpikeRecipe.AddIngredient(ItemID.DarkShard, 1);
            boneSteelSpikeRecipe.AddTile(TileID.Anvils);
            boneSteelSpikeRecipe.Register();
            
            // --- Shadow Spike Recipes (Mid Game) ---
            Recipe shadowPewterSpikeRecipe = Recipe.Create(ModContent.ItemType<ShadowPewterSpike>());
            shadowPewterSpikeRecipe.AddIngredient(ItemID.ShadowScale, 5);
            shadowPewterSpikeRecipe.AddIngredient(ItemID.TinBar, 1);
            shadowPewterSpikeRecipe.AddIngredient(ItemID.LeadBar, 1);
            shadowPewterSpikeRecipe.AddIngredient(ItemID.SoulofNight, 2);
            shadowPewterSpikeRecipe.AddTile(TileID.DemonAltar);
            shadowPewterSpikeRecipe.Register();
            
            Recipe shadowTinSpikeRecipe = Recipe.Create(ModContent.ItemType<ShadowTinSpike>());
            shadowTinSpikeRecipe.AddIngredient(ItemID.ShadowScale, 5);
            shadowTinSpikeRecipe.AddIngredient(ItemID.TinBar, 3);
            shadowTinSpikeRecipe.AddIngredient(ItemID.SoulofNight, 2);
            shadowTinSpikeRecipe.AddTile(TileID.DemonAltar);
            shadowTinSpikeRecipe.Register();
            
            // --- Blood Spike Recipes (Late Game) ---
            Recipe bloodZincSpikeRecipe = Recipe.Create(ModContent.ItemType<BloodZincSpike>());
            bloodZincSpikeRecipe.AddIngredient(ItemID.Vertebrae, 15); // Blood component
            bloodZincSpikeRecipe.AddIngredient(ModContent.ItemType<ZincOre>(), 5);
            bloodZincSpikeRecipe.AddIngredient(ItemID.SoulofNight, 3);
            bloodZincSpikeRecipe.AddIngredient(ItemID.SoulofLight, 1);
            bloodZincSpikeRecipe.AddTile(TileID.MythrilAnvil);
            bloodZincSpikeRecipe.Register();
            
            Recipe bloodBrassSpikeRecipe = Recipe.Create(ModContent.ItemType<BloodBrassSpike>());
            bloodBrassSpikeRecipe.AddIngredient(ItemID.Vertebrae, 15);
            bloodBrassSpikeRecipe.AddIngredient(ItemID.CopperBar, 2);
            bloodBrassSpikeRecipe.AddIngredient(ModContent.ItemType<ZincOre>(), 3);
            bloodBrassSpikeRecipe.AddIngredient(ItemID.SoulofNight, 3);
            bloodBrassSpikeRecipe.AddIngredient(ItemID.SoulofLight, 1);
            bloodBrassSpikeRecipe.AddTile(TileID.MythrilAnvil);
            bloodBrassSpikeRecipe.Register();
            
            // --- Atium Spike Recipe (End Game) ---
            Recipe atiumAtiumSpikeRecipe = Recipe.Create(ModContent.ItemType<AtiumAtiumSpike>());
            atiumAtiumSpikeRecipe.AddIngredient(ModContent.ItemType<AtiumBead>(), 3);
            atiumAtiumSpikeRecipe.AddIngredient(ItemID.SoulofFright, 1);
            atiumAtiumSpikeRecipe.AddIngredient(ItemID.SoulofMight, 1);
            atiumAtiumSpikeRecipe.AddIngredient(ItemID.SoulofSight, 1);
            atiumAtiumSpikeRecipe.AddIngredient(ItemID.Ectoplasm, 5);
            atiumAtiumSpikeRecipe.AddTile(TileID.LunarCraftingStation);
            atiumAtiumSpikeRecipe.Register();

            // === NEW: COMBINATION VIAL RECIPES ===
            
            // --- Physical Enhancement Elixir ---
            Recipe physicalElixirRecipe = Recipe.Create(ModContent.ItemType<PhysicalElixir>());
            physicalElixirRecipe.AddIngredient(ModContent.ItemType<PewterVial>(), 1);
            physicalElixirRecipe.AddIngredient(ModContent.ItemType<TinVial>(), 1);
            physicalElixirRecipe.AddIngredient(ItemID.BottledWater, 1);
            physicalElixirRecipe.AddIngredient(ItemID.Gel, 2); // Binding agent
            physicalElixirRecipe.AddTile(TileID.AlchemyTable);
            physicalElixirRecipe.Register();
            
            // --- Emotional Mastery ---
            Recipe emotionalMasteryRecipe = Recipe.Create(ModContent.ItemType<EmotionalMastery>());
            emotionalMasteryRecipe.AddIngredient(ModContent.ItemType<BrassVial>(), 1);
            emotionalMasteryRecipe.AddIngredient(ModContent.ItemType<ZincVial>(), 1);
            emotionalMasteryRecipe.AddIngredient(ItemID.BottledWater, 1);
            emotionalMasteryRecipe.AddIngredient(ItemID.Gel, 2);
            emotionalMasteryRecipe.AddTile(TileID.AlchemyTable);
            emotionalMasteryRecipe.Register();
            
            // --- Metallic Mastery ---
            Recipe metallicMasteryRecipe = Recipe.Create(ModContent.ItemType<MetallicMastery>());
            metallicMasteryRecipe.AddIngredient(ModContent.ItemType<IronVial>(), 1);
            metallicMasteryRecipe.AddIngredient(ModContent.ItemType<SteelVial>(), 1);
            metallicMasteryRecipe.AddIngredient(ItemID.BottledWater, 1);
            metallicMasteryRecipe.AddIngredient(ItemID.Gel, 2);
            metallicMasteryRecipe.AddTile(TileID.AlchemyTable);
            metallicMasteryRecipe.Register();
            
            // --- Detection Bundle ---
            Recipe detectionBundleRecipe = Recipe.Create(ModContent.ItemType<DetectionBundle>());
            detectionBundleRecipe.AddIngredient(ModContent.ItemType<CopperVial>(), 1);
            detectionBundleRecipe.AddIngredient(ModContent.ItemType<BronzeVial>(), 1);
            detectionBundleRecipe.AddIngredient(ItemID.BottledWater, 1);
            detectionBundleRecipe.AddIngredient(ItemID.Gel, 2);
            detectionBundleRecipe.AddTile(TileID.AlchemyTable);
            detectionBundleRecipe.Register();
            
            // --- Allomantic Supremacy (End Game) ---
            Recipe allomanticSupremacyRecipe = Recipe.Create(ModContent.ItemType<AllomanticSupremacy>());
            allomanticSupremacyRecipe.AddIngredient(ModContent.ItemType<IronVial>(), 1);
            allomanticSupremacyRecipe.AddIngredient(ModContent.ItemType<SteelVial>(), 1);
            allomanticSupremacyRecipe.AddIngredient(ModContent.ItemType<TinVial>(), 1);
            allomanticSupremacyRecipe.AddIngredient(ModContent.ItemType<PewterVial>(), 1);
            allomanticSupremacyRecipe.AddIngredient(ModContent.ItemType<ZincVial>(), 1);
            allomanticSupremacyRecipe.AddIngredient(ModContent.ItemType<BrassVial>(), 1);
            allomanticSupremacyRecipe.AddIngredient(ModContent.ItemType<CopperVial>(), 1);
            allomanticSupremacyRecipe.AddIngredient(ModContent.ItemType<BronzeVial>(), 1);
            allomanticSupremacyRecipe.AddIngredient(ItemID.LifeFruit, 1); // Life fruit for ultimate mixture
            allomanticSupremacyRecipe.AddIngredient(ItemID.SoulofFlight, 3);
            allomanticSupremacyRecipe.AddTile(TileID.LunarCraftingStation);
            allomanticSupremacyRecipe.Register();

            // === NEW: STEEL ANCHOR AMMO RECIPE ===
            Recipe steelAnchorAmmoRecipe = Recipe.Create(ModContent.ItemType<SteelAnchorAmmo>(), 100);
            steelAnchorAmmoRecipe.AddIngredient(ItemID.MusketBall, 100);
            steelAnchorAmmoRecipe.AddIngredient(ItemID.IronBar, 1);
            steelAnchorAmmoRecipe.AddIngredient(ItemID.Chain, 1);
            steelAnchorAmmoRecipe.AddTile(TileID.Anvils);
            steelAnchorAmmoRecipe.Register();
            
            // Alternative recipe with lead
            Recipe steelAnchorAmmoRecipe2 = Recipe.Create(ModContent.ItemType<SteelAnchorAmmo>(), 100);
            steelAnchorAmmoRecipe2.AddIngredient(ItemID.MusketBall, 100);
            steelAnchorAmmoRecipe2.AddIngredient(ItemID.LeadBar, 1);
            steelAnchorAmmoRecipe2.AddIngredient(ItemID.Chain, 1);
            steelAnchorAmmoRecipe2.AddTile(TileID.Anvils);
            steelAnchorAmmoRecipe2.Register();
        }
    }
}