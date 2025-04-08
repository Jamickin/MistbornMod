using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MistbornMod.Items
{
    // Base class for metal vials
    public abstract class MetalVial : ModItem
    {
        public MetalType Metal { get; protected set; }
        public int Duration { get; protected set; } = 3600; // Default 1 minute duration

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 30;
            Item.consumable = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 5);
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<MistbornPlayer>();
            modPlayer.DrinkMetalVial(Metal, Duration);
            return true;
        }
    }
}