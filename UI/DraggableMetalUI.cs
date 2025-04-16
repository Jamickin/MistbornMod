using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace MistbornMod.UI
{
    /// <summary>
    /// Enhanced UI for metal reserves with dragging, scaling, and customization options
    /// </summary>
    public class DraggableMetalUI : ModSystem
    {
        // References
        internal static DraggableMetalUI Instance;

        // UI configuration
        private Vector2 position = new Vector2(Main.screenWidth / 2 - 140, 80); // Center of screen by default

        public void EnsureVisibilityForNewMisting()
        {
            // Force visibility when a player becomes a new Misting
            isVisible = true;
        }

        private const int LINE_HEIGHT = 20;
        private float scale = 1.0f;
        private bool isVisible = true;
        private bool isDragging = false;
        private Vector2 dragOffset;
        private bool dragEnabled = true;

        // Individual metal bar positions for unlinking
        private Dictionary<MetalType, bool> unlinkedBars = new Dictionary<MetalType, bool>();
        private Dictionary<MetalType, Vector2> barPositions = new Dictionary<MetalType, Vector2>();

        // Total vials section can also be unlinked
        private bool totalSectionUnlinked = false;
        private Vector2 totalSectionPosition = Vector2.Zero;

        // Keybind for toggling the UI
        public static ModKeybind ToggleUIHotkey;

        // Bar style enum
        public enum BarStyle
        {
            Gradient,
        }

        private BarStyle currentBarStyle = BarStyle.Gradient;

        // Colors for each metal type - instantiated once to avoid recreating during draw
        private static readonly Dictionary<MetalType, Color> metalColors = new Dictionary<
            MetalType,
            Color
        >()
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
            { MetalType.Chromium, new Color(150, 220, 255) },
        };

        // Cached list to avoid creating during drawing
        private List<(string Text, Color Color, MetalType? Metal, bool IsHeader)> statusTexts =
            new List<(string, Color, MetalType?, bool)>();

        // Cache of rects for each element to handle mouse interactions
        private List<(
            Rectangle Rect,
            MetalType? Metal,
            bool IsHeader,
            bool IsTotalSection
        )> interactionRects = new List<(Rectangle, MetalType?, bool, bool)>();

        // Currently dragging element
        private MetalType? draggingMetal = null;
        private bool draggingHeader = false;
        private bool draggingTotalSection = false;

        // Shortened bar config
        private const int BAR_LENGTH = 7; // Shortened from 20
        private readonly int[] BAR_THRESHOLDS = new int[] { 40, 70, 100 }; // Percentage thresholds

        // Configuration object
        private MetalUIConfig config;

        // Calculated text widths for proper background sizing
        private Dictionary<string, float> textWidths = new Dictionary<string, float>();

        public override void Load()
        {
            Instance = this;

            // Register UI toggle hotkey
            if (ToggleUIHotkey == null) // Only register if not already registered
            {
                ToggleUIHotkey = KeybindLoader.RegisterKeybind(Mod, "Toggle Metal UI", "M");
            }

            // Initialize dictionaries for all metal types
            foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
            {
                unlinkedBars[metal] = false;
                barPositions[metal] = Vector2.Zero;
            }

            // Initialize total section position
            totalSectionPosition = Vector2.Zero;
        }

        public override void OnModLoad()
        {
            // Load config
            if (ModContent.GetInstance<MetalUIConfig>() is MetalUIConfig loadedConfig)
            {
                config = loadedConfig;
                ApplyConfig();
            }
            else
            {
                // Create a new config if none exists
                config = new MetalUIConfig();
            }
        }

        // Make sure the ApplyConfig method properly initializes all values

        internal void ApplyConfig()
        {
            if (config == null)
                return;

            // Use the config position but add a fallback to screen center if it's at origin
            if (config.DefaultPosition == Vector2.Zero)
            {
                position = new Vector2(Main.screenWidth / 2 - 140, 80); // Center fallback
            }
            else
            {
                position = config.DefaultPosition;
            }

            scale = config.UIScale;
            dragEnabled = config.AllowDragging;
            isVisible = config.ShowByDefault;

            // HideButtonHints is automatically used in the DrawTotalReserves method
            // without needing to store it locally - it's accessed directly from config

            // Apply saved positions and unlink status
            foreach (var item in config.UnlinkedMetals ?? new Dictionary<string, bool>())
            {
                if (Enum.TryParse<MetalType>(item.Key, out var metal))
                {
                    unlinkedBars[metal] = item.Value;
                }
            }

            foreach (var item in config.MetalPositions ?? new Dictionary<string, Vector2>())
            {
                if (Enum.TryParse<MetalType>(item.Key, out var metal))
                {
                    barPositions[metal] = item.Value;
                }
            }

            // Load total section settings
            totalSectionUnlinked = config.TotalSectionUnlinked;
            totalSectionPosition = config.TotalSectionPosition;
        }

        // Call this when the user clicks "Restore Defaults" in the config menu
        public void RestoreDefaults()
        {
            // Reset UI scale and position
            scale = 1.0f;
            position = new Vector2(30, 80);

            // Re-link all bars
            foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
            {
                unlinkedBars[metal] = false;
                barPositions[metal] = Vector2.Zero;
            }

            // Re-link total section
            totalSectionUnlinked = false;
            totalSectionPosition = Vector2.Zero;

            // Update config with default values
            if (config != null)
            {
                config.DefaultPosition = position;
                config.UIScale = scale;
                config.HideButtonHints = false; // Reset to default (show hints)
                config.UnlinkedMetals = unlinkedBars.ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Value
                );
                config.MetalPositions = barPositions.ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Value
                );
                config.TotalSectionUnlinked = false;
                config.TotalSectionPosition = Vector2.Zero;
            }
        }

        private void SaveUIConfiguration()
        {
            if (config != null)
            {
                config.DefaultPosition = position;
                config.UnlinkedMetals = unlinkedBars.ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Value
                );
                config.MetalPositions = barPositions.ToDictionary(
                    x => x.Key.ToString(),
                    x => x.Value
                );
                config.TotalSectionUnlinked = totalSectionUnlinked;
                config.TotalSectionPosition = totalSectionPosition;
            }
        }

        public override void OnWorldUnload()
        {
            // Save position to config
            SaveUIConfiguration();
        }

        public void ForceToggleVisibility()
        {
            isVisible = !isVisible;

            // Debug message
            Main.NewText($"Metal UI visibility: {(isVisible ? "ON" : "OFF")}");
        }

        public override void Unload()
        {
            ToggleUIHotkey = null;
            Instance = null;
        }

        public override void PostUpdateEverything()
        {
            // Handle toggle hotkey
            if (ToggleUIHotkey != null && ToggleUIHotkey.JustPressed)
            {
                isVisible = !isVisible;
            }

            HandleMouseInteraction();
        }

        private void HandleMouseInteraction()
        {
            if (!isVisible || !dragEnabled)
                return;

            // Get mouse state
            MouseState mouse = Mouse.GetState();
            Vector2 mousePos = new Vector2(mouse.X, mouse.Y) / Main.UIScale;

            // Handle dragging
            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                // Start dragging - check if clicked on a draggable element
                foreach (var (rect, metal, isHeader, isTotalSection) in interactionRects)
                {
                    if (rect.Contains(mousePos.ToPoint()))
                    {
                        isDragging = true;

                        if (isHeader)
                        {
                            draggingHeader = true;
                            dragOffset = mousePos - position;
                        }
                        else if (isTotalSection && totalSectionUnlinked)
                        {
                            draggingTotalSection = true;
                            dragOffset = mousePos - totalSectionPosition;
                        }
                        else if (metal.HasValue && unlinkedBars[metal.Value])
                        {
                            draggingMetal = metal;
                            dragOffset = mousePos - barPositions[metal.Value];
                        }

                        break;
                    }
                }
            }
            else if (Main.mouseLeft && isDragging)
            {
                // Continue dragging
                if (draggingHeader)
                {
                    // Move the entire UI
                    position = mousePos - dragOffset;
                }
                else if (draggingTotalSection)
                {
                    // Move just the total section
                    totalSectionPosition = mousePos - dragOffset;
                }
                else if (draggingMetal.HasValue)
                {
                    // Move just this metal bar
                    barPositions[draggingMetal.Value] = mousePos - dragOffset;
                }
            }
            else if (Main.mouseLeftRelease)
            {
                // When drag ends, save the configuration
                if (isDragging)
                {
                    SaveUIConfiguration();
                }

                // Stop dragging
                isDragging = false;
                draggingHeader = false;
                draggingTotalSection = false;
                draggingMetal = null;
            }

            // Handle right-click to toggle unlinking
            if (Main.mouseRight && Main.mouseRightRelease)
            {
                bool configChanged = false;

                foreach (var (rect, metal, isHeader, isTotalSection) in interactionRects)
                {
                    if (rect.Contains(mousePos.ToPoint()))
                    {
                        if (isTotalSection && !isHeader)
                        {
                            // Toggle total section unlinking
                            totalSectionUnlinked = !totalSectionUnlinked;

                            // If newly unlinked, set initial position
                            if (totalSectionUnlinked)
                            {
                                Vector2 defaultPos =
                                    position
                                    + new Vector2(
                                        0,
                                        (
                                            GetMetalCount(
                                                Main.LocalPlayer.GetModPlayer<MistbornPlayer>()
                                            ) + 3
                                        ) * LINE_HEIGHT
                                    );
                                totalSectionPosition = defaultPos;
                            }

                            configChanged = true;
                            break;
                        }
                        else if (!isHeader && metal.HasValue)
                        {
                            // Toggle unlinking for this metal
                            unlinkedBars[metal.Value] = !unlinkedBars[metal.Value];

                            // If newly unlinked, set initial position
                            if (unlinkedBars[metal.Value])
                            {
                                barPositions[metal.Value] =
                                    position
                                    + new Vector2(
                                        0,
                                        (GetMetalIndex(metal.Value) + 2) * LINE_HEIGHT
                                    );
                            }

                            configChanged = true;
                            break;
                        }
                    }
                }

                // Save configuration if we changed something
                if (configChanged)
                {
                    SaveUIConfiguration();
                }
            }
        }

        private int GetMetalIndex(MetalType metal)
        {
            Player player = Main.LocalPlayer;
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();

            List<MetalType> metalsToShow = new List<MetalType>();

            if (modPlayer.IsMistborn)
            {
                foreach (var m in Enum.GetValues(typeof(MetalType)).Cast<MetalType>())
                {
                    if (modPlayer.MetalReserves.TryGetValue(m, out int reserve) && reserve > 0)
                    {
                        metalsToShow.Add(m);
                    }
                }
            }
            else if (modPlayer.IsMisting && modPlayer.MistingMetal.HasValue)
            {
                metalsToShow.Add(modPlayer.MistingMetal.Value);
            }

            return metalsToShow.IndexOf(metal);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int resourceBarIndex = layers.FindIndex(layer =>
                layer.Name.Equals("Vanilla: Resource Bars")
            );
            if (resourceBarIndex != -1)
            {
                layers.Insert(
                    resourceBarIndex + 1,
                    new LegacyGameInterfaceLayer(
                        "MistbornMod: Metal Reserves UI",
                        delegate
                        {
                            if (!Main.gameMenu && Main.LocalPlayer.active && isVisible)
                            {
                                DrawUI(Main.spriteBatch);
                            }
                            return true;
                        },
                        InterfaceScaleType.UI
                    )
                );
            }
        }

        // Get estimated text width for proper background sizing
        private float GetTextWidth(string text, float scale)
        {
            string cacheKey = text + scale.ToString();

            if (textWidths.TryGetValue(cacheKey, out float width))
            {
                return width;
            }

            // Estimate text width based on character count and scale
            // This is an approximation - adjust multiplier as needed for your font
            float estimatedWidth = text.Length * 8.5f * scale;

            // Cache the result
            textWidths[cacheKey] = estimatedWidth;

            return estimatedWidth;
        }

        private void DrawUI(SpriteBatch spriteBatch)
        {
            if (!isVisible)
                return;

            // Get player and mod player
            Player player = Main.LocalPlayer;
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();

            // Only show UI for Mistborn or Misting players with discovered abilities
            if (
                !modPlayer.IsMistborn
                && (!modPlayer.IsMisting || !modPlayer.HasDiscoveredMistingAbility)
            )
            {
                return;
            }

            // Clear the cached lists before populating
            statusTexts.Clear();
            interactionRects.Clear();

            // Add title
            statusTexts.Add(
                (
                    modPlayer.IsMistborn ? "MISTBORN RESERVES:" : "MISTING RESERVES:",
                    Color.Gold,
                    null,
                    true
                )
            );

            // Determine which metals to show
            List<MetalType> metalsToShow = new List<MetalType>();

            if (modPlayer.IsMistborn)
            {
                foreach (var metal in Enum.GetValues(typeof(MetalType)).Cast<MetalType>().ToArray())
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

            // Draw header background with very low opacity (almost invisible)
            Rectangle headerRect = new Rectangle(
                (int)(position.X - 10 * scale),
                (int)(position.Y - 10 * scale),
                (int)(280 * scale),
                (int)(30 * scale)
            );

            spriteBatch.Draw(
                Terraria.GameContent.TextureAssets.MagicPixel.Value,
                headerRect,
                new Color(0, 0, 0, 5)
            ); // Almost completely invisible (alpha = 5)

            // Add header rect to interaction list
            interactionRects.Add((headerRect, null, true, false));

            // Draw header text
            Vector2 titlePos = position * scale;
            Terraria.Utils.DrawBorderStringFourWay(
                spriteBatch,
                Terraria.GameContent.FontAssets.MouseText.Value,
                statusTexts[0].Text,
                titlePos.X,
                titlePos.Y,
                statusTexts[0].Color,
                Color.Black,
                Vector2.Zero,
                scale
            );

            // Draw each metal bar
            for (int i = 0; i < metalsToShow.Count; i++)
            {
                var metal = metalsToShow[i];
                DrawMetalBar(spriteBatch, metal, modPlayer, i);
            }

            // Draw total reserves info for Mistborn as a footer
            if (modPlayer.IsMistborn)
            {
                DrawTotalReserves(spriteBatch, modPlayer);
            }
        }

        private void DrawMetalBar(
            SpriteBatch spriteBatch,
            MetalType metal,
            MistbornPlayer modPlayer,
            int index
        )
        {
            // Determine position based on whether this bar is unlinked
            Vector2 barPos;
            if (unlinkedBars[metal])
            {
                barPos = barPositions[metal] * scale;
            }
            else
            {
                barPos = position + new Vector2(0, (index + 2) * LINE_HEIGHT) * scale;
            }

            // Get metal reserves
            int reserve = modPlayer.MetalReserves.TryGetValue(metal, out int value) ? value : 0;

            // Calculate time values
            float timeInSeconds = reserve / 60f;
            string timeDisplay =
                timeInSeconds > 60
                    ? $"{Math.Floor(timeInSeconds / 60):0}m {timeInSeconds % 60:0}s"
                    : $"{timeInSeconds:0.0}s";

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
            string status = isBurning ? (modPlayer.IsFlaring ? "FLARING" : "BURNING") : "INACTIVE";

            // Get metal color
            Color metalColor = metalColors.TryGetValue(metal, out Color color)
                ? color
                : Color.White;

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

            // Calculate percentage
            float percentage = modPlayer.GetMetalReservesPercentage(metal) * 100;

            // Always use Gradient style
            string barText = DrawGradientBar(percentage);

            // Combine all info
            string metalText = $"{metal} {hotkey} {barText} {timeDisplay} - {status}";

            // Get estimated text width for proper background sizing
            float textWidth = GetTextWidth(metalText, scale);

            // Draw background for this bar with adequate width
            Rectangle barRect = new Rectangle(
                (int)(barPos.X - 10 * scale),
                (int)(barPos.Y - 5 * scale),
                (int)(280 * scale),
                (int)(25 * scale)
            );

            // Different background color if unlinked, but with nearly invisible alpha
            Color bgColor = unlinkedBars[metal] ? new Color(20, 20, 30, 5) : new Color(0, 0, 0, 3);

            spriteBatch.Draw(Terraria.GameContent.TextureAssets.MagicPixel.Value, barRect, bgColor);

            // Add to interaction rects
            interactionRects.Add((barRect, metal, false, false));

            // Draw text
            Terraria.Utils.DrawBorderStringFourWay(
                spriteBatch,
                Terraria.GameContent.FontAssets.MouseText.Value,
                metalText,
                barPos.X,
                barPos.Y,
                metalColor,
                Color.Black,
                Vector2.Zero,
                scale
            );
        }

        private string DrawGradientBar(float percentage)
        {
            // Use gradual characters: "░▒▓█"
            string barText = "[";

            for (int i = 0; i < BAR_LENGTH; i++)
            {
                float sectionPercentage = (float)(i + 1) / BAR_LENGTH * 100;

                if (percentage >= sectionPercentage)
                    barText += "█";
                else if (percentage >= sectionPercentage - 10)
                    barText += "▓";
                else if (percentage >= sectionPercentage - 20)
                    barText += "▒";
                else if (percentage >= sectionPercentage - 30)
                    barText += "░";
                else
                    barText += "-";
            }

            barText += "]";
            return barText;
        }

        private void DrawTotalReserves(SpriteBatch spriteBatch, MistbornPlayer modPlayer)
        {
            // Calculate position for the footer
            Vector2 footerPos;
            if (totalSectionUnlinked)
            {
                footerPos = totalSectionPosition;
            }
            else
            {
                footerPos = position + new Vector2(0, (GetMetalCount(modPlayer) + 3) * LINE_HEIGHT);
            }

            // Apply scale to position
            footerPos *= scale;

            // Calculate total reserves
            float totalPercentage = modPlayer.GetTotalReservesPercentage();
            int vialsUsed = (int)Math.Ceiling(totalPercentage * 6); // Max 6 vials

            // Total text to display
            string totalText = $"TOTAL: {vialsUsed}/6 vials ({(totalPercentage * 100):F0}%)";

            // Get estimated text width for proper background sizing
            float textWidth = GetTextWidth(totalText, scale);

            // Background with proper width
            Rectangle footerRect = new Rectangle(
                (int)(footerPos.X - 10 * scale),
                (int)(footerPos.Y - 5 * scale),
                (int)(280 * scale),
                (int)(25 * scale)
            );

            // Different color if unlinked - but nearly invisible
            Color footerBgColor = totalSectionUnlinked
                ? new Color(25, 25, 35, 5)
                : new Color(0, 0, 0, 3);

            spriteBatch.Draw(
                Terraria.GameContent.TextureAssets.MagicPixel.Value,
                footerRect,
                footerBgColor
            );

            // Add footer rect to interaction list to enable dragging
            interactionRects.Add((footerRect, null, false, true));

            // Choose color based on how full reserves are
            Color totalColor =
                totalPercentage < 0.33f
                    ? Color.LimeGreen
                    : (totalPercentage < 0.66f ? Color.Yellow : Color.OrangeRed);

            // Draw text
            Terraria.Utils.DrawBorderStringFourWay(
                spriteBatch,
                Terraria.GameContent.FontAssets.MouseText.Value,
                totalText,
                footerPos.X,
                footerPos.Y,
                totalColor,
                Color.Black,
                Vector2.Zero,
                scale
            );

            // Only draw help text if HideButtonHints is false
            if (config == null || !config.HideButtonHints)
            {
                // Help text with dynamic hotkey display
                Vector2 helpPos = footerPos + new Vector2(0, LINE_HEIGHT * scale);

                // Get current hotkey displays for dynamic help text
                string toggleUIKey = ToggleUIHotkey?.GetAssignedKeys().FirstOrDefault() ?? "M";
                string detectMetalsKey =
                    MistbornMod.MetalDetectionHotkey?.GetAssignedKeys().FirstOrDefault() ?? "X";

                string helpText =
                    $"Press [{toggleUIKey}] to hide UI • [{detectMetalsKey}] to detect metals • Right-click to unlink";

                // Get text width for help text
                float helpTextWidth = GetTextWidth(helpText, scale * 0.8f);

                // Help text background - also nearly invisible
                Rectangle helpRect = new Rectangle(
                    (int)(helpPos.X - 10 * scale),
                    (int)(helpPos.Y - 5 * scale),
                    (int)(Math.Max(280, helpTextWidth + 20) * scale),
                    (int)(20 * scale)
                );

                spriteBatch.Draw(
                    Terraria.GameContent.TextureAssets.MagicPixel.Value,
                    helpRect,
                    new Color(0, 0, 0, 5)
                );

                Terraria.Utils.DrawBorderStringFourWay(
                    spriteBatch,
                    Terraria.GameContent.FontAssets.MouseText.Value,
                    helpText,
                    helpPos.X,
                    helpPos.Y,
                    new Color(200, 200, 200),
                    Color.Black,
                    Vector2.Zero,
                    scale * 0.8f
                );
            }
        }

        private int GetMetalCount(MistbornPlayer modPlayer)
        {
            int count = 0;

            if (modPlayer.IsMistborn)
            {
                foreach (var metal in Enum.GetValues(typeof(MetalType)).Cast<MetalType>())
                {
                    if (modPlayer.MetalReserves.TryGetValue(metal, out int reserve) && reserve > 0)
                    {
                        count++;
                    }
                }
            }
            else if (modPlayer.IsMisting && modPlayer.MistingMetal.HasValue)
            {
                count = 1;
            }

            return count;
        }
    }

    // Configuration class to store UI settings
    // Updated MetalUIConfig class inside DraggableMetalUI.cs

    public class MetalUIConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("$Mods.MistbornMod.Config.UISettings")]
        [DefaultValue(1.0f)]
        [Range(0.5f, 2.0f)]
        [Increment(0.1f)]
        public float UIScale { get; set; } = 1.0f;

        public Vector2 DefaultPosition { get; set; } = new Vector2(855, 0); // Position specified in mod config

        [DefaultValue(true)]
        public bool AllowDragging { get; set; } = true;

        [DefaultValue(true)]
        public bool ShowByDefault { get; set; } = true;

        // New option to hide button hints
        [DefaultValue(false)]
        [Label("Hide Button Hints")]
        [Tooltip("Hides the button hint text from the UI for a cleaner look")]
        public bool HideButtonHints { get; set; } = false;

        // Store which metals are unlinked (serialized as string keys for enum compatibility)
        [JsonProperty]
        public Dictionary<string, bool> UnlinkedMetals { get; set; } =
            new Dictionary<string, bool>();

        // Store positions of individual metals
        [JsonProperty]
        public Dictionary<string, Vector2> MetalPositions { get; set; } =
            new Dictionary<string, Vector2>();

        // Add properties for total section
        [JsonProperty]
        public bool TotalSectionUnlinked { get; set; } = false;

        [JsonProperty]
        public Vector2 TotalSectionPosition { get; set; } = Vector2.Zero;

        public override void OnChanged()
        {
            // Apply settings immediately when changed
            if (DraggableMetalUI.Instance != null)
            {
                DraggableMetalUI.Instance.ApplyConfig();
            }
        }
    }
}
