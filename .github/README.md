<h1 align="center">
<img src="https://d33wubrfki0l68.cloudfront.net/327934f4ff80060e07c17935683ecad27cda8080/ee2bc/assets/images/photoshop_1.png" alt="PicView Logo" height="90">
</h1>

PicView is a fast, free and fully customizable picture viewer for Windows 10 and 11. It supports virtually all image file types, including _(animated)_ `WEBP`, _(animated)_ `GIF`, `SVG`, `AVIF`, `HEIC`, `PSD` and many others. 

Enjoy a clean, free, and fast experience with no bloated UI or annoying pop-ups.


<p align=center>
    <a href="https://github.com/Ruben2776/PicView/releases">
        <img alt="Downloads shield" src="https://img.shields.io/github/downloads/Ruben2776/PicView/total?color=%23007ACC&label=downloads&style=flat-square">
    </a>
    <a href="https://github.com/Ruben2776/PicView/blob/master/LICENSE.txt">
        <img alt="GPL v3 License" src="https://img.shields.io/badge/license-GPLv3-green.svg?maxAge=3600&style=flat-square">
    </a>
    <img alt="Windows OS" src="https://img.shields.io/badge/OS-Windows%2010/11%2064%20bit-00adef.svg?maxAge=3600&style=flat-square">
</p>

# Downloads

[![](https://img.shields.io/badge/Windows-x64-blue?style=flat-square&logo=windows&logoColor=fff)](https://github.com/Ruben2776/PicView/releases/download/3.1.0/PicView-v3.1.0-win-x64.exe) [![](https://img.shields.io/badge/Windows-arm64-blue?style=flat-square&logo=windows&logoColor=fff)](https://github.com/Ruben2776/PicView/releases/download/3.1.0/PicView-v3.1.0-win-arm64.exe)

[Latest releases at PicView.org](https://picview.org/download)

**Mirrors**

[uptodown](https://picview.en.uptodown.com/windows) <br>
[FossHub](https://www.fosshub.com/PicView.html) <br>
[SourceForge](https://sourceforge.net/projects/picview/)

<br>


Winget:
```cmd
cmd $> winget install picview
```

Scoop:
```cmd
cmd $> scoop bucket add extras
cmd $> scoop install extras/picview
```

___

PicView is portable by default. Settings and keybindings are stored in the same directory _(unless there is no write permissions)_. No system files are modified or installation necessary. Perfect for storing on a portable USB drive. 

If installing, it will set file associations.

<br>

If you like PicView, consider giving it a star or a like on [AlternativeTo](https://alternativeto.net/software/picview/about/)!


# Features and screenshots
![3x3 0 0](https://github.com/user-attachments/assets/1839c2bb-aff3-4d31-8093-ba3814952ce7)

Switch between a dark and a light theme and toggle between hiding the UI.

<br>

![UI-Dark-Theme-Magenta 3 0 0](https://github.com/user-attachments/assets/ede687f6-523e-49dd-b2fa-3509929434e1)

_UI overview with bottom gallery._

<br>

<h3 align="center">
    Scroll Image
</h3>

![SideBySideScroll3 0 0](https://github.com/user-attachments/assets/59ab9368-eb6c-4964-8134-44cf134ea753)


You can toggle the interface to show images `Side by side` and you can also turn scrolling on/off. Click the mousewheel for auto scroll.

<br>

<h3 align="center">
    Crop Image
</h3>

![Cropping3 0](https://github.com/user-attachments/assets/e70990d9-b607-4118-8be4-587a3f02c4d7)

Quickly crop image by pressing `C`. Hold `Shift` for square selection.

<br>

<h3 align="center">
    Image Info Window
</h3>

<h1 align="center">
    <img src="https://picview.org/assets/screenshots/exifwindow/Image%20Info%20Window%203.0.0%20.webp" />
</h1>

Perform operations such as _renaming_, _file conversion_, _copying_, _compressing_, _resizing_, adding it to the _recycle bin_ and editing the _EXIF image rating_.

Changing size can be done by editing the `width` and/or `height` text-boxes. Use the `%` keyboard button to resize it by percentage.

<h1 align="center">
    <img src="https://picview.org/assets/screenshots/exifwindow/gps3.0.webp" />
</h1>

If the GPS coordinates are saved on the image, you can click the Google or Bing buttons to open the respective maps at the GPS coordinates


<br>

<h3 align="center">
    Image Gallery
</h3>

<h1 align="center">
    <img src="https://picview.org/assets/screenshots/gallery/PicView3.0-galleryAnimation.webp" />
</h1>


**Press `G` to open or close the image gallery**

Navigate the gallery with the `arrow keys` or `W`,`A`,`S`,`D` and load the selected image with `Enter` or the `E` key.
The bottom gallery can be turned on or off

<br>

<h3 align="center">
    Batch Resizing
</h3>

<h1 align="center">
    <img src="https://picview.org/assets/screenshots/batch%20resize/batch-resize-3.0v2.webp" />
</h1>


### Convert/Optimize all your pictures
All files from the `Source folder` will be selected for processing and will be sent to `Output folder`. The default name for the output folder will be **Processed Pictures**.

If the *Output folder* is the same as the *Source folder*, or left blank, the files will be overwrittten.

The `Convert to` dropdown option allows you to convert all the files to a popular format.

The `Compression` dropdown option allows you to compress the files, either without losing quality or sacrifing some quality for greater reduced file size.

The `Quality` dropdown option allows you to change quality of supported file types. The higher the Quality setting, the more detail is preserved in the image, but the larger the file size.

The `Resize` dropdown option allows you to resize the picture by **height**, **width** and **percentage** while keeping the aspect ratio of the image.

___

<br>

### Renaming


![Screenshot 2025-01-26 141624](https://github.com/user-attachments/assets/5ff7834a-3461-4b39-9784-7cf386c8d429)

Rename or move files in the titlebar by pressing `F2` or right clicking it.
Changing the file extension will convert the image to the respective format.

<br>

## File support
 > .jpg  .jpeg  .jpe  .png  .bmp  .tif  .tiff  .gif  .ico  .jfif  .webp .svg .svgz <br>
   .psd  .psb .xcf .jxl .heic .heif .jp2 .hdr .tga .dds<br>.3fr  .arw  .cr2 .cr3  .crw  .dcr  .dng  .erf  .kdc  .mdc  .mef  .mos  .mrw  .nef  .nrw  .orf  .pef .raf  .raw  .rw2  .srf  .x3f<br>
   .pgm  .hdr  .cut  .exr  .dib  .emf  .wmf  .wpg  .pcx  .xbm  .xpm .wbmp


<br>


## Shortcuts
You can view and change keyboard shortcuts by opening the `Keybindings` window by pressing `K`.
Alternatively, they are also listed at [PicView.org](https://picview.org/#Shortcuts).

<h1 align="center">
    <img src="https://picview.org/assets/screenshots/Keybindings/KeybindingsWindow3.0.webp" />
</h1>


# Contributions
![Visual Studio 2022](https://img.shields.io/badge/IDE-Visual%20Studio%202022-964ad4.svg?maxAge=3600)
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=badge&logo=.net&logoColor=white)
[![Codacy Badge](https://app.codacy.com/project/badge/Grade/bf0fd0f740f9486ba306bdec7fe8bde7)](https://www.codacy.com/manual/ruben_8/PicView?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=Ruben2776/PicView&amp;utm_campaign=Badge_Grade)

**Building:** <br>
Open and run the solution in Visual Studio or Rider. 
If you're not using x64 hardware, make sure to change the platform target to your CPU architecture in the project properties, as well as changing the the Magick.NET Nuget packages to match.

Pull requests are welcome. Check current issues and assign yourself or create your own issue. 

Improvements to the current code or bug fixes are also welcome!


## Translators/Languages
Simplified Chinese by <a href="https://github.com/Crystal-RainSlide">Crystal-RainSlide</a><br>
Traditional Chinese by <a href="https://github.com/wcxu21">wcxu21</a><br>
Spanish by <a href="https://github.com/lk-KEVIN">lk.KEVIN</a> <i>(needs updates)</i><br>
Korean by <a href="https://github.com/VenusGirl">VenusGirl</a><br>
German by <a href="https://github.com/Brotbox">Brotbox</a>, [uDEV2019](https://github.com/uDEV2019)<br>
Polish by <a href="https://github.com/YourSenseiCreeper">YourSenseiCreeper</a><br>
French by <a href="https://www.challenger-systems.com/2021/11/picview-156.html">Sylvain LOUIS</a> <br>
Italian by <a href="https://github.com/franpoli">franpoli</a> <br>
Russian by <a href="https://github.com/andude10">andude10</a> <br>
Romanian by <a href="https://crowdin.com/profile/lmg">M. Gabriel Lup</a> <br>
Swedish by <a href="https://github.com/sparmark">Stefan Parmark</a> <br>
Brazilian Portuguese by <a href="https://github.com/andercard0">Anderson Cardoso</a> <br>
Dutch by <a href="https://github.com/Lien5">Lien5</a> <br>
Japanese by <a href="https://github.com/coolvitto">coolvitto</a> <br>
English and Danish by <a href="https://github.com/Ruben2776">Ruben2776</a><br>

**Looking for translators!**
If you want to help translate another language or update/improve a current one and be listed here, please take a look at
https://github.com/Ruben2776/PicView/issues/13



## Code Signing Policy

All releases are virus scanned and digitally signed.

Free code signing is provided by [SignPath.io](https://about.signpath.io/), certificate by [SignPath Foundation](https://signpath.org/).


## Privacy Policy

PicView does not collect data. No data is sent/recieved and/or collected by PicView.


# Donate
If you wish to thank me for my work, please

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/W7W46BJFV)
