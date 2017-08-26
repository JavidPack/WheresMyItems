using System.Collections.Generic;
using System.IO;
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

		public WheresMyItems()
		{
			Properties = new ModProperties()
			{
				Autoload = true,
			};
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
		}

		public override void Unload()
		{
			RandomBuffHotKey = null;
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int vanillaInventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (vanillaInventoryLayerIndex != -1)
			{
				layers.Insert(vanillaInventoryLayerIndex + 1, new LegacyGameInterfaceLayer(
					"WheresMyItems: Smart Quick Stack",
					delegate
					{
						if (WheresMyItemsUI.visible)
						{
							wheresMyItemsUserInterface.Update(Main._drawInterfaceGameTime);
							wheresMyItemsUI.Draw(Main.spriteBatch);
						}
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