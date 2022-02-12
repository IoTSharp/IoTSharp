using EasyCaching.Core;
using IoTSharp.Controllers.Models;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IoTSharp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class I18NController : ControllerBase
    {
        private readonly ApplicationDBInitializer _dBInitializer;
        private readonly IEasyCachingProvider _cachingprovider;
        private readonly IOptions<BaiduTranslateProfile> _profile;
        private ApplicationDbContext _context;

        public I18NController(ApplicationDbContext context, IEasyCachingProvider provider, IOptions<BaiduTranslateProfile> profile, ApplicationDBInitializer dBInitializer)
        {
            this._cachingprovider = provider;
            this._context = context;
            this._profile = profile;
            _dBInitializer = dBInitializer;
        }

        [HttpGet("[action]")]
        [AllowAnonymous]
        public async Task<ApiResult<Dictionary<string, string>>> Current(string lang)
        {
            lang = lang?.ToLower();
            var _cachedi18n = await _cachingprovider.GetAsync<BaseI18N[]>("i18n");
            var i18n = _cachedi18n?.Value;
            if (i18n == null)
            {
                i18n = _context.BaseI18Ns.ToArray();
                if (i18n.Length == 0)
                {
                    await _dBInitializer.SeedI18N();
                    i18n = _context.BaseI18Ns.ToArray();
                }
                _cachingprovider.Set<BaseI18N[]>("i18n", i18n, TimeSpan.FromMinutes(5));
            }
            switch (lang)
            {
                case "el-gr":
                    return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValueENGR }).ToDictionary(x => x.KeyName, y => y.ValueENGR));

                case "en-us":
                    return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValueENUS }).ToDictionary(x => x.KeyName, y => y.ValueENUS));

                case "fr-fr":
                    return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValueFRFR }).ToDictionary(x => x.KeyName, y => y.ValueFRFR));

                case "hr-hr":
                    return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValueHRHR }).ToDictionary(x => x.KeyName, y => y.ValueHRHR));

                case "ko-kr":
                    return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValueKOKR }).ToDictionary(x => x.KeyName, y => y.ValueKOKR));

                case "pl-pl":
                    return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValuePLPL }).ToDictionary(x => x.KeyName, y => y.ValuePLPL));

                case "sl-sl":
                    return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValueSLSL }).ToDictionary(x => x.KeyName, y => y.ValueSLSL));

                case "tr-tr":
                    return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValueTRTR }).ToDictionary(x => x.KeyName, y => y.ValueTRTR));

                case "zh-tw":
                    return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValueZHTW }).ToDictionary(x => x.KeyName, y => y.ValueZHTW));

                case "zh-cn":
                    return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValueZHCN }).ToDictionary(x => x.KeyName, y => y.ValueZHCN));
            }

            return new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", i18n.Select(c => new { c.KeyName, c.ValueZHCN }).ToDictionary(x => x.KeyName, y => y.ValueZHCN));
        }

        [HttpPost("[action]")]
        public ApiResult<PagedData<BaseI18N>> Index([FromBody] I18NParam m)
        {
            Expression<Func<BaseI18N, bool>> condition = x => x.Status > -1;
            if (!string.IsNullOrEmpty(m.KeyName))
            {
                condition = condition.And(x => x.KeyName.Contains(m.KeyName));
            }
            return new ApiResult<PagedData<BaseI18N>>(ApiCode.Success, "OK", new PagedData<BaseI18N>
            {
                total = _context.BaseI18Ns.Count(condition),
                rows = _context.BaseI18Ns.OrderByDescending(c => c.Id).Where(condition).Skip((m.offset) * m.limit).Take(m.limit).ToList()
            });
        }

        [HttpGet("[action]")]
        public ApiResult<BaseI18N> Get(int id)
        {
            var i18N = _context.BaseI18Ns.SingleOrDefault(c => c.Id == id);
            if (i18N != null)
            {
                return new ApiResult<BaseI18N>(ApiCode.Success, "OK", i18N);
            }
            else
            {
                return new ApiResult<BaseI18N>(ApiCode.CantFindObject, "can't find this object", null);
            }
        }

        [HttpGet("[action]")]
        public ApiResult<bool> CheckExist(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return new ApiResult<bool>(ApiCode.Success, "OK", false);
            }
            else
            {
                return new ApiResult<bool>(ApiCode.Success, "OK", _context.BaseI18Ns.Any(c => c.KeyName.ToLower() == key.ToLower()));
            }
        }

        [HttpPost("[action]")]
        public ApiResult<bool> Save(BaseI18N m)
        {
            var i18n = new BaseI18N()
            {
                AddDate = DateTime.Now,

                Status = 1,
                KeyName = m.KeyName,
                ValueDEDE = m.ValueDEDE,
                ValueESES = m.ValueESES,
                ValueENUS = m.ValueENUS,
                ValueENGR = m.ValueENGR,
                ValueELGR = m.ValueELGR,
                ValueFRFR = m.ValueFRFR,
                ValueHRHR = m.ValueHRHR,
                ValueITIT = m.ValueITIT,
                ValueJAJP = m.ValueJAJP,
                ValueKOKR = m.ValueKOKR,
                ValuePLPL = m.ValuePLPL,
                ValueSLSL = m.ValueSLSL,
                ValueTRTR = m.ValueTRTR,
                ValueZHCN = m.ValueZHCN,
                ValueZHTW = m.ValueZHTW,
                ValueBG = m.ValueBG,
                ValueCS = m.ValueCS,
                ValueDA = m.ValueDA,
                ValueFI = m.ValueFI,
                ValueHE = m.ValueHE,
                ValueHU = m.ValueHU,
                ValueNL = m.ValueNL,
                ValueSR = m.ValueSR,
                ValueSV = m.ValueSV,
                ValueUK = m.ValueUK,
                ValueVI = m.ValueVI,
            };

            _context.BaseI18Ns.Add(i18n);
            _context.SaveChanges();
            return new ApiResult<bool>(ApiCode.CantFindObject, "OK", true);
        }

        [HttpGet("[action]")]
        public ApiResult<bool> Delete(long id)
        {
            var i18n = _context.BaseI18Ns.FirstOrDefault(c => c.Id == id);

            if (i18n != null)
            {
                i18n.Status = -1;
                _context.BaseI18Ns.Update(i18n);
                _context.SaveChanges();

                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }

            return new ApiResult<bool>(ApiCode.CantFindObject, "can't find this object", false);
        }

        [HttpGet("[action]")]
        public ApiResult<bool> SetStatus(int id)
        {
            var obj = _context.BaseI18Ns.FirstOrDefault(c => c.Id == id);
            if (obj != null)
            {
                obj.Status = obj.Status == 1 ? 0 : 1;
                _context.BaseI18Ns.Update(obj);
                _context.SaveChanges();
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            return new ApiResult<bool>(ApiCode.CantFindObject, "can't find this object", false);
        }

        [HttpPost("[action]")]
        public ApiResult<bool> Update(BaseI18N m)
        {
            var i18n = _context.BaseI18Ns.FirstOrDefault(c => c.Id == m.Id);
            if (i18n != null)
            {
                i18n.KeyName = m.KeyName;
                i18n.ValueDEDE = m.ValueDEDE;
                i18n.ValueESES = m.ValueESES;
                i18n.ValueENUS = m.ValueENUS;
                i18n.ValueENGR = m.ValueENGR;
                i18n.ValueELGR = m.ValueELGR;
                i18n.ValueFRFR = m.ValueFRFR;
                i18n.ValueHRHR = m.ValueHRHR;
                i18n.ValueITIT = m.ValueITIT;
                i18n.ValueJAJP = m.ValueJAJP;
                i18n.ValueKOKR = m.ValueKOKR;
                i18n.ValuePLPL = m.ValuePLPL;
                i18n.ValueSLSL = m.ValueSLSL;
                i18n.ValueTRTR = m.ValueTRTR;
                i18n.ValueZHCN = m.ValueZHCN;
                i18n.ValueZHTW = m.ValueZHTW;
                i18n.ValueBG = m.ValueBG;
                i18n.ValueCS = m.ValueCS;
                i18n.ValueDA = m.ValueDA;
                i18n.ValueFI = m.ValueFI;
                i18n.ValueHE = m.ValueHE;
                i18n.ValueHU = m.ValueHU;
                i18n.ValueNL = m.ValueNL;
                i18n.ValueSR = m.ValueSR;
                i18n.ValueSV = m.ValueSV;
                i18n.ValueUK = m.ValueUK;
                i18n.ValueVI = m.ValueVI;
                _context.BaseI18Ns.Update(i18n);
                _context.SaveChanges();

                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }

            return new ApiResult<bool>(ApiCode.CantFindObject, "can't find this object", false);
        }

        [HttpGet("[action]")]
        public async Task<ApiResult<List<BaiduTranslateResult>>> Translate(string Words)
        {
            string q = Words;
            string from = _profile.Value.DefaultLang ?? "zh";
            string appId = _profile.Value.AppKey;
            Random rd = new Random();
            string secretKey = _profile.Value.AppSecret;
            int _wait = _profile.Value.ApiInterval ?? 80;
            List<BaiduTranslateResult> l = new List<BaiduTranslateResult>();
            //百度限流了，不要搞太多
            foreach (var item in _profile.Value.LangFieldMapping)
            {
                string to = item.Target;
                using (HttpClient client = new HttpClient())
                {
                    string salt = rd.Next(100000).ToString();
                    string sign = EncryptString(appId + q + salt + secretKey);
                    string url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
                    url += "q=" + HttpUtility.UrlEncode(q);
                    url += "&from=" + from;
                    url += "&to=" + to;
                    url += "&appid=" + appId;
                    url += "&salt=" + salt;
                    url += "&sign=" + sign;
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    responseBody = responseBody.TrimStart('\n');
                    l.Add(JsonConvert.DeserializeObject<BaiduTranslateResult>(responseBody));
                }
            }
            return new ApiResult<List<BaiduTranslateResult>>(ApiCode.Success, "OK", l);
        }

        public static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            byte[] byteNew = md5.ComputeHash(byteOld);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in byteNew)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}