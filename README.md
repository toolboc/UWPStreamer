# UWPStreamer #
An NTR CFW streaming client targeting [UWP](https://msdn.microsoft.com/windows/uwp/get-started/universal-application-platform-guide?WT.mc_id=iot-0000-pdecarlo) (Xbox One, Hololens, Windows 10, and Windows Phone 10)

![Demo](http://i.imgur.com/GTRoCJv.png)

## Available in the Windows Store & Precompiled .EXE ##

[![StoreLink](http://i.imgur.com/C6buqwe.png)](https://www.microsoft.com/store/p/uwpstreamer/9nd66p3vdnxt?WT.mc_id=iot-0000-pdecarlo)


- [Windows Store Link](https://www.microsoft.com/store/p/uwpstreamer/9nd66p3vdnxt?WT.mc_id=iot-0000-pdecarlo)
- [Precompiled .EXE](https://github.com/toolboc/UWPStreamer/releases/tag/wpf_v1.0.0.0)

## Features ##
* Video Streaming - Stream 3DS screens to remote device
* Input Redirection - Control N3DS with Xbox Gamepad, Keyboard, and other input devices
* Windows Game Bar Support - Allows for integrated features on Windows 10 Desktop including Screen Capture and DVR

<img src="/UWPStreamer/Assets/Gamebar.PNG">

## Instructions for Video Streaming ##
*Assumes you have installed NTR CFW on a New Nintendo 3DS and you know how to get it up and running.*
 
1. Open NTR CFW.  Suggested to use [BootNTR Selector](https://gbatemp.net/threads/release-bootntr-selector.432911/) with NTR CFW v3.4 if you are on firmware 11.2
<img src="/UWPStreamer/Assets/NTRSelector.PNG">
2. Make sure that you are connected to your Wi-Fi network and can find your 3DS's local IP
address. 
3. Launch UWPStreamer, and insert your IP address in the settings screen, select options and click "Connect"

## NTR Input Redirection Support ##

*Allows controlling the N3DS with xbox gamepad, keyboard, and other input devices*

1. Ensure that you have installed the latest version of [InputRedirectionNTR](https://github.com/Kazo/InputRedirection/releases/tag/NTR-build)
2. Start "InputProc NTR Stary" on your 3DS after launching NTR 
<img src="/UWPStreamer/Assets/NTRInputRedirect.PNG"> 
3.Connect gamepad to your Win10 Device using USB, Bluetooth, or dongle

## Controls ##
* To hide bottom menu use Right-Trigger or Right Mouse-Click (Allows more screen real-estate and block input from controller to menu)
* Disable controller input to the game screen with Left-Trigger  ​(Allows selecting the menu w/out input to game)
* On Desktop push Xbox button or Win+G to launch Game Bar for DVR / Screenshots

## Videos ##

**Xbox One**

[![Xbox One Demo](https://img.youtube.com/vi/mO7kZx6YRTU/0.jpg)](https://www.youtube.com/watch?v=mO7kZx6YRTU)

**Hololens**

[![Hololens Demo](https://img.youtube.com/vi/HVuQsCvUj_o/0.jpg)](https://www.youtube.com/watch?v=HVuQsCvUj_o)

## Video Capture ##
[No More Beta Productions](https://www.youtube.com/channel/UCdQDN1V4mMzfOirm08Nm8TA) has created a tutorial video showing how to produce custom overlays with  screen recordings taken from UWPStreamer

**Tutorial**

[![Capture Tutorial](https://img.youtube.com/vi/I1pQiyEAduA/0.jpg)](https://www.youtube.com/watch?v=I1pQiyEAduA)

**Video Capture with post effects**

[![Video Capture Example](https://img.youtube.com/vi/9HNBWVT911o/0.jpg)](https://www.youtube.com/watch?v=9HNBWVT911o)

## User Demos ##

**3DS streaming wirelessly to the Xbox One streaming wirelessly to Windows 10 2-n-1 streaming wirelessly to Roku Stick with Miracast**

<img src="http://i.imgur.com/L4Idp0M.jpg" width="500">

**Windows Phone**

<img src="https://static.wiidatabase.de/UWPStreamer-Mobile-1024x576.jpg" width="500">

**Xbox One**

<img src="http://i.imgur.com/NF5lybH.png" width="500">

## Credits ##
Inspired by  RattletraPM's original open-source streamer : [SnickerStream](https://github.com/RattletraPM/Snickerstream). 

Impossible without Cell9's NTR CFW and [BootNTR](https://github.com/44670/BootNTR)

Input Redirection thanks to Stary2001's [NTR Input Redirection](https://github.com/Stary2001/InputRedirection) and Kazo's [Input Redirection Client](https://github.com/Kazo/InputRedirectionClient) for PC.

