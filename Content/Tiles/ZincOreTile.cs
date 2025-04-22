using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

using MistbornMod.Content.Items;
namespace MistbornMod.Content.Tiles
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
        // Use the proper entity source
        var source = new Terraria.DataStructures.EntitySource_TileBreak(i, j);
        // This is the key fix - specify the correct item to drop
        Item.NewItem(source, i * 16, j * 16, 16, 16, ModContent.ItemType<ZincOre>());
    }
}
    }
}