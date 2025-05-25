namespace MistbornMod.Content.Items.HemalurgicSpikes
{
    public class BloodBrassSpike : HemalurgicSpike
    {
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            TargetMetal = MetalType.Brass;
            SpikeTier = SpikeType.Blood;
            base.SetDefaults();
        }
    }
}