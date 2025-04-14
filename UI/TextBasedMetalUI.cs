using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System.Linq;

namespace MistbornMod.UI
{
    /// <summary>
    /// A simple text-based UI to display metal reserves without requiring any custom textures
    /// </summary>
    public class TextBasedMetalUI : ModSystem
    {
        // UI configuration
        private Vector2 position = new Vector2(30, 80); // Default position on screen
        private const int LINE_HEIGHT = 20; // Height of each text line
        private bool isVisible = true; // UI visibility toggle
        
        // Keybind for toggling the UI
        public static ModKeybind ToggleUIHotkey;
        
        // Colors for each metal type
        private Dictionary<MetalType, Color> metalColors = new Dictionary<MetalType, Color>()
        {
            { MetalType.Iron, new Color(100, 100, 120) },
            { MetalType.Steel, new Color(150, 150, 180) },
            { MetalType.Tin, new Color(180, 180, 180) },
            { MetalType.Pewter, new Color(120, 120, 120) },
            { MetalType.Zinc, new Color(210, 190, 100) },
            { MetalType.Brass, new Color(190, 130, 70) },
            { MetalType.Copper, new Color(205, 95, 50) },
            { MetalType.Bronze, new Color(180, 115, 40) },
            { MetalType.Atium, new Color(230, 230, 255) },
            { MetalType.Chromium, new Color(150, 220, 255) }
        };
        
        public override void Load()
        {
            // Register UI toggle hotkey
            ToggleUIHotkey = KeybindLoader.RegisterKeybind(Mod, "Toggle Metal UI", "M");
        }
        
        public override void Unload()
        {
            ToggleUIHotkey = null;
        }
        
        public override void PostUpdateEverything()
        {
            // Handle toggle hotkey
            if (ToggleUIHotkey?.JustPressed ?? false)
            {
                isVisible = !isVisible;
            }
        }
        
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Find the resource bars layer
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                // Insert our UI after resource bars
                layers.Insert(resourceBarIndex + 1, new LegacyGameInterfaceLayer(
                    "MistbornMod: Metal Reserves Text UI",
                    delegate
                    {
                        if (!Main.gameMenu && Main.LocalPlayer.active && isVisible)
                        {
                            DrawTextUI(Main.spriteBatch);
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
        
        private void DrawTextUI(SpriteBatch spriteBatch)
{
    if (!isVisible) return;
    
    // Get player and mod player
    Player player = Main.LocalPlayer;
    MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
    
    // Only show UI for Mistborn or Misting players
    if (!modPlayer.IsMistborn && !modPlayer.IsMisting) return;
    
    // Get status strings
    List<(string Text, Color Color)> statusTexts = new List<(string, Color)>();
    
    // Add title
    statusTexts.Add((modPlayer.IsMistborn ? "MISTBORN RESERVES:" : "MISTING RESERVES:", Color.Gold));
    
    // Add a blank line
    statusTexts.Add((" ", Color.White));
    
    // Determine which metals to show
    List<MetalType> metalsToShow = new List<MetalType>();
    
    // For Mistborn, show all metals with reserves
    // For Mistings, only show their metal
    if (modPlayer.IsMistborn)
    {
        foreach (var metal in Enum.GetValues(typeof(MetalType)).Cast<MetalType>())
        {
            if (modPlayer.MetalReserves.TryGetValue(metal, out int reserve) && reserve > 0)
            {
                metalsToShow.Add(metal);
            }
        }
    }
    else if (modPlayer.IsMisting && modPlayer.MistingMetal.HasValue)
    {
        metalsToShow.Add(modPlayer.MistingMetal.Value);
    }
    
    // Add text for each metal
    foreach (var metal in metalsToShow)
    {
        // Get metal reserves
        int reserve = modPlayer.MetalReserves.TryGetValue(metal, out int value) ? value : 0;
        
        // Calculate time values
        float timeInSeconds = reserve / 60f;
        string timeDisplay = timeInSeconds > 60 ? 
            $"{Math.Floor(timeInSeconds / 60):0}m {timeInSeconds % 60:0}s" : 
            $"{timeInSeconds:0.0}s";
        
        // Check burning status
        bool isBurning = false;
        if (metal == MetalType.Steel)
            isBurning = modPlayer.IsActivelySteelPushing;
        else if (metal == MetalType.Iron)
            isBurning = modPlayer.IsActivelyIronPulling;
        else if (metal == MetalType.Chromium)
            isBurning = modPlayer.IsActivelyChromiumStripping;
        else
            isBurning = modPlayer.BurningMetals.TryGetValue(metal, out bool burning) && burning;
        
        // Get status text
        string status = isBurning ? 
            (modPlayer.IsFlaring ? "FLARING" : "BURNING") : 
            "INACTIVE";
        
        // Get metal color
        Color metalColor = metalColors.ContainsKey(metal) ? metalColors[metal] : Color.White;
        
        // Adjust color for burning/flaring
        if (isBurning)
        {
            if (modPlayer.IsFlaring)
            {
                // Pulse effect for flaring
                float pulse = 0.8f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 6) * 0.2f;
                metalColor = new Color(
                    Math.Min(255, (int)(metalColor.R * 1.7f * pulse)),
                    Math.Min(255, (int)(metalColor.G * 1.7f * pulse)),
                    Math.Min(255, (int)(metalColor.B * 1.7f * pulse))
                );
            }
            else
            {
                // Brighter for burning
                metalColor = new Color(
                    Math.Min(255, (int)(metalColor.R * 1.3f)),
                    Math.Min(255, (int)(metalColor.G * 1.3f)),
                    Math.Min(255, (int)(metalColor.B * 1.3f))
                );
            }
        }
        
        // Get hotkey for this metal
        string hotkey = modPlayer.GetHotkeyDisplayForMetal(metal);
        
        // Create bar representation using ASCII
        int barLength = 20;
        float percentage = modPlayer.GetMetalReservesPercentage(metal);
        int filledChars = (int)(barLength * percentage);
        
        string barText = "[";
        for (int i = 0; i < barLength; i++)
        {
            barText += (i < filledChars) ? "■" : "-";
        }
        barText += "]";
        
        // Combine all info
        string metalText = $"{metal} {hotkey} {barText} {timeDisplay} - {status}";
        statusTexts.Add((metalText, metalColor));
    }
    
    // Add a blank line
    statusTexts.Add((" ", Color.White));
    
    // Add total reserves info for Mistborn
    if (modPlayer.IsMistborn)
    {
        float totalPercentage = modPlayer.GetTotalReservesPercentage();
        int vialsUsed = (int)Math.Ceiling(totalPercentage * 6); // Max 6 vials
        
        // Create total bar
        int totalBarLength = 20;
        int filledChars = (int)(totalBarLength * totalPercentage);
        
        string totalBar = "[";
        for (int i = 0; i < totalBarLength; i++)
        {
            totalBar += (i < filledChars) ? "■" : "-";
        }
        totalBar += "]";
        
        string totalText = $"TOTAL: {totalBar} {vialsUsed}/6 vials ({(totalPercentage * 100):F0}%)";
        
        // Choose color based on how full reserves are
        Color totalColor = totalPercentage < 0.33f ? 
            Color.LimeGreen : 
            (totalPercentage < 0.66f ? Color.Yellow : Color.OrangeRed);
        
        statusTexts.Add((totalText, totalColor));
    }
    
    // Add help text
    statusTexts.Add((" ", Color.White));
    statusTexts.Add(("Press [ALT] to flare active metals", new Color(200, 200, 200)));
    statusTexts.Add(("Press [X] to detect metals without consuming reserves", new Color(200, 200, 200)));
    
    // NOW draw the background rectangle AFTER we know how many lines we have
    Rectangle bgRect = new Rectangle(
        (int)position.X - 10, 
        (int)position.Y - 10, 
        280,  // Width of background 
        (statusTexts.Count * LINE_HEIGHT) + 20); // Height based on text
    
    spriteBatch.Draw(
        Terraria.GameContent.TextureAssets.MagicPixel.Value, 
        bgRect, 
        new Color(0, 0, 0, 150)); // Semi-transparent black
    
    // Draw all text lines
    for (int i = 0; i < statusTexts.Count; i++)
    {
        var (text, color) = statusTexts[i];
        Vector2 textPos = position + new Vector2(0, i * LINE_HEIGHT);
        
        // Use Terraria.Utils instead of just Utils
        Terraria.Utils.DrawBorderStringFourWay(
            spriteBatch,
            Terraria.GameContent.FontAssets.MouseText.Value,
            text,
            textPos.X,
            textPos.Y,
            color,
            Color.Black,
            Vector2.Zero,
            1f
        );
    }
}
    }
}