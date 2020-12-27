ChangeLog
=========

2.0.0 (2020-12-27)
------------------
#### Implemented enhancements:
* Added support of arbitrary number of 'ChronoButtons'.
* Added settings for layout (horizontal, vertical) and block order.
* Added customizable warning intended to notify when timeScale is being change from outside (e.g. script).
* Label style of 'ChronoButtons' is now customizable (AsIs, Short, Compact).
* Other minor UI improvements.

1.4.0 (2018-01-14)
------------------
#### Implemented enhancements:
* Improved UI; added support of Dark/Pro skin.

1.3.0 (2018-01-14)
------------------
#### Fixed bugs:
* Missing window in the layout after Editor restart.
* Storing / restoring / applying chronoScale when opening / closing window and switching between play / edit mode.

1.2.1 (2017-08-06)
------------------
#### Fixed bugs:
* Doing nothing if opened in PlayMode.

1.2.0 (2017-04-10)
------------------
#### Implemented enhancements:
* Added saving and loading ChronoHelper's state thus when ChronoHelper is opened again it restores its previous state. This relates to chronoScale, canResetOnPlayEnd and canSuppressTimeScale.

1.1.0 (2017-04-09)
------------------
#### Fixed bugs:
* Problem with chronoScale reset to wrong value when switching from PlayMode back to EditMode

#### Implemented enhancements:
* chronoScale can be preset in EditMode. It will be applied to Time.timeScale on entering to PlayMode
* Correct tooltips accordingly to other changes.
* Added "Auto-reset chronoScale to value set in EditMode" toggle button. If set chronoScale will be reset to initial value which was set in EditMode. Otherwise it will keep its last value set in PlayMode.
* Added "Suppress Time.timeScale changes from without" toggle button. If set any changes made to Time.timeScale in PlayMode (either from script or from ProjectSettings > Time) will be suppressed and overriden with chronoScale. Otherwise chronoScale will catch up changes made to Time.timeScale.


1.0.0 (2017-01-12)
------------------
Initial working version