using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace WheresMyItems
{
	public class WheresMyItemsPlayer : ModPlayer
	{
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (WheresMyItems.RandomBuffHotKey.JustPressed)
			{
				WheresMyItemsUI.visible = !WheresMyItemsUI.visible;
				if (WheresMyItemsUI.visible)
				{
					WheresMyItemsUI.box.SetText("");
					WheresMyItemsUI.box.Focus();
					Main.playerInventory = false;
					
				}
				// Since Main.blockInput, not called.
				//else
				//{ 
				//						WheresMyItemsUI.box.SetText("");
				//	WheresMyItemsUI.box.Unfocus();
				//}
			}
		}

		public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
		{
			if (WheresMyItemsUI.visible && player == Main.LocalPlayer)
			{
				if (player.townNPCs < 1f)
				{
					WheresMyItemsUI.box.SetText("");
					WheresMyItemsUI.box.Unfocus();
					Main.NewText("Where's My Items search only available while near your town.");
					return;
				}
				string searchTerm = WheresMyItemsUI.SearchTerm;
				if (searchTerm.Length == 0) return;
				for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
				{
					if (Main.chest[chestIndex] != null && /*!Chest.IsPlayerInChest(i) &&*/ !Chest.isLocked(Main.chest[chestIndex].x, Main.chest[chestIndex].y))
					{
						Vector2 distanceToPlayer = new Vector2((float)(Main.chest[chestIndex].x * 16 + 16), (float)(Main.chest[chestIndex].y * 16 + 16));
						if ((distanceToPlayer - player.Center).Length() < 400f)
						{
							bool found = false;
							Item[] items = Main.chest[chestIndex].item;
							for (int i = 0; i < 40; i++)
							{
								if (items[i].name.ToLower().IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) != -1)
								{
									found = true;
									break;
								}
							}
							if (found)
							{
								Dust.NewDust(new Vector2(Main.chest[chestIndex].x * 16, Main.chest[chestIndex].y * 16), 32, 32, 6);
							}
						}
					}
				}
			}
		}
	}
}
