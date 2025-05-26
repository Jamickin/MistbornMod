using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using MistbornMod.Common.Players;

namespace MistbornMod.Content.Items.Consumables
{
    public class AtiumBead : ModItem
    {
        public override void SetStaticDefaults()
        {
            // Item will float and pulsate to show importance
            ItemID.Sets.ItemNoGravity[Item.type] = true;
            ItemID.Sets.ItemIconPulse[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99; // Can stack, unlike Lerasium
            Item.value = Item.sellPrice(gold: 2); // Valuable but less than Lerasium
            Item.rare = ItemRarityID.Purple; // Extremely rare
            Item.useStyle = ItemUseStyleID.EatFood; // Can be consumed
            Item.useAnimation = 25;
            Item.useTime = 25;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item4; // Special sound
            Item.consumable = true; // Used up on consumption
            Item.noUseGraphic = true; // Don't show animation when used
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
            // Makes the item always bright with a slight shadow tint
            return new Color(230, 230, 255, 200);
        }
        
        public override void PostUpdate()
        {
            // Visual effects - create dust to make the item look magical
            if (Main.rand.NextBool(20))
            {
                Dust.NewDust(
                    Item.position,
                    Item.width,
                    Item.height,
                    DustID.SilverFlame,
                    0f, 0f, 150, 
                    default, 
                    Main.rand.NextFloat(0.8f, 1.2f)
                );
            }
        }

        public override bool? UseItem(Player player)
        {
            // When used, grant the Atium buff directly
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            
            // Only have an effect if the player is a Mistborn
            if (modPlayer.IsMistborn)
            {
                // Grant Atium reserves directly
int currentReserve = 0;
if (modPlayer.MetalReserves.TryGetValue(MetalType.Atium, out int reserve))
{
    currentReserve = reserve;
}
modPlayer.MetalReserves[MetalType.Atium] = currentReserve + 1800; //                
                // Create a visual effect
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(
                        player.position,
                        player.width,
                        player.height,
                        DustID.SilverFlame,
                        Main.rand.NextFloat(-1.5f, 1.5f),
                        Main.rand.NextFloat(-1.5f, 1.5f),
                        100,
                        default,
                        Main.rand.NextFloat(1.0f, 1.5f)
                    );
                }
                
                // Toggle on Atium burning automatically
                int buffId = modPlayer.GetBuffIDForMetal(MetalType.Atium);
                if (buffId != -1)
                {
                    player.AddBuff(buffId, 5); // Add buff briefly, PostUpdate will maintain
                    modPlayer.BurningMetals[MetalType.Atium] = true; // Start burning Atium
                }
                
                // Show message to player
                Main.NewText("The world slows down around you as you burn Atium...", 220, 220, 255);
                
                return true; // Consume the item
            }
            else
            {
                // Not a Mistborn, can't use it
                Main.NewText("You don't have the ability to burn metals.", 255, 100, 100);
                return false; // Don't consume the item
            }
        }
        
       
    }
}