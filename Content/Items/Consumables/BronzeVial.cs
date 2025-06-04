// Items/BronzeVial.cs
namespace MistbornMod.Content.Items.Consumables
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