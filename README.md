# ChronoHelper
ChronoHelper is a free open-source tool for Unity Editor for controlling TimeScale in PlayMode with ease.
It becomes very handy when it’s required to examine a suspicious moment of gameplay in slow-motion or conversely when it’s preferable to skip uninteresting part in fast-forward.

## Installation
Just drop *'ChronoHelper'* folder to your project's *'Asset'* folder.  
However it's better to keep everything in order. That's why *'YourProject/Assets/3rdParty/ChronoHelper'* is more recommended.

## Usage
Use ‘Tools/Chrono Helper’ menu to open ChronoHelper.  
To change current timeScale use the slider or shortcut buttons.  
While being in EditorMode, ChronoHelper is inactive. That is done to protect the user from accidental changing of Time.timeScale project setting.  
![Usage](https://i.imgur.com/NqPbxkN.gif)  
Right after switching to PlayMode ChronoHelper captures the default value of Time.timeScale in order to restore this value on switching back to EditorMode (if ‘Auto-reset’ is on).  
![Auto-reset](https://i.imgur.com/mldW9zE.gif)  
Use ‘Lock’ if you want to suppress any changes of Time.timeScale from without (like other scripts or TimeManager Inspector). Be aware that this mode can break the normal flow of your game (e.g. the game won’t actually pause in main menu).  
![Lock Off](https://i.imgur.com/rfJnloe.gif)  
![Lock On](https://i.imgur.com/xFNxR7f.gif) 

> ### Tip
> ChronoHelper's window was designed to occupy as less screen space as possible. The suggested way to place the window is to dock it above or below the GameView window. In this case the slider and shortcut buttons are arranged in horizontal layout  
> ![Horizontal layout](https://i.imgur.com/XqNoHPK.gif)
>  
> If the window is not wide enough vertical layout is used  
> ![Vertical layout](https://i.imgur.com/5KVVUAO.gif)
