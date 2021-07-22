# ImageSomethingDoer
Import the image as 32bit RGBA. You can import or export by channel. The formats supported for reading are PNG, BMP, JPG, TGA, and DDS (part of). Only 32bit PNG can be saved.

# About app.
This is the first WPF application for studying MVVM.
ImageSomethingDoer... What is this a tool to do? Is it image processing?
For now, it's useful when you want to swap the Alpha channel of an image.

# Build
use publish

# Build as Single File
PM> dotnet publish -c Release -r win-x64 /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
