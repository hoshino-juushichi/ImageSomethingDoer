using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ImageSomethingDoer
{
    public class ImageSomethingDoerModel
    {
        private WriteableBitmap _contentImage;
        public WriteableBitmap ContentImage => _contentImage;

        private WriteableBitmap _finalImage;
        public WriteableBitmap FinalImage => _finalImage;

        public class ErrorInformation
        {
            public bool HasError;
            public string Message;
        }
        private ErrorInformation _lastError = new ErrorInformation();
        public bool HasError => _lastError.HasError;
        public string ErrorMessage => _lastError.Message;

        public float ImageScale { get; set; } = 1.0f;

        private ImageScalingType _imageScalingType = ImageScalingType.X1;
        public ImageScalingType ImageScalingType
        { 
            get => _imageScalingType;
            set
            {
                _imageScalingType = value;
                switch (_imageScalingType)
                {
                    case ImageScalingType.X1: ImageScale = 1.0f; break;
                    case ImageScalingType.X2: ImageScale = 2.0f; break;
                    case ImageScalingType.X4: ImageScale = 4.0f; break;
                    case ImageScalingType.X8: ImageScale = 8.0f; break;
                    case ImageScalingType.X16: ImageScale = 16.0f; break;
                    case ImageScalingType.X32: ImageScale = 32.0f; break;
                    case ImageScalingType.IX2: ImageScale = 0.5f; break;
                    case ImageScalingType.IX4: ImageScale = 0.25f; break;
                    case ImageScalingType.IX8: ImageScale = 0.125f; break;
                    case ImageScalingType.IX16: ImageScale = 0.0625f; break;
                    case ImageScalingType.IX32: ImageScale = 0.03125f; break;
                }
            }
        }

        private BackgroundColorType _backgroundColorType = BackgroundColorType.Checker;
        public BackgroundColorType BackgroundColorType
        {
            get => _backgroundColorType;
            set
            {
                _backgroundColorType = value;
                switch (BackgroundColorType)
                {
                    case BackgroundColorType.Black: _backgroundColorValue = Color.FromRgb(0, 0, 0); break;
                    case BackgroundColorType.White: _backgroundColorValue = Color.FromRgb(255, 255, 255); break;
                    case BackgroundColorType.Red: _backgroundColorValue = Color.FromRgb(255, 0, 0); break;
                    case BackgroundColorType.Green: _backgroundColorValue = Color.FromRgb(0, 255, 0); break;
                    case BackgroundColorType.Blue: _backgroundColorValue = Color.FromRgb(0, 0, 255); break;
                }
            }
        }
        private Color _backgroundColorValue = Color.FromRgb(0, 0, 0);
        public Color BackgroundColorValue => _backgroundColorValue;

        public int ColorMask { get; set; } = 0;
        public bool DisplayAlpha { get; set; } = false;

        public ImageSomethingDoerModel()
        {
            DisposeImage();
        }

        public void DisposeImage()
        {
            _contentImage = null;
            _finalImage = null;
        }

        public bool IsColorMasked(ImageChannel channel)
        {
            return (ColorMask & (1 << (int)channel)) != 0;
        }

        public bool IsImageLoaded()
        {
            return _contentImage != null;
        }

        private void ClearErrorInfo()
        {
            _lastError.HasError = false;
            _lastError.Message = "No Error";
        }

        private void SetErrorInfo(string message)
        {
            _lastError.HasError = true;
            _lastError.Message = message;
        }

		private void AddErrorMessage(string message)
		{
			_lastError.Message += message;
		}

		public bool LoadImage(string fileName)
        {
            ClearErrorInfo();
            string fullPath = System.IO.Path.GetFullPath(fileName);
            using (Stream fs_read = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
				BitmapFrame bf = null;
				PngBitmapDecoder decoder = null;

				string ext = System.IO.Path.GetExtension(fullPath);
                if (ext.ToLower() == ".bmp")
                {
					int hr = 0;
					string message = Resources.EnumResource.ModelError_Unsupported;
					try
					{
						bf = CustomBitmapLoader.Load(fs_read);
					}
					catch (Exception e)
					{
						hr = e.HResult;
						message = e.Message;
					}
					// bfがnullでもこの後の処理に任せる
				}
				else if (ext.ToLower() == ".png")
                {
					int hr = 0;
					string message = Resources.EnumResource.ModelError_Unsupported;
					try
					{
						decoder = new PngBitmapDecoder(fs_read, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
						if (decoder.Frames[0].Format.BitsPerPixel != 32 &&
							decoder.Frames[0].Format.BitsPerPixel != 24 &&
							decoder.Frames[0].Format.BitsPerPixel != 8 &&
							decoder.Frames[0].Format.BitsPerPixel != 1)
						{
							SetErrorInfo(Resources.EnumResource.ModelError_MainImageBppSupport);
							return false;
						}
						bf = decoder.Frames[0];
					}

					catch (Exception e)
					{
						hr = e.HResult;
						message = e.Message;
					}
					if (bf == null)
					{
						SetErrorInfo(message);
						AddErrorMessage("\n" + string.Format("0x{0:x}", hr));
						return false;
					}
				}
				else if (ext.ToLower() == ".tga" ||
                        ext.ToLower() == ".dds")
                {
                    // TGA & DDS by Pfim
                    // https://github.com/nickbabcock/Pfim

                    int hr = 0;
                    string message = Resources.EnumResource.ModelError_Unsupported;
                    try
                    {
                        using (var image = Pfim.Pfim.FromStream(fs_read))
                        {
                            PixelFormat pixelFormat_;
                            switch (image.Format)
                            {
                                case Pfim.ImageFormat.Rgba32:
                                    pixelFormat_ = PixelFormats.Bgra32;
                                    break;
                                case Pfim.ImageFormat.Rgb24:
                                    pixelFormat_ = PixelFormats.Bgr24;
                                    break;
                                case Pfim.ImageFormat.R5g6b5:
                                    pixelFormat_ = PixelFormats.Bgr565;
                                    break;
                                case Pfim.ImageFormat.R5g5b5a1:
                                case Pfim.ImageFormat.R5g5b5:
                                    pixelFormat_ = PixelFormats.Bgr555;
                                    break;
                                case Pfim.ImageFormat.Rgb8: // 232
                                case Pfim.ImageFormat.Rgba16: //4444
                                default:
                                    throw new NotImplementedException();
                            }

                            var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                            try
                            {
                                IntPtr bitData = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                                int bufferSize = image.Height * image.Stride;
                                var bitmapSource = BitmapSource.Create(image.Width, image.Height, 96, 96, pixelFormat_, null, bitData, bufferSize, image.Stride);
                                bf = BitmapFrame.Create(bitmapSource);

                            }
                            finally
                            {
                                handle.Free();
                            }
                        }
                    }

                    catch (Exception e)
                    {
                        hr = e.HResult;
                        message = e.Message;
                    }
                    if (bf == null)
                    {
                        SetErrorInfo(message);
                        AddErrorMessage("\n" + string.Format("0x{0:x}", hr));
                        return false;
                    }
                }

                if (bf == null)
				{
					int hr = 0;
                    string message = Resources.EnumResource.ModelError_Unsupported;
					try
					{
						fs_read.Seek(0, SeekOrigin.Begin);
						bf = BitmapFrame.Create(fs_read, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
					}
					catch (Exception e)
					{
						hr = e.HResult;
                        message = e.Message;
					}
					if (bf == null)
					{
						SetErrorInfo(message);
                        AddErrorMessage("\n" + string.Format("0x{0:x}", hr));
						return false;
					}
				}

				PixelFormat pixelFormat = PixelFormats.Bgra32;
                FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(bf, pixelFormat, null, 0);
                int w = convertedBitmap.PixelWidth;
                int h = convertedBitmap.PixelHeight;
                int stride = (w * pixelFormat.BitsPerPixel + 7) / 8;
                byte[] pixels = new byte[h * stride];
                convertedBitmap.CopyPixels(pixels, stride, 0);

                _contentImage = new WriteableBitmap(convertedBitmap);
                _finalImage = new WriteableBitmap(w, h, 96, 96, pixelFormat, null);

                UpdateFinalImage();
            }
            return true;
        }

        public bool SaveImage(string fileName, int bpp)
        {
            ClearErrorInfo();
            PixelFormat pixelFormat = (bpp == 24) ? PixelFormats.Bgr24 : PixelFormats.Bgra32;
            FormatConvertedBitmap convertedBitmap = new FormatConvertedBitmap(_contentImage, pixelFormat, null, 0);
            int w = convertedBitmap.PixelWidth;
            int h = convertedBitmap.PixelHeight;
            int stride = (w * pixelFormat.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[h * stride];
            convertedBitmap.CopyPixels(pixels, stride, 0);

            string fullPath = System.IO.Path.GetFullPath(fileName);
            using (FileStream fs_write = new FileStream(fullPath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(convertedBitmap));
                encoder.Save(fs_write);
            }
            return true;
        }

        public void UpdateFinalImage()
        {
            _contentImage.Lock();
            _finalImage.Lock();
            unsafe
            {
                byte* destPtr = (byte*)_finalImage.BackBuffer;
                byte* srcPtr = (byte*)_contentImage.BackBuffer;

                if (DisplayAlpha)
                {
                    for (int y = 0; y < _contentImage.PixelHeight; y++)
                    {
                        for (int x = 0; x < _contentImage.PixelWidth; x++)
                        {
                            byte* dp = destPtr + (y * _finalImage.BackBufferStride) + (x * 4);
                            byte* sp = srcPtr + (y * _contentImage.BackBufferStride) + (x * 4);
                            byte a = sp[3];
                            dp[0] = a;
                            dp[1] = a;
                            dp[2] = a;
                            dp[3] = 255;
                        }
                    }
                }
                else
                {
                    int checkerSize = 8;

                    bool isBMasked = IsColorMasked(ImageChannel.B);
                    bool isGMasked = IsColorMasked(ImageChannel.G);
                    bool isRMasked = IsColorMasked(ImageChannel.R);
                    bool isAMasked = IsColorMasked(ImageChannel.A);

                    int bkgndB = _backgroundColorValue.B;
                    int bkgndG = _backgroundColorValue.G;
                    int bkgndR = _backgroundColorValue.R;

                    for (int y = 0; y < _contentImage.PixelHeight; y++)
                    {
                        for (int x = 0; x < _contentImage.PixelWidth; x++)
                        {
                            byte* dp = destPtr + (y * _finalImage.BackBufferStride) + (x * 4);
                            byte* sp = srcPtr + (y * _contentImage.BackBufferStride) + (x * 4);
                            int a = isAMasked ? 255 : sp[3];
                            int b = isBMasked ? 0 : sp[0];
                            int g = isGMasked ? 0 : sp[1];
                            int r = isRMasked ? 0 : sp[2];

                            if (BackgroundColorType == BackgroundColorType.Checker)
                            {
                                int cc = (((x / checkerSize + y / checkerSize) & 1) != 0) ? 0x60 : 0xa0;
                                int ccia = cc * (255 - a);
                                dp[0] = (byte)((b * a + ccia) / 255);
                                dp[1] = (byte)((g * a + ccia) / 255);
                                dp[2] = (byte)((r * a + ccia) / 255);
                                dp[3] = 255;
                            }
                            else
                            {
                                dp[0] = (byte)((b * a + bkgndB * (255 - a)) / 255);
                                dp[1] = (byte)((g * a + bkgndG * (255 - a)) / 255);
                                dp[2] = (byte)((r * a + bkgndR * (255 - a)) / 255);
                                dp[3] = 255;
                            }
                        }
                    }
                }
            }
            _finalImage.AddDirtyRect(new Int32Rect(0, 0, _contentImage.PixelWidth, _contentImage.PixelHeight));
            _contentImage.Unlock();
            _finalImage.Unlock();
        }

        // 転送元
        // 32,24bitは元画像の指定のchannel
        // 8bit-Grayは値
        // 8bit-Paletteは色のR値
        public bool LoadImageToChannel(string fileName, ImageChannel channel)
        {
            ClearErrorInfo();
            if (!IsImageLoaded())
            {
                throw new InvalidOperationException();
            }

            string fullPath = System.IO.Path.GetFullPath(fileName);
            using (Stream fs_read = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                PngBitmapDecoder decoder = new PngBitmapDecoder(fs_read, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                if (decoder.Frames[0].PixelWidth != _contentImage.PixelWidth ||
                    decoder.Frames[0].PixelHeight != _contentImage.PixelHeight)
                {
                    SetErrorInfo(Resources.EnumResource.ModelError_ImageWidthAndHeightUnmatch);
                    return false;
                }

                if (decoder.Frames[0].Format.BitsPerPixel != 32 &&
                    decoder.Frames[0].Format.BitsPerPixel != 24 &&
                    decoder.Frames[0].Format.BitsPerPixel != 8)
                {
                    SetErrorInfo(Resources.EnumResource.ModelError_ChannelLoadImageBppSupport);
                    return false;
                }

                if ((channel == ImageChannel.A) &&
                    (decoder.Frames[0].Format.BitsPerPixel == 24))
                {
                    SetErrorInfo(Resources.EnumResource.ModelError_ImageDoesNotHaveAlpha);
                    return false;
                }

                var sourceImage = new WriteableBitmap(decoder.Frames[0]);
                TransferToChannel(sourceImage, channel);
                UpdateFinalImage();
            }
            return true;
        }

        private void TransferToChannel(WriteableBitmap sourceImage, ImageChannel channel)
        {
            int offset = (int)channel;

            _contentImage.Lock();
            sourceImage.Lock();
            unsafe
            {
                byte* destPtr = (byte*)_contentImage.BackBuffer;
                byte* srcPtr = (byte*)sourceImage.BackBuffer;

                if (sourceImage.Format.BitsPerPixel == 8)
                {
                    if (sourceImage.Palette != null)
                    {
                        BitmapPalette pal = sourceImage.Palette;

                        for (int y = 0; y < _contentImage.PixelHeight; y++)
                        {
                            for (int x = 0; x < _contentImage.PixelWidth; x++)
                            {
                                byte* dp = destPtr + (y * _contentImage.BackBufferStride) + (x * 4);
                                byte* sp = srcPtr + (y * sourceImage.BackBufferStride) + (x * 1);
                                dp[offset] = pal.Colors[sp[0]].R;
                            }
                        }
                    }
                    else
                    {
                        for (int y = 0; y < _contentImage.PixelHeight; y++)
                        {
                            for (int x = 0; x < _contentImage.PixelWidth; x++)
                            {
                                byte* dp = destPtr + (y * _contentImage.BackBufferStride) + (x * 4);
                                byte* sp = srcPtr + (y * sourceImage.BackBufferStride) + (x * 1);
                                dp[offset] = sp[0];
                            }
                        }
                    }
                }
                else if (sourceImage.Format.BitsPerPixel == 32 ||
                         sourceImage.Format.BitsPerPixel == 24)
                {
                    int bytePerPixel = sourceImage.Format.BitsPerPixel / 8;
                    for (int y = 0; y < _contentImage.PixelHeight; y++)
                    {
                        for (int x = 0; x < _contentImage.PixelWidth; x++)
                        {
                            byte* dp = destPtr + (y * _contentImage.BackBufferStride) + (x * 4);
                            byte* sp = srcPtr + (y * sourceImage.BackBufferStride) + (x * bytePerPixel);
                            dp[offset] = sp[offset];
                        }
                    }
                }
            }
            _contentImage.AddDirtyRect(new Int32Rect(0, 0, _contentImage.PixelWidth, _contentImage.PixelHeight));
            _contentImage.Unlock();
            sourceImage.Unlock();
        }

        public bool SaveImageFromChannel(string fileName, ImageChannel channel)
        {
            ClearErrorInfo();
            if (!IsImageLoaded())
            {
                throw new InvalidOperationException();
            }

            WriteableBitmap channelImage = new WriteableBitmap(_contentImage.PixelWidth, _contentImage.PixelHeight, 96, 96, PixelFormats.Gray8, null);
            TransferFromChannel(channelImage, channel);
            channelImage.Freeze();

            string fullPath = System.IO.Path.GetFullPath(fileName);
            using (FileStream fs_write = new FileStream(fullPath, FileMode.Create))
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(channelImage));
                encoder.Save(fs_write);
            }
            return true;
        }

        private void TransferFromChannel(WriteableBitmap destinationImage, ImageChannel channel)
        {
            int offset = (int)channel;
            _contentImage.Lock();
            destinationImage.Lock();

            unsafe
            {
                byte* destPtr = (byte*)destinationImage.BackBuffer;
                byte* srcPtr = (byte*)_contentImage.BackBuffer;

                for (int y = 0; y < _contentImage.PixelHeight; y++)
                {
                    for (int x = 0; x < _contentImage.PixelWidth; x++)
                    {
                        byte* dp = destPtr + (y * destinationImage.BackBufferStride) + (x * 1);
                        byte* sp = srcPtr + (y * _contentImage.BackBufferStride) + (x * 4);
                        dp[0] = sp[offset];
                    }
                }
            }
            destinationImage.AddDirtyRect(new Int32Rect(0, 0, _contentImage.PixelWidth, _contentImage.PixelHeight));
            _contentImage.Unlock();
            destinationImage.Unlock();
        }
    }
}
