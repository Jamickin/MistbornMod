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
using MistbornMod.Utils;

namespace MistbornMod
{
    public class MistbornPlayer : ModPlayer
    {
        // Metal burning state
        public Dictionary<MetalType, bool> BurningMetals { get; private set;         // Toggle flaring state
        private void ToggleFlaring()
        {
            // Check if any metals are burning before toggling
            bool anyMetalBurning = BurningMetals.Any(m => m.Value) || 
                                  IsActivelySteelPushing || 
                                  IsActivelyIronPulling ||
                                  IsActivelyChromiumStripping;
            
            if (!anyMetalBurning) {
                // No point flaring if no metals are burning - play fail sound
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                // Show a message to the player
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
            if (!IsMistborn)
            {
                Main.NewText("You don't have the ability to burn metals.", 255, 100, 100);
                SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                return;
            }
               
            // Skip Steel, Iron, and Chromium as they use the hold mechanic
            if (metal == MetalType.Steel || metal == MetalType.Iron || metal == MetalType.Chromium) return;

            int buffId = GetBuffIDForMetal(metal);
            if (buffId == -1) return;

            // Can only toggle on if reserves exist
            if (!BurningMetals.GetValueOrDefault(metal, false) && _reserveManager.GetReserves(metal) <= 0)
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
            // Always make metal reserves visible in UI if they exist
            foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
            {
                int buffId = GetBuffIDForMetal(metal);
                if (buffId != -1 && _reserveManager.GetReserves(metal) > 0)
                {
                    // Add buff with very short duration - will be refreshed each tick if reserves exist
                    Player.AddBuff(buffId, 2);
                }
            }
            
            UpdateBuffVisibility();

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

            // --- Handle Toggled Metals (NOT Steel, Iron, or Chromium) ---
            var metalsToCheck = BurningMetals.Keys.ToList();
            foreach (MetalType metal in metalsToCheck) {
                if (metal == MetalType.Steel || metal == MetalType.Iron || metal == MetalType.Chromium) continue; // Skip held metals
                
                if (BurningMetals.TryGetValue(metal, out bool isBurning) && isBurning) {
                    // Try to consume metal reserves
                    bool hasReservesLeft = _reserveManager.ConsumeReserves(metal, IsFlaring);
                    int buffId = GetBuffIDForMetal(metal);

                    if (!hasReservesLeft) { // Ran out this tick
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
                } else if (Player.HasBuff(GetBuffIDForMetal(metal))) { // Should not be burning but has buff
                    int buffId = GetBuffIDForMetal(metal);
                    if (buffId != -1) {
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
                  IsActivelyChromiumStripping;
        }
        
        // Helper method to handle held metal mechanics
        private void HandleHeldMetal(MetalType metal, bool isActivelyUsing)
        {
            int buffId = GetBuffIDForMetal(metal);
            
            if (isActivelyUsing && _reserveManager.GetReserves(metal) > 0)
            {
                // Consume metal reserves while actively using
                bool hasReservesLeft = _reserveManager.ConsumeReserves(metal, IsFlaring);

                if (!hasReservesLeft) // Ran out this tick
                {
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

        // Gets buff ID for a specific metal
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
            _reserveManager.AddReserves(metal, durationValue);
            
            // Optional: Maybe flash the buff briefly on drink?
            int buffId = GetBuffIDForMetal(metal);
            if (buffId != -1) Player.AddBuff(buffId, 2);
        }
        
        // Gets the percentage of total reserves used (0.0 to 1.0)
        public float GetTotalReservesPercentage()
        {
            return _reserveManager.GetTotalReservesPercentage();
        }
        
        // Gets the percentage of a specific metal's reserves relative to maximum total reserves
        // This method is updated to show a metal's reserves as a percentage of one vial
        public float GetMetalReservesPercentage(MetalType metal)
        {
            return _reserveManager.GetMetalReservesPercentage(metal);
        }
        
        // Get hotkey display text for a metal
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
} = new Dictionary<MetalType, bool>();
        
        // Metal reserves manager
        private MetalReserveManager _reserveManager;
        
        // Property for accessing metal reserves (for backwards compatibility)
        public Dictionary<MetalType, int> MetalReserves => _reserveManager != null ? 
            Enum.GetValues(typeof(MetalType))
                .Cast<MetalType>()
                .ToDictionary(m => m, m => _reserveManager.GetReserves(m)) 
            : new Dictionary<MetalType, int>();
        
        // Hold-type mechanic flags
        public bool IsActivelySteelPushing { get; private set; } = false;
        public bool IsActivelyIronPulling { get; private set; } = false;
        public bool IsActivelyChromiumStripping { get; set; } = false;
        public bool IsBurningAtium { get; set; } = false;
        public bool IsGeneratingCoppercloud { get; set; } = false;
        public float CoppercloudRadius { get; set; } = 0f;
        public bool IsBronzeScanning { get; set; } = false;

        // Player status
        public bool IsMistborn { get; set; } = false;

        // Flaring mechanic
        public bool IsFlaring { get; private set; } = false;
        
        // Visual feedback properties for flaring
        public int FlareEffectTimer { get; private set; } = 0;
        public float FlareIntensity { get; private set; } = 0f;

        // Property to check if player is hidden by coppercloud
        public bool IsHiddenByCoppercloud 
        { 
            get 
            {
                // Check if self-cloaked
                if (IsGeneratingCoppercloud) return true;
                
                // Skip the rest in single player
                if (Main.netMode == NetmodeID.SinglePlayer) return false;
                
                return IsWithinAnyPlayerCoppercloud();
            }
        }
        
        /// <summary>
        /// Checks if this player is within any other player's coppercloud
        /// </summary>
        private bool IsWithinAnyPlayerCoppercloud()
        {
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

        public override void Initialize()
        {
            // Initialize the metal reserve manager
            _reserveManager = new MetalReserveManager(this, Player);
            
            // Initialize burning state
            foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
            {
                BurningMetals.TryAdd(metal, false);
            }
            
            // Reset state flags
            IsActivelySteelPushing = false;
            IsActivelyIronPulling = false;
            IsActivelyChromiumStripping = false;
            IsBurningAtium = false;
            IsFlaring = false;
            FlareEffectTimer = 0;
            FlareIntensity = 0f;
            
            UpdateBuffVisibility();
        }

        private void UpdateBuffVisibility()
        {
            foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
            {
                int buffId = GetBuffIDForMetal(metal);
                if (buffId != -1 && _reserveManager.GetReserves(metal) > 0)
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
            // Save metal reserves
            _reserveManager.SaveData(tag);
            
            // Save player state
            tag["Mistborn_IsFlaring"] = IsFlaring;
            tag["Mistborn_IsMistborn"] = IsMistborn;
        }

        public override void LoadData(TagCompound tag)
        {
            // Reset state before loading
            Initialize();
            
            // Load metal reserves
            _reserveManager.LoadData(tag, Mod);
            
            // Load player state
            if (tag.ContainsKey("Mistborn_IsFlaring"))
            {
                IsFlaring = tag.GetBool("Mistborn_IsFlaring");
            }
            
            if (tag.ContainsKey("Mistborn_IsMistborn"))
            {
                IsMistborn = tag.GetBool("Mistborn_IsMistborn");
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // Process toggle hotkeys
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
                if (buffId != -1 && _reserveManager.GetReserves(MetalType.Chromium) > 0) {
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
            IsActivelyChromiumStripping = MistbornMod.ChromiumToggleHotkey?.Current ?? false;
        }