using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;


namespace MistbornMod
{
    /// <summary>
    /// Manages metal reserves for Mistborn players
    /// </summary>
    public class MetalReserveManager
    {
        // Metal reserve constants
        public const int METAL_VIAL_AMOUNT = 3600; // 60 seconds (60 ticks per second) per vial
        public const int MAX_TOTAL_RESERVES = METAL_VIAL_AMOUNT * 6; // Maximum of 6 vials worth of metals in total
        
        // Dictionary to track metal reserves
        private Dictionary<MetalType, int> _metalReserves = new Dictionary<MetalType, int>();
        
        // Reference to the player
        private MistbornPlayer _modPlayer;
        private Player _player;
        
        public MetalReserveManager(MistbornPlayer modPlayer, Player player)
        {
            _modPlayer = modPlayer;
            _player = player;
            InitializeReserves();
        }
        
        /// <summary>
        /// Initialize reserve dictionary with all metal types
        /// </summary>
        public void InitializeReserves()
        {
            foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
            {
                _metalReserves.TryAdd(metal, 0);
            }
        }
        
        /// <summary>
        /// Get the current reserves for a specific metal
        /// </summary>
        /// <param name="metal">The metal type</param>
        /// <returns>Current reserve amount</returns>
        public int GetReserves(MetalType metal)
        {
            return _metalReserves.TryGetValue(metal, out int reserve) ? reserve : 0;
        }
        
        /// <summary>
        /// Set the reserves for a specific metal
        /// </summary>
        /// <param name="metal">The metal type</param>
        /// <param name="amount">The new amount</param>
        public void SetReserves(MetalType metal, int amount)
        {
            _metalReserves[metal] = Math.Max(0, amount);
        }
        
        /// <summary>
        /// Consume metal reserves based on flaring status
        /// </summary>
        /// <param name="metal">The metal type</param>
        /// <param name="isFlaring">Whether player is flaring</param>
        /// <returns>True if reserves remain, false if depleted</returns>
        public bool ConsumeReserves(MetalType metal, bool isFlaring)
        {
            // Determine consume rate - Atium isn't affected by flaring
            int consumeRate = (metal == MetalType.Atium) ? 1 : (isFlaring ? 2 : 1);
            
            if (_metalReserves.TryGetValue(metal, out int reserves) && reserves > 0)
            {
                _metalReserves[metal] -= consumeRate;
                
                if (_metalReserves[metal] <= 0)
                {
                    _metalReserves[metal] = 0;
                    return false; // Depleted
                }
                return true; // Reserves remain
            }
            return false; // No reserves
        }
        
        /// <summary>
        /// Adds reserves from drinking a metal vial
        /// </summary>
        /// <param name="metal">The metal type</param>
        /// <param name="durationValue">Amount to add</param>
        /// <returns>True if successful, false if at max</returns>
        public bool AddReserves(MetalType metal, int durationValue)
{
    // Check if player can metabolize this metal (either Mistborn or correct Misting type)
    bool canMetabolizeThisMetal = _modPlayer.IsMistborn || 
                                 (_modPlayer.IsMisting && _modPlayer.MistingMetal.HasValue && 
                                  _modPlayer.MistingMetal.Value == metal);
    
    if (!canMetabolizeThisMetal)
    {
        if (_modPlayer.IsMisting && _modPlayer.MistingMetal.HasValue)
        {
            // Player is a Misting but tried to drink the wrong metal
            Main.NewText($"As a {_modPlayer.GetMistingName(_modPlayer.MistingMetal.Value)}, you can only metabolize {_modPlayer.MistingMetal.Value}.", 255, 100, 100);
        }
        else
        {
            // Player has no Allomantic abilities
            Main.NewText("You don't have the ability to metabolize metals.", 255, 100, 100);
        }
        SoundEngine.PlaySound(SoundID.MenuTick, _player.position);
        
        // If this is their Misting metal, mark as discovered
        if (_modPlayer.IsMisting && _modPlayer.MistingMetal.HasValue && 
            _modPlayer.MistingMetal.Value == metal && !_modPlayer.HasDiscoveredMistingAbility)
        {
            _modPlayer.HasDiscoveredMistingAbility = true;
            string mistingName = _modPlayer.GetMistingName(metal);
            Main.NewText($"You have discovered your ability as a {mistingName}!", 255, 220, 100);
        }
        
        return false;
    }
    
    int currentReserve = GetReserves(metal);
    
    // Chromium is exempt from total reserve cap
    if (metal != MetalType.Chromium)
    {
        int currentTotalReserves = GetTotalReservesExcluding(MetalType.Chromium);
        
        // Calculate how much we can actually add based on total cap
        int maxAddToTotal = MAX_TOTAL_RESERVES - currentTotalReserves;
        int actualAmountToAdd = Math.Min(durationValue, maxAddToTotal);
        
        if (actualAmountToAdd <= 0)
        {
            // Can't add any more - metal reserves full
            Main.NewText("Your total metal reserves are full!", 255, 100, 100);
            SoundEngine.PlaySound(SoundID.MenuTick, _player.position);
            return false;
        }
        
        // Add the calculated amount to the reserves
        SetReserves(metal, currentReserve + actualAmountToAdd);
        
        // Sound effect
        SoundEngine.PlaySound(SoundID.Item3, _player.position);
        
        // Feedback to player
        if (actualAmountToAdd < durationValue)
        {
            Main.NewText("Your total metal reserves are nearly full.", 200, 200, 100);
        }
        else
        {
            // Full amount was added - calculate how many seconds of reserve this metal now has
            int totalSeconds = GetReserves(metal) / 60;
            Main.NewText($"{metal} reserves: {totalSeconds} seconds", 200, 255, 200);
        }
    }
    else
    {
        // Chromium has no cap
        SetReserves(metal, currentReserve + durationValue);
        SoundEngine.PlaySound(SoundID.Item3, _player.position);
        
        int totalSeconds = GetReserves(metal) / 60;
        Main.NewText($"{metal} reserves: {totalSeconds} seconds", 200, 255, 200);
    }
    
    // If this is their first time using their Misting metal, mark as discovered
    if (_modPlayer.IsMisting && _modPlayer.MistingMetal.HasValue && 
        _modPlayer.MistingMetal.Value == metal && !_modPlayer.HasDiscoveredMistingAbility)
    {
        _modPlayer.HasDiscoveredMistingAbility = true;
        string mistingName = _modPlayer.GetMistingName(metal);
        Main.NewText($"You have discovered your ability as a {mistingName}!", 255, 220, 100);
    }
    
    return true;
}
        
        /// <summary>
        /// Clear all metal reserves except for a specific type
        /// </summary>
        /// <param name="exceptMetal">Metal type to exclude from clearing</param>
        public void ClearAllReservesExcept(MetalType exceptMetal)
        {
            foreach (MetalType metal in Enum.GetValues(typeof(MetalType)))
            {
                if (metal != exceptMetal)
                {
                    SetReserves(metal, 0);
                }
            }
        }
        
        /// <summary>
        /// Gets the total of all metal reserves, excluding specific types
        /// </summary>
        /// <param name="excludeMetal">Optional metal to exclude</param>
        /// <returns>Total reserves</returns>
        public int GetTotalReservesExcluding(MetalType? excludeMetal = null)
        {
            int total = 0;
            foreach (var pair in _metalReserves)
            {
                if (!excludeMetal.HasValue || pair.Key != excludeMetal.Value)
                {
                    total += pair.Value;
                }
            }
            return total;
        }
        
        /// <summary>
        /// Gets the percentage of total reserves used (0.0 to 1.0)
        /// </summary>
        /// <returns>Percentage as float</returns>
        public float GetTotalReservesPercentage()
        {
            return (float)GetTotalReservesExcluding(MetalType.Chromium) / MAX_TOTAL_RESERVES;
        }
        
        /// <summary>
        /// Gets the percentage of a specific metal's reserves relative to one vial
        /// </summary>
        /// <param name="metal">The metal type</param>
        /// <returns>Percentage as float</returns>
        public float GetMetalReservesPercentage(MetalType metal)
        {
            int reserve = GetReserves(metal);
            return (float)reserve / METAL_VIAL_AMOUNT;
        }
        
        /// <summary>
        /// Saves metal reserves to tag compound for persistence
        /// </summary>
        /// <param name="tag">Tag to save to</param>
        public void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            List<string> metalNames = new List<string>();
            List<int> reserveValues = new List<int>();
            
            foreach (var kvp in _metalReserves)
            {
                metalNames.Add(kvp.Key.ToString());
                reserveValues.Add(kvp.Value);
            }
            
            tag["Mistborn_ReserveMetals"] = metalNames;
            tag["Mistborn_ReserveValues"] = reserveValues;
        }
        
        /// <summary>
        /// Loads metal reserves from tag compound
        /// </summary>
        /// <param name="tag">Tag to load from</param>
        /// <param name="mod">Mod reference for logging</param>
        public void LoadData(Terraria.ModLoader.IO.TagCompound tag, Mod mod)
{
    // Reset state before loading
    InitializeReserves();
    
    if (tag.ContainsKey("Mistborn_ReserveMetals") && tag.ContainsKey("Mistborn_ReserveValues"))
    {
        var metalNames = tag.Get<List<string>>("Mistborn_ReserveMetals");
        var reserveValues = tag.Get<List<int>>("Mistborn_ReserveValues");
        
        if (metalNames.Count == reserveValues.Count)
        {
            for (int i = 0; i < metalNames.Count; i++)
            {
                if (Enum.TryParse<MetalType>(metalNames[i], out MetalType metal))
                {
                    _metalReserves[metal] = reserveValues[i];
                }
                else
                {
                    mod.Logger.Warn($"Failed to parse saved MetalType reserve: {metalNames[i]}");
                }
            }
        }
        else
        {
            mod.Logger.Warn("Saved metal reserve data was corrupt (list count mismatch).");
        }
    }
}
    }
}