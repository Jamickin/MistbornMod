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

namespace MistbornMod
{
    public class MistbornPlayer : ModPlayer
    {
        public Dictionary<MetalType, bool> BurningMetals { get; private set; } = new Dictionary<MetalType, bool>();
        public Dictionary<MetalType, int> MetalReserves { get; private set; } = new Dictionary<MetalType, int>();
        
        // Hold-type mechanic flags
        public bool IsActivelySteelPushing { get; private set; } = false;
        public bool IsActivelyIronPulling { get; private set; } = false;
        public bool IsBurningAtium { get; set; } = false;
        
        // Flaring mechanic
        public bool IsFlaring { get; private set; } = false;

        // Metal reserve constants
        public const int METAL_VIAL_AMOUNT = 3600; // 60 seconds (60 ticks per second) per vial
        public const int MAX_METAL_RESERVE = 3600; // Max per metal type stays the same
        public const int MAX_TOTAL_RESERVES = METAL_VIAL_AMOUNT * 6; // Maximum of 6 vials worth of metals in total
        
        // Property to get current total reserves
        public int TotalReserves => MetalReserves.Values.Sum();
        
        // Visual feedback properties for flaring
        public int FlareEffectTimer { get; private set; } = 0;
        public float FlareIntensity { get; private set; } = 0f;
        
        // UI visibility flag
        public bool ShowMetalUI { get; private set; } = true;

        public override void Initialize()
        {
            if (Enum.IsDefined(typeof(MetalType), 0))
            {
                foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
                {
                    BurningMetals.TryAdd(metal, false); 
                    MetalReserves.TryAdd(metal, 0);    
                }
            }
            
            // Reset state flags
            IsActivelySteelPushing = false;
            IsActivelyIronPulling = false;
            IsBurningAtium = false;
            IsFlaring = false;
            FlareEffectTimer = 0;
            FlareIntensity = 0f;
            ShowMetalUI = true;
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
            tag["Mistborn_ShowMetalUI"] = ShowMetalUI;
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
            IsFlaring = false;
            ShowMetalUI = true;

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
            
            if (tag.ContainsKey("Mistborn_ShowMetalUI")) {
                ShowMetalUI = tag.GetBool("Mistborn_ShowMetalUI");
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // Toggle metals like Pewter, Tin, Brass, Zinc, Atium
            if (MistbornMod.IronToggleHotkey?.JustPressed ?? false) { 
                // Iron is now a hold mechanic, not a toggle
                int buffId = GetBuffIDForMetal(MetalType.Iron);
                if (buffId != -1 && MetalReserves.GetValueOrDefault(MetalType.Iron, 0) > 0) {
                    SoundEngine.PlaySound(SoundID.MaxMana, Player.position);
                }
            }
            if (MistbornMod.PewterToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Pewter); }
            if (MistbornMod.TinToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Tin); }
            if (MistbornMod.BrassToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Brass); }
            if (MistbornMod.ZincToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Zinc); }
            if (MistbornMod.AtiumToggleHotkey?.JustPressed ?? false) { ToggleMetal(MetalType.Atium); }

            // Toggle UI visibility
            if (MistbornMod.UIToggleHotkey?.JustPressed ?? false) {
                ShowMetalUI = !ShowMetalUI;
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
            }

            // Handle Flare Toggle
            if (MistbornMod.FlareToggleHotkey?.JustPressed ?? false) {
                ToggleFlaring();
            }

            // Handle Held Metal mechanics
            IsActivelySteelPushing = MistbornMod.SteelToggleHotkey?.Current ?? false;
            IsActivelyIronPulling = MistbornMod.IronToggleHotkey?.Current ?? false;
        }

        // Toggle flaring state
        private void ToggleFlaring()
        {
            // Check if any metals are burning before toggling
            bool anyMetalBurning = BurningMetals.Any(m => m.Value) || 
                                  IsActivelySteelPushing || 
                                  IsActivelyIronPulling;
            
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

        // Toggles metals like Pewter, Tin, Brass, Zinc, Atium (NOT Steel or Iron)
        private void ToggleMetal(MetalType metal)
        {
            // Skip Steel and Iron as they use the hold mechanic
            if (metal == MetalType.Steel || metal == MetalType.Iron) return;

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
                Player.ClearBuff(buffId);
            }
        }

        public override void PostUpdate()
        {
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

            // --- Handle Toggled Metals (NOT Steel or Iron) ---
            var metalsToCheck = BurningMetals.Keys.ToList();
            foreach (MetalType metal in metalsToCheck) {
                if (metal == MetalType.Steel || metal == MetalType.Iron) continue; // Skip held metals
                
                // Don't double consume rate for Atium
                int metalConsumeRate = (metal == MetalType.Atium) ? 1 : consumeRate;

                if (BurningMetals.TryGetValue(metal, out bool isBurning) && isBurning) {
                    if (MetalReserves.TryGetValue(metal, out int reserves) && reserves > 0) {
                        MetalReserves[metal] -= metalConsumeRate;
                        int buffId = GetBuffIDForMetal(metal);

                        if (MetalReserves[metal] <= 0) { // Ran out this tick
                            MetalReserves[metal] = 0;
                            BurningMetals[metal] = false; // Auto-toggle off
                            if (buffId != -1) Player.ClearBuff(buffId);
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
                        if (buffId != -1 && Player.HasBuff(buffId)) Player.ClearBuff(buffId);
                    }
                }
                else { // Ensure buff is removed if not burning
                    int buffId = GetBuffIDForMetal(metal);
                    if (buffId != -1 && Player.HasBuff(buffId)) Player.ClearBuff(buffId);
                }
            }

            // --- Handle Steel (held) ---
            HandleHeldMetal(MetalType.Steel, IsActivelySteelPushing);
            
            // --- Handle Iron (held) ---
            HandleHeldMetal(MetalType.Iron, IsActivelyIronPulling);
            
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
                  IsActivelyIronPulling;
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
                    if (buffId != -1) Player.ClearBuff(buffId);
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
                    Player.ClearBuff(buffId);
                }
            }
        }

        private int GetBuffIDForMetal(MetalType metal)
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
                     default: return -1;
                 }
            } catch (Exception e) { Mod.Logger.Error($"Error getting Buff ID for MetalType.{metal}.", e); return -1; }
        }

        // Adds reserves from a vial, capped at the maximum total and individual reserves
        public void DrinkMetalVial(MetalType metal, int durationValue)
        {
            int currentReserve = MetalReserves.GetValueOrDefault(metal, 0);
            int currentTotalReserves = TotalReserves;
            
            // Calculate how much we can actually add based on both caps
            int maxAddToThisMetal = MAX_METAL_RESERVE - currentReserve; // Cap for individual metal
            int maxAddToTotal = MAX_TOTAL_RESERVES - currentTotalReserves; // Cap for all metals combined
            int actualAmountToAdd = Math.Min(durationValue, Math.Min(maxAddToThisMetal, maxAddToTotal));
            
            if (actualAmountToAdd <= 0) {
                // Can't add any more - metal reserves full
                Main.NewText("Your metal reserves are full!", 255, 100, 100);
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                return;
            }
            
            // Add the calculated amount to the reserves
            MetalReserves[metal] = currentReserve + actualAmountToAdd;
            
            // Sound effect
            SoundEngine.PlaySound(SoundID.Item3, Player.position);
            
            // Feedback to player
            if (actualAmountToAdd < durationValue) {
                if (maxAddToThisMetal < maxAddToTotal) {
                    Main.NewText($"Your {metal} reserves are getting full.", 200, 200, 100);
                } else {
                    Main.NewText("Your total metal reserves are nearly full.", 200, 200, 100);
                }
            }

            // Optional: Maybe flash the buff briefly on drink?
            int buffId = GetBuffIDForMetal(metal);
            if(buffId != -1) Player.AddBuff(buffId, 2);
        }
        
        // Gets the percentage of total reserves used (0.0 to 1.0)
        public float GetTotalReservesPercentage()
        {
            return (float)TotalReserves / MAX_TOTAL_RESERVES;
        }
        
        // Gets the percentage of a specific metal's reserves (0.0 to 1.0)
        public float GetMetalReservesPercentage(MetalType metal)
        {
            int reserve = MetalReserves.GetValueOrDefault(metal, 0);
            return (float)reserve / MAX_METAL_RESERVE;
        }
    }
}