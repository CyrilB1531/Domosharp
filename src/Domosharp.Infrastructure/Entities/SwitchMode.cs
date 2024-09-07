namespace Domosharp.Infrastructure.Entities;

internal enum SwitchMode
{
  None = -1,
  /// <summary>
  /// 1 on transition
  /// </summary>
  Toggle = 0,
  /// <summary>
  /// Follow switch state
  /// </summary>
  Follow = 1,
  /// <summary>
  /// Follow switch state inverted
  /// </summary>
  FollowInvert = 2,
  /// <summary>
  /// Push button (default 1, 0 = Toggle)
  /// </summary>
  PushButton = 3,
  PushButtonInvert = 4,
  /// <summary>
  /// Pushbutton with hold (default 1, 0 = toggle, Hold = hold)
  /// </summary>
  PushButtonHold = 5,
  PushButtonHoldInvert = 6,
  PushButtonHoldToggle = 7,
  /// <summary>
  /// = 0 with multi toggle
  /// </summary>
  ToggleMulti = 8,
  /// <summary>
  /// Multi change follow (0 = off, 1 = on, 2x change = hold)
  /// </summary>
  FollowMulti = 9,
  FollowMultiInvert = 10,
  /// <summary>
  /// Pushbutton with dimmer mode
  /// </summary>
  PushHoldMulti = 11,
  PushHoldMultiInvert = 12,
  /// <summary>
  /// Pushon mode (1 = on, switch off using PulseTime)
  /// </summary>
  PushOn = 13,
  PushOnInvert = 14,
  /// <summary>
  /// Send only MQTT message on switch change
  /// </summary>
  PushIgnore = 15,
  PushIgnoreInvert = 16
}
