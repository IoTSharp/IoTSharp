namespace IoTSharp.Data;

/// <summary>与登录身份分开保存的用户显示资料。</summary>
public sealed class PersonalProfile
{
    public string UserId { get; set; }
    public string DisplayName { get; set; }
    public string Introduction { get; set; }
    public byte[] AvatarPng { get; set; }
}
