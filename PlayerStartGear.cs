using Terraria;
using Terraria.ModLoader;
using MistbornMod.Items;

namespace MistbornMod
{
    public class PlayerStartGear : ModPlayer
    {
public override void OnEnterWorld()
{
    // Check if this is a brand new character (not 100% reliable but works)
    if (Player.statLifeMax == 100 && Player.inventory[0].type == ItemID.CopperShortsword)
    {
        // Give them a metal tester
        Player.QuickSpawnItem(Player.GetSource_GiftOrReward(), ModContent.ItemType<MetalTester>(), 1);
        
        // Give them a hint in chat
        Main.NewText("You feel a strange connection to the mists. Find a Metal Tester to learn more.", 220, 230, 255);
        
        // Give them a single vial of their metal (that they'll need to discover)
        MistbornPlayer modPlayer = Player.GetModPlayer<MistbornPlayer>();
        if (modPlayer.IsMisting && modPlayer.MistingMetal.HasValue)
        {
            int vialType = GetVialTypeForMetal(modPlayer.MistingMetal.Value);
            if (vialType > 0)
            {
                Player.QuickSpawnItem(Player.GetSource_GiftOrReward(), vialType, 1);
                Main.NewText("You find a strange vial in your pocket...", 180, 220, 255);
            }
        }
    }
}
        
        private int GetVialTypeForMetal(MetalType metal)
        {
            switch (metal)
            {
                case MetalType.Iron: return ModContent.ItemType<IronVial>();
                case MetalType.Steel: return ModContent.ItemType<SteelVial>();
                case MetalType.Tin: return ModContent.ItemType<TinVial>();
                case MetalType.Pewter: return ModContent.ItemType<PewterVial>();
                case MetalType.Zinc: return ModContent.ItemType<ZincVial>();
                case MetalType.Brass: return ModContent.ItemType<BrassVial>();
                case MetalType.Copper: return ModContent.ItemType<CopperVial>();
                case MetalType.Bronze: return ModContent.ItemType<BronzeVial>();
                default: return 0;
            }
        }
    }
}