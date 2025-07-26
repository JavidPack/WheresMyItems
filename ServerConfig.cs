using System.ComponentModel;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace WheresMyItems
{
	internal class ServerConfig : ModConfig
	{
		public static ServerConfig Instance;
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[DefaultValue(50)]
		[Range(25, 125)]
		[DrawTicks]
		[Increment(5)]
		public int ChestSearchRange { get; set; }
	}
}
