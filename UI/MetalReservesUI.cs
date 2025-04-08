using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace MistbornMod.UI
{
    // This class handles the UI state for displaying metal reserves
    internal class MetalReservesUI : UIState
    {
        // Constants for UI layout
        private const float PANEL_WIDTH = 200f;
        private const float PANEL_HEIGHT = 280f;
        private const float BAR_HEIGHT = 16f;
        private const float ICON_SIZE = 24f;
        private const float SPACING = 4f;
        private const float PADDING = 10f;
        
        // UI position
        private Vector2 _position = new Vector2(20, 260); // Default position
        
        // Dragging state
        private bool _dragging;
        private Vector2 _dragOffset;
        
        // Cache of metals to display
        private List<MetalType> _metalTypesToDisplay;

        public override void OnInitialize()
        {
            // Initialize list of metals to display
            _metalTypesToDisplay = new List<MetalType>();
            
            // Order of metals in UI - we'll display them in this order
            foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
            {
                // Only add implemented metals
                switch (metal)
                {
                    case MetalType.Iron:
                    case MetalType.Steel:
                    case MetalType.Tin:
                    case MetalType.Pewter:
                    case MetalType.Brass:
                    case MetalType.Zinc:
                    case MetalType.Atium:
                        _metalTypesToDisplay.Add(metal);
                        break;
                }
            }
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Handle UI dragging
            if (_dragging)
            {
                _position = Main.MouseScreen - _dragOffset;
                
                // Ensure the UI stays within screen bounds
                _position.X = Math.Clamp(_position.X, 0, Main.screenWidth - PANEL_WIDTH);
                _position.Y = Math.Clamp(_position.Y, 0, Main.screenHeight - PANEL_HEIGHT);
                
                // Stop dragging if mouse released
                if (!Main.mouseLeft)
                {
                    _dragging = false;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            
            // Get the MistbornPlayer instance
            MistbornPlayer modPlayer = Main.LocalPlayer.GetModPlayer<MistbornPlayer>();
            
            // Calculate the header area for dragging
            Rectangle headerArea = new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                (int)PANEL_WIDTH,
                30
            );
            
            // Check for clicking on the header (for dragging)
            if (headerArea.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && !_dragging)
            {
                _dragging = true;
                _dragOffset = Main.MouseScreen - _position;
            }
            
            // Draw panel background
            Texture2D backgroundTexture = MistbornUISystem.MetalUIBackground.Value;
            if (backgroundTexture != null)
            {
                // Draw a 9-slice panel
                Utils.DrawSplicedPanel(
                    spriteBatch,
                    backgroundTexture,
                    (int)_position.X,
                    (int)_position.Y,
                    (int)PANEL_WIDTH,
                    (int)PANEL_HEIGHT,
                    10, 10, 10, 10,
                    Color.White
                );
            }
            else
            {
                // Fallback: draw colored rectangle if texture is missing
                spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value,
                    new Rectangle((int)_position.X, (int)_position.Y, (int)PANEL_WIDTH, (int)PANEL_HEIGHT),
                    null,
                    new Color(75, 75, 75, 230),
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f
                );
            }
            
            // Draw panel title
            string title = Language.GetTextValue("Mods.MistbornMod.UI.MetalReserves.Title");
            Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(title);
            Utils.DrawBorderString(
                spriteBatch,
                title,
                new Vector2(_position.X + PANEL_WIDTH / 2 - titleSize.X / 2, _position.Y + PADDING),
                Color.White
            );
            
            // Draw total reserves bar
            float totalPercentage = modPlayer.GetTotalReservesPercentage();
            float totalBarWidth = PANEL_WIDTH - PADDING * 2;
            
            // Draw total reserve text
            string totalCapacityText = Language.GetTextValue("Mods.MistbornMod.UI.MetalReserves.TotalCapacity");
            Vector2 totalCapacitySize = FontAssets.ItemStack.Value.MeasureString(totalCapacityText);
            Utils.DrawBorderString(
                spriteBatch,
                totalCapacityText,
                new Vector2(_position.X + PADDING, _position.Y + PADDING + titleSize.Y + SPACING),
                Color.White,
                0.8f
            );
            
            // Draw total reserve bar
            DrawProgressBar(
                spriteBatch,
                new Vector2(_position.X + PADDING, _position.Y + PADDING + titleSize.Y + totalCapacitySize.Y + SPACING * 2),
                totalBarWidth,
                BAR_HEIGHT,
                totalPercentage,
                new Color(220, 220, 220),
                new Color(50, 50, 50)
            );
            
            // Draw vials used text
            int vialsUsed = (int)Math.Ceiling(modPlayer.TotalReserves / (float)MistbornPlayer.METAL_VIAL_AMOUNT);
            string vialsText = string.Format(Language.GetTextValue("Mods.MistbornMod.UI.MetalReserves.VialsUsed"), vialsUsed);
            Vector2 vialsTextSize = FontAssets.ItemStack.Value.MeasureString(vialsText);
            Utils.DrawBorderString(
                spriteBatch,
                vialsText,
                new Vector2(_position.X + PANEL_WIDTH - PADDING - vialsTextSize.X, _position.Y + PADDING + titleSize.Y + SPACING),
                vialsUsed >= 6 ? new Color(255, 100, 100) : Color.White,
                0.8f
            );
            
            // Calculate the start Y position for drawing metal bars
            float currentY = _position.Y + PADDING + titleSize.Y + totalCapacitySize.Y + BAR_HEIGHT + SPACING * 4;
            
            // Draw each metal's reserves
            foreach (MetalType metal in _metalTypesToDisplay)
            {
                // Get the metal's reserve percentage
                float metalPercentage = modPlayer.GetMetalReservesPercentage(metal);
                
                // Get metal color
                Color metalColor = Color.Gray;
                if (MistbornUISystem.MetalColors.TryGetValue(metal, out Color color))
                {
                    metalColor = color;
                }
                
                // Calculate burning status
                bool isBurning = false;
                if (metal == MetalType.Iron)
                {
                    isBurning = modPlayer.IsActivelyIronPulling;
                }
                else if (metal == MetalType.Steel)
                {
                    isBurning = modPlayer.IsActivelySteelPushing;
                }
                else
                {
                    isBurning = modPlayer.BurningMetals.TryGetValue(metal, out bool burning) && burning;
                }
                
                // Draw metal icon
                DrawMetalIcon(
                    spriteBatch,
                    new Vector2(_position.X + PADDING, currentY),
                    metal,
                    isBurning,
                    modPlayer.IsFlaring && isBurning
                );
                
                // Draw metal name
                string metalName = metal.ToString();
                Vector2 nameSize = FontAssets.ItemStack.Value.MeasureString(metalName);
                Utils.DrawBorderString(
                    spriteBatch,
                    metalName,
                    new Vector2(_position.X + PADDING + ICON_SIZE + SPACING, currentY + ICON_SIZE / 2 - nameSize.Y / 2),
                    isBurning ? (modPlayer.IsFlaring ? new Color(255, 200, 50) : new Color(255, 255, 150)) : Color.White,
                    0.8f
                );
                
                // Draw metal reserve as seconds
                int secondsLeft = modPlayer.MetalReserves.TryGetValue(metal, out int reserves) ? reserves / 60 : 0;
                string timeText = $"{secondsLeft}s";
                Vector2 timeSize = FontAssets.ItemStack.Value.MeasureString(timeText);
                Utils.DrawBorderString(
                    spriteBatch,
                    timeText,
                    new Vector2(_position.X + PANEL_WIDTH - PADDING - timeSize.X, currentY + ICON_SIZE / 2 - timeSize.Y / 2),
                    isBurning ? (modPlayer.IsFlaring ? new Color(255, 150, 50) : new Color(255, 255, 150)) : Color.White,
                    0.8f
                );
                
                // Draw metal reserve bar
                DrawProgressBar(
                    spriteBatch,
                    new Vector2(_position.X + PADDING, currentY + ICON_SIZE + SPACING / 2),
                    PANEL_WIDTH - PADDING * 2,
                    BAR_HEIGHT,
                    metalPercentage,
                    metalColor,
                    new Color(20, 20, 20)
                );
                
                // Advance Y position for next metal
                currentY += ICON_SIZE + BAR_HEIGHT + SPACING * 2;
            }
        }
        
        // Helper method to draw a progress bar
        private void DrawProgressBar(SpriteBatch spriteBatch, Vector2 position, float width, float height, float fillPercentage, Color fillColor, Color backgroundColor)
        {
            // Draw background
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                new Rectangle((int)position.X, (int)position.Y, (int)width, (int)height),
                null,
                backgroundColor,
                0f,
                Vector2.Zero,
                SpriteEffects.None,
                0f
            );
            
            // Draw fill
            int fillWidth = (int)(width * fillPercentage);
            if (fillWidth > 0)
            {
                spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value,
                    new Rectangle((int)position.X + 1, (int)position.Y + 1, fillWidth - 2, (int)height - 2),
                    null,
                    fillColor,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        
        // Helper method to draw a metal icon
        private void DrawMetalIcon(SpriteBatch spriteBatch, Vector2 position, MetalType metal, bool burning, bool flaring)
        {
            // Calculate source rectangle based on metal type
            int iconIndex = (int)metal;
            // Ensure the index is within bounds
            iconIndex = Math.Min(iconIndex, 8);
            
            // Calculate the source rectangle for the icon
            Rectangle source = new Rectangle(
                iconIndex * 24, // Assuming 24x24 icons in a sprite sheet
                0,
                24,
                24
            );
            
            // Get icon color - brighter if burning
            Color iconColor = Color.White;
            if (burning)
            {
                iconColor = flaring ? new Color(255, 200, 100) : new Color(255, 255, 200);
            }
            
            // Draw the icon
            Texture2D iconTexture = MistbornUISystem.MetalIconTexture.Value;
            if (iconTexture != null)
            {
                spriteBatch.Draw(
                    iconTexture,
                    new Rectangle((int)position.X, (int)position.Y, (int)ICON_SIZE, (int)ICON_SIZE),
                    source,
                    iconColor,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f
                );
                
                // Draw glowing effect if burning
                if (burning)
                {
                    float scale = flaring ? 1.5f : 1.2f;
                    spriteBatch.Draw(
                        iconTexture,
                        new Rectangle(
                            (int)(position.X - (ICON_SIZE * (scale - 1) / 2)),
                            (int)(position.Y - (ICON_SIZE * (scale - 1) / 2)),
                            (int)(ICON_SIZE * scale),
                            (int)(ICON_SIZE * scale)
                        ),
                        source,
                        new Color(255, 255, 200, 40),
                        0f,
                        Vector2.Zero,
                        SpriteEffects.None,
                        0f
                    );
                }
            }
            else
            {
                // Fallback to a colored rectangle if texture is missing
                spriteBatch.Draw(
                    TextureAssets.MagicPixel.Value,
                    new Rectangle((int)position.X, (int)position.Y, (int)ICON_SIZE, (int)ICON_SIZE),
                    null,
                    MistbornUISystem.MetalColors.TryGetValue(metal, out Color color) ? color : Color.Gray,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
}