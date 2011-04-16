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
			BannerText = "   " + bannerInfo.BannerText;

			TrailLength = BannerText.Length * LetterWidth;
		}

		override public void RenderAfterWorld(WorldRenderer wr, Actor self)
		{
			if (history.Value.positions.Count == 0 || BannerText == "")
				return;

			var extents = Game.Renderer.BoldFont.Measure("p").ToFloat2();
			for (var i = 0; i < history.Value.positions.Count; ++i)
			{
				var pos = history.Value.positions[i];

				pos.Y += (float)Math.Sin(Math.PI / 3f * i + Game.LocalTick * 0.5f) * 1f + 2f;

				Game.Renderer.LineRenderer.FillRect(
					new RectangleF(pos.X, pos.Y, extents.X, extents.Y),
					Color.White);

			}
			for (var i = 0; i < BannerText.Length; ++i)
			{
				if (i * LetterWidth >= history.Value.positions.Count)
					break;

				var pos = history.Value.positions[history.Value.positions.Count - i * LetterWidth - 1] - Game.viewport.Location;

				pos.Y += (float)Math.Sin(Math.PI / 3f * i + Game.LocalTick * 0.5f) * 1f;

				var l = BannerText.Substring(i, 1);

				Game.Renderer.BoldFont.DrawText(l, pos, Color.Black);
			}
		}
	}
}
