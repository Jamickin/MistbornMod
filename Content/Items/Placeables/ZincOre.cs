using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MistbornMod.Content.Tiles;

namespace MistbornMod.Content.Items.Placeables
{
    public class ZincOre : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Tooltip.SetDefault("A soft, bluish-white metal."); // Optional: Add description
            // DisplayName.SetDefault("Zinc Ore"); // Sets the item name shown in-game
        }

        public override void SetDefaults()
        {
            // --- Basic Item Properties ---
            Item.width = 12; // Adjust to match your sprite dimensions
            Item.height = 12; // Adjust to match your sprite dimensions
            Item.maxStack = 9999; // Max stack size (9999 is common for materials in 1.4.4+)
            Item.value = Item.sellPrice(silver: 2, copper: 50); // Sell price (e.g., 2 silver, 50 copper) - Adjust as needed
            Item.rare = ItemRarityID.White; // Rarity (White or Blue are common for early ores)

            // --- Usage Properties (for placing the tile) ---
            Item.useStyle = ItemUseStyleID.Swing; // How the item is used (swing animation)
            Item.useTurn = true;        // Allows turning while using
            Item.useAnimation = 15;     // Duration of the use animation (frames)
            Item.useTime = 10;          // Time between uses (frames)
            Item.autoReuse = true;      // Allows holding down mouse to reuse
            Item.consumable = true;     // Item is consumed upon successful use (placing the tile)

            // --- Tile Placement Logic ---
            // Links this item to the corresponding tile block
            // Ensure 'MistbornMod.Tiles.ZincOreTile' is the correct path to your tile class
            Item.createTile = ModContent.TileType<ZincOreTile>();
        }
    }
}