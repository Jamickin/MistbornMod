using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
namespace MistbornMod.Buffs
{
    public class SteelBuff : MetalBuff
    {
        private const float PushRange = 320f; // 20 tiles
        private const float PushForce = 5f; // Increased from 4f for smoother pushing
        private const int PushDustType = DustID.Smoke;
        private const float PlayerPushForce = 7.5f; // Increased from 6f
        private const float MaxPlayerPushSpeedSq = 12f * 12f; // Increased max speed for better mobility
        private int playerPushCooldown = 0;
        
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Steel;
        }
        
        public override void Update(Player player, ref int buffIndex)
        {
            if (playerPushCooldown > 0) {
                playerPushCooldown--;
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
                    if (tile != null && tile.HasTile && IsMetallicOre(tile.TileType))
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

            // Draw lines and apply forces to the closest target
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            bool isActivelySteelPushing = modPlayer.IsActivelySteelPushing;
            
            if (closestTargetEntity != null) 
            {
                // Always draw the line to show connection
                DrawLineWithDust(player.Center, closestTargetEntity.Center, PushDustType, 0.15f);
                
                // Apply push force to entities if actively pushing
                if (isActivelySteelPushing)
                {
                    Vector2 pushDirection = closestTargetEntity.Center - player.Center;
                    if (pushDirection != Vector2.Zero)
                    {
                        pushDirection.Normalize();
                        if (closestTargetEntity is Item targetItem) {
                             targetItem.velocity += pushDirection * PushForce * 0.8f;
                        } else if (closestTargetEntity is NPC targetNPC) {
                             targetNPC.velocity += pushDirection * PushForce * (1f - targetNPC.knockBackResist) * 1.2f;
                        }
                    }
                }
            }
            else if (closestTilePos.HasValue)
            {
                // Always draw the line to show connection
                DrawLineWithDust(player.Center, closestTilePos.Value, PushDustType, 0.15f);
                
                // Apply force to player if actively pushing against tiles
                if (isActivelySteelPushing && playerPushCooldown <= 0)
                {
                    Vector2 pushDirection = player.Center - closestTilePos.Value;
                    if (pushDirection != Vector2.Zero) {
                        pushDirection.Normalize();
                        
                        // If player is falling, allow more frequent pushes for better control
                        bool isFalling = player.velocity.Y > 0;
                        int cooldownValue = isFalling ? 5 : 10;
                        
                        if (player.velocity.LengthSquared() < MaxPlayerPushSpeedSq) {
                            // Apply stronger push force when falling for better recovery
                            float pushMultiplier = isFalling ? 1.5f : 1.0f;
                            player.velocity += pushDirection * PlayerPushForce * pushMultiplier;
                            playerPushCooldown = cooldownValue;
                            
                            // Cancel fall damage if just pushed
                            player.fallStart = (int)(player.position.Y / 16f);
                        }
                    }
                }
            }
            
            // Add ambient dust around player when ability is active
            if (isActivelySteelPushing && Main.rand.NextBool(5))
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f);
                Dust.NewDustPerfect(player.Center, PushDustType, dustVel, 150, default, 0.8f);
            }
            else if (Main.rand.NextBool(10))
            {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f);
                Dust.NewDustPerfect(player.Center, PushDustType, dustVel, 150, default, 0.8f);
            }
        } 
        
        private bool IsMetallicNPC(NPC npc) {
             bool wearsArmor = npc.defense > 10;
             bool isSpecificType = npc.type == NPCID.Probe || 
                                   npc.type == NPCID.ArmoredSkeleton || 
                                   npc.type == NPCID.PossessedArmor || 
                                   npc.type == NPCID.MeteorHead;
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
                                       sampleItem.createTile == TileID.PlatinumBrick);
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
                                   itemType == ItemID.EmptyBucket || 
                                   itemType == ItemID.MetalSink;
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
        
        private void DrawLineWithDust(Vector2 start, Vector2 end, int dustType, float density = 0.1f) {
             if (Vector2.DistanceSquared(start, end) < 16f * 16f) return;
             
             Vector2 direction = end - start;
             float distance = direction.Length();
             if (distance == 0f) return;
             
             direction.Normalize();
             int steps = (int)(distance * density);
             if (steps <= 0) return;
             
             for (int i = 1; i <= steps; i++) {
                 float progress = (float)i / steps;
                 Vector2 dustPos = start + direction * distance * progress;
                 if(Main.rand.NextBool(3)) {
                      Dust dust = Dust.NewDustPerfect(dustPos, dustType, Vector2.Zero, 150, default, 0.5f);
                      dust.noGravity = true;
                      dust.velocity *= 0.1f;
                      dust.fadeIn = 0.6f;
                 }
             }
        }
    } 
}