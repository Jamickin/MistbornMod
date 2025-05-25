
// Content/Items/SteelAnchorAmmo.cs
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MistbornMod.Content.Items
{
    /// <summary>
    /// Special ammunition that creates steel anchor points for enhanced mobility
    /// </summary>
    public class SteelAnchorAmmo : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName and Tooltip set in localization
        }

        public override void SetDefaults()
        {
            Item.damage = 8; // Slightly less damage than normal bullets
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.knockBack = 1.5f;
            Item.value = Item.sellPrice(copper: 5);
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<Projectiles.SteelAnchorBullet>();
            Item.shootSpeed = 10f;
            Item.ammo = AmmoID.Bullet; // Uses bullet slot
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(100); // Craft 100 at a time
            recipe.AddIngredient(ItemID.MusketBall, 100);
            recipe.AddIngredient(ItemID.IronBar, 1);
            recipe.AddIngredient(ItemID.Chain, 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
            
            // Alternative recipe with lead
            Recipe recipe2 = CreateRecipe(100);
            recipe2.AddIngredient(ItemID.MusketBall, 100);
            recipe2.AddIngredient(ItemID.LeadBar, 1);
            recipe2.AddIngredient(ItemID.Chain, 1);
            recipe2.AddTile(TileID.Anvils);
            recipe2.Register();
        }
    }
}