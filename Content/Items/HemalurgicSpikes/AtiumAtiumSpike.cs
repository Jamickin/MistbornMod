namespace MistbornMod.Content.Items.HemalurgicSpikes
{
    public class AtiumAtiumSpike : HemalurgicSpike
    {
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            TargetMetal = MetalType.Atium;
            SpikeTier = SpikeType.Atium;
            base.SetDefaults();
        }
    }
}