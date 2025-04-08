using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System.Collections.Generic;
using Terraria.GameContent.Events;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;

namespace MistbornMod
{
    // This class handles the Mistborn ascension mechanics and world mist effects
    public class MistbornAscension : ModSystem
    {
        // Static instance for easy access
        public static MistbornAscension Instance;
        
        // Mist related properties
        public bool MistActive { get; private set; } = false;
        public float MistIntensity { get; private set; } = 0f;
        private const float MaxMistIntensity = 0.7f;
        private const float MistFadeInSpeed = 0.002f;
        private const float MistFadeOutSpeed = 0.001f;
        
        // Mist particle system
        private const int MaxMistParticles = 200;
        private List<MistParticle> mistParticles;
        private Random random;
        
        // Textures for mist particles
        private Asset<Texture2D> mistParticleTexture;
        
        // Whether the system is initialized
        private bool initialized = false;
        
        // Recipe group for lerasium beads (special ascension item)
        public static RecipeGroup LerasiumBeadRecipeGroup;

        public override void Load()
        {
            Instance = this;
            random = new Random();
            
            if (!Main.dedServ)
            {
                // Initialize mist particle list
                mistParticles = new List<MistParticle>(MaxMistParticles);
                
                // Load mist texture
                mistParticleTexture = ModContent.Request<Texture2D>("MistbornMod/Effects/MistParticle");
            }
        }
        
        public override void PostSetupContent()
        {
            // Register special recipe groups that can be used for the ascension item
            LerasiumBeadRecipeGroup = new RecipeGroup(
                () => $"{Language.GetTextValue("LegacyMisc.37")} {Language.GetTextValue("Mods.MistbornMod.Items.LerasiumBead.DisplayName")}",
                ItemID.LifeCrystal, // Life Crystal as base - but we'll add other items too
                ItemID.LifeFruit   // Also allow Life Fruit
            );
            
            // Register the recipe group
            RecipeGroup.RegisterGroup("MistbornMod:LerasiumBead", LerasiumBeadRecipeGroup);
        }

        public override void Unload()
        {
            Instance = null;
            
            if (mistParticles != null)
            {
                mistParticles.Clear();
                mistParticles = null;
            }
            
            mistParticleTexture = null;
            initialized = false;
        }
        
        // Method to make a player become Mistborn
        public void MakePlayerMistborn(Player player)
        {
            if (player != null)
            {
                MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
                
                // Only perform Ascension if the player isn't already Mistborn
                if (!modPlayer.IsMistborn)
                {
                    // Set the player as Mistborn
                    modPlayer.IsMistborn = true;
                    
                    // Activate the mist
                    ActivateMist();
                    
                    // Play ascension sound
                    SoundEngine.PlaySound(SoundID.Item4, player.position);
                    
                    // Create a visual effect around the player
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 velocity = Main.rand.NextVector2CircularEdge(8f, 8f);
                        Dust.NewDust(player.position, player.width, player.height, DustID.Shadowflame, velocity.X, velocity.Y, 0, Color.White, 1.5f);
                    }
                    
                    // Show message to the player
                    Main.NewText("You have ascended as a Mistborn!", 255, 255, 100);
                    
                    // Optionally, grant some starting metal vials to the player
                    GrantStartingVials(player);
                }
            }
        }
        
        // Grant starting vials to a new Mistborn
        private void GrantStartingVials(Player player)
        {
            // Give the player a few starting vials
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<Items.IronVial>(), 3);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<Items.PewterVial>(), 2);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<Items.TinVial>(), 2);
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<Items.SteelVial>(), 2);
        }
        
        // Method to activate the mist effect
        public void ActivateMist()
        {
            MistActive = true;
        }
        
        // Method to deactivate the mist effect
        public void DeactivateMist()
        {
            MistActive = false;
        }
        
        public override void PreUpdatePlayers()
        {
            // Check if at least one player is Mistborn
            bool anyMistborn = false;
            bool anyFlaring = false;
            
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active)
                {
                    MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
                    if (modPlayer.IsMistborn)
                    {
                        anyMistborn = true;
                        
                        // Check if any Mistborn is flaring
                        if (modPlayer.IsFlaring)
                        {
                            anyFlaring = true;
                            break;
                        }
                    }
                }
            }
            
            // Update mist state based on player status
            if (anyMistborn)
            {
                MistActive = true;
                
                // Increase intensity when flaring
                float targetIntensity = anyFlaring ? MaxMistIntensity : MaxMistIntensity * 0.6f;
                
                if (MistIntensity < targetIntensity)
                {
                    MistIntensity += MistFadeInSpeed;
                    if (MistIntensity > targetIntensity)
                    {
                        MistIntensity = targetIntensity;
                    }
                }
                else if (MistIntensity > targetIntensity)
                {
                    MistIntensity -= MistFadeOutSpeed;
                    if (MistIntensity < targetIntensity)
                    {
                        MistIntensity = targetIntensity;
                    }
                }
            }
            else
            {
                // Fade out if no Mistborn players
                if (MistIntensity > 0)
                {
                    MistIntensity -= MistFadeOutSpeed;
                    if (MistIntensity < 0)
                    {
                        MistIntensity = 0;
                        MistActive = false;
                    }
                }
                else
                {
                    MistActive = false;
                }
            }
        }
        
        public override void PostUpdateEverything()
        {
            if (!Main.dedServ && MistActive)
            {
                UpdateMistParticles();
            }
        }
        
        private void UpdateMistParticles()
        {
            if (!initialized)
            {
                InitializeMistParticles();
                initialized = true;
            }
            
            // Update existing particles
            for (int i = mistParticles.Count - 1; i >= 0; i--)
            {
                mistParticles[i].Update();
                
                // Remove dead particles
                if (mistParticles[i].Alpha <= 0)
                {
                    mistParticles.RemoveAt(i);
                }
            }
            
            // Add new particles based on intensity
            int particlesToAdd = (int)(MaxMistParticles * MistIntensity) - mistParticles.Count;
            
            if (Main.rand.NextBool(3) && particlesToAdd > 0)
            {
                for (int i = 0; i < Math.Min(particlesToAdd, 5); i++)
                {
                    // Create a new particle near a random position on screen
                    float screenX = Main.screenPosition.X + Main.rand.Next(0, Main.screenWidth);
                    float screenY = Main.screenPosition.Y + Main.rand.Next(0, Main.screenHeight);
                    
                    // Ensure particle is within world bounds
                    screenX = MathHelper.Clamp(screenX, 0, Main.maxTilesX * 16);
                    screenY = MathHelper.Clamp(screenY, 0, Main.maxTilesY * 16);
                    
                    // Create particle
                     mistParticles.Add(new MistParticle(
                        new Vector2(screenX, screenY),
                        new Vector2(Main.rand.NextFloat(-0.05f, 0.05f), Main.rand.NextFloat(-0.05f, 0.05f)),
                        Main.rand.Next(200, 400),
                        Main.rand.NextFloat(0.3f, 0.7f) * MistIntensity,
                        Main.rand.NextFloat(0.3f, 1.0f)
                    ));
                }
            }
        }
        
        private void InitializeMistParticles()
        {
            // Clear any existing particles
            mistParticles.Clear();
            
            // Initialize with a baseline of particles
           // Initialize with a baseline of particles
            int initialParticles = (int)(MaxMistParticles * MistIntensity * 0.5f);
            
            for (int i = 0; i < initialParticles; i++)
            {
                // Create a new particle at a random position
                float posX = Main.rand.Next(0, Main.maxTilesX * 16);
                float posY = Main.rand.Next(0, Main.maxTilesY * 16);
                
                mistParticles.Add(new MistParticle(
                    new Vector2(posX, posY),
                    new Vector2(Main.rand.NextFloat(-0.05f, 0.05f), Main.rand.NextFloat(-0.05f, 0.05f)),
                    Main.rand.Next(200, 400),
                    Main.rand.NextFloat(0.3f, 0.7f) * MistIntensity,
                    Main.rand.NextFloat(0.3f, 1.0f)
                ));
            }
        }
        public void DrawMist(SpriteBatch spriteBatch)
        {
            if (!Main.dedServ && MistActive && MistIntensity > 0)
            {
                // Apply global lighting tint during mist
                if (MistIntensity > 0.1f)
                {
                    // Darker during night, lighter during day
                    float dayFactor = Main.dayTime ? 0.7f : 0.4f;
                    float lightValue = MathHelper.Lerp(1f, dayFactor, MistIntensity * 0.7f);
                    Lighting.GlobalBrightness = lightValue;
                }
                
                // Draw mist overlay
                Color mistColor = Color.White * 0.2f * MistIntensity;
                mistColor.A = (byte)(100 * MistIntensity);
                
                // Fill the screen with a light mist overlay
                spriteBatch.Draw(
                    Terraria.GameContent.TextureAssets.MagicPixel.Value,
                    new Rectangle(0, 0, Main.screenWidth, Main.screenHeight),
                    null,
                    mistColor,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f
                );
                
                // Draw individual mist particles
                if (mistParticleTexture?.Value != null)
                {
                    foreach (var particle in mistParticles)
                    {
                        // Only draw particles that are on screen
                        if (IsOnScreen(particle.Position))
                        {
                            Vector2 screenPos = particle.Position - Main.screenPosition;
                            Color particleColor = Color.White * particle.Alpha * MistIntensity;
                            
                            spriteBatch.Draw(
                                mistParticleTexture.Value,
                                screenPos,
                                null,
                                particleColor,
                                0f,
                                mistParticleTexture.Size() * 0.5f,
                                particle.Scale,
                                SpriteEffects.None,
                                0f
                            );
                        }
                    }
                }
            }
        }
        
        private bool IsOnScreen(Vector2 position)
        {
            return position.X >= Main.screenPosition.X - 50 &&
                   position.X <= Main.screenPosition.X + Main.screenWidth + 50 &&
                   position.Y >= Main.screenPosition.Y - 50 &&
                   position.Y <= Main.screenPosition.Y + Main.screenHeight + 50;
        }
        
        // Inner class for mist particles
        private class MistParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public int Lifetime;
            public float Alpha;
            public float Scale;
            private int age = 0;
            private float originalAlpha;
            
            public MistParticle(Vector2 position, Vector2 velocity, int lifetime, float alpha, float scale)
            {
                Position = position;
                Velocity = velocity;
                Lifetime = lifetime;
                Alpha = alpha;
                originalAlpha = alpha;
                Scale = scale;
            }
            
            public void Update()
            {
                // Apply slight movement for mist swirl effect
                Position += Velocity;
                
                // Apply gentle swaying based on time
                Position.X += (float)Math.Sin(age * 0.02f) * 0.1f;
                
                // Age the particle
                age++;
                
                // Fade out near the end of lifetime
                if (age > Lifetime * 0.8f)
                {
                    float fadeRatio = 1f - ((float)(age - Lifetime * 0.8f) / (Lifetime * 0.2f));
                    Alpha = originalAlpha * fadeRatio;
                }
                
                // Kill particle if lifetime exceeded
                if (age >= Lifetime)
                {
                    Alpha = 0;
                }
            }
        }
    }
}