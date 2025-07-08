using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MistbornMod.Content.NPCs
{
    /// <summary>
    /// Invisible anchor point that can be pushed against with Steel
    /// </summary>
    public class SteelAnchorPoint : ModNPC
    {
        private int lifeTime = 1800; // 30 seconds at 60 FPS
        
        public override void SetStaticDefaults()
        {
            // DisplayName set in localization
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.ActsLikeTownNPC[Type] = false;
            NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
            NPCID.Sets.ImmuneToAllBuffs[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 16;
            NPC.height = 16;
            NPC.damage = 0;
            NPC.defense = 999; // Effectively indestructible
            NPC.lifeMax = 999999;
            NPC.life = NPC.lifeMax;
            NPC.HitSound = SoundID.Tink;
            NPC.DeathSound = SoundID.Item10;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.knockBackResist = 0f; // Cannot be knocked back
            NPC.friendly = false;
            NPC.dontTakeDamage = true; // Cannot be damaged normally
            NPC.immortal = true;
            NPC.hide = true; // Hide from normal rendering
            NPC.alpha = 255; // Invisible
        }

        public override void AI()
        {
            lifeTime--;
            
            // Visual indicator every few frames
            if (lifeTime % 60 == 0) // Every second
            {
                // Subtle dust effect to show the anchor exists
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Iron, 
                    0f, 0f, 150, default, 0.5f);
            }
            
            // Fade out warning
            if (lifeTime < 300) // Last 5 seconds
            {
                if (lifeTime % 30 == 0) // Every half second
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, 
                        0f, -1f, 150, Color.Orange, 0.8f);
                }
            }
            
            // Remove after lifetime expires
            if (lifeTime <= 0)
            {
                // Visual effect when disappearing
                for (int i = 0; i < 10; i++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, 
                        Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 
                        100, default, 0.8f);
                }
                
                NPC.active = false;
            }
        }

        public override bool CheckActive()
        {
            // Don't despawn naturally
            return false;
        }

        public override void DrawBehind(int index)
        {
            // Don't draw this NPC in the normal layer
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Don't draw the NPC sprite (it's invisible)
            return false;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Draw a subtle indicator for the player who created it
            if (NPC.ai[0] >= 0 && NPC.ai[0] < Main.maxPlayers)
            {
                Player owner = Main.player[(int)NPC.ai[0]];
                if (owner != null && owner.active)
                {
                    float distance = Vector2.Distance(owner.Center, NPC.Center);
                    if (distance < 500f) // Only show if within reasonable range
                    {
                        // Draw a small crosshair to indicate the anchor point
                        Vector2 drawPos = NPC.Center - screenPos;
                        Color indicatorColor = Color.Gray * 0.7f;
                        
                        // Draw crosshair lines
                        for (int i = -2; i <= 2; i++)
                        {
                            for (int j = -2; j <= 2; j++)
                            {
                                if (i == 0 || j == 0) // Only draw the cross, not the corners
                                {
                                    spriteBatch.Draw(
                                        Terraria.GameContent.TextureAssets.MagicPixel.Value,
                                        drawPos + new Vector2(i, j),
                                        new Rectangle(0, 0, 1, 1),
                                        indicatorColor,
                                        0f,
                                        Vector2.Zero,
                                        1f,
                                        SpriteEffects.None,
                                        0f
                                    );
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}