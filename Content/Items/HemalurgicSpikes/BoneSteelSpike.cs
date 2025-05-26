namespace MistbornMod.Content.Items.HemalurgicSpikes
{
    public class BoneSteelSpike : HemalurgicSpike
    {
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            TargetMetal = MetalType.Steel;
            SpikeTier = SpikeType.Bone;
            base.SetDefaults();
        }
    }
}