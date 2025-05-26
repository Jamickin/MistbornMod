using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MistbornMod.Common.Systems;
using MistbornMod.Common.Players;

namespace MistbornMod.Content.Items.Consumables
{
    public class LerasiumBead : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Lerasium Bead");
            // Tooltip.SetDefault("A small metallic bead that contains incredible power\nConsume to become a Mistborn");
            
            // Register this item as a rare boss drop
            ItemID.Sets.ItemNoGravity[Item.type] = true; // Makes the item float
            ItemID.Sets.ItemIconPulse[Item.type] = true; // Makes the item pulsate in the inventory
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1; // Cannot stack
            Item.value = Item.sellPrice(gold: 5); // Very valuable
            Item.rare = ItemRarityID.Purple; // Extremely rare
            Item.useStyle = ItemUseStyleID.EatFood; // Consume like food
            Item.useAnimation = 30; // Longer animation to show importance
            Item.useTime = 30;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item4; // Special sound
            Item.consumable = true; // Used up on consumption
            Item.noUseGraphic = true; // Don't show animation when used
        }
        
        public override Color? GetAlpha(Color lightColor)
        {
            // Makes the item always bright and slightly glowing
            return new Color(255, 255, 255, 200);
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
                    DustID.GoldFlame,
                    0f, 0f, 150, 
                    default, 
                    Main.rand.NextFloat(0.8f, 1.2f)
                );
            }
        }

        public override bool? UseItem(Player player)
        {
            // When used, make the player a Mistborn
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            
            // Only have an effect if the player isn't already a Mistborn
            if (!modPlayer.IsMistborn)
            {
                // Make the player a Mistborn
                if (MistbornAscension.Instance != null)
                {
                    MistbornAscension.Instance.MakePlayerMistborn(player);
                    
                    // Create a visual effect
                    for (int i = 0; i < 50; i++)
                    {
                        Dust.NewDust(
                            player.position,
                            player.width,
                            player.height,
                            DustID.GoldFlame,
                            Main.rand.NextFloat(-2f, 2f),
                            Main.rand.NextFloat(-2f, 2f),
                            100,
                            default,
                            Main.rand.NextFloat(1.5f, 2.0f)
                        );
                    }
                    
                    // Return true to consume the item
                    return true;
                }
                
                // If we get here, something went wrong with the MistbornAscension system
                Main.NewText("Error: Could not ascend as Mistborn. Try again later.", 255, 0, 0);
                return false; // Don't consume the item if it didn't work
            }
            else
            {
                // Already a Mistborn, so just display a message
                Main.NewText("You are already a Mistborn.", 255, 255, 100);
                return false; // Don't consume the item unnecessarily
            }
        }
        
      
    }
}