using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MistbornMod.Content.Items.Consumables;

namespace MistbornMod
{
    // This class adds Lerasium Bead drops to certain boss NPCs
    public class NPCDrops : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // Add Lerasium Bead as a rare drop from powerful bosses
            switch (npc.type)
            {
                case NPCID.EyeofCthulhu:
                    // 5% chance from Eye of Cthulhu
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LerasiumBead>(), 20));
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AtiumBead>(), 20));
                    break;
                    
                case NPCID.KingSlime:
                    // 5% chance from King Slime
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LerasiumBead>(), 20));
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AtiumBead>(), 20));
                    break;                    
                case NPCID.WallofFlesh:
                    // 10% chance from Wall of Flesh
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LerasiumBead>(), 10));
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AtiumBead>(), 10));
                    break;                    
                case NPCID.SkeletronPrime:
                case NPCID.TheDestroyer:
                case NPCID.Retinazer:
                case NPCID.Spazmatism:
                    // 15% chance from mechanical bosses
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AtiumBead>(), 20));                    
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LerasiumBead>(), 7));
                    break;                    
                case NPCID.Plantera:
                    // 20% chance from Plantera
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LerasiumBead>(), 5));
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AtiumBead>(), 5));

                    break;
                    
                case NPCID.MoonLordCore:
                    // 100% chance from Moon Lord (guaranteed drop)
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LerasiumBead>(), 1));
                    npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AtiumBead>(), 1));

                    break;
            }
        }
    }
}