using IoTSharp.Contracts;
using IoTSharp.Controllers;
using IoTSharp.Data;
using IoTSharp.Data.SonnetDB;
using IoTSharp.Data.Sqlite;
using IoTSharp.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IoTSharp.Test;

public sealed class PersonalCenterTests
{
    [Fact(Timeout = 20000)]
    public async Task SonnetDbPersonalCenterMigrationCreatesWritableTablesAndReadState()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(18));
        var token = timeout.Token;
        var folder = Directory.CreateTempSubdirectory("iotsharp-personal-center-");
        try
        {
            await using var database = new SonnetDbDesignTimeApplicationDbContextFactory()
                .CreateDbContext([$"Data Source={Path.Combine(folder.FullName, "sonnetdb")}"]);
            var migrations = database.GetService<IMigrationsAssembly>();
            var migration = migrations.CreateMigration(migrations.Migrations["20261127090000_AddPersonalCenter"],
                database.Database.ProviderName);
            var generator = database.GetService<IMigrationsSqlGenerator>();
            var commands = generator.Generate(migration.UpOperations, database.GetService<IDesignTimeModel>().Model);
            Assert.Equal(4, commands.Count);
            foreach (var command in commands)
            {
                await database.Database.ExecuteSqlRawAsync(command.CommandText, token);
            }

            var profile = new PersonalProfile { UserId = Guid.NewGuid().ToString(), AvatarPng = [1, 2, 3] };
            var message = new InboxMessage
            {
                UserId = profile.UserId,
                TenantId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Title = "发布批次已提交",
                Body = "等待执行端拉取",
                Sender = "发布中心",
                Category = "release"
            };
            database.AddRange(profile, message);
            await database.SaveChangesAsync(token);
            database.ChangeTracker.Clear();
            var readAt = DateTime.UtcNow;
            var affected = await database.InboxMessages
                .Where(item => item.Id == message.Id && item.UserId == profile.UserId && item.ReadAt == null)
                .ExecuteUpdateAsync(update => update.SetProperty(item => item.ReadAt, readAt), token);
            Assert.Equal(1, affected);
            Assert.Equal(readAt, (await database.InboxMessages.SingleAsync(token)).ReadAt);
            Assert.Equal(profile.AvatarPng, (await database.PersonalProfiles.SingleAsync(token)).AvatarPng);
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    [Fact(Timeout = 20000)]
    public async Task PersonalCenterPersistsAvatarAndRestrictsInboxToCurrentRecipient()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(18));
        var token = timeout.Token;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(token);
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(PersonalCenterTests).Assembly.GetName().Name
        });
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IDataBaseModelBuilderOptions, SqliteModelBuilderOptions>();
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        builder.Services.AddIdentityCore<IdentityUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddAuthentication("TestUser")
            .AddScheme<AuthenticationSchemeOptions, TestUserAuthentication>("TestUser", _ => { });
        builder.Services.AddControllers().AddApplicationPart(typeof(PersonalCenterController).Assembly);

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync(token);

        var tenant = new Tenant { Name = "Tenant test" };
        var customer = new Customer { Name = "Customer test", Tenant = tenant };
        var user = new IdentityUser("operator") { Email = "operator@example.test" };
        var otherUser = new IdentityUser("other-user");
        var ownMessage = new InboxMessage { UserId = user.Id, TenantId = tenant.Id, CustomerId = customer.Id };
        var olderMessage = new InboxMessage
        {
            UserId = user.Id,
            TenantId = tenant.Id,
            CustomerId = customer.Id,
            CreatedAt = ownMessage.CreatedAt.AddMinutes(-1)
        };
        var otherRecipient = new InboxMessage { UserId = otherUser.Id, TenantId = tenant.Id, CustomerId = customer.Id };
        var otherCustomer = new InboxMessage { UserId = user.Id, TenantId = tenant.Id, CustomerId = Guid.NewGuid() };
        using (var scope = app.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await database.Database.EnsureCreatedAsync(token);
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            Assert.True((await userManager.CreateAsync(user)).Succeeded);
            Assert.True((await userManager.CreateAsync(otherUser)).Succeeded);
            database.Customer.Add(customer);
            foreach (var message in new[] { ownMessage, olderMessage, otherRecipient, otherCustomer })
            {
                message.Title = "测试消息";
                message.Body = "消息正文";
                message.Category = "system";
                message.Sender = "系统";
                database.InboxMessages.Add(message);
            }
            await database.SaveChangesAsync(token);
        }

        using var client = app.GetTestClient();
        const string endpoint = "/api/personal-center";
        using var anonymous = await client.GetAsync(endpoint + "/profile", token);
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        client.DefaultRequestHeaders.Add("X-Test-User", user.Id);
        var unscoped = await client.GetFromJsonAsync<ApiResult<InboxUnreadCountDto>>(endpoint + "/inbox/unread-count", token);
        Assert.Equal(0, unscoped.Data.UnreadCount);
        client.DefaultRequestHeaders.Add("X-Test-Tenant", tenant.Id.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Customer", customer.Id.ToString());

        using var edited = await client.PutAsJsonAsync(endpoint + "/profile",
            new UpdatePersonalProfileDto { DisplayName = "新显示名", Introduction = "简介", PhoneNumber = "+86 12345678" }, token);
        edited.EnsureSuccessStatusCode();
        var initial = await client.GetFromJsonAsync<ApiResult<PersonalProfileDto>>(endpoint + "/profile", token);
        Assert.Equal("operator", initial.Data.UserName);
        Assert.Equal("新显示名", initial.Data.DisplayName);
        Assert.Equal("+86 12345678", initial.Data.PhoneNumber);
        Assert.Equal(customer.Name, initial.Data.CustomerName);
        Assert.False(initial.Data.HasCustomAvatar);

        using var bitmap = new SKBitmap(420, 210);
        bitmap.Erase(SKColors.Purple);
        using var image = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        using var imageStream = image.AsStream();
        Assert.NotNull(PersonalCenterController.EncodeAvatar(imageStream));
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(image.ToArray()), "file", "avatar.png");
        using var uploaded = await client.PostAsync(endpoint + "/avatar", form, token);
        Assert.True(uploaded.IsSuccessStatusCode, await uploaded.Content.ReadAsStringAsync(token));
        var stored = await client.GetFromJsonAsync<ApiResult<PersonalProfileDto>>(endpoint + "/profile", token);
        Assert.True(stored.Data.HasCustomAvatar);
        Assert.StartsWith("data:image/png;base64,", stored.Data.Avatar);
        using var thumbnail = SKBitmap.Decode(Convert.FromBase64String(stored.Data.Avatar["data:image/png;base64,".Length..]));
        Assert.Equal(192, thumbnail.Width);
        Assert.Equal(96, thumbnail.Height);

        using var invalidForm = new MultipartFormDataContent();
        invalidForm.Add(new StringContent("图片格式不正确"), "file", "invalid.png");
        using var rejected = await client.PostAsync(endpoint + "/avatar", invalidForm, token);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        using var removed = await client.DeleteAsync(endpoint + "/avatar", token);
        var fallback = await removed.Content.ReadFromJsonAsync<ApiResult<PersonalProfileDto>>(token);
        Assert.False(fallback.Data.HasCustomAvatar);

        var inbox = await client.GetFromJsonAsync<ApiResult<InboxPageDto>>(endpoint + "/inbox?unreadOnly=true&pageSize=1", token);
        Assert.Equal(ownMessage.Id, Assert.Single(inbox.Data.Items).Id);
        Assert.Equal(2, inbox.Data.Total);
        Assert.Equal(2, inbox.Data.UnreadCount);
        using var unrelated = await client.PutAsync($"{endpoint}/inbox/{otherRecipient.Id}/read", null, token);
        Assert.Equal(HttpStatusCode.NotFound, unrelated.StatusCode);
        using var read = await client.PutAsync($"{endpoint}/inbox/{ownMessage.Id}/read", null, token);
        read.EnsureSuccessStatusCode();
        var remaining = await client.GetFromJsonAsync<ApiResult<InboxPageDto>>(endpoint + "/inbox?unreadOnly=true", token);
        Assert.Equal(olderMessage.Id, Assert.Single(remaining.Data.Items).Id);
        using var allRead = await client.PutAsync(endpoint + "/inbox/read-all", null, token);
        allRead.EnsureSuccessStatusCode();
        using (var scope = app.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.NotNull((await database.InboxMessages.FindAsync([ownMessage.Id], token)).ReadAt);
            Assert.NotNull((await database.InboxMessages.FindAsync([olderMessage.Id], token)).ReadAt);
            Assert.Null((await database.InboxMessages.FindAsync([otherRecipient.Id], token)).ReadAt);
            Assert.Null((await database.InboxMessages.FindAsync([otherCustomer.Id], token)).ReadAt);
        }
    }

    private sealed class TestUserAuthentication(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-User", out var user))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var identity = new ClaimsIdentity(Scheme.Name);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.ToString()));
            identity.AddClaim(new Claim(IoTSharpClaimTypes.Tenant, Request.Headers["X-Test-Tenant"].ToString()));
            identity.AddClaim(new Claim(IoTSharpClaimTypes.Customer, Request.Headers["X-Test-Customer"].ToString()));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }
}
