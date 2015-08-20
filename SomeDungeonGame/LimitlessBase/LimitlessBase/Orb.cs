using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SomeDungeonGame.Graphics;
using SomeDungeonGame.Interfaces;

namespace SomeDungeonGame
{
    public class Orb : IPositionable
    {
        private StaticGraphicsObject graphics;
        private string filePath = @"\graphics\orb.png";

        public Vector2 Position { get; set; }

        public Vector2 Size
        {
            get
            {
                return new Vector2(16f);
            }
        }

        public Orb(Vector2 position)
        {
            this.Position = position;
        }

        public void Initialize()
        {
            this.graphics.Load(filePath);
        }

        public void LoadContent()
        {
            this.graphics.LoadContent();
        }

        public void Update()
        {

        }

        public void Draw()
        {
            this.graphics.Draw(this.Position, Color.White);
        }
    }
}
