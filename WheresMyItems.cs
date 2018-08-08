using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace WheresMyItems
{
	public class WheresMyItems : Mod
	{
		private UserInterface wheresMyItemsUserInterface;
		internal WheresMyItemsUI wheresMyItemsUI;
		public static ModHotKey RandomBuffHotKey;

		public static Mod MagicStorage;
		public static int MagicStorage_TileType_StorageHeart;
		public static MethodInfo MagicStorage_TEStorageHeart_GetStoredItems;

		int lastSeenScreenWidth;
		int lastSeenScreenHeight;

		public WheresMyItems()
		{
		}

		public override void Load()
		{
			if (!Main.dedServ)
			{
				RandomBuffHotKey = RegisterHotKey("Wheres My Items", "Delete");
				wheresMyItemsUI = new WheresMyItemsUI();
				wheresMyItemsUI.Activate();
				wheresMyItemsUserInterface = new UserInterface();
				wheresMyItemsUserInterface.SetState(wheresMyItemsUI);
			}
			MagicStorage = ModLoader.GetMod("MagicStorage");
		}

		public override void Unload()
		{
			RandomBuffHotKey = null;

			MagicStorage = null; // Do this or MagicStorage won't fully unload.
			MagicStorage_TileType_StorageHeart = 0;
			MagicStorage_TEStorageHeart_GetStoredItems = null; // These 2 are for clean code and don't prevent GC 
		}

		public override void PostSetupContent()
		{
			// All Mods have done Load already, so all Tiles have IDs
			MagicStorage_TileType_StorageHeart = MagicStorage?.TileType("StorageHeart") ?? 0;
			if(MagicStorage_TileType_StorageHeart > 0)
			{
				// Namespace: MagicStorage.Components 
				// Class: TEStorageHeart
				// Method: public IEnumerable<Item> GetStoredItems()
				MagicStorage_TEStorageHeart_GetStoredItems = MagicStorage.GetType().Assembly.GetType("MagicStorage.Components.TEStorageHeart").GetMethod("GetStoredItems", BindingFlags.Instance | BindingFlags.Public);
			}
		}

		public override void UpdateUI(GameTime gameTime)
		{
			if (WheresMyItemsUI.visible)
				if (wheresMyItemsUserInterface != null)
					wheresMyItemsUserInterface.Update(gameTime);
		}

		public static string hoverItemNameBackup;
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int vanillaInventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Fancy UI"));
			if (vanillaInventoryLayerIndex != -1)
			{
				layers.Insert(vanillaInventoryLayerIndex, new LegacyGameInterfaceLayer(
					"WheresMyItems: Quick Search",
					delegate
					{
						hoverItemNameBackup = null;
						if (WheresMyItemsUI.visible)
						{
							if (lastSeenScreenWidth != Main.screenWidth || lastSeenScreenHeight != Main.screenHeight)
							{
								wheresMyItemsUserInterface.Recalculate();
								lastSeenScreenWidth = Main.screenWidth;
								lastSeenScreenHeight = Main.screenHeight;
							}
							wheresMyItemsUserInterface.Draw(Main.spriteBatch, new GameTime());
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
						if (!string.IsNullOrEmpty(hoverItemNameBackup))
							Main.hoverItemName = hoverItemNameBackup;
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
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