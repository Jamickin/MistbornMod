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
    // Remove dependence on mouse position - scan entire radius
    float detectionRadius = DETECTION_RANGE;
    List<(Entity entity, Vector2 position)> metallicEntities = new List<(Entity, Vector2)>();
    List<Vector2> metallicTilePositions = new List<Vector2>();
    
    // Scan for metallic items
    for (int i = 0; i < Main.maxItems; i++)
    {
        Item item = Main.item[i];
        if (!item.active) continue;
        
        if (MetalDetectionUtils.IsMetallicItem(item.type))
        {
            float distSq = Vector2.DistanceSquared(player.Center, item.Center);
            if (distSq < detectionRadius * detectionRadius)
            {
                metallicEntities.Add((item, item.Center));
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
            float distSq = Vector2.DistanceSquared(player.Center, npc.Center);
            if (distSq < detectionRadius * detectionRadius)
            {
                metallicEntities.Add((npc, npc.Center));
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
                    float distSq = Vector2.DistanceSquared(player.Center, tileWorldPos);
                    if (distSq < detectionRadius * detectionRadius)
                    {
                        metallicTilePositions.Add(tileWorldPos);
                    }
                }
            }
        }
    }
    
    // Draw lines to ALL detected entities and tiles
    foreach (var (entity, position) in metallicEntities)
    {
        MetalDetectionUtils.DrawLineWithDust(player.Center, position, METAL_LINE_DUST_TYPE, LINE_DENSITY);
    }
    
    foreach (Vector2 tilePos in metallicTilePositions)
    {
        MetalDetectionUtils.DrawLineWithDust(player.Center, tilePos, METAL_LINE_DUST_TYPE, LINE_DENSITY);
    }
}
}}