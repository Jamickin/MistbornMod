using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using MistbornMod.Items;


namespace MistbornMod.Buffs
{
    public class IronBuff : MetalBuff 
    {
        private const float ScanRange = 500f;
        private const float PullRange = 400f;
        private const float PullForce = 3.5f; // Base pulling force
        private const float PlayerPullForce = 5f; // Base force to pull the player toward metals
        private const float MaxPlayerPullSpeedSq = 8f * 8f; // Base max squared velocity when player is pulled
        private const int LineDustType = DustID.MagicMirror; 

        private int playerPullCooldown = 0;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Metal = MetalType.Iron; 
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
            
            // Reduce cooldown for player pulling
            if (playerPullCooldown > 0) {
                playerPullCooldown--;
            }

            // Get mouse position for targeting
            Vector2 mouseWorld = Main.MouseWorld;
            float closestDistSq = PullRange * PullRange;
            Entity closestTargetEntity = null;
            Vector2? closestTilePos = null;

            // Scan for items to pull
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                if (!item.active || item.noGrabDelay > 0) continue;

                float distanceToItem = Vector2.Distance(player.Center, item.Center);
                bool isMetallic = IsMetallicItem(item.type); 

                if (distanceToItem < ScanRange && isMetallic)
                {
                    // More dust effects when flaring
                    if (Main.rand.NextBool(modPlayer.IsFlaring ? 3 : 5))
                    {
                        Dust.NewDust(item.position, item.width, item.height, LineDustType, 0f, 0f, 150, default, modPlayer.IsFlaring ? 0.8f : 0.6f);
                    }
                    
                    // Show line only to closest item or if actively pulling
                    float distSq = Vector2.DistanceSquared(mouseWorld, item.Center);
                    if (distSq < closestDistSq && distanceToItem < PullRange)
                    {
                        closestDistSq = distSq;
                        closestTargetEntity = item;
                        closestTilePos = null;
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
            int playerTileX = (int)(player.Center.X / 16f);
            int playerTileY = (int)(player.Center.Y / 16f);
            int tileScanRadius = (int)(ScanRange / 16f) + 2;

            for (int x = playerTileX - tileScanRadius; x <= playerTileX + tileScanRadius; x++)
            {
                for (int y = playerTileY - tileScanRadius; y <= playerTileY + tileScanRadius; y++)
                {
                    if (!WorldGen.InWorld(x, y, 1)) continue;
                    Tile tile = Main.tile[x, y];
                    if (tile != null && tile.HasTile)
                    {
                        // Check if this tile is a valid metallic ore or object
                        bool isMetallic = IsMetallicOre(tile.TileType) || IsMetallicObject(tile.TileType);
                        
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
                                
                                // Check if this tile is closest to mouse cursor
                                float distSq = Vector2.DistanceSquared(mouseWorld, tileWorldPos);
                                if (distSq < closestDistSq && distToTile < PullRange)
                                {
                                    closestDistSq = distSq;
                                    closestTargetEntity = null;
                                    closestTilePos = tileWorldPos;
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
                DrawLineWithDust(player.Center, closestTargetEntity.Center, LineDustType, modPlayer.IsFlaring ? 0.22f : 0.15f);
                
                // Check if this is a "held" mechanic activation
                if (modPlayer.IsActivelyIronPulling && playerPullCooldown <= 0)
                {
                    // Only pull player if they're actively using Iron and not an item
                    if (closestTargetEntity is not Item)
                    {
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
                DrawLineWithDust(player.Center, closestTilePos.Value, LineDustType, modPlayer.IsFlaring ? 0.22f : 0.15f);
                
                // Check if actively pulling and apply force to player
                if (modPlayer.IsActivelyIronPulling && playerPullCooldown <= 0)
                {
                    Vector2 pullDirection = closestTilePos.Value - player.Center;
                    if (pullDirection != Vector2.Zero)
                    {
                        pullDirection.Normalize();
                        if (player.velocity.LengthSquared() < currentMaxSpeedSq)
                        {
                            player.velocity += pullDirection * currentPlayerPullForce;
                            playerPullCooldown = modPlayer.IsFlaring ? 5 : 8; // Shorter cooldown when flaring
                            
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

        private bool IsMetallicItem(int itemType)
        {
            if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount) {
                 return false;
            }

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
                                   itemType == ItemID.EmptyBucket ||
                                   itemType == ItemID.MetalSink; 

            if (isKnownMetallic) return true;
            bool isMetallicTool = (sampleItem.pick > 0) ||   
                                  (sampleItem.axe > 0) ||      
                                  (sampleItem.hammer > 0) || 
                                  sampleItem.createTile == TileID.Anvils || sampleItem.createTile == TileID.MythrilAnvil;
            if (isMetallicTool) return true;
            bool isArmorPiece = sampleItem.defense > 0 && !sampleItem.accessory;
            bool isWeapon = (sampleItem.DamageType == DamageClass.Melee || sampleItem.DamageType == DamageClass.Ranged) && sampleItem.damage > 0;
            bool usesAmmo = sampleItem.useAmmo > 0;
            if (isArmorPiece || isWeapon || usesAmmo) return true;           
            return false;
        }

        private bool IsMetallicOre(int tileType)
        {
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
                   tileType == TileID.AlchemyTable;          // Alchemy table
        }

        private void DrawLineWithDust(Vector2 start, Vector2 end, int dustType, float density = 0.1f)
        {
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

            for (int i = 1; i <= steps; i++)
            {
                float progress = (float)i / steps;
                Vector2 dustPos = start + direction * distance * progress;
                if(Main.rand.NextBool(isFlaring ? 2 : 3)) {
                     Dust dust = Dust.NewDustPerfect(dustPos, dustType, Vector2.Zero, 150, default, isFlaring ? 0.5f : 0.4f);
                     dust.noGravity = true;
                     dust.velocity *= 0.1f;
                     dust.fadeIn = isFlaring ? 0.8f : 0.6f;
                }
            }
        }
    } 
}