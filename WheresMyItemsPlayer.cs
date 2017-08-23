using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;

namespace WheresMyItems
{
    //draw problems > check position
	public class WheresMyItemsPlayer : ModPlayer
	{
		internal static bool[] waitingOnContents = new bool[1000];
		const int itemSearchRange = 400;
        int gameCounter;

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

        public bool TestForItem(Chest c, string searchTerm)
        {
            bool found = false;
            Item[] items = c.item;
            for (int i = 0; i < 40; i++)
            {
                if (items[i].Name.ToLower().IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }

        public void NewDustSlowed(Vector2 pos,int w, int h,int type,int interval)
        {
            Point tPos = pos.ToTileCoordinates();
            if (gameCounter % interval == 0)
            {
                
            }
        }

        public Vector2 HalfSize(Texture2D t)
        {
            return new Vector2(t.Width / 2, t.Height / 2);
        }

        public Rectangle CreateRect(Vector2 v, Texture2D t)
        {
            return new Rectangle((int)v.X, (int)v.Y, t.Width, t.Height);
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
                    Main.spriteBatch.Draw(box, CreateRect(pos[i] - HalfSize(box), box), Color.White);
                    Main.spriteBatch.Draw(bank[i], CreateRect(pos[i] - HalfSize(bank[i]),bank[i]), Color.White);
                    
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
                            if (TestForItem(chest,searchTerm))
                            {
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
                    if (TestForItem(bk, searchTerm))
                    {
                        NewDustSlowed(pos[i] + Main.screenPosition, 1, 1, 16,30); //used to be 6 //188
                    }
                }
			}
		}
	}
}
