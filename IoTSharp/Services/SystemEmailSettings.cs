using IoTSharp.Dtos;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.Json;

namespace IoTSharp.Services;

/// <summary>在持久状态目录中加密保存平台 SMTP 配置，并原子替换配置文件。</summary>
public sealed class SystemEmailSettings(IConfiguration configuration)
{
    private readonly object _gate = new();
    private readonly string _directory = Path.GetFullPath(configuration["SystemEmail:StateDirectory"] ??
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IoTSharp", "system-email"));
    private IDataProtector _protector;
    private string SettingsPath => Path.Combine(_directory, "smtp.protected");

    /// <summary>返回系统管理页面可读取的字段。</summary>
    public SystemEmailSettingsView GetView()
    {
        var settings = Read();
        return new(settings.Host, settings.Port, settings.ImplicitTls, settings.Username,
            settings.SenderAddress, !string.IsNullOrWhiteSpace(settings.Password));
    }

    /// <summary>保存新设置，空授权码表示继续使用现有授权码。</summary>
    public bool Save(SystemEmailSettingsDto settings)
    {
        lock (_gate)
        {
            settings.Password = string.IsNullOrWhiteSpace(settings.Password) ? Read().Password : settings.Password;
            if (Uri.CheckHostName(settings.Host) == UriHostNameType.Unknown || string.IsNullOrWhiteSpace(settings.Password))
                return false;

            var encrypted = GetProtector().Protect(JsonSerializer.Serialize(settings));
            var temporary = Path.Combine(_directory, $"smtp-{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllText(temporary, encrypted);
                File.Move(temporary, SettingsPath, overwrite: true);
            }
            finally
            {
                File.Delete(temporary);
            }
            return true;
        }
    }

    internal SystemEmailSettingsDto Read()
    {
        lock (_gate)
        {
            if (File.Exists(SettingsPath))
                return JsonSerializer.Deserialize<SystemEmailSettingsDto>(GetProtector().Unprotect(File.ReadAllText(SettingsPath)));

            var defaults = configuration.GetSection("SystemEmail").Get<SystemEmailSettingsDto>() ?? new();
            var passwordFile = configuration["SystemEmail:PasswordFile"];
            if (!string.IsNullOrWhiteSpace(passwordFile))
                defaults.Password = File.ReadAllText(passwordFile).TrimEnd('\r', '\n');
            return defaults;
        }
    }

    private IDataProtector GetProtector()
    {
        if (_protector != null)
            return _protector;

        if (OperatingSystem.IsWindows())
            Directory.CreateDirectory(_directory);
        else
            Directory.CreateDirectory(_directory, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);

        _protector = DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(_directory, "keys")),
            builder => builder.SetApplicationName("IoTSharp.SystemEmail")).CreateProtector("smtp-settings-v1");
        return _protector;
    }
}
