using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using MistbornMod.Common.Systems;
using MistbornMod.Common.Players;

namespace MistbornMod.Content.Buffs
{
    public class IronBuff : MetalBuff 
    {
        private const float ScanRange = 500f;
        private const float PullRange = 400f;
        private const float PullForce = 3.5f; // Base pulling force
        private const float PlayerPullForce = 5f; // Base force to pull the player toward metals
        private const float MaxPlayerPullSpeedSq = 8f * 8f; // Base max squared velocity when player is pulled
        private const int LineDustType = MetalDetectionSystem.METAL_LINE_DUST_TYPE; 

        private int playerPullCooldown = 0;
        
        // NEW: Dictionary to track NPCs that have had coins pulled from them to prevent spam
        private Dictionary<int, int> npcCoinCooldowns = new Dictionary<int, int>();
        private const int CoinPullCooldown = 180; // 3 seconds cooldown per NPC

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Iron; 
        }
        
        // NEW: Handle pulling coins off NPCs when they're pulled
        private void PullCoinsOffNPC(NPC npc, Vector2 pullDirection, bool isFlaring)
        {
            // Check cooldown to prevent spam
            if (npcCoinCooldowns.ContainsKey(npc.whoAmI))
            {
                return;
            }
            
            // Only pull coins off NPCs that would normally drop them
            if (npc.value <= 0) return;
            
            // Calculate how many coins to pull off (fraction of their total value)
            float coinPullPercent = isFlaring ? 0.15f : 0.08f; // 8-15% of their coin value
            int coinValue = (int)(npc.value * coinPullPercent);
            
            if (coinValue <= 0) return;
            
            // Convert to actual coin items and drop them
            int copper = coinValue % 100;
            coinValue /= 100;
            int silver = coinValue % 100;
            coinValue /= 100;
            int gold = coinValue % 100;
            int platinum = coinValue / 100;
            
            Vector2 npcCenter = npc.Center;
            
            // Drop coins with pull velocity (toward player)
            var source = npc.GetSource_Loot();
            
            if (platinum > 0)
            {
                DropCoinsWithVelocity(source, npcCenter, ItemID.PlatinumCoin, platinum, pullDirection, isFlaring);
            }
            if (gold > 0)
            {
                DropCoinsWithVelocity(source, npcCenter, ItemID.GoldCoin, gold, pullDirection, isFlaring);
            }
            if (silver > 0)
            {
                DropCoinsWithVelocity(source, npcCenter, ItemID.SilverCoin, silver, pullDirection, isFlaring);
            }
            if (copper > 0)
            {
                DropCoinsWithVelocity(source, npcCenter, ItemID.CopperCoin, copper, pullDirection, isFlaring);
            }
            
            // Set cooldown for this NPC
            npcCoinCooldowns[npc.whoAmI] = CoinPullCooldown;
            
            // Visual effect
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDust(npcCenter, 16, 16, DustID.GoldCoin, 
                    pullDirection.X * 2f, pullDirection.Y * 2f, 100, default, 1.2f);
            }
        }
        
        // Helper method to drop coins with velocity toward player
        private void DropCoinsWithVelocity(Terraria.DataStructures.IEntitySource source, Vector2 position, int coinType, int amount, Vector2 direction, bool isFlaring)
        {
            Vector2 velocity = direction * (isFlaring ? 8f : 5f) + Main.rand.NextVector2Circular(1f, 1f);
            
            int item = Item.NewItem(source, position, coinType, amount);
            if (item < Main.maxItems)
            {
                Main.item[item].velocity = velocity;
                Main.item[item].noGrabDelay = 30; // Shorter delay than steel push since coins are coming toward player
            }
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Get the MistbornPlayer instance to check flaring status
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            float multiplier = modPlayer.IsFlaring ? 2.0f : 1.0f;
            
            // Calculate dynamic values based on flaring status
            float currentPullForce = PullForce * multiplier;
            float currentPlayerPullForce = PlayerPullForce * multiplier;
            float currentMaxSpeedSq = MaxPlayerPullSpeedSq * multiplier;
            
            if (playerPullCooldown > 0) {
                playerPullCooldown--;
            }
            
            // NEW: Update NPC coin cooldowns
            List<int> npcKeysToRemove = new List<int>();
            foreach (var kvp in npcCoinCooldowns)
            {
                int cooldown = kvp.Value - 1;
                if (cooldown <= 0)
                {
                    npcKeysToRemove.Add(kvp.Key);
                }
                else
                {
                    npcCoinCooldowns[kvp.Key] = cooldown;
                }
            }
            
            foreach (int key in npcKeysToRemove)
            {
                npcCoinCooldowns.Remove(key);
            }

            // Find closest metal to mouse cursor
            Vector2 mouseWorld = Main.MouseWorld;
            float closestDistSq = PullRange * PullRange;
            Entity closestTargetEntity = null;
            Vector2? closestTilePos = null;
            
            // Check for items
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (item.active && MetalDetectionUtils.IsMetallicItem(item.type))
                {
                    float distSq = Vector2.DistanceSquared(mouseWorld, item.Center);
                    if (distSq < closestDistSq && Vector2.DistanceSquared(player.Center, item.Center) < PullRange * PullRange)
                    {
                        closestDistSq = distSq;
                        closestTargetEntity = item;
                        closestTilePos = null;
                    }
                }
            }
            
            // Check for NPCs
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.knockBackResist < 1f && MetalDetectionUtils.IsMetallicNPC(npc))
                {
                    float distSq = Vector2.DistanceSquared(mouseWorld, npc.Center);
                    if (distSq < closestDistSq && Vector2.DistanceSquared(player.Center, npc.Center) < PullRange * PullRange)
                    {
                        closestDistSq = distSq;
                        closestTargetEntity = npc;
                        closestTilePos = null;
                    }
                }
            }

            // Check for metallic tiles
            int playerTileX = (int)(player.Center.X / 16f);
            int playerTileY = (int)(player.Center.Y / 16f);
            int tileScanRadius = (int)(PullRange / 16f) + 2;

            for (int x = playerTileX - tileScanRadius; x <= playerTileX + tileScanRadius; x++)
            {
                for (int y = playerTileY - tileScanRadius; y <= playerTileY + tileScanRadius; y++)
                {
                    if (!WorldGen.InWorld(x, y, 1)) continue;
                    Tile tile = Main.tile[x, y];
                    if (tile != null && tile.HasTile)
                    {
                        // Check for both ores and metallic objects like anvils, metal bars, etc.
                        bool isMetallic = MetalDetectionUtils.IsMetallicOre(tile.TileType) || 
                                          MetalDetectionUtils.IsMetallicObject(tile.TileType);
                        
                        if (isMetallic)
                        {
                            Vector2 tileWorldCenter = new Vector2(x * 16f + 8f, y * 16f + 8f);
                            float distSq = Vector2.DistanceSquared(mouseWorld, tileWorldCenter);
                            if (distSq < closestDistSq && Vector2.DistanceSquared(player.Center, tileWorldCenter) < PullRange * PullRange)
                            {
                                closestDistSq = distSq;
                                closestTargetEntity = null;
                                closestTilePos = tileWorldCenter;
                            }
                        }
                    }
                }
            }

            // Scan for items to pull automatically (for all metallic items in range)
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (!item.active || item.noGrabDelay > 0) continue;

                float distanceToItem = Vector2.Distance(player.Center, item.Center);
                bool isMetallic = MetalDetectionUtils.IsMetallicItem(item.type); 

                if (distanceToItem < ScanRange && isMetallic)
                {
                    // More dust effects when flaring
                    if (Main.rand.NextBool(modPlayer.IsFlaring ? 3 : 5))
                    {
                        Dust.NewDust(item.position, item.width, item.height, LineDustType, 0f, 0f, 150, default, modPlayer.IsFlaring ? 0.8f : 0.6f);
                    }
                    
                    // Always pull items that are in range, but apply stronger force to targeted item
                    if (distanceToItem < PullRange && isMetallic)
                    {
                        Vector2 direction = player.Center - item.Center;
                        if (direction != Vector2.Zero)
                        {
                            direction.Normalize();
                            // Apply stronger pull to the targeted item
                            float pullMultiplier = (closestTargetEntity == item) ? 1.0f : 0.5f;
                            item.velocity += direction * currentPullForce * pullMultiplier;
                            
                            if (item.velocity.LengthSquared() > 100f * multiplier) {
                                item.velocity *= 10f * multiplier / item.velocity.Length();
                            }
                        }
                    }
                }
            } 

            // Scan for metallic tiles - increased range when flaring
            int playerTileX2 = (int)(player.Center.X / 16f);
            int playerTileY2 = (int)(player.Center.Y / 16f);
            int tileScanRadius2 = (int)(ScanRange / 16f) + 2;

            for (int x = playerTileX2 - tileScanRadius2; x <= playerTileX2 + tileScanRadius2; x++)
            {
                for (int y = playerTileY2 - tileScanRadius2; y <= playerTileY2 + tileScanRadius2; y++)
                {
                    if (!WorldGen.InWorld(x, y, 1)) continue;
                    Tile tile = Main.tile[x, y];
                    if (tile != null && tile.HasTile)
                    {
                        // Check if this tile is a valid metallic ore or object
                        bool isMetallic = MetalDetectionUtils.IsMetallicOre(tile.TileType) || 
                                          MetalDetectionUtils.IsMetallicObject(tile.TileType);
                        
                        if (isMetallic)
                        {
                            Vector2 tileWorldPos = new Vector2(x * 16f + 8f, y * 16f + 8f);
                            float distToTile = Vector2.Distance(player.Center, tileWorldPos);
                            
                            if (distToTile < ScanRange)
                            {
                                // More dust when flaring
                                if (Main.rand.NextBool(modPlayer.IsFlaring ? 5 : 8))
                                {
                                    Dust.NewDust(new Vector2(x * 16f, y * 16f), 16, 16, LineDustType, 0f, 0f, 150, default, modPlayer.IsFlaring ? 0.7f : 0.5f);
                                }
                            }
                        }
                    }
                }
            }

            // Draw line to the closest target and apply forces
            if (closestTargetEntity != null) 
            {
                // Thicker line when flaring
                MetalDetectionUtils.DrawLineWithDust(player.Center, closestTargetEntity.Center, LineDustType, modPlayer.IsFlaring ? 0.22f : 0.15f, modPlayer.IsFlaring);
                
                // Check if this is a "held" mechanic activation
                if (modPlayer.IsActivelyIronPulling && playerPullCooldown <= 0)
                {
                    // Apply appropriate force based on entity type
                    if (closestTargetEntity is NPC targetNPC)
                    {
                        Vector2 pullDirection = player.Center - targetNPC.Center;
                        if (pullDirection != Vector2.Zero)
                        {
                            pullDirection.Normalize();
                            targetNPC.velocity += pullDirection * currentPullForce * (1f - targetNPC.knockBackResist) * 1.2f;
                            
                            // NEW: Pull coins off the NPC when they're pulled
                            PullCoinsOffNPC(targetNPC, pullDirection, modPlayer.IsFlaring);
                        }
                    }
                    else
                    {
                        // Pull player toward the target (items or anchors)
                        Vector2 pullDirection = closestTargetEntity.Center - player.Center;
                        if (pullDirection != Vector2.Zero)
                        {
                            pullDirection.Normalize();
                            if (player.velocity.LengthSquared() < currentMaxSpeedSq) 
                            {
                                player.velocity += pullDirection * currentPlayerPullForce * 0.5f;
                                playerPullCooldown = modPlayer.IsFlaring ? 3 : 5; // Shorter cooldown when flaring
                            }
                        }
                    }
                }
            }
            else if (closestTilePos.HasValue)
            {
                // Thicker line when flaring
                MetalDetectionUtils.DrawLineWithDust(player.Center, closestTilePos.Value, LineDustType, modPlayer.IsFlaring ? 0.22f : 0.15f, modPlayer.IsFlaring);
                
                // Check if actively pulling and apply force to player
                if (modPlayer.IsActivelyIronPulling && playerPullCooldown <= 0)
                {
                    Vector2 pullDirection = closestTilePos.Value - player.Center;
                    if (pullDirection != Vector2.Zero)
                    {
                        pullDirection.Normalize();
                        if (player.velocity.LengthSquared() < currentMaxSpeedSq)
                        {
                            // Calculate what the new velocity would be
                            Vector2 newVelocity = player.velocity + (pullDirection * currentPlayerPullForce);
                            
                            // Only apply if it wouldn't exceed max speed
                            if (newVelocity.LengthSquared() < currentMaxSpeedSq * 1.1f)  // Add 10% buffer
                            {
                                player.velocity = newVelocity;
                            }
                            else
                            {
                                // If exceeding max speed, normalize and set to max
                                newVelocity.Normalize();
                                player.velocity = newVelocity * (float)Math.Sqrt(currentMaxSpeedSq);
                            }
                            
                            playerPullCooldown = modPlayer.IsFlaring ? 5 : 8;
                            
                            // Cancel fall damage when pulling forcefully
                            if (modPlayer.IsFlaring) {
                                player.fallStart = (int)(player.position.Y / 16f);
                            }
                        }
                    }
                }
            }

            // Add ambient dust around player - more when flaring
            if (Main.rand.NextBool(modPlayer.IsFlaring ? 5 : 10))
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(modPlayer.IsFlaring ? 1.5f : 1f, modPlayer.IsFlaring ? 1.5f : 1f);
                Dust.NewDustPerfect(player.Center, LineDustType, dustVel, 150, default, modPlayer.IsFlaring ? 1.0f : 0.8f);
            }
        }
    }
}