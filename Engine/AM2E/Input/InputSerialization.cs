using Newtonsoft.Json;

namespace AM2E.Input;

[method: JsonConstructor]
internal struct InputSerialization(
    Dictionary<string, KeyboardInput> keyboardListeners,
    Dictionary<string, MouseInput> mouseListeners,
    Dictionary<string, GamePadInput> gamePadListeners,
    float rightCenterDeadZone,
    float leftCenterDeadZone,
    float angularAxisDeadZone)
{
    [JsonProperty("kls")]
    public Dictionary<string, KeyboardInput> KeyboardListeners = keyboardListeners;
    [JsonProperty("mls")]
    public Dictionary<string, MouseInput> MouseListeners = mouseListeners;
    [JsonProperty("gls")]
    public Dictionary<string, GamePadInput> GamePadListeners = gamePadListeners;
    [JsonProperty("rdz")]
    public float RightCenterDeadZone = rightCenterDeadZone;
    [JsonProperty("ldz")]
    public float LeftCenterDeadZone = leftCenterDeadZone;
    [JsonProperty("adz")]
    public float AngularAxisDeadZone = angularAxisDeadZone;
}