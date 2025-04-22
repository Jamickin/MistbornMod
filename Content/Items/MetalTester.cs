using MistbornMod.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MistbornMod.Content.Items
{
    public class MetalTester : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip are set in Localization files
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 1;
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Green;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.UseSound = SoundID.Item4;
        }

        public override bool? UseItem(Player player)
        {
            MistbornPlayer modPlayer = player.GetModPlayer<MistbornPlayer>();
            
            if (modPlayer.IsMistborn)
            {
                Main.NewText("You are a Mistborn and can burn all metals!", 255, 220, 100);
                return true;
            }
            else if (modPlayer.IsMisting && modPlayer.MistingMetal.HasValue)
            {
                if (modPlayer.HasDiscoveredMistingAbility)
                {
                    // They already know what they are
                    string mistingName = modPlayer.GetMistingName(modPlayer.MistingMetal.Value);
                    Main.NewText($"You are a {mistingName} and can burn {modPlayer.MistingMetal}.", 255, 220, 100);
                }
                else
                {
                    // If they haven't discovered ability yet, give strong hint but don't directly reveal
                    MetalType metal = modPlayer.MistingMetal.Value;
                    string hint = GetMetalHint(metal);
                    Main.NewText("Your Allomantic ability seems tied to: " + hint, 200, 220, 255);
                    Main.NewText("Try drinking a vial of this metal to confirm your ability.", 200, 255, 200);
                }
                return true;
            }
            else
            {
                Main.NewText("The tester shows no Allomantic ability. Find a Lerasium Bead to gain powers.", 255, 100, 100);
                return true;
            }
        }
        
        private string GetMetalHint(MetalType metal)
        {
            switch (metal)
            {
                case MetalType.Iron: return "Iron - you feel drawn to metal objects";
                case MetalType.Steel: return "Steel - you feel like you could push metals away";
                case MetalType.Tin: return "Tin - your senses seem unusually sharp at times";
                case MetalType.Pewter: return "Pewter - you occasionally feel stronger than you should";
                case MetalType.Zinc: return "Zinc - others' emotions seem to flare when you're angry";
                case MetalType.Brass: return "Brass - you have a calming effect on others";
                case MetalType.Copper: return "Copper - you feel like you can hide your presence";
                case MetalType.Bronze: return "Bronze - you can sense strange pulses from other Allomancers";
                default: return "an unknown metal";
            }
        }
        
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Glass, 5);
            recipe.AddIngredient(ItemID.IronBar, 2);
            recipe.AddIngredient(ItemID.CopperBar, 2);
            recipe.AddIngredient(ItemID.TinBar, 2);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}