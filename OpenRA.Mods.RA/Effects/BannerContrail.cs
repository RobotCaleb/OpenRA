using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenRA.Graphics;
using System.Drawing;

namespace OpenRA.Mods.RA.Effects
{
	class BannerContrailInfo : ContrailInfo
	{
		public readonly string BannerText = "";

		override public object Create(ActorInitializer init) { return new BannerContrail(init.self, this); }
	}

	class BannerContrail : Contrail
	{
		public string BannerText = "Empty Banner Text";

		private int LetterWidth = 4;

		public BannerContrail(Actor self, ContrailInfo info)
			: base(self, info)
		{
			var bannerInfo = ((BannerContrailInfo)Info);
			BannerText = bannerInfo.BannerText;

			TrailLength = BannerText.Length * LetterWidth;
		}

		override public void RenderAfterWorld(WorldRenderer wr, Actor self)
		{
			if (history.Value.positions.Count == 0 || BannerText == "")
				return;
			var r = (new Random()).Next(BannerText.Length);
			for (var i = 0; i < BannerText.Length; ++i)
			{
				if (i * LetterWidth >= history.Value.positions.Count)
					break;

				var pos = history.Value.positions[history.Value.positions.Count - i * LetterWidth - 1] - Game.viewport.Location;

				//pos.Y += (float)Math.Sin(/* magic sauce here*/) * 5;

				Game.Renderer.BoldFont.DrawTextWithContrast(BannerText.Substring(i, 1),
					pos,
					Color.White,
					Color.Black,
					1);
			}
		}
	}
}
