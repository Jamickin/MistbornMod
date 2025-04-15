using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding; // Contains GenPass, GenerationProgress
using Terraria.IO;          // Needed for GameConfiguration
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic; // Required for List<>


namespace MistbornMod
{
    // This class handles inserting the custom ore generation pass into the world generation steps
    public class ZincOreGeneration : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double weightTotal)
        {
            // Also keep a small amount of normal underground zinc for consistency
            int oreIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Ores"));
            
            // Find a MUCH LATER generation step, after most terrain has been processed
            // Microbios or Grass Wall is usually one of the last terrain passes
            int lateStageIndex = tasks.FindIndex(genpass => 
                genpass.Name.Equals("Micro Biomes") || 
                genpass.Name.Equals("Grass Wall") ||
                genpass.Name.Equals("Final Cleanup"));
            
            if (lateStageIndex != -1)
            {
                // Add our custom zinc stalagmite pass AFTER most terrain is generated
                tasks.Insert(lateStageIndex, new ZincStalagmitePass("Zinc Stalagmites", 100f));
                ModContent.GetInstance<MistbornMod>()?.Logger.Info($"Scheduled zinc stalagmites after '{tasks[lateStageIndex - 1].Name}' pass");
            }
            else
            {
                // Fall back to a known late pass if we can't find our preferred ones
                int finalCleanupIndex = tasks.FindIndex(genpass => genpass.Name.Contains("Cleanup") || genpass.Name.Equals("Planting Trees"));
                if (finalCleanupIndex != -1)
                {
                    tasks.Insert(finalCleanupIndex, new ZincStalagmitePass("Zinc Stalagmites", 100f));
                    ModContent.GetInstance<MistbornMod>()?.Logger.Info($"Scheduled zinc stalagmites before cleanup pass");
                }
                else
                {
                    Mod.Logger.Warn("Could not find appropriate late-stage GenPass for zinc stalagmites. Trying final position.");
                    // Last resort: add to the very end
                    tasks.Add(new ZincStalagmitePass("Zinc Stalagmites", 100f));
                }
            }
            
            if (oreIndex != -1)
            {
                // Keep a small amount of normal underground zinc ore
                tasks.Insert(oreIndex + 1, new ZincOrePass("Underground Zinc Ore", 100f));
            }
        }
    }

    // This class creates distinctive zinc stalagmites on the surface
    public class ZincStalagmitePass : GenPass
    {
        public ZincStalagmitePass(string name, float weight) : base(name, weight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Growing Zinc Stalagmites"; // Message for WorldGen Previewer

            // Calculate how many stalagmites to place based on world width
            // Fewer stalagmites with greater spacing between them
            int stalagmiteCount = (int)(Main.maxTilesX * 0.03f); // Reduced from 0.05f (about 1 per 33 blocks of width)
            
            int successfulPlacements = 0;
            int maxAttempts = stalagmiteCount * 5; // Allow more attempts to find good spots
            
            // Keep track of placed stalagmites to prevent overlapping
            List<Rectangle> placedAreas = new List<Rectangle>();
            
            for (int attempts = 0; attempts < maxAttempts && successfulPlacements < stalagmiteCount; attempts++)
            {
                // Choose random X position across the world surface
                int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
                
                // Find the surface at this X position
                int surfaceY = 0;
                bool foundSurface = false;
                
                // Scan downward from sky to find the first solid block
                for (int y = 50; y < Main.worldSurface + 20; y++)
                {
                    Tile tile = Framing.GetTileSafely(x, y);
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        surfaceY = y;
                        foundSurface = true;
                        break;
                    }
                }
                
                // Skip if no valid surface or if too high (sky islands)
                if (!foundSurface || surfaceY < 120)
                {
                    continue;
                }
                
                // Determine height of stalagmite (15-25 blocks)
                int height = WorldGen.genRand.Next(15, 26);
                
                // Width of base (5-9 blocks)
                int baseWidth = WorldGen.genRand.Next(5, 10);
                
                // Define the area this stalagmite would occupy (with buffer zone)
                Rectangle stalagmiteArea = new Rectangle(
                    x - baseWidth * 2,         // Left with padding
                    surfaceY - height - 5,     // Top with padding
                    baseWidth * 4,             // Width with padding
                    height + 10                // Height with padding
                );
                
                // Check if this would overlap with an existing stalagmite
                bool overlaps = false;
                foreach (var existingArea in placedAreas)
                {
                    // Add a minimum spacing between stalagmites (at least 20 tiles)
                    Rectangle bufferZone = new Rectangle(
                        existingArea.X - 20,
                        existingArea.Y,
                        existingArea.Width + 40,
                        existingArea.Height
                    );
                    
                    if (bufferZone.Intersects(stalagmiteArea))
                    {
                        overlaps = true;
                        break;
                    }
                }
                
                if (!overlaps)
                {
                    // Make the stalagmite start a bit deeper in the ground
                    int deeperSurfaceY = surfaceY + WorldGen.genRand.Next(3, 8);
                    
                    // Create the zinc stalagmite from a deeper position
                    CreateZincStalagmite(x, deeperSurfaceY, height, baseWidth);
                    successfulPlacements++;
                    
                    // Record this stalagmite's area to prevent overlaps
                    placedAreas.Add(stalagmiteArea);
                    
                    // Also record in the cleanup system for later restoration
                    ZincCleanupSystem.ZincStalagmiteAreas.Add(stalagmiteArea);
                }
                
                progress.Value = (float)successfulPlacements / stalagmiteCount;
            }
            
            // Update message with results
            progress.Message = $"Grew {successfulPlacements} Zinc Stalagmites";
            ModContent.GetInstance<MistbornMod>()?.Logger.Info($"Placed {successfulPlacements} zinc stalagmites on the world surface.");
        }
        
        // Custom method to create a stalagmite formation
        private void CreateZincStalagmite(int centerX, int bottomY, int height, int baseWidth)
        {
            // Use the shared method in MistbornMod class
            MistbornMod.CreateZincStalagmite(centerX, bottomY, height, baseWidth);
        }
    }

    // This class defines the original underground ore spawning logic
    public class ZincOrePass : GenPass
    {
        public ZincOrePass(string name, float weight) : base(name, weight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Spawning Underground Zinc Ore"; // Message for WorldGen Previewer

            // Reduced underground ore since we have prominent surface stalagmites
            int oreCount = (int)(Main.maxTilesX * Main.maxTilesY * 0.01); // Just a small amount underground
            oreCount = WorldGen.genRand.Next((int)(oreCount * 0.8), (int)(oreCount * 1.2));
            int attemptsMade = 0;

            for (int i = 0; i < oreCount; i++)
            {
                int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
                int y = WorldGen.genRand.Next((int)Main.worldSurface + 50, Main.maxTilesY - 150);
                int size = WorldGen.genRand.Next(4, 9); // Slightly larger veins

                WorldGen.TileRunner(
                    x, y,                                           // Position
                    size,                                           // Size of ore vein
                    WorldGen.genRand.Next(3, 8),                    // Steps
                    ModContent.TileType<Tiles.ZincOreTile>(),       // Tile type
                    false,                                          // Replace only air?
                    0f, 0f,                                         // Direction bias
                    false, true);                                   // No random direction change, use default strength
                
                attemptsMade++;
                progress.Value = (float)(i + 1) / oreCount;
            }

            progress.Message = $"Finished spawning Underground Zinc Ore ({attemptsMade} attempts)";
        }
    }
}