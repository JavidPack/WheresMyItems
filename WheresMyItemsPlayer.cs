using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace WheresMyItems
{
	public class WheresMyItemsPlayer : ModPlayer
	{
		internal static bool[] waitingOnContents = new bool[1000];
		const int itemSearchRange = 400;

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
					//Main.NewText("Where's My Items search only available while near your town.");
					return;
				}
				string searchTerm = WheresMyItemsUI.SearchTerm;
				if (searchTerm.Length == 0) return;
				// TODO, include piggybank, safe, and defenders forge in search.
				for (int chestIndex = 0; chestIndex < 1000; chestIndex++)
				{
					// If we are waiting on chest contents, skip.
					if (waitingOnContents[chestIndex])
					{
						continue;
					}
					Chest chest = Main.chest[chestIndex];
					if (chest != null && /*!Chest.IsPlayerInChest(i) &&*/ !Chest.isLocked(chest.x, chest.y))
					{
						Vector2 distanceToPlayer = new Vector2((float)(chest.x * 16 + 16), (float)(chest.y * 16 + 16));
						if ((distanceToPlayer - player.Center).Length() < itemSearchRange)
						{
							if (chest.item[0] == null)
							{
								var message = mod.GetPacket();
								message.Write((byte)MessageType.SilentRequestChestContents);
								message.Write(chestIndex);
								message.Send();
								waitingOnContents[chestIndex] = true;
								//Main.NewText($"Wait on {chestIndex}");
								continue;
							}
							// We could technically get item 0 but not item 39, so this check just makes sure we have all the items synced.
							//if (chest.item[chest.item.Length - 1] == null)
							//{
							//	// add 10 frames to wait time
							//	waitTimes[chestIndex] = 10;
							//	continue;
							//}

							bool found = false;
							Item[] items = chest.item;
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
								Dust.NewDust(new Vector2(chest.x * 16, chest.y * 16), 32, 32, 6);
							}
						}
					}
				}
			}
		}
	}
}
