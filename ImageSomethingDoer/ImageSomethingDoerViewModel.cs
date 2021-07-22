using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using System.Globalization;
using System.Runtime.CompilerServices;
using GongSolutions.Wpf.DragDrop;

namespace ImageSomethingDoer
{
    public class ImageSomethingDoerViewModel : INotifyPropertyChanged, IDropTarget
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ImageSomethingDoerModel _model;

        public AboutAppCommand AboutAppCmd { get; set; }
        public QuitAppCommand QuitAppCmd { get; set; }
        public OpenFileCommand OpenFileCmd { get; set; }
        public SaveFileCommand SaveFileCmd { get; set; }
        public CloseFileCommand CloseFileCmd { get; set; }
        public OpenFileToChannelCommand OpenFileToChannelCmd { get; set; }
        public SaveFileFromChannelCommand SaveFileFromChannelCmd { get; set; }
        public ColorMaskCommand ColorMaskCmd { get; set; }
        public DisplayAlphaCommand DisplayAlphaCmd { get; set; }

        public BitmapSource ImageSource { get; set; }
        public Matrix ImageScalingMatrix { get; set; } = new Matrix();
        public double ImageWidth { get; set; } = 0;
        public double ImageHeight { get; set; } = 0;

        public bool IsChecked_ColorMaskR { get; set; } = false;
        public bool IsChecked_ColorMaskG { get; set; } = false;
        public bool IsChecked_ColorMaskB { get; set; } = false;
        public bool IsChecked_ColorMaskA { get; set; } = false;

        public bool IsChecked_DisplayAlpha { get; set; } = false;

		public string FileMenu { get; } = Resources.EnumResource.FileMenu;
		public string ViewMenu { get; } = Resources.EnumResource.ViewMenu;
		public string HelpMenu { get; } = Resources.EnumResource.HelpMenu;

        public string AboutAppCmdMenu { get; } = Resources.EnumResource.AboutAppCmdMenu;
        public string OpenFileCmdMenu { get; } = Resources.EnumResource.OpenFileCmdMenu;
		public string SaveFile32CmdMenu { get; } = Resources.EnumResource.SaveFile32CmdMenu;
		public string SaveFile24CmdMenu { get; } = Resources.EnumResource.SaveFile24CmdMenu;
		public string CloseFileCmdMenu { get; } = Resources.EnumResource.CloseFileCmdMenu;
        public string QuitAppCmdMenu { get; } = Resources.EnumResource.QuitAppCmdMenu;
        public string LoadToChannelMenu { get; } = Resources.EnumResource.LoadToChannelMenu;
        public string LoadToChannelRCmdMenu { get; } = Resources.EnumResource.LoadToChannelRCmdMenu;
        public string LoadToChannelGCmdMenu { get; } = Resources.EnumResource.LoadToChannelGCmdMenu;
        public string LoadToChannelBCmdMenu { get; } = Resources.EnumResource.LoadToChannelBCmdMenu;
        public string LoadToChannelACmdMenu { get; } = Resources.EnumResource.LoadToChannelACmdMenu;
        public string ExportChannelMenu { get; } = Resources.EnumResource.ExportChannelMenu;
        public string ExportChannelRCmdMenu { get; } = Resources.EnumResource.ExportChannelRCmdMenu;
        public string ExportChannelGCmdMenu { get; } = Resources.EnumResource.ExportChannelGCmdMenu;
        public string ExportChannelBCmdMenu { get; } = Resources.EnumResource.ExportChannelBCmdMenu;
        public string ExportChannelACmdMenu { get; } = Resources.EnumResource.ExportChannelACmdMenu;

        public string OpenFileCmdTooltip { get; } = Resources.EnumResource.OpenFileCmdTooltip;
        public string SaveFileCmdTooltip { get; } = Resources.EnumResource.SaveFileCmdTooltip;
        public string CloseFileCmdTooltip { get; } = Resources.EnumResource.CloseFileCmdTooltip;
        public string DisplayAlphaCmdTooltip { get; } = Resources.EnumResource.DisplayAlphaCmdTooltip;
        public string ColorMaskCmdRTooltip { get; } = Resources.EnumResource.ColorMaskCmdRTooltip;
        public string ColorMaskCmdGTooltip { get; } = Resources.EnumResource.ColorMaskCmdGTooltip;
        public string ColorMaskCmdBTooltip { get; } = Resources.EnumResource.ColorMaskCmdBTooltip;
        public string ColorMaskCmdATooltip { get; } = Resources.EnumResource.ColorMaskCmdATooltip;
        public string ImageScalingTooltip { get; } = Resources.EnumResource.ImageScalingTooltip;
        public string BackgroundColorTooltip { get; } = Resources.EnumResource.BackgroundColorTooltip;

        public ImageSomethingDoerViewModel()
        {
            AboutAppCmd = new AboutAppCommand(this);
            QuitAppCmd = new QuitAppCommand(this);
            OpenFileCmd = new OpenFileCommand(this);
            SaveFileCmd = new SaveFileCommand(this);
            CloseFileCmd = new CloseFileCommand(this);
            OpenFileToChannelCmd = new OpenFileToChannelCommand(this);
            SaveFileFromChannelCmd = new SaveFileFromChannelCommand(this);
            ColorMaskCmd = new ColorMaskCommand(this);
            DisplayAlphaCmd = new DisplayAlphaCommand(this);

            _model = new ImageSomethingDoerModel();
        }

        public void UpdateFinalImage()
        {
            _model.UpdateFinalImage();
        }

        private void UpdateImageSize()
        {
            Matrix m0 = new Matrix();
            m0.Scale(_model.ImageScale, _model.ImageScale);
            ImageScalingMatrix = m0;
            RaisePropertyChanged("ImageScalingMatrix");

            if (ImageSource != null)
            {
                ImageWidth = ImageSource.Width * _model.ImageScale;
                ImageHeight = ImageSource.Height * _model.ImageScale;
                RaisePropertyChanged("ImageWidth");
                RaisePropertyChanged("ImageHeight");
            }
        }

        public void CloseFile()
        {
            _model.DisposeImage();
            ImageSource = null;
            RaisePropertyChanged("ImageSource");
            SaveFileCmd.InvokeCanExecute();
            OpenFileToChannelCmd.InvokeCanExecute();
            SaveFileFromChannelCmd.InvokeCanExecute();
        }

        public (bool, string) LoadImage(string fileName)
        {
            _model.LoadImage(fileName);
            if (_model.HasError)
            {
                return (false, _model.ErrorMessage);
            }
            ImageSource = _model.FinalImage;
            RaisePropertyChanged("ImageSource");
            UpdateImageSize();
            SaveFileCmd.InvokeCanExecute();
            OpenFileToChannelCmd.InvokeCanExecute();
            SaveFileFromChannelCmd.InvokeCanExecute();
            return (true, string.Empty);
        }

        public (bool, string) SaveImage(string fileName, int bpp)
        {
            _model.SaveImage(fileName, bpp);
            if (_model.HasError)
            {
                return (false, _model.ErrorMessage);
            }
            return (true, string.Empty);
        }

        public (bool, string) LoadImageToChannel(string fileName, ImageChannel channel)
        {
            _model.LoadImageToChannel(fileName, channel);
            if (_model.HasError)
            {
                return (false, _model.ErrorMessage);
            }
            return (true, string.Empty);
        }

        public (bool, string) SaveImageFromChannel(string fileName, ImageChannel channel)
        {
            _model.SaveImageFromChannel(fileName, channel);
            if (_model.HasError)
            {
                return (false, _model.ErrorMessage);
            }
            return (true, string.Empty);
        }

        public bool IsImageLoaded()
        {
            return ImageSource != null;
        }

        public ImageScalingType ImageScalingType
        {
            get => _model.ImageScalingType;
            set
            {
                _model.ImageScalingType = value;
                RaisePropertyChanged("ImageScalingType");
                UpdateImageSize();
            }
        }

        public BackgroundColorType BackgroundColorType
        {
            get => _model.BackgroundColorType;
            set
            {
                _model.BackgroundColorType = value;
                RaisePropertyChanged("BackgroundColorType");

                if (IsImageLoaded())
                {
                    UpdateFinalImage();
                }
            }
        }

        public Color BackgroundColorValue
        {
            get => _model.BackgroundColorValue;
        }

        public int ColorMask
        {
            get => _model.ColorMask;
            set
            {
                _model.ColorMask = value;

                IsChecked_ColorMaskR = _model.IsColorMasked(ImageChannel.R);
                RaisePropertyChanged("IsChecked_ColorMaskR");
                IsChecked_ColorMaskG = _model.IsColorMasked(ImageChannel.G);
                RaisePropertyChanged("IsChecked_ColorMaskG");
                IsChecked_ColorMaskB = _model.IsColorMasked(ImageChannel.B);
                RaisePropertyChanged("IsChecked_ColorMaskB");
                IsChecked_ColorMaskA = _model.IsColorMasked(ImageChannel.A);
                RaisePropertyChanged("IsChecked_ColorMaskA");
                if (IsImageLoaded())
                {
                    UpdateFinalImage();
                }
            }
        }

        public bool DisplayAlpha
        {
            get => _model.DisplayAlpha;
            set
            {
                _model.DisplayAlpha = value;
                IsChecked_DisplayAlpha = _model.DisplayAlpha;
                RaisePropertyChanged("IsChecked_DisplayAlpha");
                if (IsImageLoaded())
                {
                    UpdateFinalImage();
                }
                ColorMaskCmd.InvokeCanExecute();
            }
        }

        public bool IsColorMasked(ImageChannel channel)
        {
            return _model.IsColorMasked(channel);
        }

        private bool EndsWithSupportedExt(string str)
        {
            return str.EndsWith(".png") ||
                   str.EndsWith(".bmp") ||
                   str.EndsWith(".jpg") ||
                   str.EndsWith(".dds") ||
                   str.EndsWith(".tga");
        }

        public void DragOver(IDropInfo dropInfo)
        {
            var files = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>();
            dropInfo.Effects = files.Any(fname => EndsWithSupportedExt(fname)) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        public void Drop(IDropInfo dropInfo)
        {
            var files = ((DataObject)dropInfo.Data).GetFileDropList().Cast<string>()
                .Where(fname => EndsWithSupportedExt(fname)).ToList();

            if (files.Count == 0) return;

            (bool isSuccess, string errorMessage) = LoadImage(files[0]);
            if (!isSuccess)
            {
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
