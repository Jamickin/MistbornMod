using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MistbornMod.Items
{
    public class ZincOre : ModItem
    {
        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 13;
            Item.height = 13;
            Item.maxStack = 999;
            Item.value = Item.sellPrice(0, 0, 10, 0); 
            Item.rare = ItemRarityID.Blue; 
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.ZincOreTile>(); 
        }
    }
}