using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MistbornMod.Content.Items;

namespace MistbornMod.Recipes
{
    public class AllMetalRecipes : ModSystem
    {
        public override void AddRecipes()
        {
            // Base requirements
            int oreAmount = 4;
            int barAmount = 1;
            
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

            Recipe recipe = Recipe.Create(ModContent.ItemType<MetalTester>());
    recipe.AddIngredient(ItemID.Glass, 3);          // Reduced from 5
    recipe.AddIngredient(ItemID.IronBar, 1);        // Reduced from 2
    recipe.AddIngredient(ItemID.CopperBar, 1);      // Reduced from 2
    // Make it an alternative requirement (iron OR lead)
    Recipe recipe2 = Recipe.Create(ModContent.ItemType<MetalTester>());
    recipe2.AddIngredient(ItemID.Glass, 3);
    recipe2.AddIngredient(ItemID.LeadBar, 1);
    recipe2.AddIngredient(ItemID.CopperBar, 1);
    recipe2.AddTile(TileID.Anvils);
    recipe2.Register();
        }
    }
}