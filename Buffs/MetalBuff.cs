using Terraria;
using Terraria.ModLoader;
namespace MistbornMod.Buffs
{
    public abstract class MetalBuff : ModBuff
    {
        public MetalType Metal { get; protected set; }

        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true; 
            Main.debuff[Type] = false;
        }
        
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            Player player = Main.LocalPlayer; 
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>(); 
            
            // Show metal reserves
            if (modPlayer != null && modPlayer.MetalReserves.TryGetValue(this.Metal, out int reserves))
            {
                // Format time based on how many full vials worth we have
                double secondsLeft = reserves / 60.0;
                int fullVials = (int)(secondsLeft / 60);
                int partialSeconds = (int)(secondsLeft % 60);
                
                // Format differently based on how many vials worth we have
                string timeLeftFormatted;
                if (fullVials > 0)
                {
                    timeLeftFormatted = $"{fullVials} vial{(fullVials > 1 ? "s" : "")} {partialSeconds}s";
                }
                else
                {
                    timeLeftFormatted = secondsLeft.ToString("F1") + "s"; 
                }
                
                // Base tip with metal reserves
                tip += $"\nReserves: {timeLeftFormatted}"; 
                
                // Add flaring status indicator if flaring
                if (modPlayer.IsFlaring && Metal != MetalType.Atium)
                {
                    tip += "\n[c/FFD700:FLARING] - Double power, double consumption";
                    // Change rarity to gold when flaring
                    rare = 2; // Uncommon (blue) to Rare (orange)
                }
            }
            else
            {
                 tip += "\nReserves: N/A";
            }

            // Set buff opacity based on active status
bool isActive = false;
if (this.Metal == MetalType.Iron)
    isActive = modPlayer.IsActivelyIronPulling;
else if (this.Metal == MetalType.Steel)
    isActive = modPlayer.IsActivelySteelPushing;
else if (this.Metal == MetalType.Chromium)
    isActive = modPlayer.IsActivelyChromiumStripping;
else
    isActive = modPlayer.BurningMetals.TryGetValue(this.Metal, out bool burning) && burning;

Main.buffAlpha[Type] = isActive ? 1f : 0.4f;

        }
        
        
        // Virtual method that can be overridden by specific metal buffs
        // Called when a buff is removed
        public virtual void OnBuffEnd(Player player, MistbornPlayer modPlayer)
        {
            // Default implementation does nothing
        }
    }
}