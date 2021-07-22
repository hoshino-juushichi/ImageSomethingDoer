using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageSomethingDoer
{
	// BMP画像の読み込み。
	// 次のフォーマットに対応
	// 1,4,8 bit グレースケール
	// 16 bit 565,555
	// 24 bit
	// 32 bit Alphaつき

	// BitmapFrame.Createでとの違い: 32bit BMPを読むとき、PixelFormats.Bgr32ではなくBgra32として読み込む。

	// BitmapBinaryクラスはこのサイトのコードを使用・改造している
	// https://imagingsolution.blog.fc2.com/blog-entry-254.html

	class CustomBitmapLoader
	{
		public static BitmapFrame Load(Stream fs)
		{
			BitmapBinary.BITMAPFILEHEADER bfh;
			BitmapBinary.BITMAPINFOHEADER bih;
			uint[] colorMask;
			System.Drawing.Color[] colorPal;
			byte[] bitData;
			if (!BitmapBinary.Load(fs, out bfh, out bih, out colorMask, out colorPal, out bitData))
			{
				return null;
			}

			int stride = ((bih.biWidth * bih.biBitCount + 31) / 32) * 4;

			BitmapPalette palette = null;
			if (colorPal != null)
			{
				List<System.Windows.Media.Color> colors = new List<System.Windows.Media.Color>();
				foreach (var c in colorPal)
				{
					colors.Add(item: System.Windows.Media.Color.FromRgb(c.R, c.G, c.B));
				}

				palette = new BitmapPalette(colors);
			}

			// RLE圧縮。面倒なのでサポートしない
			if (bih.biCompression == 1 ||
				bih.biCompression == 2)
			{
				return null;
			}

			PixelFormat pixelFormat = PixelFormats.Pbgra32;
			switch (bih.biBitCount)
			{
				case 32:
					pixelFormat = PixelFormats.Bgra32;
					break;
				case 24:
					pixelFormat = PixelFormats.Bgr24;
					break;
				case 16:
					pixelFormat = PixelFormats.Bgr565;
					if (bih.biCompression == 0)
					{
						pixelFormat = PixelFormats.Bgr555;
					}
					if (colorMask != null)
					{
						if (colorMask[0] == 0xf800 &&
							colorMask[1] == 0x07e0 &&
							colorMask[2] == 0x001f)
						{
							pixelFormat = PixelFormats.Bgr565;
						}
						if (colorMask[0] == 0x7c00 &&
							colorMask[1] == 0x03e0 &&
							colorMask[2] == 0x001f)
						{
							pixelFormat = PixelFormats.Bgr555;
						}
					}
					break;
				case 8:
					pixelFormat = PixelFormats.Indexed8;
					break;
				case 4:
					pixelFormat = PixelFormats.Indexed4;
					break;
				case 1:
					pixelFormat = PixelFormats.Indexed1;
					break;
				default:
					Debug.Print("未対応 BitCount:" + bih.biBitCount);
					break;
			}

			var bitmapSource = BitmapSource.Create(bih.biWidth, bih.biHeight, 96, 96, pixelFormat, palette, bitData, stride);
			return BitmapFrame.Create(bitmapSource);
		}
	}

	class BitmapBinary
	{
		//typedef struct tagBITMAPFILEHEADER {全14Byte
		//        WORD    bfType;		2Byte
		//        DWORD   bfSize;		4Byte
		//        WORD    bfReserved1;		2Byte
		//        WORD    bfReserved2;		2Byte
		//        DWORD   bfOffBits;		4Byte
		//} BITMAPFILEHEADER;
		public struct BITMAPFILEHEADER
		{
			/// <summary>
			/// ファイルタイプ
			/// </summary>
			public UInt16 bfType;
			/// <summary>
			/// ファイル全体のサイズ
			/// </summary>
			public UInt32 bfSize;
			/// <summary>
			/// 予約領域
			/// </summary>
			public UInt16 bfReserved1;
			/// <summary>
			/// 予約領域
			/// </summary>
			public UInt16 bfReserved2;
			/// <summary>
			/// ファイルの先頭から画像データまでのオフセット数（バイト数）
			/// </summary>
			public UInt32 bfOffBits;
		}

		//typedef struct tagBITMAPINFOHEADER{全40Byte
		//        DWORD      biSize;		4Byte
		//        LONG       biWidth;		4Byte
		//        LONG       biHeight;		4Byte
		//        WORD       biPlanes;		2Byte
		//        WORD       biBitCount;	2Byte
		//        DWORD      biCompression;	4Byte
		//        DWORD      biSizeImage;	4Byte
		//        LONG       biXPelsPerMeter;	4Byte
		//        LONG       biYPelsPerMeter;	4Byte
		//        DWORD      biClrUsed;		4Byte
		//        DWORD      biClrImportant;	4Byte
		//} BITMAPINFOHEADER;
		public struct BITMAPINFOHEADER
		{
			/// <summary>
			/// BITMAPINFOHEADERのサイズ (40)
			/// </summary>
			public UInt32 biSize;
			/// <summary>
			/// ビットマップの幅
			/// </summary>
			public Int32 biWidth;
			/// <summary>
			/// ビットマップの高さ
			/// </summary>
			public Int32 biHeight;
			/// <summary>
			/// プレーン数(常に1)
			/// </summary>
			public UInt16 biPlanes;
			/// <summary>
			/// 1ピクセルあたりのビット数(1,4,8,16,24,32)
			/// </summary>
			public UInt16 biBitCount;
			/// <summary>
			/// 圧縮形式
			/// </summary>
			public UInt32 biCompression;
			/// <summary>
			/// イメージのサイズ(バイト数)
			/// </summary>
			public UInt32 biSizeImage;
			/// <summary>
			/// ビットマップの水平解像度
			/// </summary>
			public Int32 biXPelsPerMeter;
			/// <summary>
			/// ビットマップの垂直解像度
			/// </summary>
			public Int32 biYPelsPerMeter;
			/// <summary>
			/// カラーパレット数
			/// </summary>
			public UInt32 biClrUsed;
			/// <summary>
			/// 重要なカラーパレットのインデックス
			/// </summary>
			public UInt32 biClrImportant;
		}

		/// <summary>
		/// ビットマップファイルをバイナリで開く
		/// </summary>
		/// <param name="fs">Stream</param>
		/// <param name="bfh">BITMAPFILEHEADER</param>
		/// <param name="bih">BITMAPINFOHEADER</param>
		/// <param name="colorPal">カラーパレット</param>
		/// <param name="bitData">画像のデータ（輝度値）</param>
		public static bool Load(
			Stream fs,
			out BITMAPFILEHEADER bfh,
			out BITMAPINFOHEADER bih,
			out uint[] colorMask,
			out System.Drawing.Color[] colorPal,
			out byte[] bitData)
		{

			int i;

			// データ読込用配列の確保
			byte[] readData = new byte[4];

			//////////////////////////////////
			//
			// BITMAPFILEHEADERの読込
			//
			//////////////////////////////////

			// bfType
			fs.Read(readData, 0, 2);
			bfh.bfType = BitConverter.ToUInt16(readData, 0);
			// bfSize
			fs.Read(readData, 0, 4);
			bfh.bfSize = BitConverter.ToUInt32(readData, 0);
			// bfReserved1
			fs.Read(readData, 0, 2);
			bfh.bfReserved1 = BitConverter.ToUInt16(readData, 0);
			// bfReserved2
			fs.Read(readData, 0, 2);
			bfh.bfReserved2 = BitConverter.ToUInt16(readData, 0);
			// bfOffBits
			fs.Read(readData, 0, 4);
			bfh.bfOffBits = BitConverter.ToUInt32(readData, 0);

			//////////////////////////////////
			//
			// BITMAPINFOHEADERの読込
			//
			//////////////////////////////////

			// biSize
			fs.Read(readData, 0, 4);
			bih.biSize = BitConverter.ToUInt32(readData, 0);
			// biWidth
			fs.Read(readData, 0, 4);
			bih.biWidth = BitConverter.ToInt32(readData, 0);
			// biHeight
			fs.Read(readData, 0, 4);
			bih.biHeight = BitConverter.ToInt32(readData, 0);
			// biPlanes
			fs.Read(readData, 0, 2);
			bih.biPlanes = BitConverter.ToUInt16(readData, 0);
			//biBitCount
			fs.Read(readData, 0, 2);
			bih.biBitCount = BitConverter.ToUInt16(readData, 0);
			// biCompression
			fs.Read(readData, 0, 4);
			bih.biCompression = BitConverter.ToUInt32(readData, 0);
			// biSizeImage
			fs.Read(readData, 0, 4);
			bih.biSizeImage = BitConverter.ToUInt32(readData, 0);
			// biXPelsPerMeter
			fs.Read(readData, 0, 4);
			bih.biXPelsPerMeter = BitConverter.ToInt32(readData, 0);
			// biYPelsPerMeter
			fs.Read(readData, 0, 4);
			bih.biYPelsPerMeter = BitConverter.ToInt32(readData, 0);
			// biClrUsed
			fs.Read(readData, 0, 4);
			bih.biClrUsed = BitConverter.ToUInt32(readData, 0);
			// biClrImportant
			fs.Read(readData, 0, 4);
			bih.biClrImportant = BitConverter.ToUInt32(readData, 0);

			if (bih.biSize == 108)
			{
				fs.Seek(108 - 40, SeekOrigin.Current);
			}
			else if (bih.biSize != 40)
			{
				// その他の形式?
				colorMask = null;
				colorPal = null;
				bitData = null;
				return false;
			}

			// Bitfield
			int bitFieldSize = 0;
			const int BI_BITFIELD = 3;
			colorMask = null;
			if (bih.biCompression == BI_BITFIELD)
			{
				colorMask = new uint[3];

				fs.Read(readData, 0, 4);
				colorMask[0] = BitConverter.ToUInt32(readData, 0);
				fs.Read(readData, 0, 4);
				colorMask[1] = BitConverter.ToUInt32(readData, 0);
				fs.Read(readData, 0, 4);
				colorMask[2] = BitConverter.ToUInt32(readData, 0);

				bitFieldSize = 12;
			}

			//////////////////////////////////
			//
			// カラーパレットの読込
			//
			//////////////////////////////////

			// カラーパレットのサイズの計算
			//   bfOffBitsからBITMAPFILEHEADERとBITMAPINFOHEADERのサイズ文を
			//   引いたのがカラーパレットのサイズ
			long palSize = 0;
			if (bfh.bfOffBits != 0)	// これが0であるデータがあるので回避
			{
				palSize = (bfh.bfOffBits - 14 - bih.biSize - bitFieldSize) / 4;
			}

			if (palSize != 0)
			{
				colorPal = new System.Drawing.Color[palSize];

				for (i = 0; i < palSize; i++)
				{
					fs.Read(readData, 0, 4);
					colorPal[i] =
						System.Drawing.Color.FromArgb(
								readData[3],
								readData[2],
								readData[1],
								readData[0]);
				}
			}
			else
			{
				colorPal = null;
			}

			//////////////////////////////////
			//
			// 画像データ（輝度値）の読込
			//
			//////////////////////////////////

			// 画像データの幅（バイト数）の計算
			int stride = ((bih.biWidth * bih.biBitCount + 31) / 32) * 4;

			//メモリの確保
			bitData = new Byte[stride * bih.biHeight];

			//画像データを画像の下側から読み込む（上下を反転させて読み込む）
			for (int j = bih.biHeight - 1; j >= 0; j--)
			{
				fs.Read(bitData, j * stride, stride);
			}

			// 正常読込
			return true;

		//////////////////////////////////
		//
		// エラー処理
		//
		//////////////////////////////////
		ErrorHandler:
			bfh.bfType = 0;
			bfh.bfSize = 0;
			bfh.bfReserved1 = 0;
			bfh.bfReserved2 = 0;
			bfh.bfOffBits = 0;

			bih.biSize = 0;
			bih.biWidth = 0;
			bih.biHeight = 0;
			bih.biPlanes = 0;
			bih.biBitCount = 0;
			bih.biCompression = 0;
			bih.biSizeImage = 0;
			bih.biXPelsPerMeter = 0;
			bih.biYPelsPerMeter = 0;
			bih.biClrUsed = 0;
			bih.biClrImportant = 0;

			colorPal = null;
			bitData = null;

			//読込失敗
			return false;
		}

		/// <summary>
		/// バイナリデータをビットマップファイルに保存する
		/// </summary>
		/// <param name="fs">ストリーム</param>
		/// <param name="width">ビットマップの幅</param>
		/// <param name="height">ビットマップの高さ</param>
		/// <param name="bitCount">ビット数</param>
		/// <param name="bitData">画像のデータ（輝度値）</param>
		public static bool Save(
			Stream fs,
			int width,
			int height,
			int bitCount,
			byte[] bitData)
		{
			//配列の有無の確認
			if (bitData == null) return false;

			// 画像データの幅（バイト数）の計算
			int stride = ((width * bitCount + 31) / 32) * 4;

			//画像データサイズの確認
			if (bitData.Length != stride * height) return false;

			//カラーパレットの個数
			UInt32 palSize;
			byte[] colorPal = null;
			if (bitCount == 8)
			{
				palSize = 256;
				//カラーパレットをバイト配列で確保
				colorPal = new byte[palSize * 4];
				for (int i = 0; i < 256; i++)
				{
					colorPal[i * 4] = (byte)i;    //B
					colorPal[i * 4 + 1] = (byte)i;    //G
					colorPal[i * 4 + 2] = (byte)i;    //R
					colorPal[i * 4 + 3] = 0;          //A
				}
			}
			else
			{
				palSize = 0;
			}

			//BITMAPFILEHEADERの作成
			BITMAPFILEHEADER bfh;
			bfh.bfType = 0x4d42;
			//bfh.bfSize = 0;
			bfh.bfReserved1 = 0;
			bfh.bfReserved2 = 0;
			bfh.bfOffBits = 14 + 40 + palSize * 4;
			bfh.bfSize = bfh.bfOffBits + (uint)(stride * height);

			//BITMAPINFOHEADERの作成
			BITMAPINFOHEADER bih;
			bih.biSize = 40;
			bih.biWidth = width;
			bih.biHeight = height;
			bih.biPlanes = 1;
			bih.biBitCount = (ushort)bitCount;
			bih.biCompression = 0;
			bih.biSizeImage = 0;
			bih.biXPelsPerMeter = 0;
			bih.biYPelsPerMeter = 0;
			bih.biClrUsed = palSize;
			bih.biClrImportant = palSize;

			//////////////////////////////////
			//
			// BITMAPFILEHEADERの書込
			//
			//////////////////////////////////

			// bfType
			fs.Write(BitConverter.GetBytes(bfh.bfType), 0, 2);
			// bfSize
			fs.Write(BitConverter.GetBytes(bfh.bfSize), 0, 4);
			// bfReserved1
			fs.Write(BitConverter.GetBytes(bfh.bfReserved1), 0, 2);
			// bfReserved2
			fs.Write(BitConverter.GetBytes(bfh.bfReserved2), 0, 2);
			// bfOffBits
			fs.Write(BitConverter.GetBytes(bfh.bfOffBits), 0, 4);

			//////////////////////////////////
			//
			// BITMAPINFOHEADERの書込
			//
			//////////////////////////////////

			// biSize
			fs.Write(BitConverter.GetBytes(bih.biSize), 0, 4);
			// biWidth
			fs.Write(BitConverter.GetBytes(bih.biWidth), 0, 4);
			// biHeight
			fs.Write(BitConverter.GetBytes(bih.biHeight), 0, 4);
			// biPlanes
			fs.Write(BitConverter.GetBytes(bih.biPlanes), 0, 2);
			//biBitCount
			fs.Write(BitConverter.GetBytes(bih.biBitCount), 0, 2);
			// biCompression
			fs.Write(BitConverter.GetBytes(bih.biCompression), 0, 4);
			// biSizeImage
			fs.Write(BitConverter.GetBytes(bih.biSizeImage), 0, 4);
			// biXPelsPerMeter
			fs.Write(BitConverter.GetBytes(bih.biXPelsPerMeter), 0, 4);
			// biYPelsPerMeter
			fs.Write(BitConverter.GetBytes(bih.biYPelsPerMeter), 0, 4);
			// biClrUsed
			fs.Write(BitConverter.GetBytes(bih.biClrUsed), 0, 4);
			// biClrImportant
			fs.Write(BitConverter.GetBytes(bih.biClrImportant), 0, 4);

			//////////////////////////////////
			//
			// カラーパレットの書込
			//
			//////////////////////////////////

			if (palSize != 0)
			{
				fs.Write(colorPal, 0, colorPal.Length);
			}

			//////////////////////////////////
			//
			// 画像データ（輝度値）の書込
			//
			//////////////////////////////////

			//画像データを画像の下側から書き込む（上下を反転させて読み込む）
			for (int j = bih.biHeight - 1; j >= 0; j--)
			{
				fs.Write(bitData, j * stride, stride);
			}

			// 正常書込
			return true;

		}
	}
}
