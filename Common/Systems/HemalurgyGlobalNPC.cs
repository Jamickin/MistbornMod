using Terraria;
using Terraria.ModLoader;
using MistbornMod.Common.Players;

namespace MistbornMod.Common.Systems
{
    /// <summary>
    /// Global NPC class to handle Hemalurgic spike kill tracking
    /// </summary>
    public class HemalurgyGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            // Only process if this NPC was killed by a player
            if (npc.lastInteraction == 255) return; // No player interaction
            
            Player killerPlayer = Main.player[npc.lastInteraction];
            if (killerPlayer == null || !killerPlayer.active) return;
            
            MistbornPlayer modPlayer = killerPlayer.GetModPlayer<MistbornPlayer>();
            
            // Check if the player has a Hemalurgic spike equipped
            if (modPlayer.EquippedSpike != null)
            {
                // Let the spike handle the kill
                modPlayer.EquippedSpike.OnKillNPC(npc);
            }
        }
        
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            // Track the last player to hit this NPC with a projectile
            if (projectile.owner >= 0 && projectile.owner < Main.maxPlayers)
            {
                npc.lastInteraction = projectile.owner;
            }
        }
        
        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            // Track the last player to hit this NPC with an item
            if (player.whoAmI >= 0 && player.whoAmI < Main.maxPlayers)
            {
                npc.lastInteraction = player.whoAmI;
            }
        }
    }
}