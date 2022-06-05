using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageSomethingDoer
{
	public static class TgaLoader
	{
        public static TgaImage FromStream(Stream stream)
        {
			var image = new TgaImage();
			try
			{
				ReadHeader(image.FileHeader, stream);

				if (image.FileHeader.IdLength != 0)
				{
					var idBuffer = new byte[image.FileHeader.IdLength];
					var amount = stream.Read(idBuffer, 0, image.FileHeader.IdLength);
					image.ImageID = Encoding.Unicode.GetString(idBuffer, 0, amount);
				}

				if (image.FileHeader.ColorMapType == TgaConst.ColorMapType.ColorMap)
				{
					int colorMapBytes = image.FileHeader.ColorMapDepthBits / 8;
					var colorMapBuffer = new byte[image.FileHeader.ColorMapLength * colorMapBytes];
					stream.Read(colorMapBuffer, image.FileHeader.ColorMapOrigin * colorMapBytes, (image.FileHeader.ColorMapLength - image.FileHeader.ColorMapOrigin) * colorMapBytes);
					image.Palette = CreateBitmapPalette(colorMapBuffer, colorMapBytes, image.FileHeader.ColorMapLength);
				}

				image.Width = image.FileHeader.Width;
				image.Height = image.FileHeader.Height;

				if (image.FileHeader.ImageType == TgaConst.ImageType.RLEColorMappedImage ||
					image.FileHeader.ImageType == TgaConst.ImageType.ColorMappedImage)
				{
					image.PixelFormat = PixelFormats.Indexed8;
				}
				else if (image.FileHeader.ImageType == TgaConst.ImageType.RLEBlackAndWhiteImage ||
						 image.FileHeader.ImageType == TgaConst.ImageType.BlackAndWhiteImage)
				{
					image.PixelFormat = PixelFormats.Gray8;
				}
				else if (image.FileHeader.ImageType == TgaConst.ImageType.RLETrueColorImage ||
						image.FileHeader.ImageType == TgaConst.ImageType.TrueColorImage)
				{
					if (image.FileHeader.PixelDepthBits == 16)
					{
						image.PixelFormat = PixelFormats.Bgr555;
					}
					else if (image.FileHeader.PixelDepthBits == 24)
					{
						image.PixelFormat = PixelFormats.Bgr24;
					}
					else if (image.FileHeader.PixelDepthBits == 32)
					{
						image.PixelFormat = PixelFormats.Bgra32;
					}
				}
				if (image.PixelFormat == default)
				{
					// 未対応フォーマット
					image.PixelFormat = PixelFormats.Bgra32;
					image.Stride = image.FileHeader.Width * 4;
					image.Data = new byte[image.Stride * image.Height];
					return image;
				}

				int pixelBytes = image.FileHeader.PixelDepthBits / 8;
				image.Stride = image.FileHeader.Width * pixelBytes;
				image.Data = LoadImage(stream, image.Width, image.Height, image.Stride, pixelBytes, image.FileHeader.ImageType);

				if (image.FileHeader.Orientation != TgaConst.Orientation.BottomLeft)
				{
					image.Data = CreateOrientationChangedImage(image.FileHeader.Orientation, image.Data, image.Width, image.Height, image.Stride, pixelBytes);
				}
			}
			catch
			{
			}
			return image;
        }

		private static void ReadHeader(TgaFileHeader fileHeader, Stream stream)
		{
			var buffer = new byte[4];

			stream.Read(buffer, 0, 1);
			fileHeader.IdLength = buffer[0];

			stream.Read(buffer, 0, 1);
			fileHeader.ColorMapType = (TgaConst.ColorMapType)buffer[0];

			stream.Read(buffer, 0, 1);
			fileHeader.ImageType = (TgaConst.ImageType)buffer[0];

			stream.Read(buffer, 0, 2);
			fileHeader.ColorMapOrigin = BitConverter.ToUInt16(buffer, 0);

			stream.Read(buffer, 0, 2);
			fileHeader.ColorMapLength = BitConverter.ToUInt16(buffer, 0);

			stream.Read(buffer, 0, 1);
			fileHeader.ColorMapDepthBits = buffer[0];

			stream.Read(buffer, 0, 2);
			fileHeader.ImageXOrigin = BitConverter.ToUInt16(buffer, 0);

			stream.Read(buffer, 0, 2);
			fileHeader.ImageYOrigin = BitConverter.ToUInt16(buffer, 0);

			stream.Read(buffer, 0, 2);
			fileHeader.Width = BitConverter.ToUInt16(buffer, 0);

			stream.Read(buffer, 0, 2);
			fileHeader.Height = BitConverter.ToUInt16(buffer, 0);

			stream.Read(buffer, 0, 1);
			fileHeader.PixelDepthBits = buffer[0];

			stream.Read(buffer, 0, 1);
			fileHeader.DescriptorBits = buffer[0];
		}

		private static BitmapPalette CreateBitmapPalette(byte[] colorMapBuffer, int colorMapBytes, int colorMapLength)
		{
			var colors = new List<Color>();
			int colorMapTail = (colorMapLength < 256) ? colorMapLength : 256;
			for (int i = 0; i < colorMapTail; ++i)
			{
				var color = new Color();
				if (colorMapBytes == 1)
				{
					color.B =
					color.G =
					color.R = colorMapBuffer[i];
					color.A = 255;
				}
				else if (colorMapBytes == 2)
				{
					byte b0 = colorMapBuffer[i * 2 + 0];
					byte b1 = colorMapBuffer[i * 2 + 1];
					color.B = (byte)(255 * (b0 & 0x1f) / 0x1f);
					color.G = (byte)(255 * (((b1 & 0x03) << 3) + (b0 >> 5)) / 0x1f);
					color.R = (byte)(255 * ((b1 >> 2) & 0x1f) / 0x1f);
					color.A = 255;
				}
				else if (colorMapBytes == 3)
				{
					color.B = colorMapBuffer[i * 3 + 0];
					color.G = colorMapBuffer[i * 3 + 1];
					color.R = colorMapBuffer[i * 3 + 2];
					color.A = 255;
				}
				else if (colorMapBytes == 4) // これありえる?
				{
					color.B = colorMapBuffer[i * 4 + 0];
					color.G = colorMapBuffer[i * 4 + 1];
					color.R = colorMapBuffer[i * 4 + 2];
					color.A = colorMapBuffer[i * 4 + 3];
				}
				colors.Add(color);
			}
			return new BitmapPalette(colors);
		}

		private static byte[] LoadImage(Stream stream, int imageWidth, int imageHeight, int imageStride, int pixelBytes, TgaConst.ImageType imageType)
		{
			var imageData = new byte[imageStride * imageHeight];

			bool rle = (imageType == TgaConst.ImageType.RLEColorMappedImage ||
						imageType == TgaConst.ImageType.RLETrueColorImage ||
						imageType == TgaConst.ImageType.RLEBlackAndWhiteImage);
			if (!rle)
			{
				for (int y = imageHeight - 1; y >= 0; --y)
				{
					stream.Read(imageData, y * imageStride, imageStride);
				}
			}
			else
			{
				int remainSize = (int)(stream.Length - stream.Position);
				var imageBuffer = new byte[remainSize];
				stream.Read(imageBuffer, 0, remainSize);

				int x = 0;
				int y = 0;
				int ptrSrc = 0;
				int ptrDst = (imageHeight - 1 - y) * imageStride;

				while (remainSize > 0)
				{
					uint cb = imageBuffer[ptrSrc++];
					remainSize--;
					uint cnt = (cb & 0x7f) + 1;

					if ((cb & 0x80) != 0)
					{
						// 連続
						while (cnt != 0)
						{
							for (int i = 0; i < pixelBytes; ++i)
							{
								imageData[ptrDst + i] = imageBuffer[ptrSrc + i];
							}
							ptrDst += pixelBytes;
							x += 1;
							if (x >= imageWidth)
							{
								if (++y >= imageHeight)
								{
									break;
								}
								x = 0;
								ptrDst = (imageHeight - 1 - y) * imageStride;
							}
							--cnt;
						}
						ptrSrc += pixelBytes;
						remainSize -= pixelBytes;
					}
					else
					{
						// リテラル
						while (cnt != 0)
						{
							for (int i = 0; i < pixelBytes; ++i)
							{
								imageData[ptrDst + i] = imageBuffer[ptrSrc + i];
							}
							ptrDst += pixelBytes;
							x += 1;
							ptrSrc += pixelBytes;
							if (x >= imageWidth)
							{
								if (++y >= imageHeight)
								{
									break;
								}
								x = 0;
								ptrDst = (imageHeight - 1 - y) * imageStride;
							}
							--cnt;
						}
						remainSize -= (int)(cnt * pixelBytes);
					}
					if (y >= imageHeight)
					{
						break;
					}
				}
			}
			return imageData;
		}

		private static byte[] CreateOrientationChangedImage(TgaConst.Orientation orientation, byte[] sourceImageData, int imageWidth, int imageHeight, int imageStride, int pixelBytes )
		{
			var nextImageData = new byte[imageStride * imageHeight];

			int startY = 0;
			int ddy = 1;
			if (orientation == TgaConst.Orientation.TopLeft ||
				orientation == TgaConst.Orientation.TopRight)
			{
				startY = imageHeight - 1;
				ddy = -1;
			}
			int startX = 0;
			int ddx = 1;
			if (orientation == TgaConst.Orientation.BottomRight ||
				orientation == TgaConst.Orientation.TopRight)
			{
				startX = imageWidth - 1;
				ddx = -1;
			}

			int dy = startY;
			for (int sy = 0; sy < imageHeight; ++sy)
			{
				int dx = startX;
				for (int sx = 0; sx < imageWidth; ++sx)
				{
					for (int p = 0; p < pixelBytes; ++p)
					{
						nextImageData[dy * imageStride + dx * pixelBytes + p] = sourceImageData[sy * imageStride + sx * pixelBytes + p];
					}
					dx += ddx;
				}
				dy += ddy;
			}
			return nextImageData;
		}
	}

	public static class TgaConst
	{
		//public const byte IDB_ATTRIBUTES = 0x0f;// How many attrib bits per pixel ie: 1 for T16, 8 for T32
		//public const byte IDB_ORIGIN = 0x20;    // Origin in top left corner bit else its in bottom left corner
		//public const byte IDB_INTERLEAVE = 0xc0;// Interleave bits as defined below
		//public const byte IDB_NON_INT = 0x00;   // Non-Interlaced
		//public const byte IDB_2_WAY = 0x40;     // 2 way ( even/odd ) interleaving
		//public const byte IDB_4_WAY = 0x80;     // 4 way interleaving ( eg: AT&T PC )
		//public const byte IDB___ = 0x08;        // ?
		public enum ColorMapType : byte
		{
			NoColorMap = 0,
			ColorMap = 1
		};
		public enum ImageType : byte
		{
			NoImage = 0,				// No image data included in file
			ColorMappedImage = 1,		// Uncompressed, Color-Mapped ( VDA/D  and Targa M-8 images )
			TrueColorImage = 2,         // Uncompressed, RGB images ( eg: ICB Targa 16, 24 and 32 )
			BlackAndWhiteImage = 3,		// Uncompressed, B/W images ( eg: Targa 8 and Targa M-8 images )
			RLEColorMappedImage = 9,    // Run-length, Color-Mapped ( VDA/D and Targa M-8 images )
			RLETrueColorImage = 10,     // Run-length, RGB images ( eg: ICB Targa 16, 24 and 32 )
			RLEBlackAndWhiteImage = 11, // Run-length, B/W images ( eg: Targa 8 and Targa M-8 )
			CompressedColorMappedImage = 32,    // Compressed Color-Mapped ( VDA/D ) data using Huffman, Delta, and run length encoding   
			CompressedColorMappedImage_4 = 33,  // Compressed Color-Mapped ( VDA/D ) data using Huffman, Delta, and run length encoding in 4 passes
		};
		public enum Orientation
		{
			BottomLeft = 0,
			BottomRight = 1,
			TopLeft = 2,
			TopRight = 3
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public class TgaFileHeader
	{
		public byte IdLength;
		public TgaConst.ColorMapType ColorMapType;
		public TgaConst.ImageType ImageType;
		public ushort ColorMapOrigin;
		public ushort ColorMapLength;
		public byte ColorMapDepthBits;
		public ushort ImageXOrigin;
		public ushort ImageYOrigin;
		public ushort Width;
		public ushort Height;
		public byte PixelDepthBits;
		public byte DescriptorBits;

		public TgaConst.Orientation Orientation => (TgaConst.Orientation)(DescriptorBits >> 4);
	}

	public class TgaImage : IDisposable
	{
		public int Width;
		public int Height;
		public int Stride;
		public PixelFormat PixelFormat = default;
		public TgaFileHeader FileHeader = new TgaFileHeader();
		public string ImageID = string.Empty;
		public BitmapPalette? Palette;
		public byte[]? Data;

		public void Dispose()
		{
			Palette = null;
			Data = null;
		}
	}
}