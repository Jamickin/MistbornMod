// Items/CopperVial.cs
namespace MistbornMod.Items
{
    public class CopperVial : MetalVial
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Metal = MetalType.Copper;
        }
    }
}