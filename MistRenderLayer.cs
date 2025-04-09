using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MistbornMod
{
    // This class handles rendering the mist over the world
    public class MistRenderLayer : ModSystem
    {
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Find the index of the vanilla hotbar layer (to place our UI above it)
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Interface Logic 1"));
            
            if (inventoryIndex != -1)
            {
                // Insert our mist rendering just before the inventory
                layers.Insert(inventoryIndex, new LegacyGameInterfaceLayer(
                    "MistbornMod: Mist Effect",
                    delegate
                    {
                        // Only draw if mist is active and we're in the world
                        
                        
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}