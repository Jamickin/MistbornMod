// Update your HemalurgicSpike.cs to work with regular accessory slots
using MistbornMod.Common.Players;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;

namespace MistbornMod.Content.Items.HemalurgicSpikes
{
    /// <summary>
    /// Base class for all Hemalurgic Spikes that steal Allomantic powers through violence
    /// </summary>
    public abstract class HemalurgicSpike : ModItem
    {
        public MetalType TargetMetal { get; protected set; }
        public int RequiredKills { get; protected set; } = 50;
        public int CurrentKills { get; set; } = 0;
        public bool PowerUnlocked { get; set; } = false;
        
        // Different spike tiers with different requirements
        public enum SpikeType
        {
            Bone,    // Early game - 25 kills
            Shadow,  // Mid game - 50 kills  
            Blood,   // Late game - 75 kills
            Atium    // End game - 100 kills
        }
        
        public SpikeType SpikeTier { get; protected set; } = SpikeType.Bone;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 1;
            Item.rare = GetRarityForTier();
            Item.value = GetValueForTier();
            Item.accessory = true; // This makes it equippable in regular accessory slots
            
            // Set required kills based on tier
            RequiredKills = SpikeTier switch
            {
                SpikeType.Bone => 25,
                SpikeType.Shadow => 50,
                SpikeType.Blood => 75,
                SpikeType.Atium => 100,
                _ => 50
            };
        }

        private int GetRarityForTier()
        {
            return SpikeTier switch
            {
                SpikeType.Bone => ItemRarityID.Green,
                SpikeType.Shadow => ItemRarityID.Orange,
                SpikeType.Blood => ItemRarityID.LightRed,
                SpikeType.Atium => ItemRarityID.Purple,
                _ => ItemRarityID.White
            };
        }

        private int GetValueForTier()
        {
            return SpikeTier switch
            {
                SpikeType.Bone => Item.sellPrice(silver: 50),
                SpikeType.Shadow => Item.sellPrice(gold: 2),
                SpikeType.Blood => Item.sellPrice(gold: 5),
                SpikeType.Atium => Item.sellPrice(gold: 20),
                _ => Item.sellPrice(silver: 10)
            };
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Add progress information
            var progressLine = new TooltipLine(Mod, "SpikeProgress", 
                $"Kills: {CurrentKills}/{RequiredKills}");
            
            if (PowerUnlocked)
            {
                progressLine.Text = $"[c/00FF00:Power Unlocked!] Grants {TargetMetal} Allomancy";
                progressLine.OverrideColor = Color.LimeGreen;
            }
            else if (CurrentKills > 0)
            {
                float progress = (float)CurrentKills / RequiredKills * 100f;
                progressLine.Text += $" ({progress:F0}%)";
                progressLine.OverrideColor = Color.Orange;
            }
            else
            {
                progressLine.OverrideColor = Color.Gray;
            }
            
            tooltips.Add(progressLine);
            
            // Add metal-specific description
            var metalLine = new TooltipLine(Mod, "MetalPower", 
                GetMetalDescription(TargetMetal));
            metalLine.OverrideColor = Color.LightBlue;
            tooltips.Add(metalLine);
            
            // Add warning about Hemalurgy
            var warningLine = new TooltipLine(Mod, "HemalurgyWarning", 
                "[c/FF4444:Hemalurgy corrupts the soul with each kill]");
            tooltips.Add(warningLine);
        }

        private string GetMetalDescription(MetalType metal)
        {
            return metal switch
            {
                MetalType.Iron => "Pull metals toward you",
                MetalType.Steel => "Push metals away from you",
                MetalType.Tin => "Enhanced senses and perception",
                MetalType.Pewter => "Increased physical strength and durability",
                MetalType.Zinc => "Riot emotions, making enemies more aggressive",
                MetalType.Brass => "Soothe emotions, calming enemies",
                MetalType.Copper => "Hide your Allomantic signature",
                MetalType.Bronze => "Detect other Allomancers",
                MetalType.Atium => "Precognitive abilities",
                MetalType.Chromium => "Strip away others' metal reserves",
                _ => "Unknown Allomantic power"
            };
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var modPlayer = player.GetModPlayer<MistbornPlayer>();
            
            // Track this spike in the player's mod data
            modPlayer.EquippedSpike = this;
            
            // If power is unlocked, grant the ability
            if (PowerUnlocked)
            {
                GrantAllomanticPower(modPlayer);
                
                // Visual effect when equipped (only show occasionally)
                if (!hideVisual && Main.rand.NextBool(300)) // Rare visual effect
                {
                    Dust.NewDust(
                        player.position,
                        player.width,
                        player.height,
                        DustID.Blood,
                        Main.rand.NextFloat(-1f, 1f),
                        Main.rand.NextFloat(-1f, 1f),
                        100,
                        default,
                        0.8f
                    );
                }
            }
            else
            {
                // Show subtle reminder that spike needs more kills
                if (Main.rand.NextBool(1800)) // Once per 30 seconds on average
                {
                    Main.NewText($"The {TargetMetal} spike hungers for {RequiredKills - CurrentKills} more souls...", 255, 100, 100);
                }
            }
        }

        private void GrantAllomanticPower(MistbornPlayer modPlayer)
        {
            // If player is already Mistborn, they already have all powers
            if (modPlayer.IsMistborn) return;
            
            // Grant this power via Hemalurgy
            if (!modPlayer.HemalurgicPowers.Contains(TargetMetal))
            {
                modPlayer.HemalurgicPowers.Add(TargetMetal);
            }
        }

        /// <summary>
        /// Called when the player kills an NPC while wearing this spike
        /// </summary>
        public void OnKillNPC(NPC npc)
        {
            if (PowerUnlocked) return; // Already unlocked
            
            // Only count kills of non-friendly NPCs with some life
            if (npc.friendly || npc.lifeMax <= 5) return;
            
            CurrentKills++;
            
            // Check if we've reached the threshold
            if (CurrentKills >= RequiredKills)
            {
                PowerUnlocked = true;
                
                // Visual effect
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(
                        Main.LocalPlayer.position,
                        Main.LocalPlayer.width,
                        Main.LocalPlayer.height,
                        DustID.Blood,
                        Main.rand.NextFloat(-3f, 3f),
                        Main.rand.NextFloat(-3f, 3f),
                        100,
                        default,
                        1.5f
                    );
                }
                
                // Notification
                Main.NewText($"[c/FF0000:The {TargetMetal} spike pulses with stolen power!]", 
                    Color.Red);
                Main.NewText($"[c/00FF00:You have gained {TargetMetal} Allomancy through Hemalurgy!]", 
                    Color.LimeGreen);
            }
            else
            {
                // Progress notification every 5 kills
                if (CurrentKills % 5 == 0)
                {
                    Main.NewText($"Spike Progress: {CurrentKills}/{RequiredKills} kills", 
                        Color.Orange);
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["CurrentKills"] = CurrentKills;
            tag["PowerUnlocked"] = PowerUnlocked;
        }

        public override void LoadData(TagCompound tag)
        {
            CurrentKills = tag.GetInt("CurrentKills");
            PowerUnlocked = tag.GetBool("PowerUnlocked");
        }
    }
}