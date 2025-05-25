namespace MistbornMod.Content.Items.HemalurgicSpikes
{
    public class ShadowTinSpike : HemalurgicSpike
    {
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            TargetMetal = MetalType.Tin;
            SpikeTier = SpikeType.Shadow;
            base.SetDefaults();
        }
    }
}