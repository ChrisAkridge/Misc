using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SomeDungeonGame.Interfaces;

namespace SomeDungeonGame.Entities
{
	public abstract class Tile : IPositionable
	{
		public abstract int ID { get; }
		public string GraphicsPath { get; protected set; }

		public Vector2 Position { get; set; }
		public Vector2 Size { get; protected set; }

		public bool IsSolid { get; set; }
		public bool IsVisible { get; set; }

		public abstract void LoadContent();
		public abstract void Update();
		public abstract void Draw();
		public abstract void UnloadContent();
	}
}
