using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using MistbornMod.Buffs;

namespace MistbornMod
{
    public class MistbornPlayer : ModPlayer
    {
        public Dictionary<MetalType, bool> BurningMetals { get; private set; } = new Dictionary<MetalType, bool>();
        public Dictionary<MetalType, int> MetalReserves { get; private set; } = new Dictionary<MetalType, int>();
        
        
        // Hold-type mechanic flags
        public bool IsActivelySteelPushing { get; private set; } = false;
        public bool IsActivelyIronPulling { get; private set; } = false;
        public bool IsActivelyChromiumStripping { get; set; } = false; // Added for Chromium
        public bool IsBurningAtium { get; set; } = false;
        public bool IsGeneratingCoppercloud { get; set; } = false;
        public float CoppercloudRadius { get; set; } = 0f;
        public bool IsBronzeScanning { get; set; } = false;

        // Mistborn/Misting status
        public bool IsMistborn { get; set; } = false;
        public bool IsMisting { get; set; } = false;
        public MetalType? MistingMetal { get; set; } = null; // The one metal a Misting can burn
        public bool HasDiscoveredMistingAbility { get; set; } = false;

        // Flaring mechanic
        public bool IsFlaring { get; private set; } = false;
        // Add this to MistbornPlayer class
public bool HasBeenInitialized { get; set; } = false;

// Then modify initialization method
private void InitializeRandomMisting()
{
    if (HasBeenInitialized) return;
    
    // All metal types except Atium and Chromium
    MetalType[] validMistingTypes = new MetalType[]
    {
        MetalType.Iron,
        MetalType.Steel,
        MetalType.Tin,
        MetalType.Pewter,
        MetalType.Zinc,
        MetalType.Brass,
        MetalType.Copper,
        MetalType.Bronze
    };
    
    // Select random metal
    MistingMetal = validMistingTypes[Main.rand.Next(validMistingTypes.Length)];
    IsMisting = true;
    HasDiscoveredMistingAbility = false;
    HasBeenInitialized = true;
    
    // Give a hint message without revealing the exact ability
    Main.NewText("You feel a strange connection to the mists...", 200, 230, 255);
}
        // Metal reserve constants
        public const int METAL_VIAL_AMOUNT = 3600; // 60 seconds (60 ticks per second) per vial
        // Removed the individual metal cap to allow stacking multiple vials of the same metal
        public const int MAX_TOTAL_RESERVES = METAL_VIAL_AMOUNT * 6; // Maximum of 6 vials worth of metals in total
        
        // Property to get current total reserves
        public int TotalReserves => MetalReserves.Values.Sum();
        
        // Visual feedback properties for flaring
        public int FlareEffectTimer { get; private set; } = 0;
        public float FlareIntensity { get; private set; } = 0f;

        public bool IsHiddenByCoppercloud 
        { 
            get 
            {
                // Check if self-cloaked
                if (IsGeneratingCoppercloud) return true;
                
                // Skip the rest in single player
                if (Main.netMode == NetmodeID.SinglePlayer) return false;
                
                // Check if within another player's coppercloud
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player otherPlayer = Main.player[i];
                    if (!otherPlayer.active || otherPlayer == Player) continue;
                    
                    MistbornPlayer otherModPlayer = otherPlayer.GetModPlayer<MistbornPlayer>();
                    if (otherModPlayer.IsGeneratingCoppercloud)
                    {
                        float distSq = Vector2.DistanceSquared(Player.Center, otherPlayer.Center);
                        if (distSq < otherModPlayer.CoppercloudRadius * otherModPlayer.CoppercloudRadius)
                        {
                            return true;
                        }
                    }
                }
                
                return false;
            }
        }
        
        // Initialize player as a random Misting on first spawn
        public override void OnEnterWorld()
{
            // If new character and not already a Misting or Mistborn
            if (!IsMistborn && !IsMisting && MistingMetal == null)
            {
                InitializeRandomMisting();
            }
}
        
        
        
        private void RevealMistingAbility()
        {
            HasDiscoveredMistingAbility = true;
            string mistingName = GetMistingName(MistingMetal.Value);
            
            // Visual effect
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDust(
                    Player.position,
                    Player.width,
                    Player.height,
                    DustID.MagicMirror,
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-1f, 1f),
                    150,
                    default,
                    1.2f
                );
            }
            
            // Reveal message
            Main.NewText($"You've discovered your Allomantic ability! You are a {mistingName}!", 255, 220, 100);
            Main.NewText($"You can burn {MistingMetal} metal. Press {GetHotkeyDisplayForMetal(MistingMetal.Value)} to activate it.", 200, 255, 200);
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item4, Player.position);
        }
        
        public string GetMistingName(MetalType metal)
        {
            switch (metal)
            {
                case MetalType.Iron: return "Lurcher";
                case MetalType.Steel: return "Coinshot";
                case MetalType.Tin: return "Tineye";
                case MetalType.Pewter: return "Thug";
                case MetalType.Zinc: return "Rioter";
                case MetalType.Brass: return "Soother";
                case MetalType.Copper: return "Smoker";
                case MetalType.Bronze: return "Seeker";
                default: return "Misting";
            }
        }

        public override void Initialize()
        {
            UpdateBuffVisibility();

            if (Enum.IsDefined(typeof(MetalType), 0))
            {
                foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
                {
                    BurningMetals.TryAdd(metal, false); 
                    MetalReserves.TryAdd(metal, 0);  
                    int buffId = GetBuffIDForMetal(metal);
                    if (buffId != -1) Player.AddBuff(buffId, 2);  
                }
            }
            
            // Reset state flags
            IsActivelySteelPushing = false;
            IsActivelyIronPulling = false;
            IsActivelyChromiumStripping = false;  // Initialize Chromium flag
            IsBurningAtium = false;
            IsFlaring = false;
            FlareEffectTimer = 0;
            FlareIntensity = 0f;
        }

        private void UpdateBuffVisibility()
        {
            foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
            {
                int buffId = GetBuffIDForMetal(metal);
                if (buffId != -1 && MetalReserves.TryGetValue(metal, out int reserves) && reserves > 0)
                {
                    Player.AddBuff(buffId, 5);
                }
            }
        }
        
        public override void ResetEffects()
        {
            // Reset the flag each frame. The AtiumBuff.Update will set it true if active.
            IsBurningAtium = false;
            
            // Note: We don't reset IsActivelySteelPushing, IsActivelyIronPulling, or IsFlaring here
            // as they are controlled in ProcessTriggers
        }
        
        public override void SaveData(TagCompound tag)
        {
            List<string> metalNames = new List<string>();
            List<int> reserveValues = new List<int>();
            
            foreach (var kvp in MetalReserves) {
                metalNames.Add(kvp.Key.ToString());
                reserveValues.Add(kvp.Value);
            }
            
            tag["Mistborn_ReserveMetals"] = metalNames;
            tag["Mistborn_ReserveValues"] = reserveValues;
            tag["Mistborn_IsFlaring"] = IsFlaring;
            tag["Mistborn_IsMistborn"] = IsMistborn; // Save Mistborn status
            tag["Mistborn_IsMisting"] = IsMisting;
            tag["Mistborn_MistingMetal"] = MistingMetal.HasValue ? (int)MistingMetal.Value : -1;
            tag["Mistborn_HasDiscoveredMistingAbility"] = HasDiscoveredMistingAbility;
        }

        public override void LoadData(TagCompound tag)
        {
            // Reset state before loading
            if (Enum.IsDefined(typeof(MetalType), 0)) {
                foreach (MetalType metal in Enum.GetValues(typeof(MetalType))) {
                    BurningMetals[metal] = false;
                    MetalReserves[metal] = 0;
                }
            }
            
            IsActivelySteelPushing = false;
            IsActivelyIronPulling = false;
            IsActivelyChromiumStripping = false;  // Reset Chromium flag
            IsFlaring = false;
            IsMistborn = false; // Reset Mistborn status
            IsMisting = false; // Reset Misting status
            MistingMetal = null;
            HasDiscoveredMistingAbility = false;

            if (tag.ContainsKey("Mistborn_ReserveMetals") && tag.ContainsKey("Mistborn_ReserveValues")) {
                var metalNames = tag.Get<List<string>>("Mistborn_ReserveMetals");
                var reserveValues = tag.Get<List<int>>("Mistborn_ReserveValues");
                
                if (metalNames.Count == reserveValues.Count) {
                    for (int i = 0; i < metalNames.Count; i++) {
                        if (Enum.TryParse<MetalType>(metalNames[i], out MetalType metal)) {
                            MetalReserves[metal] = reserveValues[i];
                        } else { 
                            Mod.Logger.Warn($"Failed to parse saved MetalType reserve: {metalNames[i]}"); 
                        }
                    }
                } else { 
                    Mod.Logger.Warn("Saved metal reserve data was corrupt (list count mismatch).");
                }
            }
            
            if (tag.ContainsKey("Mistborn_IsFlaring")) {
                IsFlaring = tag.GetBool("Mistborn_IsFlaring");
            }
            
            if (tag.ContainsKey("Mistborn_IsMistborn")) {
                IsMistborn = tag.GetBool("Mistborn_IsMistborn");
            }
            
            if (tag.ContainsKey("Mistborn_IsMisting")) {
                IsMisting = tag.GetBool("Mistborn_IsMisting");
            }
            
            if (tag.ContainsKey("Mistborn_MistingMetal")) {
                int metalTypeValue = tag.GetInt("Mistborn_MistingMetal");
                MistingMetal = metalTypeValue >= 0 ? (MetalType?)metalTypeValue : null;
            }
            
            if (tag.ContainsKey("Mistborn_HasDiscoveredMistingAbility")) {
                HasDiscoveredMistingAbility = tag.GetBool("Mistborn_HasDiscoveredMistingAbility");
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // Only allow the appropriate hotkey for Mistings
            if (IsMisting && !IsMistborn && MistingMetal.HasValue)
            {
                // Only enable the hotkey for their specific metal
                switch (MistingMetal.Value)
                {
                    case MetalType.Iron:
                        IsActivelyIronPulling = MistbornMod.IronToggleHotkey?.Current ?? false;
                        break;
                    case MetalType.Steel:
                        IsActivelySteelPushing = MistbornMod.SteelToggleHotkey?.Current ?? false;
                        break;
                    case MetalType.Pewter:
                        if (MistbornMod.PewterToggleHotkey?.JustPressed ?? false) ToggleMetal(MetalType.Pewter);
                        break;
                    case MetalType.Tin:
                        if (MistbornMod.TinToggleHotkey?.JustPressed ?? false) ToggleMetal(MetalType.Tin);
                        break;
                    case MetalType.Brass:
                        if (MistbornMod.BrassToggleHotkey?.JustPressed ?? false) ToggleMetal(MetalType.Brass);
                        break;
                    case MetalType.Zinc:
                        if (MistbornMod.ZincToggleHotkey?.JustPressed ?? false) ToggleMetal(MetalType.Zinc);
                        break;
                    case MetalType.Bronze:
                        if (MistbornMod.BronzeToggleHotkey?.JustPressed ?? false) ToggleMetal(MetalType.Bronze);
                        break;
                    case MetalType.Copper:
                        if (MistbornMod.CopperToggleHotkey?.JustPressed ?? false) ToggleMetal(MetalType.Copper);
                        break;
                }
                
                // Handle flaring only for the metal they can burn
                if (MistbornMod.FlareToggleHotkey?.JustPressed ?? false)
                {
                    bool canFlare = false;
                    
                    if ((MistingMetal == MetalType.Steel && IsActivelySteelPushing) ||
                        (MistingMetal == MetalType.Iron && IsActivelyIronPulling) ||
                        (BurningMetals.TryGetValue(MistingMetal.Value, out bool burning) && burning))
                    {
                        canFlare = true;
                    }
                    
                    if (canFlare)
                    {
                        ToggleFlaring();
                    }
                    else
                    {
                        Main.NewText("You must be burning your metal to flare it!", 255, 100, 100);
                        SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                    }
                }
            }
            else
            {
                // Full Mistborn has access to all metals
                if (MistbornMod.PewterToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Pewter); }
                if (MistbornMod.TinToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Tin); }
                if (MistbornMod.BrassToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Brass); }
                if (MistbornMod.ZincToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Zinc); }
                if (MistbornMod.AtiumToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Atium); }
                if (MistbornMod.CopperToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Copper); }
                if (MistbornMod.BronzeToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Bronze); }
                
                // Chromium is now a hold mechanic like Iron/Steel, not a toggle
                if (MistbornMod.ChromiumToggleHotkey?.JustPressed ?? false) { 
                    int buffId = GetBuffIDForMetal(MetalType.Chromium);
                    if (buffId != -1 && MetalReserves.GetValueOrDefault(MetalType.Chromium, 0) > 0) {
                        SoundEngine.PlaySound(SoundID.MaxMana, Player.position);
                    }
                }

                // Handle Flare Toggle
                if (MistbornMod.FlareToggleHotkey?.JustPressed ?? false) {
                    ToggleFlaring();
                }

                // Handle Held Metal mechanics
                IsActivelySteelPushing = MistbornMod.SteelToggleHotkey?.Current ?? false;
                IsActivelyIronPulling = MistbornMod.IronToggleHotkey?.Current ?? false;
                IsActivelyChromiumStripping = MistbornMod.ChromiumToggleHotkey?.Current ?? false;  // Handle Chromium as a held key
            }
        }

        // Toggle flaring state
        private void ToggleFlaring()
        {
            // Check if any metals are burning before toggling
            bool anyMetalBurning = BurningMetals.Any(m => m.Value) || 
                                  IsActivelySteelPushing || 
                                  IsActivelyIronPulling ||
                                  IsActivelyChromiumStripping;  // Add Chromium to check
            
            if (!anyMetalBurning) {
                // No point flaring if no metals are burning - play fail sound
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                // Optionally show a message to the player
                Main.NewText("No metals are burning to flare!", 255, 100, 100);
                return;
            }
            
            IsFlaring = !IsFlaring;
            
            if (IsFlaring) {
                // Turn ON flaring
                SoundEngine.PlaySound(SoundID.Item74, Player.position); // More intense sound for flaring
                
                // Visual effect for flaring on
                FlareEffectTimer = 30; // Half a second of effect
                FlareIntensity = 1.0f;
                
                // Spawn dust around the player
                for (int i = 0; i < 20; i++) {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    Dust.NewDustPerfect(Player.Center, DustID.Torch, dustVel, 100, default, 1.2f);
                }
                
                // Message to player
                Main.NewText("Burning metals with intensity!", 255, 200, 0);
            } else {
                // Turn OFF flaring
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                
                // Message to player
                Main.NewText("Burning metals normally.", 200, 200, 200);
            }
        }

        // Toggles metals like Pewter, Tin, Brass, Zinc, Atium (NOT Steel, Iron, or Chromium)
        private void ToggleMetal(MetalType metal)
        {
            // Check if player has the ability to burn this metal
            if (!IsMistborn && (!IsMisting || MistingMetal != metal))
            {
                // If they're a Misting but try to burn the wrong metal
                if (IsMisting && MistingMetal.HasValue)
                {
                    Main.NewText($"As a {GetMistingName(MistingMetal.Value)}, you can only burn {MistingMetal}.", 255, 150, 100);
                }
                else
                {
                    Main.NewText("You don't have the ability to burn metals.", 255, 100, 100);
                }
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                return;
            }
               
            // Skip Steel, Iron, and Chromium as they use the hold mechanic
            if (metal == MetalType.Steel || metal == MetalType.Iron || metal == MetalType.Chromium) return;

            int buffId = GetBuffIDForMetal(metal);
            if (buffId == -1) return;

            // Can only toggle on if reserves exist
            if (!BurningMetals.GetValueOrDefault(metal, false) && MetalReserves.GetValueOrDefault(metal, 0) <= 0)
            {
                 // Tried to turn on without reserves, play fail sound
                 SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                 return;
            }

            BurningMetals[metal] = !BurningMetals[metal]; // Flip the toggle state

            if (BurningMetals[metal]) { // Turned ON
                SoundEngine.PlaySound(SoundID.MaxMana, Player.position);
                Player.AddBuff(buffId, 5); // Add buff briefly, PostUpdate will maintain
            } else { // Turned OFF
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                
                // Call OnBuffEnd before removing the buff
                if (Player.HasBuff(buffId))
                {
                    // Get the ModBuff instance
                    ModBuff modBuff = ModContent.GetModBuff(buffId);
                    if (modBuff is MetalBuff metalBuff)
                    {
                        metalBuff.OnBuffEnd(Player, this);
                    }
                }
                
                Player.ClearBuff(buffId);
            }
        }
        

        public override void PostUpdate()
        {

             foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
    {
        int buffId = GetBuffIDForMetal(metal);
        if (buffId != -1 && MetalReserves.GetValueOrDefault(metal, 0) > 0)
        {
            // Add buff with very short duration - will be refreshed each tick if reserves exist
            Player.AddBuff(buffId, 2);
        }
    }
            UpdateBuffVisibility();

            if (!Enum.IsDefined(typeof(MetalType), 0)) return;

            // Handle flare visual effects
            if (FlareEffectTimer > 0) {
                FlareEffectTimer--;
                
                // Create visual effects around player
                if (Main.rand.NextBool(3)) {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(2f, 2f);
                    Dust.NewDustPerfect(Player.Center, DustID.Torch, dustVel, 150, default, 0.8f);
                }
            }
            
            // Regular effect for flaring when active
            if (IsFlaring && Main.rand.NextBool(10)) {
                Vector2 dustVel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f);
                Dust.NewDustPerfect(Player.Center, DustID.Torch, dustVel, 150, default, 0.6f);
            }

            // Base consume rate - doubled when flaring
            int consumeRate = IsFlaring ? 2 : 1; 

            // --- Handle Toggled Metals (NOT Steel, Iron, or Chromium) ---
            var metalsToCheck = BurningMetals.Keys.ToList();
            foreach (MetalType metal in metalsToCheck) {
                if (metal == MetalType.Steel || metal == MetalType.Iron || metal == MetalType.Chromium) continue; // Skip held metals
                
                // Don't double consume rate for Atium
                int metalConsumeRate = (metal == MetalType.Atium) ? 1 : consumeRate;

                if (BurningMetals.TryGetValue(metal, out bool isBurning) && isBurning) {
                    if (MetalReserves.TryGetValue(metal, out int reserves) && reserves > 0) {
                        MetalReserves[metal] -= metalConsumeRate;
                        int buffId = GetBuffIDForMetal(metal);

                        if (MetalReserves[metal] <= 0) { // Ran out this tick
                            MetalReserves[metal] = 0;
                            BurningMetals[metal] = false; // Auto-toggle off
                            
                            if (buffId != -1) {
                                // Get the ModBuff instance
                                ModBuff modBuff = ModContent.GetModBuff(buffId);
                                if (modBuff is MetalBuff metalBuff)
                                {
                                    metalBuff.OnBuffEnd(Player, this);
                                }
                                Player.ClearBuff(buffId);
                            }
                            
                            SoundEngine.PlaySound(SoundID.MenuTick, Player.position); // Ran out sound
                            
                            // If we ran out and were flaring, maybe turn off flaring too
                            if (IsFlaring && !AnyMetalBurning()) {
                                IsFlaring = false;
                                Main.NewText("No metals left to flare!", 255, 150, 0);
                            }
                        } else {
                             // Keep buff active
                             if (buffId != -1) Player.AddBuff(buffId, 5);
                        }
                    } else { // Should be burning but no reserves? Turn off.
                        BurningMetals[metal] = false;
                        int buffId = GetBuffIDForMetal(metal);
                        if (buffId != -1 && Player.HasBuff(buffId)) {
                            // Get the ModBuff instance before removing
                            ModBuff modBuff = ModContent.GetModBuff(buffId);
                            if (modBuff is MetalBuff metalBuff)
                            {
                                metalBuff.OnBuffEnd(Player, this);
                            }
                            Player.ClearBuff(buffId);
                        }
                    }
                }
                else { // Ensure buff is removed if not burning
                    int buffId = GetBuffIDForMetal(metal);
                    if (buffId != -1 && Player.HasBuff(buffId)) {
                        // Get the ModBuff instance before removing
                        ModBuff modBuff = ModContent.GetModBuff(buffId);
                        if (modBuff is MetalBuff metalBuff)
                        {
                            metalBuff.OnBuffEnd(Player, this);
                        }
                        Player.ClearBuff(buffId);
                    }
                }
            }

            // --- Handle Steel (held) ---
            HandleHeldMetal(MetalType.Steel, IsActivelySteelPushing);
            
            // --- Handle Iron (held) ---
            HandleHeldMetal(MetalType.Iron, IsActivelyIronPulling);
            
            // --- Handle Chromium (held) ---
            HandleHeldMetal(MetalType.Chromium, IsActivelyChromiumStripping);
            
            // If no metals are burning, make sure flaring is turned off
            if (IsFlaring && !AnyMetalBurning()) {
                IsFlaring = false;
            }
        }
        
        // Helper method to check if any metal is burning
        private bool AnyMetalBurning()
        {
            return BurningMetals.Any(m => m.Value) || 
                  IsActivelySteelPushing || 
                  IsActivelyIronPulling ||
                  IsActivelyChromiumStripping;  // Add Chromium to check
        }
        
        // Helper method to handle held metal mechanics
        private void HandleHeldMetal(MetalType metal, bool isActivelyUsing)
        {
            int buffId = GetBuffIDForMetal(metal);
            int reserves = MetalReserves.GetValueOrDefault(metal, 0);
            
            // Determine consume rate - Atium isn't affected by flaring
            int metalConsumeRate = (metal == MetalType.Atium) ? 1 : (IsFlaring ? 2 : 1);
            
            if (isActivelyUsing && reserves > 0)
            {
                // Consume metal reserves while actively using
                MetalReserves[metal] -= metalConsumeRate;

                if (MetalReserves[metal] <= 0) // Ran out this tick
                {
                    MetalReserves[metal] = 0;
                    if (buffId != -1) {
                        // Call OnBuffEnd before removing the buff
                        ModBuff modBuff = ModContent.GetModBuff(buffId);
                        if (modBuff is MetalBuff metalBuff)
                        {
                            metalBuff.OnBuffEnd(Player, this);
                        }
                        Player.ClearBuff(buffId);
                    }
                    SoundEngine.PlaySound(SoundID.MenuTick, Player.position); // Ran out sound
                    
                    // If we ran out and were flaring, check if we need to turn off flaring
                    if (IsFlaring && !AnyMetalBurning()) {
                        IsFlaring = false;
                        Main.NewText("No metals left to flare!", 255, 150, 0);
                    }
                }
                else
                {
                    if (buffId != -1) Player.AddBuff(buffId, 5);
                }
            }
            else
            {
                // Remove buff if not actively using
                if (buffId != -1 && Player.HasBuff(buffId))
                {
                    // Call OnBuffEnd before removing the buff
                    ModBuff modBuff = ModContent.GetModBuff(buffId);
                    if (modBuff is MetalBuff metalBuff)
                    {
                        metalBuff.OnBuffEnd(Player, this);
                    }
                    Player.ClearBuff(buffId);
                }
            }
        }

        // Make this method public so it can be called from ChromiumBuff
        public int GetBuffIDForMetal(MetalType metal)
        {
             try {
                 switch (metal) {
                     case MetalType.Iron: return ModContent.BuffType<Buffs.IronBuff>();
                     case MetalType.Steel: return ModContent.BuffType<Buffs.SteelBuff>();
                     case MetalType.Pewter: return ModContent.BuffType<Buffs.PewterBuff>();
                     case MetalType.Tin: return ModContent.BuffType<Buffs.TinBuff>();
                     case MetalType.Brass: return ModContent.BuffType<Buffs.BrassBuff>();
                     case MetalType.Zinc: return ModContent.BuffType<Buffs.ZincBuff>();
                     case MetalType.Atium: return ModContent.BuffType<Buffs.AtiumBuff>();
case MetalType.Chromium: return ModContent.BuffType<Buffs.ChromiumBuff>();
                     case MetalType.Copper: return ModContent.BuffType<Buffs.CopperBuff>();
                     case MetalType.Bronze: return ModContent.BuffType<Buffs.BronzeBuff>();
                     default: return -1;
                 }
            } catch (Exception e) { Mod.Logger.Error($"Error getting Buff ID for MetalType.{metal}.", e); return -1; }
        }

        // Adds reserves from a vial, capped at the maximum total reserves
        public void DrinkMetalVial(MetalType metal, int durationValue)
        {
            // Players without powers can't drink metals effectively
            if (!IsMistborn && !IsMisting)
            {
                Main.NewText("You don't have the ability to metabolize metals.", 255, 100, 100);
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                return;
            }
            
            // Misting can only drink their specific metal
            if (IsMisting && !IsMistborn && metal != MistingMetal)
            {
                // If they haven't discovered their ability yet, give them a hint
                if (!HasDiscoveredMistingAbility)
                {
                    Main.NewText("This metal feels wrong in your body. Try another...", 255, 200, 100);
                }
                else
                {
                    Main.NewText($"As a {GetMistingName(MistingMetal.Value)}, you can only burn {MistingMetal}.", 255, 150, 100);
                }
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                return;
            }
            
            // If they are a Misting and this is their first time drinking their metal
            if (IsMisting && !IsMistborn && !HasDiscoveredMistingAbility && metal == MistingMetal)
            {
                RevealMistingAbility();
            }
            
            // Calculate reserves
            int currentReserve = MetalReserves.GetValueOrDefault(metal, 0);
            int currentTotalReserves = 0;
            foreach (var pair in MetalReserves)
            {
                if (pair.Key != MetalType.Chromium)
                {
                    currentTotalReserves += pair.Value;
                }
            }            
            // Calculate how much we can actually add based on total cap
            // No longer checking against MAX_METAL_RESERVE for individual metals
            int maxAddToTotal = MAX_TOTAL_RESERVES - currentTotalReserves; // Cap for all metals combined
            int actualAmountToAdd = metal == MetalType.Chromium ? 
                durationValue : 
                Math.Min(durationValue, maxAddToTotal);            
            if (actualAmountToAdd <= 0) {
                // Can't add any more - metal reserves full
                Main.NewText("Your total metal reserves are full!", 255, 100, 100);
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                return;
            }
            
            // Add the calculated amount to the reserves
            MetalReserves[metal] = currentReserve + actualAmountToAdd;
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item3, Player.position);
            
            // Feedback to player
            if (actualAmountToAdd < durationValue) {
                Main.NewText("Your total metal reserves are nearly full.", 200, 200, 100);
            }
            else {
                // Full amount was added - calculate how many seconds of reserve this metal now has
                int totalSeconds = MetalReserves[metal] / 60;
                Main.NewText($"{metal} reserves: {totalSeconds} seconds", 200, 255, 200);
            }

            // Optional: Maybe flash the buff briefly on drink?
            int buffId = GetBuffIDForMetal(metal);
            if (buffId != -1) Player.AddBuff(buffId, 2);
        }
        
        // Gets the percentage of total reserves used (0.0 to 1.0)
        public float GetTotalReservesPercentage()
        {
            int totalWithoutChromium = 0;
            foreach (var pair in MetalReserves)
            {
                if (pair.Key != MetalType.Chromium)
                {
                    totalWithoutChromium += pair.Value;
                }
            }
            return (float)totalWithoutChromium / MAX_TOTAL_RESERVES;
        }
        
        // Gets the percentage of a specific metal's reserves relative to maximum total reserves
        // This method is updated to show a metal's reserves as a percentage of one vial
        public float GetMetalReservesPercentage(MetalType metal)
        {
            int reserve = MetalReserves.GetValueOrDefault(metal, 0);
            
            // Calculate what percentage of one vial this represents
            // If reserve > METAL_VIAL_AMOUNT, percentage will be > 1.0
            return (float)reserve / METAL_VIAL_AMOUNT;
        }
        
        public string GetHotkeyDisplayForMetal(MetalType metal)
        {
            string keybind = "";
            switch (metal)
            {
                case MetalType.Iron: keybind = MistbornMod.IronToggleHotkey?.GetAssignedKeys()[0] ?? "F"; break;
                case MetalType.Steel: keybind = MistbornMod.SteelToggleHotkey?.GetAssignedKeys()[0] ?? "J"; break;
                case MetalType.Pewter: keybind = MistbornMod.PewterToggleHotkey?.GetAssignedKeys()[0] ?? "G"; break;
                case MetalType.Tin: keybind = MistbornMod.TinToggleHotkey?.GetAssignedKeys()[0] ?? "H"; break;
                case MetalType.Brass: keybind = MistbornMod.BrassToggleHotkey?.GetAssignedKeys()[0] ?? "B"; break;
                case MetalType.Zinc: keybind = MistbornMod.ZincToggleHotkey?.GetAssignedKeys()[0] ?? "Z"; break;
                case MetalType.Atium: keybind = MistbornMod.AtiumToggleHotkey?.GetAssignedKeys()[0] ?? "V"; break;
                case MetalType.Chromium: keybind = MistbornMod.ChromiumToggleHotkey?.GetAssignedKeys()[0] ?? "K"; break;
                case MetalType.Copper: keybind = MistbornMod.CopperToggleHotkey?.GetAssignedKeys()[0] ?? "C"; break;
                case MetalType.Bronze: keybind = MistbornMod.BronzeToggleHotkey?.GetAssignedKeys()[0] ?? "N"; break;
                default: keybind = ""; break;
            }
            return $"[{keybind}]";
        }
    }
}