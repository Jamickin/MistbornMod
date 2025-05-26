namespace MistbornMod.Content.Items.HemalurgicSpikes
{
    public class BloodZincSpike : HemalurgicSpike
    {
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            TargetMetal = MetalType.Zinc;
            SpikeTier = SpikeType.Blood;
            base.SetDefaults();
        }
    }
}