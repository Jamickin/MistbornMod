using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using MistbornMod.Common.UI;
using MistbornMod.Common.Players;
using MistbornMod.Content.Items;

namespace MistbornMod
{
    public class PlayerStartGear : ModPlayer
    {
       public override void OnEnterWorld()
{
    // Check if this is a brand new character (not 100% reliable but works)
    if (Player.statLifeMax == 100 && Player.inventory[0].type == ItemID.CopperShortsword)
    {
        // Get the MistbornPlayer instance
        MistbornPlayer modPlayer = Player.GetModPlayer<MistbornPlayer>();
        
        // If the player isn't already a Mistborn or Misting, make them a Misting
        if (!modPlayer.IsMistborn && !modPlayer.IsMisting)
        {
            // Assign a random metal type from the primary 8 metals
            MetalType[] primaryMetals = new[] {
                MetalType.Iron,
                MetalType.Steel,
                MetalType.Tin,
                MetalType.Pewter,
                MetalType.Zinc,
                MetalType.Brass,
                MetalType.Copper,
                MetalType.Bronze
            };
            
            // Select a random metal type
            int randomIndex = Main.rand.Next(primaryMetals.Length);
            MetalType randomMetal = primaryMetals[randomIndex];
            
            // Set the player as a Misting with the selected metal
            modPlayer.IsMisting = true;
            modPlayer.MistingMetal = randomMetal;
            
            // Log this for debugging
            Mod.Logger.Info($"New player assigned Misting ability: {randomMetal}");
            
            // Important: Make sure the UI is visible for new players
            if (DraggableMetalUI.Instance != null)
            {
                DraggableMetalUI.Instance.EnsureVisibilityForNewMisting();
            }
        }
        
        // Give them a hint in chat
        Main.NewText("You feel a connection to the mists. Try crafting a Metal Tester at an anvil.", 220, 230, 255);
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