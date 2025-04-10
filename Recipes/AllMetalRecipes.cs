using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MistbornMod.Items;

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
             // Alcohol component for mixture
            // No tile requirement - craftable by hand
            ironVialRecipe.Register();

            // --- Steel Vial Recipe ---
            Recipe steelVialRecipe = Recipe.Create(ModContent.ItemType<SteelVial>());
            steelVialRecipe.AddIngredient(ItemID.BottledWater);
            steelVialRecipe.AddIngredient(ItemID.IronBar, barAmount); // Iron base
            steelVialRecipe.AddIngredient(ItemID.Coal, 2); // Carbon to convert to steel
             // Alcohol component
            // No tile requirement
            steelVialRecipe.Register();

            // --- Pewter Vial Recipe ---
            Recipe pewterVialRecipe = Recipe.Create(ModContent.ItemType<PewterVial>());
            pewterVialRecipe.AddIngredient(ItemID.BottledWater);
            pewterVialRecipe.AddIngredient(ItemID.TinBar, 1); // Tin component
            pewterVialRecipe.AddIngredient(ItemID.LeadBar, 1); // Lead component
             // Alcohol base
            // No tile requirement
            pewterVialRecipe.Register();

            // --- Tin Vial Recipe ---
            Recipe tinVialRecipe = Recipe.Create(ModContent.ItemType<TinVial>());
            tinVialRecipe.AddIngredient(ItemID.BottledWater);
            tinVialRecipe.AddIngredient(ItemID.TinBar, barAmount);
             // Alcohol component
            // No tile requirement
            tinVialRecipe.Register();

            // --- Brass Vial Recipe ---
            Recipe brassVialRecipe = Recipe.Create(ModContent.ItemType<BrassVial>());
            brassVialRecipe.AddIngredient(ItemID.BottledWater);
            brassVialRecipe.AddIngredient(ItemID.CopperBar, 1); // Copper component
            brassVialRecipe.AddIngredient(ModContent.ItemType<ZincOre>(), 2); // Zinc component
             // Alcohol base
            // No tile requirement
            brassVialRecipe.Register();

            // --- Zinc Vial Recipe ---
            Recipe zincVialRecipe = Recipe.Create(ModContent.ItemType<ZincVial>());
            zincVialRecipe.AddIngredient(ItemID.BottledWater);
            zincVialRecipe.AddIngredient(ModContent.ItemType<ZincOre>(), oreAmount);
             // Alcohol component
            // No tile requirement
            zincVialRecipe.Register();
            
            // --- Copper Vial Recipe ---
            Recipe copperVialRecipe = Recipe.Create(ModContent.ItemType<CopperVial>());
            copperVialRecipe.AddIngredient(ItemID.BottledWater);
            copperVialRecipe.AddIngredient(ItemID.CopperBar, barAmount);
             // Alcohol component
            // No tile requirement
            copperVialRecipe.Register();

            // --- Bronze Vial Recipe ---
            Recipe bronzeVialRecipe = Recipe.Create(ModContent.ItemType<BronzeVial>());
            bronzeVialRecipe.AddIngredient(ItemID.BottledWater);
            bronzeVialRecipe.AddIngredient(ItemID.CopperBar, 1); // Copper component
            bronzeVialRecipe.AddIngredient(ItemID.TinBar, 1); // Tin component
             // Alcohol component
            // No tile requirement
            bronzeVialRecipe.Register();
            
            // --- Atium Vial Recipe 1 (by hand from Atium Bead) ---
            Recipe atiumVialSimpleRecipe = Recipe.Create(ModContent.ItemType<AtiumVial>());
            atiumVialSimpleRecipe.AddIngredient(ItemID.BottledWater);
             // Alcohol base
            atiumVialSimpleRecipe.AddIngredient(ModContent.ItemType<AtiumBead>(), 1); // Use Lerasium bead as source
            // No tile requirement
            atiumVialSimpleRecipe.Register();
            
            // --- Atium Vial Recipe 2 (alternative complex recipe) ---
            Recipe atiumVialComplexRecipe = Recipe.Create(ModContent.ItemType<AtiumVial>());
            atiumVialComplexRecipe.AddIngredient(ItemID.BottledWater);
            atiumVialComplexRecipe.AddIngredient(ItemID.FallenStar, 3); // Celestial component
            atiumVialComplexRecipe.AddIngredient(ItemID.SoulofLight, 1); // Essence component
             // Alcohol base
            atiumVialComplexRecipe.AddTile(TileID.DemonAltar); // Special altar still required for direct creation
            atiumVialComplexRecipe.Register();

             Recipe lerasiumBeadRecipe = Recipe.Create(ModContent.ItemType<LerasiumBead>());
            // High-tier ingredients to make it challenging to craft
            lerasiumBeadRecipe.AddIngredient(ItemID.LifeCrystal, 1); // Life Crystal
            lerasiumBeadRecipe.AddIngredient(ItemID.FallenStar, 5);  // Fallen Stars
            lerasiumBeadRecipe.AddIngredient(ItemID.GoldBar, 5);     // Gold Bars
            lerasiumBeadRecipe.AddIngredient(ItemID.SoulofLight, 5); // Souls of Light 
            lerasiumBeadRecipe.AddIngredient(ItemID.SoulofNight, 5); // Souls of Night
            // Require a special crafting station
            lerasiumBeadRecipe.AddTile(TileID.DemonAltar); // At a Demon Altar
            
            // Register the recipe
            lerasiumBeadRecipe.Register();

            Recipe AtiumBeadRecipe = Recipe.Create(ModContent.ItemType<AtiumBead>());
            // High-tier ingredients to make it challenging to craft
            AtiumBeadRecipe.AddIngredient(ItemID.FallenStar, 5);  // Fallen Stars
            AtiumBeadRecipe.AddIngredient(ItemID.SoulofLight, 1); // Souls of Light 
            AtiumBeadRecipe.AddIngredient(ItemID.SoulofNight, 1); // Souls of Night
            // Require a special crafting station
            AtiumBeadRecipe.AddTile(TileID.DemonAltar); // At a Demon Altar
            
            // Register the recipe
            AtiumBeadRecipe.Register();
        }
    }
}