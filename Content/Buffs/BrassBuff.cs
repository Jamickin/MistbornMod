using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MistbornMod.Common.Players;

namespace MistbornMod.Content.Buffs
{
    public class BrassBuff : MetalBuff
    {
        private const float BaseSootheRange = 500f; // Base soothe range
        private const int BaseDebuffDuration = 120; // Base debuff duration (2 seconds)
        
        // Track affected NPCs to properly reset their state
        private static Dictionary<int, int> soothedNPCs = new Dictionary<int, int>();

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Brass; 
        }
        
public override void ApplyBuffEffect(Player player, bool isFlaring)
        {
            // Get the MistbornPlayer instance to check flaring status
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            float multiplier = modPlayer.IsFlaring ? 2.0f : 1.0f;
            
            // Calculate dynamic values based on flaring
            float currentSootheRange = BaseSootheRange * multiplier;
            int currentDebuffDuration = (int)(BaseDebuffDuration * multiplier);
            
            // Decrement timers for all soothed NPCs and remove when expired
            List<int> keysToRemove = new List<int>();
            foreach (var entry in soothedNPCs)
            {
                int npcIndex = entry.Key;
                int timeLeft = entry.Value - 1;
                
                if (timeLeft <= 0 || !Main.npc[npcIndex].active)
                {
                    // Reset the NPC's target when effect expires
                    if (Main.npc[npcIndex].active)
                    {
                        // Find closest player as target
                        Main.npc[npcIndex].TargetClosest(true);
                    }
                    keysToRemove.Add(npcIndex);
                }
                else
                {
                    soothedNPCs[npcIndex] = timeLeft;
                }
            }
            
            // Remove expired entries
            foreach (int key in keysToRemove)
            {
                soothedNPCs.Remove(key);
            }
            
            // Apply soothing to NPCs in range
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (npc.active && !npc.friendly && !npc.boss && npc.CanBeChasedBy())
                {
                    float distanceSq = Vector2.DistanceSquared(player.Center, npc.Center);
                    if (distanceSq < currentSootheRange * currentSootheRange)
                    {
                        // Apply Slow debuff
                        npc.AddBuff(BuffID.Slow, currentDebuffDuration);
                        
                        // When flaring, apply additional calming effects
                        if (modPlayer.IsFlaring)
                        {
                            // Weak represents diminished aggression
                            npc.AddBuff(BuffID.Weak, currentDebuffDuration);
                            
                            // Apply frozen for brief moments to simulate hesitation
                            // but not permanent freezing
                            npc.AddBuff(BuffID.Frozen, currentDebuffDuration / 4);
                            
                            // Make enemies deal less damage when heavily soothed
                            npc.damage = (int)(npc.damage * 0.7f); // 30% less damage when flaring
                        }
                        
                        // Direct target away from player
                        // Store the original NPC in our tracking dictionary
                        if (!soothedNPCs.ContainsKey(i))
                        {
                            soothedNPCs.Add(i, currentDebuffDuration);
                        }
                        else
                        {
                            // Refresh the duration
                            soothedNPCs[i] = currentDebuffDuration;
                        }
                        
                        // Temporarily make NPC ignore player
                        npc.target = Main.maxPlayers;
                        
                        // Visual effects - blue calming dust
                        if (Main.rand.NextBool(modPlayer.IsFlaring ? 5 : 10))
                        {
                            int dustType = DustID.BlueTorch;
                            float scale = modPlayer.IsFlaring ? 1.0f : 0.7f;
                            
                            Dust.NewDust(
                                npc.position, 
                                npc.width, 
                                npc.height, 
                                dustType, 
                                0f, 
                                -0.3f, 
                                100, 
                                default, 
                                scale
                            );
                        }
                    }
                }
            }
            
            // Visual effect around player to show brass burning
            if (modPlayer.IsFlaring && Main.rand.NextBool(8))
            {
                Vector2 dustPos = player.Center + Main.rand.NextVector2Circular(32f, 32f);
                Dust.NewDustPerfect(dustPos, DustID.BlueTorch, Vector2.Zero, 150, Color.SkyBlue, 0.8f);
            }
        }
        
        // Reset dictionary when mod unloads
        public override void Unload()
        {
            soothedNPCs.Clear();
        }
    }
}