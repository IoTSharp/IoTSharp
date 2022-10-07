using EasyCaching.Core;
using IoTSharp.Contracts;
using IoTSharp.Data;
using IoTSharp.EventBus;
using IoTSharp.FlowRuleEngine;
using IoTSharp.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client.Extensions.Msal;
using MQTTnet.Client;
using MQTTnet.Server;
using Storage.Net.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IoTSharp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlobStorageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MqttClientOptions _mqtt;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger _logger;
        private readonly IStorage _storage;
        private readonly MqttServer _serverEx;
        private readonly AppSettings _setting;
        private readonly IPublisher _queue;
        private readonly FlowRuleProcessor _flowRuleProcessor;
        private readonly IEasyCachingProvider _caching;
        private readonly IServiceScopeFactory _scopeFactor;
        private readonly IBlobStorage _blob;

        public BlobStorageController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ILogger<DevicesController> logger, MqttServer serverEx, ApplicationDbContext context, MqttClientOptions mqtt, IStorage storage, IOptions<AppSettings> options, IPublisher queue
            , IEasyCachingProviderFactory factory, FlowRuleProcessor flowRuleProcessor, IServiceScopeFactory scopeFactor, IBlobStorage blob)
        {
            string _hc_Caching = $"{nameof(CachingUseIn)}-{Enum.GetName(options.Value.CachingUseIn)}";
            _context = context;
            _mqtt = mqtt;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _storage = storage;
            _serverEx = serverEx;
            _setting = options.Value;
            _queue = queue;
            _flowRuleProcessor = flowRuleProcessor;
            _caching = factory.GetCachingProvider(_hc_Caching);
            _scopeFactor = scopeFactor;
            _blob = blob;
        }

        [HttpGet]
        public async Task<ApiResult<List<Blob>>> List()
        {
            var lst = await _blob.ListAsync();
            return new ApiResult<List<Blob>>(ApiCode.Success, "OK", lst.ToList());
        }

        [HttpGet()]
        [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResult>> Download([FromQuery] string fullpath)
        {
            var blob = await _blob.GetBlobAsync(fullpath);
            if (blob.IsFile)
            {
                using (var stream = new MemoryStream())
                {
                    using (Stream ss = await _blob.OpenReadAsync(fullpath))
                    {
                        await ss.CopyToAsync(stream);
                    }
                    return File(stream, MimeMapping.MimeUtility.GetMimeMapping(blob.Name), blob.Name);
                }
            }
            else
            {
                return NotFound(new ApiResult(ApiCode.NotFile, "不文件类型"));
            }
        }

        [HttpPost()]
        public async Task<ApiResult<Dictionary<string, string>>> Upload([FromQuery] string path, List<IFormFile> files)
        {
            long size = files.Sum(f => f.Length);
            var result = new ApiResult<Dictionary<string, string>>(ApiCode.Success, "OK", new Dictionary<string, string>());
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var fi = new FileInfo(Path.GetTempFileName());
                    try
                    {
                        using (var stream = fi.OpenWrite())
                        {
                            await formFile.CopyToAsync(stream);
                        }
                        await _blob.WriteFileAsync($"{path}/{formFile.FileName}", fi.FullName);
                        result.Data.Add(formFile.FileName, "OK");
                    }
                    catch (Exception ex)
                    {
                        result.Data.Add(formFile.FileName, ex.Message);
                    }
                    finally
                    {
                        fi.Delete();
                    }
                }
                else
                {
                    result.Data.Add(formFile.FileName, "空文件");
                }

            }
            return result;
        }

        [HttpPut()]
        public async Task<ApiResult> Modify([FromQuery] string path, IFormFile file)
        {
            var result = new ApiResult(ApiCode.Success, "OK");
            if (file.Length > 0)
            {
                var fi = new FileInfo(Path.GetTempFileName());
                try
                {
                    using (var stream = fi.OpenWrite())
                    {
                        await file.CopyToAsync(stream);
                    }
                    await _blob.WriteFileAsync($"{path}/{file.FileName}", fi.FullName);
                    result.Msg = "OK";
                }
                catch (Exception ex)
                {
                    result.Code = (int)ApiCode.Exception;
                    result.Msg = ex.Message;
                }
                finally
                {
                    fi.Delete();
                }
            }
            else
            {
                result.Code = (int)ApiCode.Empty;
                result.Msg = "空文件";
            }
            return result;
        }

        [HttpDelete()]
        public async Task<ApiResult> Delete([FromQuery] string fullpath)
        {
            await _blob.DeleteAsync(fullpath);
            return new ApiResult(ApiCode.Success, "OK");
        }
    }
}
