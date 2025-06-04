// Items/CopperVial.cs
namespace MistbornMod.Content.Items.Consumables
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