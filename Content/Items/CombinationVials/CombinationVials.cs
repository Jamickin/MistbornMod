// Content/Items/CombinationVials/CombinationVial.cs (Base class - no changes)
using MistbornMod.Common.Players;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MistbornMod.Content.Items.CombinationVials
{
    /// <summary>
    /// Base class for vials containing multiple metals
    /// </summary>
    public abstract class CombinationVial : ModItem
    {
        public Dictionary<MetalType, int> ContainedMetals { get; protected set; } = new Dictionary<MetalType, int>();
        public string CombinationName { get; protected set; } = "Unknown Mixture";
        
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useAnimation = 25; // Longer animation for complex mixtures
            Item.useTime = 25;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item3;
            Item.maxStack = 10; // Lower stack than single vials due to complexity
            Item.consumable = true;
            Item.rare = ItemRarityID.Orange; // More rare than basic vials
            Item.value = CalculateValue();
        }

        private int CalculateValue()
        {
            // Value based on number of metals and their amounts
            int baseValue = Item.sellPrice(silver: 20);
            int metalCount = ContainedMetals.Count;
            return baseValue * metalCount;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // Add information about contained metals
            var contentsLine = new TooltipLine(Mod, "Contents", 
                $"Contains: {string.Join(", ", ContainedMetals.Keys)}");
            contentsLine.OverrideColor = Microsoft.Xna.Framework.Color.LightBlue;
            tooltips.Add(contentsLine);
            
            // Add duration information
            foreach (var metal in ContainedMetals)
            {
                int seconds = metal.Value / 60;
                var durationLine = new TooltipLine(Mod, $"Duration_{metal.Key}", 
                    $"  {metal.Key}: {seconds} seconds");
                durationLine.OverrideColor = Microsoft.Xna.Framework.Color.Gray;
                tooltips.Add(durationLine);
            }
            
            // Add efficiency note
            var efficiencyLine = new TooltipLine(Mod, "Efficiency", 
                "[c/00FF00:More efficient than drinking separate vials]");
            tooltips.Add(efficiencyLine);
        }

        public override bool? UseItem(Player player)
        {
            var modPlayer = player.GetModPlayer<MistbornPlayer>();
            
            // Add each metal to the player's reserves
            foreach (var metalPair in ContainedMetals)
            {
                modPlayer.DrinkMetalVial(metalPair.Key, metalPair.Value);
            }
            
            // Special visual effect for combination vials
            CreateCombinationEffect(player);
            
            // Show summary message
            var metalNames = string.Join(" + ", ContainedMetals.Keys);
            Main.NewText($"Absorbed {CombinationName}: {metalNames}", 
                Microsoft.Xna.Framework.Color.LightBlue);
            
            return true;
        }

        private void CreateCombinationEffect(Player player)
        {
            // Create a more elaborate visual effect
            for (int i = 0; i < 25; i++)
            {
                var velocity = Main.rand.NextVector2CircularEdge(3f, 3f);
                var dust = Dust.NewDustPerfect(
                    player.Center,
                    DustID.RainbowMk2,
                    velocity,
                    150,
                    default,
                    Main.rand.NextFloat(1.0f, 1.5f)
                );
                dust.noGravity = true;
            }
        }
    }
}

// Content/Items/CombinationVials/PhysicalElixir.cs
namespace MistbornMod.Content.Items.CombinationVials
{
    /// <summary>
    /// Combination of Pewter and Tin for enhanced physical abilities
    /// Uses PewterVial texture since Pewter is the primary component
    /// </summary>
    public class PhysicalElixir : CombinationVial
    {
        // Reuse existing PewterVial texture
        public override string Texture => "MistbornMod/Content/Items/PewterVial";
        
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            CombinationName = "Physical Enhancement";
            ContainedMetals = new Dictionary<MetalType, int>
            {
                { MetalType.Pewter, 3600 }, // 1 minute of Pewter
                { MetalType.Tin, 3600 }     // 1 minute of Tin
            };
            base.SetDefaults();
        }
    }
}

// Content/Items/CombinationVials/EmotionalMastery.cs
namespace MistbornMod.Content.Items.CombinationVials
{
    /// <summary>
    /// Combination of Brass and Zinc for emotional manipulation
    /// Uses BrassVial texture since it represents the soothing/calming aspect
    /// </summary>
    public class EmotionalMastery : CombinationVial
    {
        // Reuse existing BrassVial texture
        public override string Texture => "MistbornMod/Content/Items/BrassVial";
        
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            CombinationName = "Emotional Control";
            ContainedMetals = new Dictionary<MetalType, int>
            {
                { MetalType.Brass, 3600 }, // 1 minute of Brass
                { MetalType.Zinc, 3600 }   // 1 minute of Zinc
            };
            base.SetDefaults();
        }
    }
}

// Content/Items/CombinationVials/MetallicMastery.cs
namespace MistbornMod.Content.Items.CombinationVials
{
    /// <summary>
    /// Combination of Iron and Steel for metallic manipulation
    /// Uses SteelVial texture since Steel pushing is more iconic
    /// </summary>
    public class MetallicMastery : CombinationVial
    {
        // Reuse existing SteelVial texture
        public override string Texture => "MistbornMod/Content/Items/SteelVial";
        
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            CombinationName = "Metallic Control";
            ContainedMetals = new Dictionary<MetalType, int>
            {
                { MetalType.Iron, 3600 },  // 1 minute of Iron
                { MetalType.Steel, 3600 }  // 1 minute of Steel
            };
            base.SetDefaults();
        }
    }
}

// Content/Items/CombinationVials/DetectionBundle.cs
namespace MistbornMod.Content.Items.CombinationVials
{
    /// <summary>
    /// Combination of Copper and Bronze for Allomantic detection and hiding
    /// Uses CopperVial texture since hiding is often more important than seeking
    /// </summary>
    public class DetectionBundle : CombinationVial
    {
        // Reuse existing CopperVial texture
        public override string Texture => "MistbornMod/Content/Items/CopperVial";
        
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            CombinationName = "Detection Mastery";
            ContainedMetals = new Dictionary<MetalType, int>
            {
                { MetalType.Copper, 3600 }, // 1 minute of Copper
                { MetalType.Bronze, 3600 }  // 1 minute of Bronze
            };
            base.SetDefaults();
        }
    }
}

// Content/Items/CombinationVials/AllomanticSupremacy.cs
namespace MistbornMod.Content.Items.CombinationVials
{
    /// <summary>
    /// The ultimate combination vial containing all basic metals
    /// Uses AtiumVial texture since this is the ultimate/godly mixture
    /// </summary>
    public class AllomanticSupremacy : CombinationVial
    {
        // Reuse existing AtiumVial texture for the ultimate combination
        public override string Texture => "MistbornMod/Content/Items/AtiumVial";
        
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            CombinationName = "Allomantic Supremacy";
            ContainedMetals = new Dictionary<MetalType, int>
            {
                { MetalType.Iron, 1800 },   // 30 seconds each
                { MetalType.Steel, 1800 },
                { MetalType.Tin, 1800 },
                { MetalType.Pewter, 1800 },
                { MetalType.Zinc, 1800 },
                { MetalType.Brass, 1800 },
                { MetalType.Copper, 1800 },
                { MetalType.Bronze, 1800 }
            };
            base.SetDefaults();
            Item.rare = ItemRarityID.Purple; // Extremely rare
            Item.maxStack = 1; // Very precious
        }
    }
}