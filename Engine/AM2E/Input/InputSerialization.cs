using Newtonsoft.Json;

namespace AM2E.Control;

internal struct InputSerialization
{
    [JsonProperty("kls")]
    public Dictionary<string, KeyboardInput> KeyboardListeners;
    [JsonProperty("mls")]
    public Dictionary<string, MouseInput> MouseListeners;
    [JsonProperty("gls")]
    public Dictionary<string, GamePadInput> GamePadListeners;
    [JsonProperty("rdz")]
    public float RightCenterDeadZone;
    [JsonProperty("ldz")]
    public float LeftCenterDeadZone;
    [JsonProperty("adz")]
    public float AngularAxisDeadZone;

    [JsonConstructor]
    public InputSerialization(
        Dictionary<string, KeyboardInput> keyboardListeners, 
        Dictionary<string, MouseInput> mouseListeners,
        Dictionary<string, GamePadInput> gamePadListeners,
        float rightCenterDeadZone,
        float leftCenterDeadZone,
        float angularAxisDeadZone)
    {
        KeyboardListeners = keyboardListeners;
        MouseListeners = mouseListeners;
        GamePadListeners = gamePadListeners;
        RightCenterDeadZone = rightCenterDeadZone;
        LeftCenterDeadZone = leftCenterDeadZone;
        AngularAxisDeadZone = angularAxisDeadZone;
    }
}