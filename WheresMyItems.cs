using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace WheresMyItems
{

	public class missingStuff : ModSystem
    {
		public override void UpdateUI(GameTime gameTime)
		{
			if (WheresMyItemsUI.visible)
				if (WheresMyItems.wheresMyItemsUserInterface != null)
					WheresMyItems.wheresMyItemsUserInterface.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int vanillaInventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Fancy UI"));
			if (vanillaInventoryLayerIndex != -1)
			{
				layers.Insert(vanillaInventoryLayerIndex, new LegacyGameInterfaceLayer(
					"WheresMyItems: Quick Search",
					delegate
					{
						WheresMyItems.hoverItemNameBackup = null;
						if (WheresMyItemsUI.visible)
						{
							if (WheresMyItems.lastSeenScreenWidth != Main.screenWidth || WheresMyItems.lastSeenScreenHeight != Main.screenHeight)
							{
								WheresMyItems.wheresMyItemsUserInterface.Recalculate();
								WheresMyItems.lastSeenScreenWidth = Main.screenWidth;
								WheresMyItems.lastSeenScreenHeight = Main.screenHeight;
							}
							WheresMyItems.wheresMyItemsUserInterface.Draw(Main.spriteBatch, new GameTime());
						}
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
			int mouseTextLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextLayerIndex != -1)
			{
				layers.Insert(mouseTextLayerIndex, new LegacyGameInterfaceLayer(
					"WheresMyItems: Hover Text Logic",
					delegate
					{
						if (!string.IsNullOrEmpty(WheresMyItems.hoverItemNameBackup))
							Main.hoverItemName = WheresMyItems.hoverItemNameBackup;
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}


	public class WheresMyItems : Mod
	{
		public static UserInterface wheresMyItemsUserInterface;
		internal WheresMyItemsUI wheresMyItemsUI;
		public static ModKeybind RandomBuffHotKey;

		public static Mod MagicStorage;
		public static int MagicStorage_TileType_StorageHeart;
		public static MethodInfo MagicStorage_TEStorageHeart_GetStoredItems;

		public static int lastSeenScreenWidth;
		public static int lastSeenScreenHeight;

		public WheresMyItems()
		{
		}

		public override void Load()
		{
			if (!Main.dedServ)
			{
				RandomBuffHotKey = KeybindLoader.RegisterKeybind(this, "Delete", "Delete");
				wheresMyItemsUI = new WheresMyItemsUI();
				wheresMyItemsUI.Activate();
				wheresMyItemsUserInterface = new UserInterface();
				wheresMyItemsUserInterface.SetState(wheresMyItemsUI);
			}
		}

		public override void Unload()
		{
			RandomBuffHotKey = null;

			MagicStorage = null; // Do this or MagicStorage won't fully unload.
			MagicStorage_TileType_StorageHeart = 0;
			MagicStorage_TEStorageHeart_GetStoredItems = null; // These 2 are for clean code and don't prevent GC 
		}

		public void UpdateUI(GameTime gameTime)
		{
			var missingStuff = new missingStuff();
			missingStuff.UpdateUI(gameTime);
		}

		public static string hoverItemNameBackup;
		public void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			var missingStuff = new missingStuff();
			missingStuff.ModifyInterfaceLayers(layers);
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType msgType = (MessageType)reader.ReadByte();
			switch (msgType)
			{
				case MessageType.SilentRequestChestContents:
					int chestIndex = reader.ReadInt32();
					//System.Console.WriteLine($"Request for {chestIndex}");
					if (chestIndex > -1)
					{
						for (int i = 0; i < 40; i++)
						{
							NetMessage.SendData(32, whoAmI, -1, null, chestIndex, (float)i, 0f, 0f, 0, 0, 0);
						}
						var message = GetPacket();
						message.Write((byte)MessageType.SilentSendChestContentsComplete);
						//System.Console.WriteLine($"Request for {chestIndex} complete");
						message.Write(chestIndex);
						message.Send(whoAmI);
					}
					break;

				case MessageType.SilentSendChestContentsComplete:
					int completedChestindex = reader.ReadInt32();
					WheresMyItemsPlayer.waitingOnContents[completedChestindex] = false;
					//Main.NewText($"Complete on {completedChestindex}");
					break;

				default:
					//DebugText("Unknown Message type: " + msgType);
					break;
			}
		}
	}

	internal enum MessageType : byte
	{
		/// <summary>
		/// Vanilla client sends 31 to server, getting 32s, 33, and 80 in response, also claiming the chest open.
		/// We don't want that, so we'll do an alternate version of that
		/// 32 for each item -- Want
		/// We don't want 33 -- Syncs name, make noise.
		/// We don't want 80 -- informs others that the chest is open
		/// </summary>
		SilentRequestChestContents,

		/// <summary>
		/// Once the 40 items are sent, send this packet so we don't have to wait anymore
		/// </summary>
		SilentSendChestContentsComplete,
	}
}