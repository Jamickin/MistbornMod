// Update the MakePlayerMistborn method in MistbornAscension.cs

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.Localization;
using System;
using MistbornMod.Common.Players;
using MistbornMod.Content.Items;

namespace MistbornMod.Common.Systems
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
                    // Set the player as Mistborn and clear Misting status
                    modPlayer.IsMistborn = true;
                    
                    // Keep track of whether they were a Misting before
                    bool wasMisting = modPlayer.IsMisting;
                    
                    // Clear Misting flags
                    modPlayer.IsMisting = false;
                    modPlayer.MistingMetal = null;
                    
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
                    if (!wasMisting)
                    {
                        Main.NewText("You have ascended as a Mistborn!", 255, 255, 100);
                    }
                    // Note: Skip message if wasMisting, as LerasiumBead will show a specialized message
                    
                    // Grant starting vials
                    GrantStartingVials(player);
                }
            }
        }
        
        // Grant starting vials to a new Mistborn
        private void GrantStartingVials(Player player)
        {
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<IronVial>(), 3);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<PewterVial>(), 2);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<TinVial>(), 2);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<SteelVial>(), 2);
        }

        // Rest of the existing methods...
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
            
            // Apply graveyard effect if player is Mistborn AND it's night
            if (anyMistborn && isNight)
            {
                // Set the counts extremely high to ensure the effect triggers
                Main.SceneMetrics.GraveyardTileCount = 100;
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
                    Main.SceneMetrics.GraveyardTileCount = 0;
                }
                // Otherwise let the normal game mechanics handle it
                // (this allows real graveyards to still work normally)
            }
        }
        
        public override void PostUpdateEverything()
        {
            // Check again to ensure effect remains after other updates
            bool anyMistborn = false;
            bool isNight = !Main.dayTime;
            
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && player.GetModPlayer<MistbornPlayer>().IsMistborn)
                {
                    anyMistborn = true;
                    break;
                }
            }
            
            if (anyMistborn && isNight)
            {
                Main.SceneMetrics.GraveyardTileCount = 100;
                // Force visual intensity directly
                Main.GraveyardVisualIntensity = 1f;
            }
        }
    }
}