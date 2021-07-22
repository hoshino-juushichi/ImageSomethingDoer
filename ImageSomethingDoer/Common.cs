using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ImageSomethingDoer
{
    public enum ImageChannel
    {
        B = 0,
        G = 1,
        R = 2,
        A = 3,
    };

    [TypeConverter(typeof(EnumDisplayTypeConverter))]
    public enum BackgroundColorType
    {
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.BackgroundColorTypeChecker))]
        Checker = 0,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.BackgroundColorTypeBlack))]
        Black = 1,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.BackgroundColorTypeWhite))]
        White = 2,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.BackgroundColorTypeRed))]
        Red = 3,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.BackgroundColorTypeGreen))]
        Green = 4,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.BackgroundColorTypeBlue))]
        Blue = 5,
    };

    [TypeConverter(typeof(EnumDisplayTypeConverter))]
    public enum ImageScalingType
    {
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeX1))]
        X1 = 0,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeX2))]
        X2,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeX4))]
        X4,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeX8))]
        X8,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeX16))]
        X16,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeX32))]
        X32,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeIX2))]
        IX2,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeIX4))]
        IX4,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeIX8))]
        IX8,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeIX16))]
        IX16,
        [Display(ResourceType = typeof(Resources.EnumResource), Name = nameof(Resources.EnumResource.ImageScalingTypeIX32))]
        IX32,
    }

    public static class MathUtils
    {
        private static int Clamp(int value, int min, int max)
        {
            if (min > max)
            {
                (min, max) = (max, min);
            }
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            return value;
        }
    }
}
