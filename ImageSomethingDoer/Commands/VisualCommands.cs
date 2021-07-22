using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace ImageSomethingDoer
{
    /// <summary>
    /// Alphaの表示
    /// </summary>
    public class DisplayAlphaCommand : ICommand
    {
        private ImageSomethingDoerViewModel _vm;

        public DisplayAlphaCommand(ImageSomethingDoerViewModel vm)
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
            _vm.DisplayAlpha ^= true;
        }
    }

    /// <summary>
    /// カラーマスク
    /// </summary>
    public class ColorMaskCommand : ICommand
    {
        private ImageSomethingDoerViewModel _vm;

        public ColorMaskCommand(ImageSomethingDoerViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return !_vm.DisplayAlpha;
        }

        public void Execute(object parameter)
        {
            _vm.ColorMask ^= 1 << (int)(ImageChannel)parameter;
        }

        public void InvokeCanExecute()
        {
            CanExecuteChanged.Invoke(this, new EventArgs());
        }
    }
}
