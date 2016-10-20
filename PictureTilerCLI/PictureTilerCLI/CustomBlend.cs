using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessorCore;

namespace PictureTilerCLI
{
	internal static class CustomBlend
	{
		public static void Blend(Image canvas, Image<Color, uint> image, Point destPoint)
		{
			using (PixelAccessor<Color, uint> canvasAccessor = canvas.Lock())
			using (PixelAccessor<Color, uint> imageAccessor = image.Lock())
			{
				//Parallel.For(0, image.Height,
				//y =>
				//{
				//	int offsetY = y + destPoint.Y;
				//	for (int x = 0; x < image.Width; x++)
				//	{
				//		int offsetX = x + destPoint.X;
				//		canvasAccessor[offsetX, offsetY] = imageAccessor[x, y];
				//	}
				//});

				for (int y = 0; y < image.Height; y++)
				{
					int posY = destPoint.Y + y;
					for (int x = 0; x < image.Width; x++)
					{
						int posX = destPoint.X + x;
						canvasAccessor[posX, posY] = imageAccessor[x, y];
					}
				}
			}
		}
	}
}
