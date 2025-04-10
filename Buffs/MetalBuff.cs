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
        
        public override void Update(Player player, ref int buffIndex)
        {
            // Get the MistbornPlayer instance to check burning status
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            
            bool isActiveBurning = false;
            
            // Check if this metal is actively burning based on its type
            if (Metal == MetalType.Steel)
                isActiveBurning = modPlayer.IsActivelySteelPushing && modPlayer.MetalReserves.TryGetValue(Metal, out int _);
            else if (Metal == MetalType.Iron)
                isActiveBurning = modPlayer.IsActivelyIronPulling && modPlayer.MetalReserves.TryGetValue(Metal, out int _);
            else if (Metal == MetalType.Chromium)
                isActiveBurning = modPlayer.IsActivelyChromiumStripping && modPlayer.MetalReserves.TryGetValue(Metal, out int _);
            else
                isActiveBurning = modPlayer.BurningMetals.TryGetValue(Metal, out bool burning) && burning;
            
            // Only apply effects if actively burning
            if (isActiveBurning)
            {
                // Call the derived class implementation for the specific metal effect
                ApplyBuffEffect(player, modPlayer.IsFlaring);
            }
            
            // Set the buff opacity based on burning status
            Main.buffAlpha[Type] = isActiveBurning ? 1f : 0.4f;
        }
        
        // Virtual method for derived classes to override with their specific effects
        public virtual void ApplyBuffEffect(Player player, bool isFlaring)
        {
            // Base implementation does nothing
            // Each metal buff will override this to implement its specific effects
        }
        
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
{
    Player player = Main.LocalPlayer; 
    MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>(); 
    
    // Get hotkey display string for this metal
    string hotkeyDisplay = modPlayer.GetHotkeyDisplayForMetal(this.Metal);
    
    // Add hotkey to buff name
    buffName = $"{buffName} {hotkeyDisplay}";
    
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
        
        // Check if this metal is actively burning
        bool isActiveBurning = false;
        if (Metal == MetalType.Steel)
            isActiveBurning = modPlayer.IsActivelySteelPushing;
        else if (Metal == MetalType.Iron)
            isActiveBurning = modPlayer.IsActivelyIronPulling;
        else if (Metal == MetalType.Chromium)
            isActiveBurning = modPlayer.IsActivelyChromiumStripping;
        else
            isActiveBurning = modPlayer.BurningMetals.TryGetValue(Metal, out bool burning) && burning;
        
        // Add burning status to the tooltip
        if (isActiveBurning)
        {
            tip += "\n[c/FF6600:BURNING]";
            
            // Add flaring status indicator if flaring
            if (modPlayer.IsFlaring && Metal != MetalType.Atium)
            {
                tip += " - [c/FFD700:FLARING]";
                // Change rarity to gold when flaring
                rare = 2; // Uncommon (blue) to Rare (orange)
            }
        }
        else
        {
            tip += "\n[c/888888:INACTIVE]";
        }
    }
    else
    {
        tip += "\nReserves: N/A";
    }
}
        // Virtual method that can be overridden by specific metal buffs
        // Called when a buff is removed
        public virtual void OnBuffEnd(Player player, MistbornPlayer modPlayer)
        {
            // Default implementation does nothing
        }
    }
}