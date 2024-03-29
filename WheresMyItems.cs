﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace WheresMyItems
{
	public class WheresMyItems : Mod
	{
		private UserInterface wheresMyItemsUserInterface;
		internal WheresMyItemsUI wheresMyItemsUI;
		public static ModKeybind RandomBuffHotKey;

		public static WheresMyItems instance;
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
			instance = this;
			RandomBuffHotKey = KeybindLoader.RegisterKeybind(this, "ShowSearchInterface", "Delete");
			ModLoader.TryGetMod("MagicStorage", out MagicStorage);
		}

		public override void Unload()
		{
			RandomBuffHotKey = null;

			instance = null;
			MagicStorage = null; // Do this or MagicStorage won't fully unload.
			MagicStorage_TileType_StorageHeart = 0;
			MagicStorage_TEStorageHeart_GetStoredItems = null; // These 2 are for clean code and don't prevent GC 
		}

		public override void PostSetupContent() {
			if (!Main.dedServ) {
				wheresMyItemsUI = new WheresMyItemsUI();
				wheresMyItemsUI.Activate();
				wheresMyItemsUserInterface = new UserInterface();
				wheresMyItemsUserInterface.SetState(wheresMyItemsUI);
			}

			// All Mods have done Load already, so all Tiles have IDs
			if (MagicStorage != null) {
				if(MagicStorage.TryFind<ModTile>("StorageHeart", out ModTile StorageHeart)) {
					MagicStorage_TileType_StorageHeart = StorageHeart.Type;
				}
				if (MagicStorage_TileType_StorageHeart > 0) {
					// Namespace: MagicStorage.Components 
					// Class: TEStorageHeart
					// Method: public IEnumerable<Item> GetStoredItems()
					// TODO: might need AssemblyManager.GetLoadableTypes(assembly) once MagicStorage updates?
					MagicStorage_TEStorageHeart_GetStoredItems = MagicStorage.GetType().Assembly.GetType("MagicStorage.Components.TEStorageHeart").GetMethod("GetStoredItems", BindingFlags.Instance | BindingFlags.Public);
				}
			}
		}

		public void UpdateUI(GameTime gameTime) {
			if (WheresMyItemsUI.visible)
				if (wheresMyItemsUserInterface != null)
					wheresMyItemsUserInterface.Update(gameTime);
		}

		public static string hoverItemNameBackup;
		public void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int vanillaInventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Fancy UI"));
			if (vanillaInventoryLayerIndex != -1) {
				layers.Insert(vanillaInventoryLayerIndex, new LegacyGameInterfaceLayer(
					"WheresMyItems: Quick Search",
					delegate {
						hoverItemNameBackup = null;
						if (WheresMyItemsUI.visible) {
							if (lastSeenScreenWidth != Main.screenWidth || lastSeenScreenHeight != Main.screenHeight) {
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
			if (mouseTextLayerIndex != -1) {
				layers.Insert(mouseTextLayerIndex, new LegacyGameInterfaceLayer(
					"WheresMyItems: Hover Text Logic",
					delegate {
						if (!string.IsNullOrEmpty(WheresMyItems.hoverItemNameBackup))
							Main.hoverItemName = WheresMyItems.hoverItemNameBackup;
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
							NetMessage.SendData(MessageID.SyncChestItem, whoAmI, -1, null, chestIndex, (float)i, 0f, 0f, 0, 0, 0);
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

	public class WheresMyItemsSystem : ModSystem {
		public override void UpdateUI(GameTime gameTime) => ModContent.GetInstance<WheresMyItems>().UpdateUI(gameTime);

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) => ModContent.GetInstance<WheresMyItems>().ModifyInterfaceLayers(layers);
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