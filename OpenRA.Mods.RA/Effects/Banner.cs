#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Drawing;
using OpenRA.FileFormats;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.RA
{
	class BannerInfo : ITraitInfo
	{
		public readonly int[] BannerOffset = {0, 0};

		public readonly bool UsePlayerColor = true;

		public readonly Color BannerColor = Color.White;

		public readonly string BannerText = "Empty Banner Text";

		public object Create(ActorInitializer init) { return new Banner(init.self, this); }
	}

	class Banner : ITick, IPostRender
	{
		private BannerInfo Info = null;

		private List<float2> LetterPositions = new List<float2>();
		private List<float2> LetterBounds = new List<float2>();

		private Turret BannerTurret = null;
		private Color BannerColor = Color.White;
		private float2 BannerPosition;

		private string BannerText = "Empty Banner Text";

		private List<BannerSpring> BannerRope = new List<BannerSpring>();
		private List<BannerLetter> BannerLetters = new List<BannerLetter>();

		public Banner(Actor self, BannerInfo info)
		{
			var facing = self.Trait<IFacing>();

			Info = info;

			BannerText = Info.BannerText;

			BannerTurret = new Turret(Info.BannerOffset);

			if (Info.UsePlayerColor)
			{
				var ownerColor = Color.FromArgb(255, self.Owner.ColorRamp.GetColor(0));
				BannerColor = PlayerColorRemap.ColorLerp(0.5f, ownerColor, Color.White);
			}
			else
			{
				BannerColor = Info.BannerColor;
			}

			BannerPosition = self.CenterLocation - Combat.GetTurretPosition(self, facing, BannerTurret);
			
			for (int i = 0; i < BannerText.Length; ++i)
			{
				var l = BannerText.Substring(i, 1);
				var letter = new BannerLetter(l,
					Game.Renderer.BoldFont.Measure(l).ToFloat2());
				letter.pos = BannerPosition;

				BannerLetters.Add(letter);
			}

			for (int i = 0; i < BannerLetters.Count - 1; ++i)
			{
				var letter1 = BannerLetters[i];
				var letter2 = BannerLetters[i + 1];

				var spring = new BannerSpring(letter1, letter2);

				BannerRope.Add(spring);
			}
		}

		private bool FirstTick = true;
		public void Tick(Actor self)
		{
			var facing = self.Trait<IFacing>();
			var altitude = new float2(0, self.Trait<IMove>().Altitude);
			var lastPosition = BannerPosition;

			BannerPosition = self.CenterLocation - Combat.GetTurretPosition(self, facing, BannerTurret) - altitude;

			if (FirstTick)
			{
				for (int i = 0; i < BannerLetters.Count; ++i)
				{
					var letter = BannerLetters[i];
					letter.pos = BannerPosition;
				}
				FirstTick = false;
			}

			RopeSolve(BannerPosition - lastPosition);
		}

		private void RopeSolve(float2 vel)
		{
			foreach (var s in BannerRope)
			{
				s.step();
			}

			foreach (var l in BannerLetters)
			{
				l.tick();
			}

			BannerRope[0].letter1.velocity = vel;
			BannerRope[0].letter1.pos = BannerPosition;
		}

		public void RenderAfterWorld(WorldRenderer wr, Actor self)
		{
			foreach (var l in BannerLetters)
			{
				if (self.World.LocalShroud.IsVisible(OpenRA.Traits.Util.CellContaining(l.pos)))
				{
					Game.Renderer.BoldFont.DrawTextWithContrast(
						l.letter,
						l.pos - Game.viewport.Location,
						Color.White,
						Color.Black,
						1);
				}
			}
		}
	}
	
	class BannerLetter
	{
		public string letter;
		public float2 bounds;
		public float2 pos;
		public float2 velocity;
		public float2 force;

		public float mass = 14.15f;

		public BannerLetter(char l, float2 b)
		{
			letter = l.ToString();
			bounds = b;
		}

		public BannerLetter(string l, float2 b)
		{
			letter = l;
			bounds = b;
		}

		public void reset()
		{
			force = new float2(0f, 0f);
		}

		public void applyForce(float2 f)
		{
			force += f;
		}

		public void tick()
		{
			var dt = 40f / 1000f; // Game.Settings.Game.Timestep;

			velocity += (force / new float2(mass, mass)) * dt;

			pos += velocity * dt;
		}
	}

	class BannerSpring
	{
		public BannerLetter letter1;
		public BannerLetter letter2;

		private float springK = 1.15f;
		private float friction = 1.2f;
		private float springL = 0.55f;

		public BannerSpring(BannerLetter l1, BannerLetter l2)
		{
			letter1 = l1;
			letter2 = l2;
		}

		public void step()
		{
			float2 connection = letter1.pos - letter2.pos;
			float dist = connection.Length;

			float2 force = new float2(0f, 0f);

			if (dist != 0)
			{
				force += -(connection / new float2(dist, dist)) * (dist - springL) * springK;
				force += -(letter1.velocity - letter2.velocity) * friction;

				letter1.applyForce(force);
				letter2.applyForce(-force);
			}
		}
	}
}
