using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
namespace MistbornMod.Buffs
{
    public class SteelBuff : MetalBuff
    {
        private const float PushRange = 320f; // 20 tiles
        private const float PushForce = 5f; // Base pushing force
        private const int PushDustType = DustID.BlueTorch;
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
            
            Vector2 mouseWorld = Main.MouseWorld;
            float closestDistSq = PushRange * PushRange;
            Entity closestTargetEntity = null;
            Vector2? closestTilePos = null;
            
            // Check for items
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (item.active && IsMetallicItem(item.type))
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
                if (npc.active && !npc.friendly && npc.knockBackResist < 1f && IsMetallicNPC(npc))
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
                        bool isMetallic = IsMetallicOre(tile.TileType) || IsMetallicObject(tile.TileType);
                        
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
                DrawLineWithDust(player.Center, closestTargetEntity.Center, PushDustType, modPlayer.IsFlaring ? 0.22f : 0.15f);
                
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
                DrawLineWithDust(player.Center, closestTilePos.Value, PushDustType, modPlayer.IsFlaring ? 0.22f : 0.15f);
                
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
        
        private bool IsMetallicNPC(NPC npc) {
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
        
        private bool IsMetallicItem(int itemType) {
             if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount) { return false; }
             Item sampleItem = ContentSamples.ItemsByType[itemType];
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
                                   itemType == ItemID.EmptyBucket;
             if (isKnownMetallic) return true;
             
             bool isMetallicTool = (sampleItem.pick > 0) || 
                                  (sampleItem.axe > 0) || 
                                  (sampleItem.hammer > 0) || 
                                  sampleItem.createTile == TileID.Anvils || 
                                  sampleItem.createTile == TileID.MythrilAnvil;
             if (isMetallicTool) return true;
             
             bool isArmorPiece = sampleItem.defense > 0 && !sampleItem.accessory;
             bool isWeapon = (sampleItem.DamageType == DamageClass.Melee || 
                             sampleItem.DamageType == DamageClass.Ranged) && 
                             sampleItem.damage > 0;
             bool usesAmmo = sampleItem.useAmmo > 0;
             
             if (isArmorPiece || isWeapon || usesAmmo) return true;
             return false;
        }
        
        private bool IsMetallicOre(int tileType) {
            if (tileType < 0 || tileType >= TileLoader.TileCount) return false;
            
            return TileID.Sets.Ore[tileType] ||
                   tileType == TileID.Iron || 
                   tileType == TileID.Lead ||
                   tileType == TileID.Silver || 
                   tileType == TileID.Tungsten ||
                   tileType == TileID.Gold || 
                   tileType == TileID.Platinum ||
                   tileType == ModContent.TileType<Tiles.ZincOreTile>();
        }
        
        private bool IsMetallicObject(int tileType)
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
           // Corrected chest references:
           tileType == TileID.Containers ||          // Container group 1 (various chests)
           tileType == TileID.Containers2 ||         // Container group 2 (more chests)
           tileType == TileID.FakeContainers ||      // Fake container group 1
           tileType == TileID.FakeContainers2 ||     // Fake container group 2
          
           // Other metal objects:
           tileType == TileID.MetalBars ||           // Metal bars again (for emphasis)
           tileType == TileID.GoldBirdCage ||        // Gold bird cage
           tileType == TileID.Campfire ||            // Campfire (iron grate underneath)
           // Fixed missing IDs (using correct TileID references):
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
           tileType == TileID.TargetDummy ||         // Training dummy (correct ID)
           tileType == TileID.MinecartTrack;         // Minecart track
}

        
        private void DrawLineWithDust(Vector2 start, Vector2 end, int dustType, float density = 0.1f) {
             if (Vector2.DistanceSquared(start, end) < 16f * 16f) return;
             
             Vector2 direction = end - start;
             float distance = direction.Length();
             if (distance == 0f) return;
             
             direction.Normalize();
             int steps = (int)(distance * density);
             if (steps <= 0) return;
             
             // Get the player's flaring status for dust intensity
             MistbornPlayer modPlayer = Main.LocalPlayer.GetModPlayer<MistbornPlayer>();
             bool isFlaring = modPlayer?.IsFlaring ?? false;
             
             for (int i = 1; i <= steps; i++) {
                 float progress = (float)i / steps;
                 Vector2 dustPos = start + direction * distance * progress;
                 if(Main.rand.NextBool(isFlaring ? 2 : 3)) {
                      Dust dust = Dust.NewDustPerfect(dustPos, dustType, Vector2.Zero, 150, default, isFlaring ? 0.6f : 0.5f);
                      dust.noGravity = true;
                      dust.velocity *= 0.1f;
                      dust.fadeIn = isFlaring ? 0.8f : 0.6f;
                 }
             }
        }
    } 
}