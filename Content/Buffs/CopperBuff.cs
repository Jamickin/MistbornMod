// Buffs/CopperBuff.cs
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MistbornMod.Common.Players;

namespace MistbornMod.Content.Buffs
{
    public class CopperBuff : MetalBuff
    {
        private const float CoppercloudRadius = 400f; // Base radius
        
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Copper;
        }
        
public override void ApplyBuffEffect(Player player, bool isFlaring)
        {
            // Get the MistbornPlayer instance to check flaring status
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            float multiplier = modPlayer.IsFlaring ? 2.0f : 1.0f;
            
            // Calculate dynamic values based on flaring
            float currentRadius = CoppercloudRadius * multiplier;
            
            // Register this player as generating a coppercloud
            modPlayer.IsGeneratingCoppercloud = true;
            modPlayer.CoppercloudRadius = currentRadius;
            
            // Visual effects - subtle blue/copper dust cloud
            if (Main.rand.NextBool(modPlayer.IsFlaring ? 5 : 8))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(currentRadius * 0.5f, currentRadius * 0.5f);
                Dust.NewDustPerfect(dustPos, DustID.CopperCoin, Vector2.Zero, 150, default, modPlayer.IsFlaring ? 0.9f : 0.6f);
            }
        }
        
        public override void OnBuffEnd(Player player, MistbornPlayer modPlayer)
        {
            // When buff ends, stop generating coppercloud
            modPlayer.IsGeneratingCoppercloud = false;
            modPlayer.CoppercloudRadius = 0f;
        }
    }
}