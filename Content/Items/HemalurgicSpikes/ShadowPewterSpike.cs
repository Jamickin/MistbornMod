// Content/Items/HemalurgicSpikes/ShadowPewterSpike.cs
namespace MistbornMod.Content.Items.HemalurgicSpikes
{
    public class ShadowPewterSpike : HemalurgicSpike
    {
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            TargetMetal = MetalType.Pewter;
            SpikeTier = SpikeType.Shadow;
            base.SetDefaults();
        }
    }
}