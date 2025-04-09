// Simplified MistbornAscension class that uses Terraria's native fog system

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.Localization;
using System;

namespace MistbornMod
{
    // This class handles the Mistborn ascension mechanics
    public class MistbornAscension : ModSystem
    {
        // Static instance for easy access
        public static MistbornAscension Instance;
        
        // Recipe group for lerasium beads (special ascension item)
        public static RecipeGroup LerasiumBeadRecipeGroup;

        public override void Load()
        {
            Instance = this;
        }
        
        public override void PostSetupContent()
        {
            // Register special recipe groups that can be used for the ascension item
            LerasiumBeadRecipeGroup = new RecipeGroup(
                () => $"{Language.GetTextValue("LegacyMisc.37")} {Language.GetTextValue("Mods.MistbornMod.Items.LerasiumBead.DisplayName")}",
                ItemID.LifeCrystal,
                ItemID.LifeFruit
            );
            
            RecipeGroup.RegisterGroup("MistbornMod:LerasiumBead", LerasiumBeadRecipeGroup);
        }

        public override void Unload()
        {
            Instance = null;
        }
        
        // Method to make a player become Mistborn
        public void MakePlayerMistborn(Player player)
        {
            if (player != null)
            {
                MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
                
                // Only perform Ascension if the player isn't already Mistborn
                if (!modPlayer.IsMistborn)
                {
                    // Set the player as Mistborn
                    modPlayer.IsMistborn = true;
                    
                    // Play ascension sound
                    SoundEngine.PlaySound(SoundID.Item4, player.position);
                    
                    // Create a visual effect around the player
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 velocity = Main.rand.NextVector2CircularEdge(8f, 8f);
                        Dust.NewDust(player.position, player.width, player.height, DustID.Shadowflame, velocity.X, velocity.Y, 0, Color.White, 1.5f);
                    }
                    
                    // Add some green dust for graveyard effect
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 velocity = Main.rand.NextVector2CircularEdge(6f, 6f);
                        Dust.NewDust(player.position, player.width, player.height, DustID.GreenTorch, 
                            velocity.X, velocity.Y, 0, default, 1.2f);
                    }
                    
                    // Show message to the player
                    Main.NewText("You have ascended as a Mistborn!", 255, 255, 100);
                    
                    // Grant starting vials
                    GrantStartingVials(player);
                }
            }
        }
        
        // Grant starting vials to a new Mistborn
        private void GrantStartingVials(Player player)
        {
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<Items.IronVial>(), 3);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<Items.PewterVial>(), 2);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<Items.TinVial>(), 2);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<Items.SteelVial>(), 2);
        }

        public override void PreUpdatePlayers()
        {
            // Check if at least one player is Mistborn
            bool anyMistborn = false;
            
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active)
                {
                    MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
                    if (modPlayer.IsMistborn)
                    {
                        anyMistborn = true;
                        break;
                    }
                }
            }
            
            // Night check - only apply effects at night
            bool isNight = !Main.dayTime;
            
            // Graveyard effect active if player is Mistborn AND it's night
            if (anyMistborn && isNight)
            {
                // Here's the key - we directly modify the game's graveyard visual intensity
                // This controls the fog, ambiance, and all graveyard effects automatically
                Main.GraveyardVisualIntensity = 1f;  // Full intensity
            }
            else
            {
                // Let the normal graveyard system work during daytime
                // If there are no real tombstone graveyards, this will be zero
                // If there are actual graveyards, their normal effect will remain
                
                // Force reset only if we were forcing it before
                if (anyMistborn)
                {
                    Main.GraveyardVisualIntensity = 0f;
                }
                // Otherwise let the normal game mechanics handle it
                // (this allows real graveyards to still work normally)
            }
        }
    }
}