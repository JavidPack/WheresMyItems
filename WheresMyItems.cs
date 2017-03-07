using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using System.Collections.Generic;
using Terraria.DataStructures;

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

		public override void ModifyInterfaceLayers(List<MethodSequenceListItem> layers)
		{
			int vanillaInventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (vanillaInventoryLayerIndex != -1)
			{
				layers.Insert(vanillaInventoryLayerIndex + 1, new MethodSequenceListItem(
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
					null)
				);
			}
		}
	}
}
