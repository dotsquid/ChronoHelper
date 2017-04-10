ChangeLog
=========

v 1.2 (2017-04-10)
------------------
#### Implemented enhancements:
* Added saving and loading ChronoHelper's state thus when ChronoHelper is opened again it restores its previous state. This relates to chronoScale, canResetOnPlayEnd and canSuppressTimeScale.

v 1.1 (2017-04-09)
------------------
#### Fixed bugs:
* Problem with chronoScale reset to wrong value when switching from PlayMode back to EditMode

#### Implemented enhancements:
* chronoScale can be preset in EditMode. It will be applied to Time.timeScale on entering to PlayMode
* Correct tooltips accordingly to other changes.
* Added "Auto-reset chronoScale to value set in EditMode" toggle button. If set chronoScale will be reset to initial value which was set in EditMode. Otherwise it will keep its last value set in PlayMode.
* Added "Suppress Time.timeScale changes from without" toggle button. If set any changes made to Time.timeScale in PlayMode (either from script or from ProjectSettings > Time) will be suppressed and overriden with chronoScale. Otherwise chronoScale will catch up changes made to Time.timeScale.


v 1.0 (2017-01-12)
------------------
Initial working version
