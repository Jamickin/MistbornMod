// Items/BronzeVial.cs
namespace MistbornMod.Items
{
    public class BronzeVial : MetalVial
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Metal = MetalType.Bronze;
        }
    }
}