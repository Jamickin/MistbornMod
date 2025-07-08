// Content/Projectiles/SteelAnchorBullet.cs
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MistbornMod.Content.NPCs; // ADD THIS LINE

namespace MistbornMod.Content.Projectiles
{
    /// <summary>
    /// Special ammunition that creates steel-pushable anchor points
    /// </summary>
    public class SteelAnchorBullet : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName set in localization
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300; // 5 seconds flight time
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Add some trailing dust to make it visible
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Iron, 0f, 0f, 100, default, 0.5f);
            }
            
            // Slight gravity
            Projectile.velocity.Y += 0.01f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateAnchorPoint();
            return true; // Destroy the projectile
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            CreateAnchorPoint();
        }

        private void CreateAnchorPoint()
        {
            // Create the anchor point NPC at impact location
            var source = Projectile.GetSource_FromThis();
            int anchorIndex = NPC.NewNPC(source, (int)Projectile.Center.X, (int)Projectile.Center.Y, 
                ModContent.NPCType<SteelAnchorPoint>()); // Now this will work

            if (anchorIndex < Main.maxNPCs)
            {
                NPC anchor = Main.npc[anchorIndex];
                anchor.ai[0] = Projectile.owner; // Remember who created this anchor
                
                // Visual effect
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 
                        DustID.Iron, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 
                        100, default, 1.2f);
                }
                
                // Sound effect
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.Gray;
        }
    }
}