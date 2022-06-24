using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria;

namespace WheresMyItems
{
	//draw problems > check position
	public class WheresMyItemsPlayer : ModPlayer
	{
		internal static bool[] waitingOnContents = new bool[8000];
		private const int itemSearchRange = 400;
		private int gameCounter;
		private Item[] curInv;
		private float sc = 0.8f;
		public static bool hover;

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

		public bool ChestWithinRange(Chest c, int range)
		{
			Vector2 chestCenter = new Vector2((c.x * 16 + 16), (c.y * 16 + 16));
			return (chestCenter - Player.Center).Length() < range;
		}

		public int TestForItem(Chest c, string searchTerm, ref Item[] nInv)
		{
			int found = 0;
			Item[] items = c.item;
			Item[] inv = new Item[4];
			for (int i = 0; i < 40; i++)
			{
				if (items[i] == null)
				{
					continue;
				}
				if (items[i].Name.ToLower().IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) != -1)
				{
					inv[found] = items[i].Clone();
					if (found > 0)
					{
						if (inv[found].type == inv[found - 1].type)
						{
							inv[found] = null;
							found--;
						}
					}
					found++;
					if (found == 4)
					{
						break;
					}
				}
			}
			nInv = inv;
			return found;
		}

		public int TestForItem(IEnumerable<Item> c, string searchTerm, ref Item[] nInv)
		{
			int found = 0;
			Item[] inv = new Item[4];
			foreach (Item item in c)
			{
				if (item == null)
				{
					continue;
				}
				if (item.Name.ToLower().IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) != -1)
				{
					inv[found] = item.Clone();
					if (found > 0)
					{
						if (inv[found].type == inv[found - 1].type)
						{
							inv[found] = null;
							found--;
						}
					}
					found++;
					if (found == 4)
					{
						break;
					}
				}
			}
			nInv = inv;
			return found;
		}

		public void NewDustSlowed(Vector2 pos, int w, int h, int type, int interval)
		{
			Point tPos = pos.ToTileCoordinates();
			if (gameCounter % interval == 0)
			{
				int d = Dust.NewDust(pos, w, h, type, 0f, 0f, 0, Color.White, 0.9f);
			}
		}

		public Vector2 HalfSize(Texture2D t, float scale)
		{
			return new Vector2(t.Width * scale / 2, t.Height * scale / 2);
		}

		public Rectangle CreateRect(Vector2 v, Texture2D t)
		{
			return new Rectangle((int)v.X, (int)v.Y, t.Width, t.Height);
		}

		public DrawData[] DrawDataSlot(Vector2 cPos, Texture2D item, Texture2D box, float scale, Color colour)
		{
			DrawData[] d = new DrawData[2];
			d[0] = new DrawData(box, cPos - HalfSize(box, scale), CreateRect(Vector2.Zero, box), colour, 0f, Vector2.Zero, scale, SpriteEffects.None, 1);
			d[1] = new DrawData(item, cPos - HalfSize(item, scale), CreateRect(Vector2.Zero, item), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1);
			return d;
		}

		public Vector2 GetTile(Vector2 start, Vector2 end, int i)
		{
			int y = (int)start.Y;
			int x = (int)start.X + i;
			while (x - end.X > -1)
			{
				y++;
				x -= (int)(end.X - start.X);
			}
			return new Vector2(x, y);
		}

		public int NoTile(Vector2 start, Vector2 end)
		{
			return (int)((end.X - start.X) * (end.Y - start.Y));
		}

		public Vector2 Gtp(Vector2 xy)
		{
			//get tile position
			//is it x or y, bool X tells you
			xy /= 16;
			xy.X = (int)xy.X;
			xy.Y = (int)xy.Y;
			return xy;
		}

		public void DeactTiles(ref int nodt, ref Vector2[] deactivatedTiles, bool deactivated, Vector2 tilePos, Vector2 size)
		{
			int nodtiles = NoTile(tilePos, tilePos + size);
			if (!deactivated)
			{
				for (int k = 1; k < nodtiles; k++)
				{
					deactivatedTiles[nodt] = GetTile(tilePos, tilePos + size, k);
					//string text = (deactivatedTiles[nodt] - tilePos).ToString();
					nodt++;
				}
			}
		}

		public void AddItem(Vector2 pos, Texture2D itemText, Texture2D box, float scale, Color colour, Item curItem)
		{

			WheresMyItemsUI.worldZoomDrawDatas.Add(DrawDataSlot(pos, itemText, box, scale, colour));
			WheresMyItemsUI.worldZoomItems.Add(curItem);
			WheresMyItemsUI.worldZoomPositions.Add(pos - 1.25f * HalfSize(box, scale));
		}

		public void DrawPeeks(Vector2[] peekPos, Texture2D[] itemT, Texture2D box, Item[] inv, int no, int bank)// int bank is only used when you want to draw peeks for the bank icons above the Player's head. Bank has to be -1 for those peeks to be drawn correctly. Otherwise, it can be 1; 
		{
			for (int i = 0; i < 4; i++)
			{
				peekPos[i] += new Vector2(0, sc * 24 * (4 - no));
				if (inv[i] != null && !inv[i].IsAir)
				{
					itemT[i] = Terraria.GameContent.TextureAssets.Item[inv[i].type].Value;
					AddItem(peekPos[i], itemT[i], box, sc, Color.Red, inv[i]);
				}
			}
		}


		public override void DrawEffects(Terraria.DataStructures.PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
		{
			if (WheresMyItemsUI.visible && Player == Main.LocalPlayer)
			{
				gameCounter++;
				if (gameCounter == 99999)
				{
					gameCounter = 0;
				}
				WheresMyItemsUI.worldZoomDrawDatas.Clear();
				WheresMyItemsUI.worldZoomItems.Clear();
				WheresMyItemsUI.worldZoomPositions.Clear();

				Texture2D[] bank = new Texture2D[4];
				Vector2[] pos = new Vector2[4];
				Texture2D box = Mod.Assets.Request<Texture2D>("box").Value;
				bank[0] = Terraria.GameContent.TextureAssets.Item[87].Value;
				bank[1] = Terraria.GameContent.TextureAssets.Item[346].Value;
				bank[2] = Terraria.GameContent.TextureAssets.Item[3813].Value;
				bank[3] = Terraria.GameContent.TextureAssets.Item[4076].Value;
				Vector2 plTopCenter = Player.position + new Vector2(Player.width / 2, 0f) - Main.screenPosition;
				pos[0] = plTopCenter + new Vector2(-72, -32);
				pos[1] = plTopCenter + new Vector2(-24, -32);
				pos[2] = plTopCenter + new Vector2(24, -32);
				pos[3] = plTopCenter + new Vector2(72, -32);

				for (int i = 0; i < 4; i++)
				{
					AddItem(pos[i], bank[i], box, 1f, Color.White, null);
				}
				//Main.NewText(Main.Player[Main.myPlayer].chest.ToString());
				/*if (Player.townNPCs < 1f)
				{
					WheresMyItemsUI.box.SetText("");
					WheresMyItemsUI.box.Unfocus();
					//Main.NewText("Where's My Items search only available while near your town.");
					return;
				}*/
				string searchTerm = WheresMyItemsUI.SearchTerm;
				if (searchTerm.Length == 0) return;
				for (int chestIndex = 0; chestIndex < 8000; chestIndex++)
				{
					// If we are waiting on chest contents, skip.
					if (waitingOnContents[chestIndex])
					{
						continue;
					}
					Chest chest = Main.chest[chestIndex];
					if (chest != null && /*!Chest.IsPlayerInChest(i) &&*/ !Chest.IsLocked(chest.x, chest.y))
					{
						if (ChestWithinRange(chest, itemSearchRange))
						{
							if (chest.item[0] == null)
							{
								var message = Mod.GetPacket();
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
							int no = TestForItem(chest, searchTerm, ref curInv);
							if (no > 0)
							{
								NewDustSlowed(new Vector2(chest.x * 16, chest.y * 16), 32, 32, 16, 10); //107
																										// draw peek boxes
								Rectangle chestArea = new Rectangle(chest.x * 16, chest.y * 16, 32, 32);
								Vector2[] peekPos = new Vector2[4];
								Texture2D[] itemT = new Texture2D[4];
								if (hover)
								{
									Vector2 mousePosition = new Vector2(Main.mouseX, Main.mouseY) + Main.screenPosition;
									peekPos[1] = mousePosition - Main.screenPosition;

									// hover check
									if (!chestArea.Contains(mousePosition.ToPoint()))
									{
										continue;
									}
								}
								else
								{
									peekPos[1] = chestArea.Center.ToVector2() - Main.screenPosition;
								}
								peekPos[0] = peekPos[1] - new Vector2(0, 48 * sc);
								peekPos[2] = peekPos[1] + new Vector2(0, 48 * sc);
								DrawPeeks(peekPos, itemT, box, curInv, no, 1);
							}
						}
					}
				}
				// deal with extra invens
				Chest bk;
				for (int bankno = 0; bankno < 4; bankno++)
				{
					switch (bankno)
					{
						case 1:
							bk = Player.bank2;
							break;

						case 2:
							bk = Player.bank3;
							break;

						case 3:
							bk = Player.bank4;
							break;

						default:
							bk = Player.bank;
							break;
					}
					int no = TestForItem(bk, searchTerm, ref curInv);
					if (no > 0)
					{
						NewDustSlowed(pos[bankno] + Main.screenPosition, 1, 1, 16, 30);
						pos[bankno].X -= 16;
						pos[bankno].Y -= 16;
						Vector2 hoverCorner = pos[bankno] + Main.screenPosition;
						Rectangle chestArea = new Rectangle((int)hoverCorner.X, (int)hoverCorner.Y, 32, 32);
						Vector2[] peekPos = new Vector2[4];
						Texture2D[] itemT = new Texture2D[4];
						if (hover)
						{
							Vector2 mousePosition = new Vector2(Main.mouseX, Main.mouseY) + Main.screenPosition;
							peekPos[1] = mousePosition - Main.screenPosition;// + new Vector2(0, 48 * sc);
							peekPos[0] = peekPos[1] - new Vector2(0, 48 * sc);
							peekPos[2] = peekPos[1] + new Vector2(0, 48 * sc);
							if (!chestArea.Contains(mousePosition.ToPoint()))
							{
								continue;
							}
							DrawPeeks(peekPos, itemT, box, curInv, no, -1); // pass 4 as "no" so that there's no offset
						}
						else
						{
							peekPos[0] = chestArea.Center.ToVector2() - Main.screenPosition;
							peekPos[1] = peekPos[0] - new Vector2(0, 48 * sc);
							peekPos[2] = peekPos[1] - new Vector2(0, 48 * sc);
							DrawPeeks(peekPos, itemT, box, curInv, 4, -1); // pass 4 as "no" so that there's no offset
						}
					}
				}
				// deal with tiles of extra invens
				//Vector2 mouseTilePos = Gtp(new Vector2(Main.mouseX, Main.mouseY) + Main.screenPosition); for debugging
				//Main.NewText(Main.tile[(int)mouseTilePos.X, (int)mouseTilePos.Y].ToString()); for debugging
				Tile tile;
				Vector2 tilePos = Vector2.Zero;
				Chest bktile;

				float dist = (float)Math.Sqrt(Math.Pow(itemSearchRange, 2) / 2);
				Vector2 start = Gtp(Player.Center - new Vector2(dist, dist));
				Vector2 end = Gtp(Player.Center + new Vector2(dist, dist));

				// This deactivating system is used to ensure we don't have several peek boxes drawn for multi-tile banks (since the peek system checks each tile to see if it's a bank and hence draw a peek). It "deactivates" every tile except for the one in the top-left corner.
				int notiles = NoTile(start, end);
				Vector2[] deactivatedTiles = new Vector2[notiles];
				int nodt = 0;
				for (int j = 0; j < notiles; j++)
				{
					tilePos = GetTile(start, end, j);
					tile = Main.tile[(int)tilePos.X, (int)tilePos.Y];
					if (tile.HasTile)
					{
						bool deactivated = false;
						for (int k = 0; k < nodt; k++)
						{
							if (tilePos == deactivatedTiles[k])
							{
								deactivated = true;
								/*if (tilePos == mouseTilePos) //for debugging
								{
									NewDustSlowed(deactivatedTiles[k] * 16, 32, 32, 21, 10);
								}*/
							}
						}
						Rectangle chestArea = new Rectangle();
						Vector2 tSize = Vector2.Zero;
						if (tile.TileFrameX != 0 || tile.TileFrameY != 0) // if we haven't found the top-left corner of the block
						{
							continue;
						}
						switch (tile.TileType)
						{
							case 29:
								bktile = Player.bank;
								tSize = new Vector2(2, 1);
								DeactTiles(ref nodt, ref deactivatedTiles, deactivated, tilePos, tSize);
								break;
							case 97:
								bktile = Player.bank2;
								tSize = new Vector2(2, 2);
								DeactTiles(ref nodt, ref deactivatedTiles, deactivated, tilePos, tSize);
								break;
							case 463:
								bktile = Player.bank3;
								tSize = new Vector2(3, 4);
								DeactTiles(ref nodt, ref deactivatedTiles, deactivated, tilePos, tSize);
								break;
							case 491:
								bktile = Player.bank4;
								tSize = new Vector2(3, 4);
								DeactTiles(ref nodt, ref deactivatedTiles, deactivated, tilePos, tSize);
								break;
							default:
								continue;
						}
						int no = TestForItem(bktile, searchTerm, ref curInv);
						if (no > 0)
						{
							// draw peek boxes
							chestArea = new Rectangle((int)tilePos.X * 16, (int)tilePos.Y * 16, (int)tSize.X * 16, (int)tSize.Y * 16);
							// chestArea declared above instead since the "chest area" varies for the piggy bank + forge
							Vector2[] peekPos = new Vector2[4];
							Texture2D[] itemT = new Texture2D[4];
							if (hover)
							{
								Vector2 mousePosition = new Vector2(Main.mouseX, Main.mouseY) + Main.screenPosition;
								peekPos[1] = mousePosition - Main.screenPosition;

								// hover check
								if (!chestArea.Contains(mousePosition.ToPoint()))
								{
									continue;
								}
							}
							else if (!deactivated)
							{
								peekPos[1] = chestArea.Center.ToVector2() - Main.screenPosition;
							}
							else
							{
								continue;
							}
							peekPos[0] = peekPos[1] - new Vector2(0, 48 * sc);
							peekPos[2] = peekPos[1] + new Vector2(0, 48 * sc);
							DrawPeeks(peekPos, itemT, box, curInv, no, 1);
						}
					}
				}

				// Magic Storage Integration/Support
				if (WheresMyItems.MagicStorage != null)
				{
					var center = Main.LocalPlayer.Center.ToTileCoordinates();
					int tileRange = itemSearchRange / 16;
					for (int i = -tileRange + center.X; i < tileRange + center.X; i++) // Maybe fix this square range to be circular.
					{
						for (int j = -tileRange + center.Y; j < tileRange + center.Y; j++)
						{
							if (WorldGen.InWorld(i, j) && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == WheresMyItems.MagicStorage_TileType_StorageHeart && Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
							{
								//NewDustSlowed(new Vector2(i + 0.5f, j + 0.5f) * 16, 16, 16, 16, 20);

								TileEntity teStorageHeart = TileEntity.ByPosition[new Point16(i, j)];
								if (teStorageHeart == null)
									continue;

								IEnumerable<Item> items = (IEnumerable<Item>)WheresMyItems.MagicStorage_TEStorageHeart_GetStoredItems.Invoke(teStorageHeart, new object[] { });

								int no = TestForItem(items, searchTerm, ref curInv);
								if (no > 0)
								{
									NewDustSlowed(new Vector2(i * 16, j * 16), 32, 32, 16, 10); //107
																											// draw peek boxes
									Rectangle chestArea = new Rectangle(i * 16, j * 16, 32, 32);
									Vector2[] peekPos = new Vector2[4];
									Texture2D[] itemT = new Texture2D[4];
									if (hover)
									{
										Vector2 mousePosition = new Vector2(Main.mouseX, Main.mouseY) + Main.screenPosition;
										peekPos[1] = mousePosition - Main.screenPosition;

										// hover check
										if (!chestArea.Contains(mousePosition.ToPoint()))
										{
											continue;
										}
									}
									else
									{
										peekPos[1] = chestArea.Center.ToVector2() - Main.screenPosition;
									}
									peekPos[0] = peekPos[1] - new Vector2(0, 48 * sc);
									peekPos[2] = peekPos[1] + new Vector2(0, 48 * sc);
									DrawPeeks(peekPos, itemT, box, curInv, no, 1);
								}
							}
						}
					}
				}
			}
		}
	}
}