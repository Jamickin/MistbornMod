using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MistbornMod.Utils;

namespace MistbornMod.Buffs
{
    public class SteelBuff : MetalBuff
    {
        private const float PushRange = 320f; // 20 tiles
        private const float PushForce = 5f; // Base pushing force
        private const int PushDustType = MetalDetectionSystem.METAL_LINE_DUST_TYPE;
        private const float PlayerPushForce = 7.5f; // Base force
        private const float MaxPlayerPushSpeedSq = 12f * 12f; // Base max speed squared
        
        // Item damage configuration
        private const float ItemDamageBase = 8f; // Base damage for pushing an item into an NPC
        private const float ItemDamageVelocityMultiplier = 1.5f; // Damage multiplier based on item velocity
        private const int ItemDamageCooldown = 15; // Cooldown in ticks before the same item can damage again
        
        private int playerPushCooldown = 0;
        
        // Dictionary to track item damage cooldowns
        private Dictionary<int, int> itemDamageCooldowns = new Dictionary<int, int>();
        
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Steel;
        }
        
        // Handle collisions between items and NPCs for damage
        private void HandleItemCollisionsWithNPCs(Item item, Player player, bool isFlaring)
        {
            // Skip if the item is on cooldown
            if (itemDamageCooldowns.ContainsKey(item.whoAmI))
            {
                return;
            }
            
            // Calculate item collision box
            Rectangle itemHitbox = new Rectangle(
                (int)(item.position.X),
                (int)(item.position.Y),
                item.width,
                item.height
            );
            
            // Check for collisions with NPCs
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    // Calculate NPC collision box
                    Rectangle npcHitbox = new Rectangle(
                        (int)(npc.position.X),
                        (int)(npc.position.Y),
                        npc.width,
                        npc.height
                    );
                    
                    // Check for intersection
                    if (itemHitbox.Intersects(npcHitbox))
                    {
                        // Calculate damage based on item velocity and weight
                        float itemSpeed = item.velocity.Length();
                        float itemWeight = item.value * 0.0002f + 1f; // Use item value as an approximation of mass/importance
                        float baseDamage = ItemDamageBase * (isFlaring ? 2f : 1f);
                        
                        // Calculate final damage
                        int damage = (int)(baseDamage + (itemSpeed * ItemDamageVelocityMultiplier * itemWeight));
                        damage = System.Math.Min(damage, isFlaring ? 60 : 30); // Cap damage to prevent extreme values
                        
                        // Apply damage to the NPC - using HitInfo struct for newer tModLoader versions
                        int hitDirection = item.position.X < npc.position.X ? 1 : -1;
                        bool crit = Main.rand.NextBool(10); // 10% chance for critical hit
                        
                        // Create a HitInfo struct for newer tModLoader versions
                        var hitInfo = new NPC.HitInfo
                        {
                            Damage = damage,
                            Knockback = 8f,
                            HitDirection = hitDirection,
                            Crit = crit
                        };
                        
                        // Apply the damage
                        npc.StrikeNPC(hitInfo);
                        
                        // Visual effect
                        for (int d = 0; d < 8; d++)
                        {
                            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Iron, 
                                item.velocity.X * 0.2f, item.velocity.Y * 0.2f, 100, default, isFlaring ? 1.5f : 1f);
                        }
                        
                        // Set cooldown for this item to prevent rapid hits
                        itemDamageCooldowns[item.whoAmI] = ItemDamageCooldown;
                        
                        // Bounce the item away a bit to avoid getting stuck
                        item.velocity = -item.velocity * 0.3f;
                        
                        // Break out of the loop after hitting one NPC
                        break;
                    }
                }
            }
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            // Get the MistbornPlayer instance to check flaring status
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            float multiplier = modPlayer.IsFlaring ? 2.0f : 1.0f;
            
            // Calculate dynamic values based on flaring status
            float currentPushForce = PushForce * multiplier;
            float currentPlayerPushForce = PlayerPushForce * multiplier;
            float currentMaxSpeedSq = MaxPlayerPushSpeedSq * multiplier;
            
            if (playerPushCooldown > 0) {
                playerPushCooldown--;
            }
            
            // Update item damage cooldowns
            List<int> keysToRemove = new List<int>();
            foreach (var kvp in itemDamageCooldowns)
            {
                int cooldown = kvp.Value - 1;
                if (cooldown <= 0)
                {
                    keysToRemove.Add(kvp.Key);
                }
                else
                {
                    itemDamageCooldowns[kvp.Key] = cooldown;
                }
            }
            
            foreach (int key in keysToRemove)
            {
                itemDamageCooldowns.Remove(key);
            }
            
            // Find closest metal to mouse cursor
            Vector2 mouseWorld = Main.MouseWorld;
            float closestDistSq = PushRange * PushRange;
            Entity closestTargetEntity = null;
            Vector2? closestTilePos = null;
            
            // Check for items
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (item.active && MetalDetectionUtils.IsMetallicItem(item.type))
                {
                    float distSq = Vector2.DistanceSquared(mouseWorld, item.Center);
                    if (distSq < closestDistSq && Vector2.DistanceSquared(player.Center, item.Center) < PushRange * PushRange)
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
                    if (distSq < closestDistSq && Vector2.DistanceSquared(player.Center, npc.Center) < PushRange * PushRange)
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
            int tileScanRadius = (int)(PushRange / 16f) + 2;

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
                            if (distSq < closestDistSq && Vector2.DistanceSquared(player.Center, tileWorldCenter) < PushRange * PushRange)
                            {
                                closestDistSq = distSq;
                                closestTargetEntity = null;
                                closestTilePos = tileWorldCenter;
                            }
                        }
                    }
                }
            }
            
            // Draw lines and apply forces to the closest target
            bool isActivelySteelPushing = modPlayer.IsActivelySteelPushing;
            
            if (closestTargetEntity != null) 
            {
                // Always draw the line to show connection - thicker when flaring
                MetalDetectionUtils.DrawLineWithDust(player.Center, closestTargetEntity.Center, PushDustType, 
                                                    modPlayer.IsFlaring ? 0.22f : 0.15f, modPlayer.IsFlaring);
                
                // Apply push force to entities if actively pushing
                if (isActivelySteelPushing)
                {
                    Vector2 pushDirection = closestTargetEntity.Center - player.Center;
                    if (pushDirection != Vector2.Zero)
                    {
                        pushDirection.Normalize();
                        if (closestTargetEntity is Item targetItem) {
                             // Store the previous item velocity for damage calculation
                             Vector2 prevVelocity = targetItem.velocity;
                             
                             // Apply push force
                             targetItem.velocity += pushDirection * currentPushForce * 0.8f;
                             
                             // Check for collisions with NPCs to deal damage
                             if (targetItem.velocity.LengthSquared() > 4f) // Only check fast-moving items
                             {
                                 HandleItemCollisionsWithNPCs(targetItem, player, modPlayer.IsFlaring);
                             }
                        } else if (closestTargetEntity is NPC targetNPC) {
                             targetNPC.velocity += pushDirection * currentPushForce * (1f - targetNPC.knockBackResist) * 1.2f;
                        }
                    }
                }
            }
            else if (closestTilePos.HasValue)
            {
                // Always draw the line to show connection - thicker when flaring
                MetalDetectionUtils.DrawLineWithDust(player.Center, closestTilePos.Value, PushDustType, 
                                                    modPlayer.IsFlaring ? 0.22f : 0.15f, modPlayer.IsFlaring);
                
                // Apply force to player if actively pushing against tiles
                if (isActivelySteelPushing && playerPushCooldown <= 0)
                {
                    Vector2 pushDirection = player.Center - closestTilePos.Value;
                    if (pushDirection != Vector2.Zero) {
                        pushDirection.Normalize();
                        
                        // If player is falling, allow more frequent pushes for better control
                        bool isFalling = player.velocity.Y > 0;
                        int cooldownValue = isFalling ? (modPlayer.IsFlaring ? 3 : 5) : (modPlayer.IsFlaring ? 6 : 10);
                        
                        if (player.velocity.LengthSquared() < currentMaxSpeedSq) {
                            // Apply stronger push force when falling for better recovery
                            float pushMultiplier = isFalling ? 1.5f : 1.0f;
                            player.velocity += pushDirection * currentPlayerPushForce * pushMultiplier;
                            playerPushCooldown = cooldownValue;
                            
                            // Cancel fall damage if just pushed
                            player.fallStart = (int)(player.position.Y / 16f);
                        }
                    }
                }
            }
            
            // Add ambient dust around player when ability is active - more intense when flaring
            if (isActivelySteelPushing) 
            {
                int dustChance = modPlayer.IsFlaring ? 3 : 5;
                float dustScale = modPlayer.IsFlaring ? 1.8f : 1.5f;
                
                if (Main.rand.NextBool(dustChance))
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(dustScale, dustScale);
                    Dust.NewDustPerfect(player.Center, PushDustType, dustVel, 150, default, modPlayer.IsFlaring ? 1.0f : 0.8f);
                }
            }
            else if (Main.rand.NextBool(10))
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust.NewDustPerfect(player.Center, PushDustType, dustVel, 150, default, 0.8f);
            }
        }
    }
}