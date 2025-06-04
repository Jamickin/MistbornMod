namespace MistbornMod.Content.Items.Consumables
{
    public class ChromiumVial : MetalVial
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Metal = MetalType.Chromium;
            // Make it rare since it's a testing item
            Item.rare = Terraria.ID.ItemRarityID.Purple;
            // Add a tooltip indicating it's for testing
            Item.UseSound = Terraria.ID.SoundID.Item4; // Different sound for special metal
        }
    }
}