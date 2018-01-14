# ChronoHelper
Unity Editor plugin which allows controlling of Time.timeScale in PlayMode.

## Installation
Just drop *'ChronoHelper'* folder to your project's *'Asset'* folder.  
However it's better to keep everything in order. That's why *'YourProject/Assets/3rdParty/ChronoHelper'* is more recommended.

## Usage
Use *'Window/Chrono Helper'* menu to open ChronoHelper.  
While being in EditorMode, ChronoHelper is inactive. That is done to protect the user from accidental changing of Time.timeScale project setting.  
Right after switching to PlayMode ChronoHelper captures the default Time.timeScale in order to restore this value on switching back to EditorMode.  
To change current timeScale use the slider or shortcut buttons.  
![ChronoHelper usage](https://i.imgur.com/Y3Ryxpc.gif)

> ### Tip
> ChronoHelper's window was designed to occupy as less screen space as possible. The suggested way to place the window is to dock it above or below the GameView window. In this case the slider and shortcut buttons are arranged in horizontal layout  
> ![Horizontal layout](https://i.imgur.com/MxDtpFN.gif)
>  
> If the window is not wide enough vertical layout is used  
> ![Vertical layout](https://i.imgur.com/K1NR5ok.gif)
