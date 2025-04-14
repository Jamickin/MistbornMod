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
            int oreIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Ores"));
            if (oreIndex != -1)
            {
                tasks.Insert(oreIndex + 1, new ZincOrePass("Zinc Ore", 100f));
            }
            else
            {
                Mod.Logger.Warn("Vanilla 'Ores' GenPass not found. Zinc Ore will not be generated via standard insertion.");
            }
        }
    }

    // This class defines the actual ore spawning logic for Zinc Ore
    public class ZincOrePass : GenPass
    {
        public ZincOrePass(string name, float weight) : base(name, weight)
        {
        }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Spawning Zinc Ore"; // Message for WorldGen Previewer

            int oreCount = (int)(Main.maxTilesX * Main.maxTilesY * 0.02);
            oreCount = WorldGen.genRand.Next((int)(oreCount * 0.8), (int)(oreCount * 1.2));
            int attemptsMade = 0; // Renamed for clarity

            for (int i = 0; i < oreCount; i++)
            {
                int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
                int y = WorldGen.genRand.Next((int)Main.worldSurface + 50, Main.maxTilesY - 150);
                int size = WorldGen.genRand.Next(3, 9);

                // --- CORRECTED CODE BELOW ---
                // WorldGen.TileRunner returns void, so we cannot assign its result to a bool.
                // We simply call it to attempt the ore placement.
WorldGen.TileRunner(
    x, y,                                           // Position
    size,                                           // Size of ore vein
    WorldGen.genRand.Next(3, 8),                    // Steps (increased for more reliable placement)
    ModContent.TileType<Tiles.ZincOreTile>(),       // Tile type
    false,                                          // Replace only air?
    0f, 0f,                                         // Direction bias
    false, true);                                   // No random direction change, use default strength
                // Increment the counter for each attempt made.
                attemptsMade++;

                progress.Value = (float)(i + 1) / oreCount;
            }

            // Update message for WorldGen Previewer after completion
            // Use the count of attempts.
            progress.Message = $"Finished spawning Zinc Ore ({attemptsMade} attempts)";

            // Log completion to Mod's log file
            ModContent.GetInstance<MistbornMod>()?.Logger.Info($"Finished Zinc Ore pass. Made {attemptsMade} placement attempts.");
        }
    }
}


