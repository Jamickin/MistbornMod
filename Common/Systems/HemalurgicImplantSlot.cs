using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.UI;
using Microsoft.Xna.Framework;
using MistbornMod.Content.Items.HemalurgicSpikes;
using MistbornMod.Common.Players;

namespace MistbornMod.Common.Systems
{
    /// <summary>
    /// Specialized accessory slot for Hemalurgic Spikes
    /// This prevents spikes from taking up normal accessory slots while maintaining balance
    /// </summary>
    public class HemalurgicImplantSlot : ModAccessorySlot
    {
        // Textures for the slot (22x22 pixel images)
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

        // Check if this slot should be available to the player
        public override bool IsEnabled()
        {
            // Always available - Hemalurgy is dangerous and available to anyone desperate enough
            return true;
        }
    }
}