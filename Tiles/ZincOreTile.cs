using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization; // <--- Add this using statement

namespace MistbornMod.Tiles
{
    public class ZincOreTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            // --- Basic Tile Properties ---
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            // --- Ore-Specific Properties ---
            TileID.Sets.Ore[Type] = true;
            Main.tileShine[Type] = 900;
            Main.tileShine2[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 310;

            // --- Map Entry (Updated Method) ---
            // The actual name "Zinc Ore" will be defined in a localization file.
            // The key "MapEntry" is standard for map names.
            AddMapEntry(new Color(160, 170, 180), this.GetLocalization("MapEntry"));

            // --- Mining Properties ---
            DustType = DustID.Silver;
            HitSound = SoundID.Tink;
            MinPick = 35;
            // MineResist = 1f; // Default is 1f, usually fine to omit unless you want it tougher/weaker
        }

        // --- Item Drop Logic ---
        // (Keep your KillTile method as is, assuming you have or will create Items.ZincOreItem)
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!fail && !noItem)
            {
                // Ensure the path to your item is correct
                // Example: Item.NewItem(new Terraria.DataStructures.EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<Items.Materials.ZincOreItem>());

                // --- Make sure this line points to your actual Zinc Ore ITEM class ---
                // Item.NewItem(new Terraria.DataStructures.EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<Items.ZincOreItem>()); // Adjust path if needed
            }
        }
    }
}