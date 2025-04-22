using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MistbornMod.Content.Tiles;

namespace MistbornMod.Common.Systems
{
    /// <summary>
    /// Utility class for detecting metals in the game world
    /// </summary>
    public static class MetalDetectionUtils
    {
        /// <summary>
        /// Checks if an NPC is considered metallic for Allomantic powers
        /// </summary>
        /// <param name="npc">The NPC to check</param>
        /// <returns>True if metallic</returns>
        public static bool IsMetallicNPC(NPC npc)
        {
            bool wearsArmor = npc.defense > 10;
            bool isSpecificType = npc.type == NPCID.Probe || 
                                  npc.type == NPCID.ArmoredSkeleton || 
                                  npc.type == NPCID.PossessedArmor || 
                                  npc.type == NPCID.MeteorHead ||
                                  npc.type == NPCID.Golem ||
                                  npc.type == NPCID.GolemHead ||
                                  npc.type == NPCID.GolemFistLeft ||
                                  npc.type == NPCID.GolemFistRight ||
                                  npc.type == NPCID.SkeletronPrime ||
                                  npc.type == NPCID.PrimeCannon ||
                                  npc.type == NPCID.PrimeSaw ||
                                  npc.type == NPCID.PrimeVice ||
                                  npc.type == NPCID.PrimeLaser ||
                                  npc.type == NPCID.TheDestroyer ||
                                  npc.type == NPCID.TheDestroyerBody ||
                                  npc.type == NPCID.TheDestroyerTail ||
                                  npc.type == NPCID.Mimic;
                                  
            return wearsArmor || isSpecificType;
        }
        
        /// <summary>
        /// Checks if an item is considered metallic for Allomantic powers
        /// </summary>
        /// <param name="itemType">The item type ID to check</param>
        /// <returns>True if metallic</returns>
        public static bool IsMetallicItem(int itemType)
        {
            if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount) { return false; }
            
            Item sampleItem = ContentSamples.ItemsByType[itemType];
            
            // Check if the item places a metallic tile
            bool placesMetallicTile = sampleItem.createTile >= TileID.Dirt && 
                                     (TileID.Sets.Ore[sampleItem.createTile] || 
                                      sampleItem.createTile == TileID.MetalBars || 
                                      sampleItem.createTile == TileID.IronBrick || 
                                      sampleItem.createTile == TileID.LeadBrick || 
                                      sampleItem.createTile == TileID.SilverBrick || 
                                      sampleItem.createTile == TileID.TungstenBrick || 
                                      sampleItem.createTile == TileID.GoldBrick || 
                                      sampleItem.createTile == TileID.PlatinumBrick ||
                                      sampleItem.createTile == TileID.Anvils);
            if (placesMetallicTile) return true;
            
            // Check if it's a known metallic item
            bool isKnownMetallic = itemType == ItemID.IronBar || 
                                  itemType == ItemID.LeadBar || 
                                  itemType == ItemID.SilverBar || 
                                  itemType == ItemID.TungstenBar || 
                                  itemType == ItemID.GoldBar || 
                                  itemType == ItemID.PlatinumBar || 
                                  itemType == ItemID.CopperCoin || 
                                  itemType == ItemID.SilverCoin || 
                                  itemType == ItemID.GoldCoin || 
                                  itemType == ItemID.PlatinumCoin || 
                                  itemType == ItemID.Chain || 
                                  itemType == ItemID.Hook || 
                                  itemType == ItemID.Wire || 
                                  itemType == ItemID.Minecart || 
                                  itemType == ItemID.EmptyBucket ||
                                  itemType == ItemID.MetalSink;
            if (isKnownMetallic) return true;
            
            // Check if it's a metallic tool
            bool isMetallicTool = sampleItem.pick > 0 || 
                                 sampleItem.axe > 0 || 
                                 sampleItem.hammer > 0 || 
                                 sampleItem.createTile == TileID.Anvils || 
                                 sampleItem.createTile == TileID.MythrilAnvil;
            if (isMetallicTool) return true;
            
            // Check if it's armor or a weapon
            bool isArmorPiece = sampleItem.defense > 0 && !sampleItem.accessory;
            bool isWeapon = (sampleItem.DamageType == DamageClass.Melee || 
                            sampleItem.DamageType == DamageClass.Ranged) && 
                            sampleItem.damage > 0;
            bool usesAmmo = sampleItem.useAmmo > 0;
            
            return isArmorPiece || isWeapon || usesAmmo;
        }
        
        /// <summary>
        /// Checks if a tile is a metallic ore
        /// </summary>
        /// <param name="tileType">The tile type ID to check</param>
        /// <returns>True if metallic ore</returns>
        public static bool IsMetallicOre(int tileType)
        {
            if (tileType < 0 || tileType >= TileLoader.TileCount) return false;
            
            return TileID.Sets.Ore[tileType] ||
                   tileType == TileID.Iron || 
                   tileType == TileID.Lead ||
                   tileType == TileID.Silver || 
                   tileType == TileID.Tungsten ||
                   tileType == TileID.Gold || 
                   tileType == TileID.Platinum ||
                   // Adding ZincOreTile dynamically based on mod content
                   tileType == ModContent.TileType<ZincOreTile>();
        }
        
        /// <summary>
        /// Checks if a tile is a metallic object
        /// </summary>
        /// <param name="tileType">The tile type ID to check</param>
        /// <returns>True if metallic object</returns>
        public static bool IsMetallicObject(int tileType)
        {
            // Check for various metal objects
            return tileType == TileID.MetalBars ||           // Metal bars
                   tileType == TileID.Anvils ||              // Anvils (regular)
                   tileType == TileID.MythrilAnvil ||        // Mythril anvil
                   tileType == TileID.AdamantiteForge ||     // Adamantite forge
                   tileType == TileID.Furnaces ||            // Furnaces
                   tileType == TileID.Hellforge ||           // Hellforge
                   tileType == TileID.Chain ||               // Chains
                   tileType == TileID.Bathtubs ||            // Bathtubs
                   tileType == TileID.Chandeliers ||         // Chandeliers
                   tileType == TileID.Cannon ||              // Cannons
                   tileType == TileID.LandMine ||            // Land mines
                   tileType == TileID.Traps ||               // Traps
                   tileType == TileID.Boulder ||             // Boulders
                   tileType == TileID.IronBrick ||           // Iron bricks
                   tileType == TileID.LeadBrick ||           // Lead bricks
                   tileType == TileID.CopperBrick ||         // Copper bricks
                   tileType == TileID.TinBrick ||            // Tin bricks
                   tileType == TileID.SilverBrick ||         // Silver bricks
                   tileType == TileID.TungstenBrick ||       // Tungsten bricks
                   tileType == TileID.GoldBrick ||           // Gold bricks
                   tileType == TileID.PlatinumBrick ||       // Platinum bricks
                   tileType == TileID.AlchemyTable ||        // Alchemy table
                   tileType == TileID.Containers ||          // Container group 1 (various chests)
                   tileType == TileID.Containers2 ||         // Container group 2 (more chests)
                   tileType == TileID.FakeContainers ||      // Fake container group 1
                   tileType == TileID.FakeContainers2 ||     // Fake container group 2
                   tileType == TileID.GoldBirdCage ||        // Gold bird cage
                   tileType == TileID.Campfire ||            // Campfire (iron grate underneath)
                   tileType == TileID.Kegs ||                // Kegs (metal bands)
                   tileType == TileID.GrandfatherClocks ||   // Grandfather clocks (gears)
                   tileType == TileID.Lamps ||               // Lamps (metal components)
                   tileType == TileID.WaterFountain ||       // Water fountain
                   tileType == TileID.TrashCan ||            // Trash can
                   tileType == TileID.Sawmill ||             // Sawmill (metal parts)
                   tileType == TileID.Lever ||               // Lever
                   tileType == TileID.Switches ||            // Switches
                   tileType == TileID.PressurePlates ||      // Pressure plates
                   tileType == TileID.ClosedDoor ||          // Doors (metal components)
                   tileType == TileID.OpenDoor ||            // Open doors (metal components)
                   tileType == TileID.DisplayDoll ||         // Display doll (metal stand)
                   tileType == TileID.WeaponsRack ||         // Weapons rack
                   tileType == TileID.TargetDummy ||         // Training dummy
                   tileType == TileID.MinecartTrack ||         // Minecart track
                   tileType == TileID.Candelabras ||  // Add candles
           tileType == TileID.Candles ||
           tileType == TileID.HangingLanterns ||  // Add lanterns
           tileType == TileID.WaterFountain ||
           tileType == TileID.CopperCoinPile ||  // Add coins
           tileType == TileID.SilverCoinPile ||
           tileType == TileID.GoldCoinPile ||
           tileType == TileID.PlatinumCoinPile ||
           tileType == TileID.CopperPlating ||  // Add plating
           tileType == TileID.GoldBirdCage;

        }
        
        /// <summary>
        /// Draws a dust line between two points
        /// </summary>
        /// <param name="start">Start position</param>
        /// <param name="end">End position</param>
        /// <param name="dustType">Dust type ID</param>
        /// <param name="density">Line density (0.1f default)</param>
        /// <param name="isFlaring">Whether the player is flaring</param>
        public static void DrawLineWithDust(Vector2 start, Vector2 end, int dustType, float density = 0.1f, bool isFlaring = false)
        {
            if (Vector2.DistanceSquared(start, end) < 16f * 16f) return;
            
            Vector2 direction = end - start;
            float distance = direction.Length();
            if (distance == 0f) return;
            
            direction.Normalize();
            int steps = (int)(distance * density);
            if (steps <= 0) return;
            
            for (int i = 1; i <= steps; i++)
            {
                float progress = (float)i / steps;
                Vector2 dustPos = start + direction * distance * progress;
                if (Main.rand.NextBool(isFlaring ? 2 : 3))
                {
                    Dust dust = Dust.NewDustPerfect(dustPos, dustType, Vector2.Zero, 150, default, isFlaring ? 0.6f : 0.5f);
                    dust.noGravity = true;
                    dust.velocity *= 0.1f;
                    dust.fadeIn = isFlaring ? 0.8f : 0.6f;
                }
            }
        }
    }
}