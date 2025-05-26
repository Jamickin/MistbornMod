namespace MistbornMod.Content.Items.HemalurgicSpikes
{
    public class BoneIronSpike : HemalurgicSpike
    {
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            TargetMetal = MetalType.Iron;
            SpikeTier = SpikeType.Bone;
            base.SetDefaults();
        }
    }
}