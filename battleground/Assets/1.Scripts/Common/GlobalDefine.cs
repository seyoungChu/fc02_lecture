
public enum EffectType
{
    None = -1,
    NORMAL,
}

public enum SoundPlayType
{
    None = -1,
    BGM,
    EFFECT,
    UI,
}

#region //Tool Defines 
public enum EventStartType { INTERACT, AUTOSTART, TRIGGER_ENTER, TRIGGER_EXIT, NONE, KEY_PRESS, DROP };
public enum AIConditionNeeded { ALL, ONE };
public enum ValueCheck { EQUALS, LESS, GREATER };
public enum SimpleOperator { ADD, SUB, SET };
public enum MouseButton { Left = 0, Right, Wheel };
#endregion