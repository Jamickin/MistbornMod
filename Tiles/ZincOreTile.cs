using Microsoft.Xna.Framework; 
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MistbornMod.Tiles 
{
    public class ZincOreTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            TileID.Sets.Ore[Type] = true; 
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true; 
            Main.tileBlockLight[Type] = true; 
            Main.tileShine[Type] = 900; 
            Main.tileShine2[Type] = true; 
            Main.tileSpelunker[Type] = true; 
            Main.tileOreFinderPriority[Type] = 310;

            AddMapEntry(new Color(160, 170, 180)); 

            DustType = DustID.Silver; 
            HitSound = SoundID.Tink; 
            MinPick = 35; 
        }

    }
}
