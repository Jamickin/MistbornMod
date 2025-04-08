using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Audio;
using Terraria.ModLoader.IO;
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

        public const int MAX_METAL_RESERVE = 3600; // 60 seconds (60 ticks per second)

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
            
            // Reset hold state flags
            IsActivelySteelPushing = false;
            IsActivelyIronPulling = false;
            IsBurningAtium = false;
        }
        
        public override void ResetEffects()
        {
            // Reset the flag each frame. The AtiumBuff.Update will set it true if active.
            IsBurningAtium = false;
            
            // Note: We don't reset IsActivelySteelPushing or IsActivelyIronPulling here
            // as they are controlled by current key state in ProcessTriggers
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

            // Handle Held Metal mechanics
            IsActivelySteelPushing = MistbornMod.SteelToggleHotkey?.Current ?? false;
            IsActivelyIronPulling = MistbornMod.IronToggleHotkey?.Current ?? false;
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

            int consumeRate = 1; // Ticks consumed per update when active

            // --- Handle Toggled Metals (NOT Steel or Iron) ---
            var metalsToCheck = BurningMetals.Keys.ToList();
            foreach (MetalType metal in metalsToCheck) {
                if (metal == MetalType.Steel || metal == MetalType.Iron) continue; // Skip held metals

                if (BurningMetals.TryGetValue(metal, out bool isBurning) && isBurning) {
                    if (MetalReserves.TryGetValue(metal, out int reserves) && reserves > 0) {
                        MetalReserves[metal] -= consumeRate;
                        int buffId = GetBuffIDForMetal(metal);

                        if (MetalReserves[metal] <= 0) { // Ran out this tick
                            MetalReserves[metal] = 0;
                            BurningMetals[metal] = false; // Auto-toggle off
                            if (buffId != -1) Player.ClearBuff(buffId);
                            SoundEngine.PlaySound(SoundID.MenuTick, Player.position); // Ran out sound
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
        }
        
        // Helper method to handle held metal mechanics
        private void HandleHeldMetal(MetalType metal, bool isActivelyUsing)
        {
            int buffId = GetBuffIDForMetal(metal);
            int reserves = MetalReserves.GetValueOrDefault(metal, 0);
            
            if (isActivelyUsing && reserves > 0)
            {
                // Consume metal reserves while actively using
                MetalReserves[metal] -= 1;

                if (MetalReserves[metal] <= 0) // Ran out this tick
                {
                    MetalReserves[metal] = 0;
                    if (buffId != -1) Player.ClearBuff(buffId);
                    SoundEngine.PlaySound(SoundID.MenuTick, Player.position); // Ran out sound
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

        // Adds reserves from a vial, capped at the maximum
        public void DrinkMetalVial(MetalType metal, int durationValue)
        {
            int currentReserve = MetalReserves.GetValueOrDefault(metal, 0);
            // Add the vial's value to the current reserve, capped at max
            int newReserve = Math.Min(currentReserve + durationValue, MAX_METAL_RESERVE);

            MetalReserves[metal] = newReserve;
            SoundEngine.PlaySound(SoundID.Item3, Player.position);

            // Optional: Maybe flash the buff briefly on drink?
            int buffId = GetBuffIDForMetal(metal);
            if(buffId != -1) Player.AddBuff(buffId, 2);
        }
    }
}