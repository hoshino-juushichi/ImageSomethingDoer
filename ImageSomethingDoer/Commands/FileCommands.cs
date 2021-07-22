using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace ImageSomethingDoer
{
    /// <summary>
    /// アプリ終了
    /// </summary>
    public class QuitAppCommand : ICommand
    {
        private ImageSomethingDoerViewModel _vm;

        public QuitAppCommand(ImageSomethingDoerViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// ファイルを開く
    /// </summary>
    public class OpenFileCommand : ICommand
    {
        private ImageSomethingDoerViewModel _vm;

        public OpenFileCommand(ImageSomethingDoerViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Filter = Resources.EnumResource.FileDialogFilterAll;

            if (dialog.ShowDialog() == true)
            {
                (bool isSuccess, string errorMessage) = _vm.LoadImage(dialog.FileName);
                if (!isSuccess)
                {
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

    /// <summary>
    /// 名前を付けて保存
    /// </summary>
    public class SaveFileCommand : ICommand
    {
        private ImageSomethingDoerViewModel _vm;

        public SaveFileCommand(ImageSomethingDoerViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _vm.IsImageLoaded();
        }

        public void Execute(object parameter)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Filter = Resources.EnumResource.FileDialogFilterPNG;

            if (dialog.ShowDialog() == true)
            {
                int bpp = Int32.Parse((string)parameter);
                (bool isSuccess, string errorMessage) = _vm.SaveImage(dialog.FileName, bpp);
                if (!isSuccess)
                {
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public void InvokeCanExecute()
        {
            CanExecuteChanged.Invoke(this, new EventArgs());
        }
    }

    /// <summary>
    /// ファイルを開いてチャンネルへ
    /// </summary>
    public class OpenFileToChannelCommand : ICommand
    {
        private ImageSomethingDoerViewModel _vm;

        public OpenFileToChannelCommand(ImageSomethingDoerViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _vm.IsImageLoaded();
        }

        public void Execute(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Filter = Resources.EnumResource.FileDialogFilterPNG;

            if (dialog.ShowDialog() == true)
            {
                (bool isSuccess, string errorMessage) = _vm.LoadImageToChannel(dialog.FileName, (ImageChannel)parameter);
                if (!isSuccess)
                {
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public void InvokeCanExecute()
        {
            CanExecuteChanged.Invoke(this, new EventArgs());
        }
    }

    /// <summary>
    /// 名前を付けて保存
    /// </summary>
    public class SaveFileFromChannelCommand : ICommand
    {
        private ImageSomethingDoerViewModel _vm;

        public SaveFileFromChannelCommand(ImageSomethingDoerViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _vm.IsImageLoaded();
        }

        public void Execute(object parameter)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = Directory.GetCurrentDirectory();
            dialog.Filter = Resources.EnumResource.FileDialogFilterPNG;

            if (dialog.ShowDialog() == true)
            {
                (bool isSuccess, string errorMessage) = _vm.SaveImageFromChannel(dialog.FileName, (ImageChannel)parameter);
                if (!isSuccess)
                {
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        public void InvokeCanExecute()
        {
            CanExecuteChanged.Invoke(this, new EventArgs());
        }
    }

    /// <summary>
    /// ファイルを閉じる
    /// </summary>
    public class CloseFileCommand : ICommand
    {
        private ImageSomethingDoerViewModel _vm;

        public CloseFileCommand(ImageSomethingDoerViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _vm.CloseFile();
        }
    }

    /// <summary>
    /// アプリについて
    /// </summary>
    public class AboutAppCommand : ICommand
    {
        private ImageSomethingDoerViewModel _vm;

        public AboutAppCommand(ImageSomethingDoerViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MessageBox.Show(Resources.EnumResource.AboutApp);
        }
    }
}
