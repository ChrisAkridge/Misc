﻿//-----------------------------------------------------------------------
// <copyright file="AnimatedGraphicsObject.cs" company="The Limitless Development Team">
//     Copyrighted under the MIT license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SomeDungeonGame.Extensions;
using SomeDungeonGame.IO;

namespace SomeDungeonGame.Graphics
{
    /// <summary>
    /// A graphics object with multiple textures that are drawn in sequence.
    /// </summary>
    public class AnimatedGraphicsObject : IGraphicsObject
    {
        /// <summary>
        /// A constant field equaling 16.67.
        /// </summary>
        private const float FrameLengthInMilliseconds = 1000f / 60f; // precision is nice

        /// <summary>
        /// Set when the Load method is called successfully.
        /// </summary>
        private bool isLoaded;

        /// <summary>
        /// Set when the LoadContent method is called successfully.
        /// </summary>
        private bool isContentLoaded;

        /// <summary>
        /// A field containing the path to the image that
        /// this object was loaded from.
        /// </summary>
        private string filePath;

        /// <summary>
        /// A field containing the path to the configuration
        /// file that this object was loaded from.
        /// </summary>
        private string configFilePath;

        /// <summary>
        /// The textures of this object.
        /// </summary>
        private List<Texture2D> textures;
        
        /// <summary>
        /// The zero-based number of textures in this object.
        /// Set to -1 until the object is loaded.
        /// </summary>
        private int frameCount = -1;

        /// <summary>
        /// How many rendered frames have been drawn since the last texture change.
        /// </summary>
        private int renderedFramesElapsed;

        /// <summary>
        /// The index of the current texture.
        /// </summary>
        private int frameIndex;

        /// <summary>
        /// The width of each texture, measured in pixels.
        /// </summary>
        private int frameWidth;

        /// <summary>
        /// The ComplexGraphicsObject that owns this object.
        /// </summary>
        private ComplexGraphicsObject cgoOwner;

        /// <summary>
        /// The field containing the animation cycle length.
        /// </summary>
        private decimal animationCycleLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatedGraphicsObject"/> class.
        /// </summary>
        public AnimatedGraphicsObject()
        {
            this.textures = new List<Texture2D>();
            this.CgoSourceRects = new List<Rectangle>();
            this.IsRunning = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the object runs through the textures.
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Gets a value indicating whether this object will run once.
        /// Run-once objects will cycle through their textures once,
        /// and then continuously draw the last texture until the
        /// Reset method is called.
        /// </summary>
        public bool IsRunOnce { get; internal set; }

        /// <summary>
        /// Gets or sets the time, measured in seconds, for the animation to play through all the frames.
        /// </summary>
        public decimal AnimationCycleLength
        {
            get
            {
                return this.animationCycleLength;
            }

            set
            {
                if (value < ((1m / 60m) * this.textures.Count))
                {
                    this.animationCycleLength = (1m / 60m) * this.textures.Count;
                }
                else
                {
                    this.animationCycleLength = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of source rectangles of the textures of this object from the complex graphic texture.
        /// This field is required by ComplexGraphicsObjects and not to be used otherwise.
        /// </summary>
        internal List<Rectangle> CgoSourceRects { get; set; }

        /// <summary>
        /// Gets the frame time, which is the number of rendered frames that
        /// each texture is drawn for.
        /// </summary>
        private int FrameTime
        {
            get
            {
                if (this.textures == null || this.textures.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return (int)(this.AnimationCycleLength * 60m) / this.textures.Count;
                }
            }
        }

        /// <summary>
        /// Loads this AnimatedGraphicsObject from the specified file path.
        /// This overload is only included to fulfill the IGraphicsObject contract.
        /// Don't call it.
        /// </summary>
        /// <param name="filePath">The file path to the image to use.</param>
        public void Load(string filePath)
        {
            throw new Exception("AnimatedGraphicsObject.Load(string): Use the overload Load(string, DataReader) instead.");
        }

        /// <summary>
        /// Loads an instance of an AnimatedGraphicsObject.
        /// </summary>
        /// <param name="filePath">The file path to the image to use.</param>
        /// <param name="config">A DataReader containing the configuration for this file.</param>
        public void Load(string filePath, DataReader config)
        {
            if (!this.isLoaded)
            {
                this.filePath = filePath;
                this.configFilePath = config.FilePath;

                if (config[0] != "[Animated]" && config[0] != "[Animated_RunOnce]")
                {
                    throw new Exception(string.Format("AnimatedGraphicsObject.Load(string, DataReader): Invalid or corrupt configuration data (expected header [Animated] or [Animated_RunOnce], got header {0})", config[0]));
                }

                Dictionary<string, string> data;
                if (config[0] == "[Animated]")
                {
                    data = config.ReadFullSection("[Animated]");
                }
                else
                {
                    data = config.ReadFullSection("[Animated_RunOnce]");
                    this.IsRunOnce = true;
                }

                this.frameWidth = int.Parse(data["FrameWidth"]);
                this.AnimationCycleLength = decimal.Parse(data["CycleLength"]);

                this.isLoaded = true;
            }
        }

        /// <summary>
        /// Loads the content for this AnimatedGraphicsObject.
        /// </summary>
        public void LoadContent()
        {
            if (this.isLoaded && !this.isContentLoaded)
            {
                Texture2D fullTexture = GraphicsManager.LoadTextureFromFile(this.filePath);
                int frameHeight = fullTexture.Height;

                if (fullTexture.Width % this.frameWidth != 0)
                {
                    throw new Exception("AnimatedGraphicsObject.LoadContent(): The specified frame width for this texture is invalid.");
                }

                for (int x = 0; x < fullTexture.Width; x += this.frameWidth)
                {
                    this.textures.Add(GraphicsManager.Crop(fullTexture, new Rectangle(x, 0, this.frameWidth, frameHeight)));
                    this.frameCount++;
                }

                this.isContentLoaded = true;
            }
        }

        /// <summary>
        /// Updates this AnimatedGraphicsObject.
        /// </summary>
        public void Update()
        {
            if (this.IsRunning)
            {
                this.renderedFramesElapsed++;
                if (this.renderedFramesElapsed == this.FrameTime)
                {
                    if (this.frameIndex == this.frameCount)
                    {
                        if (this.IsRunOnce)
                        {
                            this.IsRunning = false;
                            return;
                        }
                        else
                        {
                            this.frameIndex = 0;
                        }
                    }
                    else
                    {
                        this.frameIndex++;
                    }

                    this.renderedFramesElapsed = 0;
                }
            }
        }

        /// <summary>
        /// Draws this AnimatedGraphicsObject to the screen.
        /// </summary>
        /// <param name="position">The position to draw this object at.</param>
        /// <param name="color">The color to shade this object. Use Color.White for no shading.</param>
        public void Draw(Vector2 position, Color color)
        {
            GameServices.SpriteBatch.Draw(this.textures[this.frameIndex], position, color);
        }

        /// <summary>
        /// Draws this AnimatedGraphicsObject to the screen.
        /// </summary>
        /// <param name="position">The position to draw this object at.</param>
        /// <param name="color">The color to shade this object. Use Color.White for no shading.</param>
        /// <param name="spriteEffects">How to mirror this object.</param>
        public void Draw(Vector2 position, Color color, SpriteEffects spriteEffects)
        {
            GameServices.SpriteBatch.Draw(this.textures[this.frameIndex], position, color, spriteEffects);
        }

        /// <summary>
        /// Draws this AnimatedGraphicsObject to the screen.
        /// </summary>
        /// <param name="position">The position to draw this object at.</param>
        /// <param name="color">The color to shade this object. Use Color.White for no shading.</param>
        /// <param name="debug">If true, the frame index will be drawn in the top-left corner of the sprite.</param>
        public void Draw(Vector2 position, Color color, bool debug)
        {
            this.Draw(position, color);
            if (debug)
            {
                GameServices.DebugFont.DrawString(this.frameIndex.ToString(), position);
            }
        }

        /// <summary>
        /// Draws this AnimatedGraphicsObject to the screen.
        /// </summary>
        /// <param name="position">The position to draw this object at.</param>
        /// <param name="color">The color to shade this object. Use Color.White for no shading.</param>
        /// <param name="spriteEffects">How to mirror this object.</param>
        /// /// <param name="debug">If true, the frame index will be drawn in the top-left corner of the sprite.</param>
        public void Draw(Vector2 position, Color color, SpriteEffects spriteEffects, bool debug)
        {
            this.Draw(position, color, spriteEffects);
            if (debug)
            {
                GameServices.DebugFont.DrawString(this.frameIndex.ToString(), position);
            }
        }

        /// <summary>
        /// Adjusts the time it takes for this animated object
        /// to complete one loop through its frames.
        /// </summary>
        /// <param name="newCycleLength">The time, in seconds, each loop takes.</param>
        public void SetSpeed(decimal newCycleLength)
        {
            this.AnimationCycleLength = newCycleLength;
            this.renderedFramesElapsed = 0;
        }

        /// <summary>
        /// Adjusts how many rendered frames each
        /// frame of this object is shown for.
        /// </summary>
        /// <param name="newFrameTime">How many rendered frames each frame is shown for.</param>
        public void SetSpeed(int newFrameTime)
        {
            this.AnimationCycleLength = 60m / this.textures.Count;
            this.renderedFramesElapsed = 0;
        }

        /// <summary>
        /// Adjusts the speed of the animation of this
        /// object by percentage. Rounded to the closest
        /// frame boundary (usually one-sixtieth of a second).
        /// </summary>
        /// <param name="percentage">The percentage by which to adjust the animation speed.</param>
        public void AdjustSpeed(float percentage)
        {
            percentage /= 100f;
            decimal addend = this.AnimationCycleLength * (decimal)percentage;
            this.AnimationCycleLength += addend;
            this.renderedFramesElapsed = 0;

            // Round it to the nearest frame boundary if necessary.
            if ((this.AnimationCycleLength * 60m) % 1 != 0)
            {
                decimal cycleInFrames = this.AnimationCycleLength * 60m;
                if (percentage > 0f) 
                {
                    // If we're slowing down
                    cycleInFrames = NumericExtensions.RoundUp(cycleInFrames);
                }
                else if (percentage < 0f) 
                {
                    // If we're speeding up
                    cycleInFrames = NumericExtensions.RoundDown(cycleInFrames);
                }
                else
                {
                    // somehow we're zero
                    return;
                }

                this.AnimationCycleLength = cycleInFrames / 60m;
            }
        }

        /// <summary>
        /// Resets this AnimatedGraphicsObject.
        /// The texture index becomes 0, and
        /// the rendered frame count also becomes 0.
        /// </summary>
        /// <param name="startRunning">If true, the object will restart.</param>
        public void Reset(bool startRunning)
        {
            this.renderedFramesElapsed = 0;
            this.frameIndex = 0;
            if (startRunning)
            {
                this.IsRunning = true;
            }
        }

        /// <summary>
        /// Returns a deep copy of this object.
        /// The texture is not cloned, but everything else is.
        /// </summary>
        /// <returns>A deep copy of this object.</returns>
        public IGraphicsObject Clone()
        {
            var clone = new AnimatedGraphicsObject();
            clone.filePath = this.filePath;
            clone.configFilePath = this.configFilePath;
            clone.textures = this.textures;
            clone.frameCount = this.frameCount;
            clone.frameWidth = this.frameWidth;
            clone.AnimationCycleLength = this.AnimationCycleLength;
            clone.IsRunOnce = this.IsRunOnce;
            clone.isLoaded = this.isLoaded;
            clone.isContentLoaded = this.isContentLoaded;
            return clone;
        }

        /// <summary>
        /// Loads an AnimatedGraphicsObjects from a configuration section in a ComplexGraphicsObject.
        /// </summary>
        /// <param name="section">The section from the CGO configuration that specifies this object.</param>
        /// <param name="owner">The CGO that owns this object.</param>
        internal void Load(Dictionary<string, string> section, ComplexGraphicsObject owner)
        {
            if (!this.isLoaded)
            {
                int frames = int.Parse(section["Frames"]);
                Vector2 frameSize = owner.FrameSize;
                this.filePath = owner.FilePath;
                for (int i = 0; i < frames; i++)
                {
                    this.CgoSourceRects.Add(Vector2Extensions.Parse(section[string.Concat("Frame", i)]).ToRectangle(frameSize));
                }

                if (section["Type"] == "animated_runonce")
                {
                    this.IsRunOnce = true;
                }

                this.AnimationCycleLength = decimal.Parse(section["CycleLength"]);
                this.cgoOwner = owner;
                this.frameCount = frames - 1;
                this.isLoaded = true;
            }
        }

        /// <summary>
        /// Loads the content for this AnimatedGraphicsObject.
        /// </summary>
        /// <param name="fileTexture">The texture of the ComplexGraphicsObject to take the textures from.</param>
        internal void LoadContentCGO(Texture2D fileTexture)
        {
            if (this.isLoaded && !this.isContentLoaded && this.CgoSourceRects.Any())
            {
                foreach (Rectangle sourceRect in this.CgoSourceRects)
                {
                    this.textures.Add(fileTexture.Crop(sourceRect));
                }

                this.isContentLoaded = true;
            }
        }

        /// <summary>
        /// Returns the size, in pixels, of the frames of this object.
        /// </summary>
        /// <returns>The size of the object.</returns>
        public Vector2 GetSize()
        {
            if (this.isContentLoaded)
            {
                return new Vector2(this.textures[0].Width, this.textures[0].Height);
            }
            else
            {
                throw new Exception("AnimatedGraphicsObject.GetSize(): This object hasn't been fully loaded, and cannot return its size.");
            }
        }
    }
}
