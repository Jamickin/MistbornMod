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
            if (modPlayer != null && modPlayer.MetalReserves.TryGetValue(this.Metal, out int reserves))
            {
                double secondsLeft = reserves / 60.0;
                string timeLeftFormatted = secondsLeft.ToString("F1"); 
                tip += $"\nReserves: {timeLeftFormatted}s"; 
            }
            else
            {
                 tip += "\nReserves: N/A";
            }
        }
    }
} 

