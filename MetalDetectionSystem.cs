using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MistbornMod.Utils;

namespace MistbornMod
{
    /// <summary>
    /// Handles the metal detection mechanic that shows blue lines to metal objects
    /// </summary>
    public class MetalDetectionSystem : ModSystem
    {
        // Standardize on the blue dust type for all metal detection lines
        public const int METAL_LINE_DUST_TYPE = DustID.BlueTorch;
        private const float DETECTION_RANGE = 500f;
        private const float LINE_DENSITY = 0.15f;
        
        public override void PostUpdateEverything()
        {
            // For every player that's using the metal detection hotkey
            for (int p = 0; p < Main.maxPlayers; p++)
            {
                Player player = Main.player[p];
                if (!player.active) continue;
                
                MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
                if (!modPlayer.IsDetectingMetals) continue;
                
                // Handle metal detection for this player
                DetectMetalsForPlayer(player, modPlayer);
            }
        }
        
        private void DetectMetalsForPlayer(Player player, MistbornPlayer modPlayer)
        {
            Vector2 mouseWorld = Main.MouseWorld;
            float closestDistSq = DETECTION_RANGE * DETECTION_RANGE;
            Entity closestTargetEntity = null;
            Vector2? closestTilePos = null;
            
            // Scan for metallic items
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (!item.active) continue;
                
                if (MetalDetectionUtils.IsMetallicItem(item.type))
                {
                    float distSq = Vector2.DistanceSquared(mouseWorld, item.Center);
                    if (distSq < closestDistSq && Vector2.DistanceSquared(player.Center, item.Center) < DETECTION_RANGE * DETECTION_RANGE)
                    {
                        closestDistSq = distSq;
                        closestTargetEntity = item;
                        closestTilePos = null;
                    }
                }
            }
            
            // Scan for metallic NPCs
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active) continue;
                
                if (MetalDetectionUtils.IsMetallicNPC(npc))
                {
                    float distSq = Vector2.DistanceSquared(mouseWorld, npc.Center);
                    if (distSq < closestDistSq && Vector2.DistanceSquared(player.Center, npc.Center) < DETECTION_RANGE * DETECTION_RANGE)
                    {
                        closestDistSq = distSq;
                        closestTargetEntity = npc;
                        closestTilePos = null;
                    }
                }
            }
            
            // Scan for metallic tiles
            int playerTileX = (int)(player.Center.X / 16f);
            int playerTileY = (int)(player.Center.Y / 16f);
            int tileScanRadius = (int)(DETECTION_RANGE / 16f) + 2;
            
            for (int x = playerTileX - tileScanRadius; x <= playerTileX + tileScanRadius; x++)
            {
                for (int y = playerTileY - tileScanRadius; y <= playerTileY + tileScanRadius; y++)
                {
                    if (!WorldGen.InWorld(x, y, 1)) continue;
                    Tile tile = Main.tile[x, y];
                    if (tile != null && tile.HasTile)
                    {
                        bool isMetallic = MetalDetectionUtils.IsMetallicOre(tile.TileType) || 
                                          MetalDetectionUtils.IsMetallicObject(tile.TileType);
                        
                        if (isMetallic)
                        {
                            Vector2 tileWorldPos = new Vector2(x * 16f + 8f, y * 16f + 8f);
                            float distSq = Vector2.DistanceSquared(mouseWorld, tileWorldPos);
                            if (distSq < closestDistSq && Vector2.DistanceSquared(player.Center, tileWorldPos) < DETECTION_RANGE * DETECTION_RANGE)
                            {
                                closestDistSq = distSq;
                                closestTargetEntity = null;
                                closestTilePos = tileWorldPos;
                            }
                        }
                    }
                }
            }
            
            // Draw the detection line to the closest target
            if (closestTargetEntity != null)
            {
                MetalDetectionUtils.DrawLineWithDust(player.Center, closestTargetEntity.Center, METAL_LINE_DUST_TYPE, LINE_DENSITY);
            }
            else if (closestTilePos.HasValue)
            {
                MetalDetectionUtils.DrawLineWithDust(player.Center, closestTilePos.Value, METAL_LINE_DUST_TYPE, LINE_DENSITY);
            }
        }
    }
}