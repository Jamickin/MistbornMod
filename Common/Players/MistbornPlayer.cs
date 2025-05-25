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
using MistbornMod.Content.Buffs;
using MistbornMod.Common.Systems;
using MistbornMod.Content.Items.HemalurgicSpikes;

namespace MistbornMod.Common.Players
{
    public class MistbornPlayer : ModPlayer
    {
        // Metal burning state
        public Dictionary<MetalType, bool> BurningMetals { get; private set; } = new Dictionary<MetalType, bool>();
        
        // Metal reserves manager
        public MetalReserveManager _reserveManager;
        
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
        public bool IsDetectingMetals { get; private set; } = false;

        // Player status
        public bool IsMistborn { get; set; } = false;
        public bool IsMisting { get; set; } = false; // New property to track Misting status
        public MetalType? MistingMetal { get; set; } = null; // The one metal a Misting can burn
        public bool HasDiscoveredMistingAbility { get; set; } = false; // Whether they've discovered their ability

        // NEW: Hemalurgic system
        public HashSet<MetalType> HemalurgicPowers { get; set; } = new HashSet<MetalType>();
        public HemalurgicSpike EquippedSpike { get; set; } = null;

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

        /// <summary>
        /// Check if player can burn a specific metal (Mistborn, Misting, or Hemalurgic power)
        /// </summary>
        public bool CanBurnMetal(MetalType metal)
        {
            // Mistborn can burn everything
            if (IsMistborn) return true;
            
            // Misting can burn their specific metal
            if (IsMisting && MistingMetal.HasValue && MistingMetal.Value == metal) return true;
            
            // Check if they have this power through Hemalurgy
            if (HemalurgicPowers.Contains(metal)) return true;
            
            return false;
        }

        public override void Initialize()
        {
            // Initialize the metal reserve manager
            _reserveManager = new MetalReserveManager(this, Player);
            
            // Initialize burning state
            BurningMetals.Clear();
            foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
            {
                BurningMetals.TryAdd(metal, false);
            }
            
            // Initialize Hemalurgic powers
            HemalurgicPowers.Clear();
            
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

        // Get Misting name based on metal type
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
                default: return "Unknown Misting";
            }
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
            tag["Mistborn_IsMisting"] = IsMisting;
            
            // Save misting metal as a string to avoid potential issues with enum serialization
            if (MistingMetal.HasValue)
                tag["Mistborn_MistingMetal"] = MistingMetal.Value.ToString();
            
            tag["Mistborn_HasDiscoveredMistingAbility"] = HasDiscoveredMistingAbility;
            
            // NEW: Save Hemalurgic powers
            List<string> hemalurgicPowerNames = HemalurgicPowers.Select(m => m.ToString()).ToList();
            tag["Mistborn_HemalurgicPowers"] = hemalurgicPowerNames;
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
            
            if (tag.ContainsKey("Mistborn_IsMisting"))
            {
                IsMisting = tag.GetBool("Mistborn_IsMisting");
            }
            
            // Load misting metal from string
            if (tag.ContainsKey("Mistborn_MistingMetal"))
            {
                string metalName = tag.GetString("Mistborn_MistingMetal");
                if (Enum.TryParse(metalName, out MetalType metal))
                {
                    MistingMetal = metal;
                }
                else
                {
                    // Fallback to numeric format for backward compatibility
                    if (tag.ContainsKey("Mistborn_MistingMetal") && tag["Mistborn_MistingMetal"] is int metalValue)
                    {
                        MistingMetal = (MetalType)metalValue;
                    }
                }
            }
            
            if (tag.ContainsKey("Mistborn_HasDiscoveredMistingAbility"))
            {
                HasDiscoveredMistingAbility = tag.GetBool("Mistborn_HasDiscoveredMistingAbility");
            }
            
            // NEW: Load Hemalurgic powers
            if (tag.ContainsKey("Mistborn_HemalurgicPowers"))
            {
                var powerNames = tag.Get<List<string>>("Mistborn_HemalurgicPowers");
                HemalurgicPowers.Clear();
                foreach (string powerName in powerNames)
                {
                    if (Enum.TryParse<MetalType>(powerName, out MetalType power))
                    {
                        HemalurgicPowers.Add(power);
                    }
                }
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            // First check if player can burn any metals (Mistborn, Misting, or has Hemalurgic powers)
            bool isAllomancer = IsMistborn || IsMisting || HemalurgicPowers.Count > 0;
            
            // Only allow metal burning if player has Allomantic abilities
            if (!isAllomancer)
            {
                // If player has no abilities, just skip all the processing
                return;
            }
            
            // Process toggle hotkeys, but check if player has permission for each
            if (MistbornMod.PewterToggleHotkey?.JustPressed ?? false) 
            { 
                if (CanBurnMetal(MetalType.Pewter))
                    ToggleMetal(MetalType.Pewter); 
                else
                    ShowCannotBurnMessage(MetalType.Pewter);
            }
            
            if (MistbornMod.TinToggleHotkey?.JustPressed ?? false) 
            { 
                if (CanBurnMetal(MetalType.Tin))
                    ToggleMetal(MetalType.Tin); 
                else
                    ShowCannotBurnMessage(MetalType.Tin);
            }
            
            if (MistbornMod.BrassToggleHotkey?.JustPressed ?? false) 
            { 
                if (CanBurnMetal(MetalType.Brass))
                    ToggleMetal(MetalType.Brass); 
                else
                    ShowCannotBurnMessage(MetalType.Brass);
            }
            
            if (MistbornMod.ZincToggleHotkey?.JustPressed ?? false) 
            { 
                if (CanBurnMetal(MetalType.Zinc))
                    ToggleMetal(MetalType.Zinc); 
                else
                    ShowCannotBurnMessage(MetalType.Zinc);
            }
            
            if (MistbornMod.AtiumToggleHotkey?.JustPressed ?? false) 
            { 
                if (IsMistborn) // Only Mistborn can burn Atium (Hemalurgy can't steal this)
                    ToggleMetal(MetalType.Atium); 
                else
                    ShowCannotBurnMessage(MetalType.Atium);
            }
            
            if (MistbornMod.CopperToggleHotkey?.JustPressed ?? false) 
            { 
                if (CanBurnMetal(MetalType.Copper))
                    ToggleMetal(MetalType.Copper); 
                else
                    ShowCannotBurnMessage(MetalType.Copper);
            }
            
            if (MistbornMod.BronzeToggleHotkey?.JustPressed ?? false) 
            { 
                if (CanBurnMetal(MetalType.Bronze))
                    ToggleMetal(MetalType.Bronze); 
                else
                    ShowCannotBurnMessage(MetalType.Bronze);
            }

            // For held metals (Steel, Iron, Chromium), check permissions before setting the active flags
            if (MistbornMod.ChromiumToggleHotkey?.JustPressed ?? false) 
            { 
                if (CanBurnMetal(MetalType.Chromium))
                {
                    int buffId = GetBuffIDForMetal(MetalType.Chromium);
                    if (buffId != -1 && _reserveManager.GetReserves(MetalType.Chromium) > 0) {
                        SoundEngine.PlaySound(SoundID.MaxMana, Player.position);
                    }
                }
                else
                    ShowCannotBurnMessage(MetalType.Chromium);
            }

            IsDetectingMetals = MistbornMod.MetalDetectionHotkey?.Current ?? false;
            
            // Only allow detection if player has reserves of either Iron or Steel
            if (IsDetectingMetals)
            {
                bool hasIronReserves = _reserveManager.GetReserves(MetalType.Iron) > 0;
                bool hasSteelReserves = _reserveManager.GetReserves(MetalType.Steel) > 0;
                
                // Player needs to have either Iron or Steel reserves to use this ability
                if (!hasIronReserves && !hasSteelReserves)
                {
                    IsDetectingMetals = false;
                    
                    // Show a message to the player if they try to use it without reserves
                    if (MistbornMod.MetalDetectionHotkey?.JustPressed ?? false)
                    {
                        Main.NewText("You need Iron or Steel reserves to detect metals!", 255, 100, 100);
                        SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
                    }
                }
            }

            // Handle Flare Toggle
            if (MistbornMod.FlareToggleHotkey?.JustPressed ?? false) {
                ToggleFlaring();
            }

            // Handle Held Metal mechanics (only set active if player has permission)
            IsActivelySteelPushing = (MistbornMod.SteelToggleHotkey?.Current ?? false) && 
                                    CanBurnMetal(MetalType.Steel);
                                
            IsActivelyIronPulling = (MistbornMod.IronToggleHotkey?.Current ?? false) && 
                                   CanBurnMetal(MetalType.Iron);
                                   
            IsActivelyChromiumStripping = (MistbornMod.ChromiumToggleHotkey?.Current ?? false) && 
                                         CanBurnMetal(MetalType.Chromium);
        }

        // Helper method to show consistent error message
        private void ShowCannotBurnMessage(MetalType metal)
        {
            Main.NewText($"You don't have the ability to burn {metal}.", 255, 100, 100);
            SoundEngine.PlaySound(SoundID.MenuTick, Player.position);
        }
        
        // Toggle flaring state
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
            // Check if player has permission to burn this metal
            bool canBurnMetal = CanBurnMetal(metal);
                               
            if (!canBurnMetal)
            {
                Main.NewText("You don't have the ability to burn " + metal + ".", 255, 100, 100);
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
                
                // If this is first time using ability as Misting, mark as discovered
                if (IsMisting && MistingMetal.HasValue && MistingMetal.Value == metal && !HasDiscoveredMistingAbility)
                {
                    HasDiscoveredMistingAbility = true;
                    string mistingName = GetMistingName(metal);
                    Main.NewText($"You have discovered your ability as a {mistingName}!", 255, 220, 100);
                }
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
                     case MetalType.Iron: return ModContent.BuffType<IronBuff>();
                     case MetalType.Steel: return ModContent.BuffType<SteelBuff>();
                     case MetalType.Pewter: return ModContent.BuffType<PewterBuff>();
                     case MetalType.Tin: return ModContent.BuffType<TinBuff>();
                     case MetalType.Brass: return ModContent.BuffType<BrassBuff>();
                     case MetalType.Zinc: return ModContent.BuffType<ZincBuff>();
                     case MetalType.Atium: return ModContent.BuffType<AtiumBuff>();
                     case MetalType.Chromium: return ModContent.BuffType<ChromiumBuff>();
                     case MetalType.Copper: return ModContent.BuffType<CopperBuff>();
                     case MetalType.Bronze: return ModContent.BuffType<BronzeBuff>();
                     default: return -1;
                 }
            } catch (Exception e) { Mod.Logger.Error($"Error getting Buff ID for MetalType.{metal}.", e); return -1; }
        }

        // NEW: Handle NPC kills for spike progression
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Check if the NPC died and if we have a spike equipped
            if (target.life <= 0 && EquippedSpike != null)
            {
                EquippedSpike.OnKillNPC(target);
            }
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Check if the NPC died and if we have a spike equipped
            if (target.life <= 0 && EquippedSpike != null)
            {
                EquippedSpike.OnKillNPC(target);
            }
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
                case MetalType.Iron: keybind = MistbornMod.IronToggleHotkey?.GetAssignedKeys().FirstOrDefault() ?? "F"; break;
                case MetalType.Steel: keybind = MistbornMod.SteelToggleHotkey?.GetAssignedKeys().FirstOrDefault() ?? "J"; break;
                case MetalType.Pewter: keybind = MistbornMod.PewterToggleHotkey?.GetAssignedKeys().FirstOrDefault() ?? "G"; break;
                case MetalType.Tin: keybind = MistbornMod.TinToggleHotkey?.GetAssignedKeys().FirstOrDefault() ?? "H"; break;
                case MetalType.Brass: keybind = MistbornMod.BrassToggleHotkey?.GetAssignedKeys().FirstOrDefault() ?? "B"; break;
                case MetalType.Zinc: keybind = MistbornMod.ZincToggleHotkey?.GetAssignedKeys().FirstOrDefault() ?? "Z"; break;
                case MetalType.Atium: keybind = MistbornMod.AtiumToggleHotkey?.GetAssignedKeys().FirstOrDefault() ?? "V"; break;
                case MetalType.Chromium: keybind = MistbornMod.ChromiumToggleHotkey?.GetAssignedKeys().FirstOrDefault() ?? "K"; break;
                case MetalType.Copper: keybind = MistbornMod.CopperToggleHotkey?.GetAssignedKeys().FirstOrDefault() ?? "C"; break;
                case MetalType.Bronze: keybind = MistbornMod.BronzeToggleHotkey?.GetAssignedKeys().FirstOrDefault() ?? "N"; break;
                default: keybind = ""; break;
            }
            return $"[{keybind}]";
        }
    }
}