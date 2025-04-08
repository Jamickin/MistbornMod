using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MistbornMod.Items; // Ensure this using directive points to where your vial items are defined

namespace MistbornMod.Recipes
{
    public class AllMetalRecipes : ModSystem
    {
        public override void AddRecipes()
        {
            int oreAmount = 5; // Standard amount of ore per vial
            int barAmount = 2; // Standard amount for bars if used

            // --- Iron Vial Recipe ---
            Recipe ironVialRecipe = Recipe.Create(ModContent.ItemType<IronVial>());
            ironVialRecipe.AddIngredient(ItemID.BottledWater);
            // Requires Iron Ore OR Lead Ore, depending on world generation
            ironVialRecipe.AddRecipeGroup(RecipeGroupID.IronBar, barAmount); // Using bars might make more sense than ore for consistency
            // Alternative using Ore: ironVialRecipe.AddCondition(Condition.PlayerCarriesItem(ItemID.IronOre, oreAmount)); // This condition doesn't work directly, use RecipeGroup or specific ore
            // Let's use a RecipeGroup for Iron/Lead ore as well if preferred over bars:
            // ironVialRecipe.AddRecipeGroup("IronOre", oreAmount); // Requires setting up "IronOre" RecipeGroup elsewhere if not default
            // Simplest: Just require Iron Ore directly, players might need Lead Vial recipe too if world has Lead.
            // Let's stick to the user's original:
            // ironVialRecipe.AddIngredient(ItemID.IronOre, oreAmount); // Use this if your world has Iron
            // ironVialRecipe.AddIngredient(ItemID.LeadOre, oreAmount); // Use this if your world has Lead - Create separate LeadVial? No, Iron/Lead are equivalents.
             // Best approach: Use the Iron Ore item directly, assuming Iron exists or is the primary.
             // If Lead is the world equivalent, TModLoader might handle recipe substitution, or you might need a separate LeadVial item/recipe.
             // For simplicity here, we assume Iron Ore is desired.
            ironVialRecipe.AddIngredient(ItemID.IronOre, oreAmount);
            ironVialRecipe.AddTile(TileID.Bottles); // Use Alchemy Table or Bottles as station
            ironVialRecipe.Register();

            // --- Steel Vial Recipe ---
            // Steel isn't a direct ore. Often represented by Iron + strengthening agent.
            Recipe steelVialRecipe = Recipe.Create(ModContent.ItemType<SteelVial>()); // Assumes SteelVial class exists
            steelVialRecipe.AddIngredient(ItemID.BottledWater);
            steelVialRecipe.AddIngredient(ItemID.IronBar, barAmount); // Use Iron Bar as base
            // steelVialRecipe.AddIngredient(ItemID.Coal); // If you have Coal modded in
            // Or just require more iron/different station? Let's use Iron Bar.
            steelVialRecipe.AddTile(TileID.Anvils); // Steel implies stronger forging - Use Anvil? Or keep Bottles? Let's use Anvil.
            steelVialRecipe.Register();

            // --- Pewter Vial Recipe ---
            // Pewter is an alloy, typically Tin + Lead (or Copper/Antimony).
            Recipe pewterVialRecipe = Recipe.Create(ModContent.ItemType<PewterVial>()); // Assumes PewterVial class exists
            pewterVialRecipe.AddIngredient(ItemID.BottledWater);
            pewterVialRecipe.AddIngredient(ItemID.TinOre, oreAmount / 2 + 1); // Requires Tin
            pewterVialRecipe.AddIngredient(ItemID.LeadOre, oreAmount / 2 + 1); // Requires Lead
            pewterVialRecipe.AddTile(TileID.Bottles);
            pewterVialRecipe.Register();
            // Note: This recipe requires the world to have *both* Tin and Lead available, which isn't default.
            // Alternative: Use Tin Bar + Lead Bar at Anvil? Or Tin + Shadow Scale/Tissue Sample?
            // Let's stick to ores for now. Player might need to acquire both via other means if world only has one.

            // --- Tin Vial Recipe ---
            Recipe tinVialRecipe = Recipe.Create(ModContent.ItemType<TinVial>()); // Assumes TinVial class exists
            tinVialRecipe.AddIngredient(ItemID.BottledWater);
            tinVialRecipe.AddIngredient(ItemID.TinOre, oreAmount); // Requires Tin Ore OR Zinc Ore
            // Use RecipeGroup if Zinc is the alternative
            // tinVialRecipe.AddRecipeGroup("TinOre", oreAmount); // Requires setting up "TinOre" RecipeGroup elsewhere
            // Simplest: Assume Tin Ore is desired.
            tinVialRecipe.AddIngredient(ItemID.TinOre, oreAmount);
            tinVialRecipe.AddTile(TileID.Bottles);
            tinVialRecipe.Register();

             // --- Brass Vial Recipe (Updated) ---
            Recipe brassVialRecipe = Recipe.Create(ModContent.ItemType<BrassVial>()); 
            brassVialRecipe.AddIngredient(ItemID.BottledWater);
            brassVialRecipe.AddIngredient(ItemID.CopperOre, oreAmount / 2 + 1); 
            // Use ModContent.ItemType for your modded Zinc Ore
            brassVialRecipe.AddIngredient(ModContent.ItemType<ZincOre>(), oreAmount / 2 + 1); 
            brassVialRecipe.AddTile(TileID.Bottles);
            brassVialRecipe.Register();
            // Note: Still requires Copper Ore naturally.

            // --- Zinc Vial Recipe (Updated) ---
            Recipe zincVialRecipe = Recipe.Create(ModContent.ItemType<ZincVial>()); 
            zincVialRecipe.AddIngredient(ItemID.BottledWater);
             // Use ModContent.ItemType for your modded Zinc Ore
            zincVialRecipe.AddIngredient(ModContent.ItemType<ZincOre>(), oreAmount);
            zincVialRecipe.AddTile(TileID.Bottles);
            zincVialRecipe.Register();

            Recipe chromiumVialRecipe = Recipe.Create(ModContent.ItemType<ChromiumVial>());
            chromiumVialRecipe.AddIngredient(ItemID.BottledWater);
            // Require simple ingredients for testing
            chromiumVialRecipe.AddIngredient(ItemID.Diamond, 1); // Requires a diamond for testing
            chromiumVialRecipe.AddTile(TileID.Bottles);
            chromiumVialRecipe.Register();        
            
            Recipe copperVialRecipe = Recipe.Create(ModContent.ItemType<CopperVial>());
copperVialRecipe.AddIngredient(ItemID.BottledWater);
copperVialRecipe.AddIngredient(ItemID.CopperOre, oreAmount);
copperVialRecipe.AddTile(TileID.Bottles);
copperVialRecipe.Register();

// Add Bronze Vial Recipe (mix of copper and tin)
Recipe bronzeVialRecipe = Recipe.Create(ModContent.ItemType<BronzeVial>());
bronzeVialRecipe.AddIngredient(ItemID.BottledWater);
bronzeVialRecipe.AddIngredient(ItemID.CopperOre, oreAmount / 2 + 1);
bronzeVialRecipe.AddIngredient(ItemID.TinOre, oreAmount / 2 + 1);
bronzeVialRecipe.AddTile(TileID.Bottles);
bronzeVialRecipe.Register();
            }
    }
}

