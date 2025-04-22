using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.IO;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using MistbornMod.Content.Tiles;

namespace MistbornMod
{
    // This class adds a final cleanup pass to restore any overwritten zinc stalagmites
    public class ZincCleanupSystem : ModSystem
    {
        // Keep track of where we placed zinc stalagmites
        public static List<Rectangle> ZincStalagmiteAreas = new List<Rectangle>();

        // Clear the list when world is unloaded
        public override void OnWorldUnload()
        {
            ZincStalagmiteAreas.Clear();
        }

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double weightTotal)
        {
            // Add our cleanup pass as the absolute final task
            tasks.Add(new ZincCleanupPass("Final Zinc Cleanup", 10f));
        }
    }

    // The cleanup pass that runs at the very end of world generation
    public class ZincCleanupPass : GenPass
    {
        public ZincCleanupPass(string name, float weight) : base(name, weight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Restoring Zinc Stalagmites";
            
            // Skip if no stalagmites were recorded
            if (ZincCleanupSystem.ZincStalagmiteAreas.Count == 0)
            {
                ModContent.GetInstance<MistbornMod>()?.Logger.Info("No zinc stalagmite areas to restore.");
                return;
            }

            int restoredAreas = 0;
            int zincTileType = ModContent.TileType<ZincOreTile>();
            
            // Check each recorded stalagmite area
            for (int i = 0; i < ZincCleanupSystem.ZincStalagmiteAreas.Count; i++)
            {
                Rectangle area = ZincCleanupSystem.ZincStalagmiteAreas[i];
                bool needsRestoration = true;
                int zincCount = 0;
                
                // First pass: count existing zinc tiles in this area
                for (int x = area.X; x < area.X + area.Width; x += 2)
                {
                    for (int y = area.Y; y < area.Y + area.Height; y += 2)
                    {
                        // Only sample every other tile to speed up the check
                        if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
                        {
                            Tile tile = Framing.GetTileSafely(x, y);
                            if (tile.HasTile && tile.TileType == zincTileType)
                            {
                                zincCount++;
                            }
                        }
                    }
                }
                
                // If we still have a good number of zinc tiles, don't restore
                if (zincCount > area.Width * area.Height / 20)
                {
                    needsRestoration = false;
                }
                
                // Second pass: restore if needed
                if (needsRestoration)
                {
                    // Find the ground level in this area
                    int groundY = 0;
                    for (int y = area.Y + area.Height - 1; y >= area.Y; y--)
                    {
                        Tile tile = Framing.GetTileSafely(area.X + area.Width / 2, y);
                        if (tile.HasTile && Main.tileSolid[tile.TileType])
                        {
                            groundY = y;
                            break;
                        }
                    }
                    
                    if (groundY > 0)
                    {
                        // Create a new stalagmite in this location
                        int centerX = area.X + area.Width / 2;
                        int baseWidth = area.Width / 4;
                        int height = (int)(area.Height * 0.7f);
                        
                        // Use the helper method from the zinc generation class
                        MistbornMod.CreateZincStalagmite(centerX, groundY, height, baseWidth);
                        restoredAreas++;
                    }
                }
                
                progress.Value = (float)(i + 1) / ZincCleanupSystem.ZincStalagmiteAreas.Count;
            }
            
            // Log results
            ModContent.GetInstance<MistbornMod>()?.Logger.Info($"Restored {restoredAreas} zinc stalagmite areas during final cleanup.");
            progress.Message = $"Restored {restoredAreas} Zinc Stalagmites";
        }
    }
}