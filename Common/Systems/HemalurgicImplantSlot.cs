using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using MistbornMod.Content.Items.HemalurgicSpikes;

namespace MistbornMod.Common.Systems
{
    /// <summary>
    /// Specialized accessory slot for Hemalurgic Spikes
    /// This prevents spikes from taking up normal accessory slots while maintaining balance
    /// </summary>
    public class HemalurgicImplantSlot : ModAccessorySlot
    {
        // Textures for the slot (you'll need to create these 22x22 pixel images)
        public override string FunctionalTexture => "MistbornMod/Assets/UI/HemalurgicSlot";
        public override string VanityTexture => "MistbornMod/Assets/UI/HemalurgicSlotVanity";
        
        // Position the slot near other accessory slots
        public override bool DrawDyeSlot => false; // Spikes don't have dye variants
        public override bool DrawVanitySlot => true; // Allow vanity spikes for appearance

        public override bool CanAcceptItem(Item item, AccessorySlotType context)
        {
            // Only accept Hemalurgic Spikes in this slot
            return item.ModItem is HemalurgicSpike;
        }

        public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo)
        {
            // When shift-clicking a spike, try to put it in this slot first
            return item.ModItem is HemalurgicSpike;
        }

        // Called when an item is equipped in this slot
        public override void OnEquip(Item item, Player player)
        {
            if (item.ModItem is HemalurgicSpike spike)
            {
                var modPlayer = player.GetModPlayer<MistbornPlayer>();
                
                // Visual effect when equipping a spike
                for (int i = 0; i < 15; i++)
                {
                    Dust.NewDust(
                        player.position,
                        player.width,
                        player.height,
                        DustID.Blood,
                        Main.rand.NextFloat(-2f, 2f),
                        Main.rand.NextFloat(-2f, 2f),
                        100,
                        default,
                        1.0f
                    );
                }
                
                // Notification about the dangerous nature of Hemalurgy
                if (!spike.PowerUnlocked)
                {
                    Main.NewText($"The {spike.TargetMetal} spike pierces your soul...", 255, 100, 100);
                    Main.NewText($"Kill {spike.RequiredKills - spike.CurrentKills} more enemies to unlock its power.", 255, 200, 100);
                }
                else
                {
                    Main.NewText($"The power of {spike.TargetMetal} flows through you!", 100, 255, 100);
                }
            }
        }

        // Called when an item is unequipped from this slot
        public override void OnUnequip(Item item, Player player)
        {
            if (item.ModItem is HemalurgicSpike spike)
            {
                var modPlayer = player.GetModPlayer<MistbornPlayer>();
                
                // Remove the equipped spike reference
                modPlayer.EquippedSpike = null;
                
                // If this spike granted a power, temporarily remove it
                if (spike.PowerUnlocked && modPlayer.HemalurgicPowers.Contains(spike.TargetMetal))
                {
                    // The power is lost when the spike is removed
                    // (This maintains the dangerous nature of Hemalurgy - spikes can be stolen!)
                    Main.NewText($"The {spike.TargetMetal} power fades as the spike is removed...", 255, 150, 100);
                }
            }
        }

        // Tooltip shown when hovering over the slot
        public override bool IsEnabled(Player player)
        {
            // Always available - Hemalurgy is dangerous and available to anyone desperate enough
            return true;
        }

        public override void DrawSlotText(ItemSlot.Context context, Item item, Vector2 position, Color lightColor)
        {
            // Custom text or effects could be drawn here
            // For now, let the base implementation handle it
            base.DrawSlotText(context, item, position, lightColor);
        }
    }
}