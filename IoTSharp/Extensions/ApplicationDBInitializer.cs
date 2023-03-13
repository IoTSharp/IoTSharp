using IoTSharp.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IoTSharp.Models.FormFieldTypes;
using IoTSharp.Contracts;

namespace IoTSharp.Data
{
    public class ApplicationDBInitializer
    {
        private readonly RoleManager<IdentityRole> _role;

        private ApplicationDbContext _context;
        private ILogger _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;

        public ApplicationDBInitializer(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration, ILogger<ApplicationDBInitializer> logger,
            ApplicationDbContext context, RoleManager<IdentityRole> role
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _role = role;
        }

        public async Task SeedRoleAsync()
        {
            var roles = new IdentityRole[]{new IdentityRole(nameof(UserRole.Anonymous))
                    , new IdentityRole(nameof(UserRole.NormalUser))
                    , new IdentityRole(nameof(UserRole.CustomerAdmin))
                    , new IdentityRole(nameof(UserRole.TenantAdmin))
                    , new IdentityRole(nameof(UserRole.SystemAdmin)) };
            foreach (var role in roles)
            {
                if (!await _role.RoleExistsAsync(role.Name))
                {
                    if ((await _role.CreateAsync(role)).Succeeded)
                    {
                        await _role.UpdateNormalizedRoleNameAsync(role);
                    }
                };
            }
        }

         

        public async Task SeedDictionary()
        {
            var controltype = this._context.BaseDictionaryGroups.Add(new BaseDictionaryGroup { DictionaryGroupName = "控件类型",  DictionaryGroupStatus = 1});
            this._context.SaveChanges();
            var datatype = this._context.BaseDictionaryGroups.Add(new BaseDictionaryGroup { DictionaryGroupName = "数据类型", DictionaryGroupStatus = 1 });
            this._context.SaveChanges();

            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "bool", DictionaryTag = typeof(bool).FullName, DictionaryValue = "13", Dictionary18NKeyName = "dic.types.bool", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "decimal[]", DictionaryTag = typeof(decimal[]).FullName, DictionaryValue = "12", Dictionary18NKeyName = "dic.types.decimalarray", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "datetime[]", DictionaryTag = typeof(DateTime[]).FullName, DictionaryValue = "11", Dictionary18NKeyName = "dic.types.datetimearray", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "single数组", DictionaryTag = typeof(Single[]).FullName, DictionaryValue = "10", Dictionary18NKeyName = "dic.types.singlearray", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "single", DictionaryTag = typeof(Single[]).FullName, DictionaryValue = "9", Dictionary18NKeyName = "dic.types.single", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "double数组", DictionaryTag = typeof(double[]).FullName, DictionaryValue = "8", Dictionary18NKeyName = "dic.types.doublearray", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "string[]", DictionaryTag = typeof(string[]).FullName, DictionaryValue = "7", Dictionary18NKeyName = "dic.types.stringarray", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "int[]", DictionaryTag = typeof(int[]).FullName, DictionaryValue = "6", Dictionary18NKeyName = "dic.types.intarray", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "评分", DictionaryValue = "19", Dictionary18NKeyName = "dic.controltypes.rate", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "富文本编辑器", DictionaryValue = "18", Dictionary18NKeyName = "dic.controltypes.editor", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "多行文本", DictionaryValue = "17", Dictionary18NKeyName = "dic.controltypes.textarea", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "范围(数字)", DictionaryValue = "11", Dictionary18NKeyName = "dic.controltypes.range", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "文件上传", DictionaryValue = "16", Dictionary18NKeyName = "dic.controltypes.uploader", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "穿梭框", DictionaryValue = "15", Dictionary18NKeyName = "dic.controltypes.transfer", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "树形选择器", DictionaryValue = "14", Dictionary18NKeyName = "dic.controltypes.treeselect", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "monaco编辑器", DictionaryValue = "20", Dictionary18NKeyName = "dic.controltypes.monacoeditor", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });

            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "级联选择器", DictionaryValue = "13", Dictionary18NKeyName = "dic.controltypes.cascader", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "下拉列表(select)", DictionaryValue = "12", Dictionary18NKeyName = "dic.controltypes.select", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "文本框(markdown)", DictionaryValue = "4", Dictionary18NKeyName = "dic.controltypes.markdown", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "时间", DictionaryValue = "10", Dictionary18NKeyName = "dic.controltypes.time", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "日期", DictionaryValue = "9", Dictionary18NKeyName = "dic.controltypes.date", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "Radio单选", DictionaryValue = "8", Dictionary18NKeyName = "dic.controltypes.radioselect", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "布尔值(Radio)", DictionaryValue = "5", Dictionary18NKeyName = "dic.controltypes.radio", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "多选框", DictionaryValue = "6", Dictionary18NKeyName = "dic.controltypes.checkbox", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "数字", DictionaryValue = "1", Dictionary18NKeyName = "dic.controltypes.number", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "文本框", DictionaryValue = "2", Dictionary18NKeyName = "dic.controltypes.textbox", DictionaryStatus = 1, DictionaryGroupId = controltype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "DateTime", DictionaryTag = typeof(DateTime).FullName, DictionaryValue = "5", Dictionary18NKeyName = "dic.types.DateTime", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "string", DictionaryTag = typeof(string).FullName, DictionaryValue = "4", Dictionary18NKeyName = "dic.types.string", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "decimal", DictionaryTag = typeof(decimal).FullName, DictionaryValue = "3", Dictionary18NKeyName = "dic.types.decimal", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "double", DictionaryTag = typeof(double).FullName, DictionaryValue = "2", Dictionary18NKeyName = "dic.types.double", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "int", DictionaryTag = typeof(int).FullName, DictionaryValue = "1", Dictionary18NKeyName = "dic.types.int", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            //唯一的自定义类型
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "文件数组", DictionaryTag = typeof(NzUploadFile[]).FullName, DictionaryValue = "14", Dictionary18NKeyName = "dic.types.filearray", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "DateTimeOffset", DictionaryTag = typeof(DateTimeOffset).FullName, DictionaryValue = "15", Dictionary18NKeyName = "dic.types.datetimeoffset", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });
            this._context.BaseDictionaries.Add(new BaseDictionary { DictionaryName = "DateTimeOffset[]", DictionaryTag = typeof(DateTimeOffset[]).FullName, DictionaryValue = "16", Dictionary18NKeyName = "dic.types.datetimeoffsetarray", DictionaryStatus = 1, DictionaryGroupId = datatype.Entity.DictionaryGroupId, DictionaryDesc = "", DictionaryIcon = "" });

            await this._context.SaveChangesAsync();

        }


        public async Task SeedUserAsync(InstallDto model)
        {
            var tenant = _context.Tenant.FirstOrDefault(t => t.Email == model.TenantEMail && t.Deleted==false);
            var customer = _context.Customer.FirstOrDefault(t => t.Email == model.CustomerEMail && t.Deleted==false);
            if (tenant == null && customer == null)
            {
                tenant = new Tenant() { Id = Guid.NewGuid(), Name = model.TenantName, Email = model.TenantEMail };
                customer = new Customer() { Id = Guid.NewGuid(), Name = model.CustomerName, Email = model.CustomerEMail };
                customer.Tenant = tenant;
                tenant.Customers = new List<Customer>();
                tenant.Customers.Add(customer);
                _context.Tenant.Add(tenant);
                _context.Customer.Add(customer);
                await _context.SaveChangesAsync();
            }
            IdentityUser user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                user = new IdentityUser
                {
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    await _signInManager.UserManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, model.Email));
                    await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Customer, customer.Id.ToString()));
                    await _signInManager.UserManager.AddClaimAsync(user, new Claim(IoTSharpClaimTypes.Tenant, tenant.Id.ToString()));
                    await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.Anonymous));
                    await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.NormalUser));
                    await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.CustomerAdmin));
                    await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.TenantAdmin));
                    await _signInManager.UserManager.AddToRoleAsync(user, nameof(UserRole.SystemAdmin));

                }
                else
                {
                    throw new Exception(string.Join(',', result.Errors.ToList().Select(ie => $"code={ie.Code},msg={ie.Description}")));
                }
            }
            var rship = new Relationship
            {
                IdentityUser = _context.Users.Find(user.Id),
                Customer = _context.Customer.Find(customer.Id),
                Tenant = _context.Tenant.Find(tenant.Id)
            };
            _context.Add(rship);
            await _context.SaveChangesAsync();
        }
    }
}