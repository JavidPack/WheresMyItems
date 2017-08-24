using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI;
using System.Collections.Generic;

namespace WheresMyItems
{
    //draw problems > check position
	public class WheresMyItemsPlayer : ModPlayer
	{
		internal static bool[] waitingOnContents = new bool[1000];
		const int itemSearchRange = 400;
        int gameCounter;
        Item[] curInv;

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
            Vector2 distanceToPlayer = new Vector2((c.x * 16 + 16), (c.y * 16 + 16));
            return (distanceToPlayer - player.Center).Length() < range;
        }

        public bool TestForItem(Chest c, string searchTerm, ref Item[] nInv)
        {
            int found = 0;
            Item[] items = c.item;
            Item[] inv = new Item[3];
            for (int i = 0; i < 40; i++)
            {
                if (items[i].Name.ToLower().IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    inv[found] = items[i];
                    if (i > 0)
                    {
                        if (items[i].type == items[i-1].type)
                        {
                            found--;
                        }
                    }
                    found++;
                    if (found == 3)
                    {
                        break;
                    }
                    
                }
            }
            nInv = inv;
            return found > 0;
        }

        public void NewDustSlowed(Vector2 pos,int w, int h,int type,int interval)
        {
            Point tPos = pos.ToTileCoordinates();
            if (gameCounter % interval == 0)
            {
                int d = Dust.NewDust(pos, w, h, type,0f,0f,0, Color.White, 0.9f);
            }
        }

        public Vector2 HalfSize(Texture2D t,float scale)
        {
            return new Vector2(t.Width * scale / 2, t.Height * scale / 2);
        }

        public Rectangle CreateRect(Vector2 v, Texture2D t)
        {
            return new Rectangle((int)v.X, (int)v.Y, t.Width, t.Height);
        }

        public void DrawSlot(Vector2 cPos,Texture2D item, Texture2D box, float scale, Color colour)
        {
            //Main.spriteBatch.Draw(box, CreateRect(cPos - HalfSize(box), box), CreateRect(Vector2.Zero,box), Color.White,0f,Vector2.Zero,scale ,SpriteEffects.None,0);
            //Main.spriteBatch.Draw(item, CreateRect(cPos - HalfSize(item), item), CreateRect(Vector2.Zero, item), Color.White, 0f,Vector2.Zero,scale, SpriteEffects.None,0);
            Main.spriteBatch.Draw(box, cPos - HalfSize(box,scale), CreateRect(Vector2.Zero,box), colour,0f,Vector2.Zero,scale ,SpriteEffects.None,0);
            Main.spriteBatch.Draw(item, cPos - HalfSize(item,scale), CreateRect(Vector2.Zero, item), Color.White, 0f,Vector2.Zero,scale, SpriteEffects.None,0);
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            base.ModifyDrawLayers(layers);
        }

        public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
		{
            gameCounter++;
            if (gameCounter == 99999)
            {
                gameCounter = 0;
            }
			if (WheresMyItemsUI.visible && player == Main.LocalPlayer)
			{
                Texture2D[] bank = new Texture2D[3];
                Vector2[] pos = new Vector2[3];
                Texture2D box = mod.GetTexture("box");
                bank[0] = mod.GetTexture("b1");
                bank[1] = mod.GetTexture("b2");
                bank[2] = mod.GetTexture("b3");
                Vector2 plTopCenter = player.position + new Vector2(player.width / 2, 0f) - Main.screenPosition;
                pos[0] = plTopCenter + new Vector2(-48, -32);
                pos[1] = plTopCenter + new Vector2(0, -32);
                pos[2] = plTopCenter + new Vector2(48, -32);
                
                for (int i = 0; i < 3; i++)
                {
                    DrawSlot(pos[i], bank[i], box,1f, Color.White);
                }
                //Main.NewText(Main.player[Main.myPlayer].chest.ToString());
                /*if (player.townNPCs < 1f)
				{
					WheresMyItemsUI.box.SetText("");
					WheresMyItemsUI.box.Unfocus();
					//Main.NewText("Where's My Items search only available while near your town.");
					return;
				}*/
                string searchTerm = WheresMyItemsUI.SearchTerm;
				if (searchTerm.Length == 0) return;
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
                        if (ChestWithinRange(chest, itemSearchRange))
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
                            if (TestForItem(chest,searchTerm, ref curInv))
                            {
                                
                                Vector2 mousePosition = new Vector2(Main.mouseX, Main.mouseY) + Main.screenPosition;
                                Rectangle chestArea = new Rectangle(chest.x*16, chest.y*16, 32, 32);
                                Vector2[] hoverPos = new Vector2[3];
                                Texture2D[] itemT = new Texture2D[3];

                                hoverPos[1] = mousePosition - Main.screenPosition;
                                hoverPos[0] = hoverPos[1] - new Vector2(48, 0);
                                hoverPos[2] = hoverPos[1] + new Vector2(48, 0);
                                // hover check
                                if (chestArea.Contains(mousePosition.ToPoint()))
                                {
                                    for (int i = 0; i < 3; i++)
                                    {
                                        if(curInv[i] != null)
                                        {
                                            if (curInv[i].type > Main.itemTexture.Length)
                                            {
                                                itemT[i] = curInv[i].modItem.mod.GetTexture(curInv[i].modItem.Texture);
                                            }
                                            else
                                            {
                                                itemT[i] = Main.itemTexture[curInv[i].type];
                                            }
                                            DrawSlot(hoverPos[i], itemT[i], box,1f,Color.Red);
                                        }
                                    }
                                }
                                NewDustSlowed(new Vector2(chest.x * 16, chest.y * 16), 32, 32, 16,10); //107
                            }
                        }
                    }
                }
                // deal with extra invens
                
                Chest bk;
                for (int i = 0; i < 3; i++)
                {
                    switch(i)
                    {
                        case 1:
                            bk = player.bank2;
                            break;
                        case 2:
                            bk = player.bank3;
                            break;
                        default:
                            bk = player.bank;
                            break;
                    }
                    if (TestForItem(bk, searchTerm, ref curInv))
                    {
                        NewDustSlowed(pos[i] + Main.screenPosition, 1, 1, 16,30); //used to be 6 //188
                    }
                }
			}
		}
	}
}
