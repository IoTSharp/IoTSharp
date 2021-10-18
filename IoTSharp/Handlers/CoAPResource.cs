using CoAP;
using CoAP.Server.Resources;
using DotNetCore.CAP;
using IoTSharp.Data;
using IoTSharp.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Handlers
{
    public class CoApResource : Resource
    {
        private readonly CoApRes _res;
        private readonly ApplicationDbContext _dbContext;
        private Int32[] _supported = new Int32[] {
            MediaType.ApplicationJson,
            MediaType.TextPlain,
            MediaType.TextXml,
            MediaType.ApplicationOctetStream
        };
        private readonly ILogger _logger;
        private readonly ICapPublisher _eventBus;

        public CoApResource(string name, ApplicationDbContext dbContext, ILogger logger, ICapPublisher eventBus)
       : base(name)
        {
            Attributes.Title = name;
            _res = (CoApRes)Enum.Parse(typeof(CoApRes), name);
            _dbContext = dbContext;
            foreach (Int32 item in _supported)
            {
                Attributes.AddContentType(item);
            }
            _logger = logger;
            _eventBus = eventBus;
            _logger.LogInformation($"CoApResource {name} is created.");
        }
        protected override void DoPost(CoapExchange exchange)
        {
            Task.Run(async () =>
            {
                try
                {
                    Int32 ct = MediaType.TextPlain;
                    Dictionary<string, object> keyValues = new Dictionary<string, object>();
                    if ((ct = MediaType.NegotiationContent(ct, _supported, exchange.Request.GetOptions(OptionType.Accept))) == MediaType.Undefined)
                    {
                        exchange.Respond(StatusCode.NotAcceptable, "supported list: ApplicationJson,TextPlain,TextXml,ApplicationOctetStream");
                        exchange.Reject();
                    }
                    else
                    {


                        if (!exchange.Request.UriQueries.Any())
                        {
                            exchange.Respond(StatusCode.BadRequest, "Forgot the parameters?");
                            exchange.Reject();
                        }
                        else
                        {
                            var querys = exchange.Request.UriQueries.ToArray();
                            var acctoken = exchange.Request.UriQueries.FirstOrDefault();
                            switch (ct)
                            {
                                case MediaType.ApplicationJson:
                                case MediaType.TextPlain:
                                    keyValues = JToken.Parse(exchange.Request.PayloadString)?.JsonToDictionary();
                                    break;
                                case MediaType.TextXml:
                                    if (querys.Length >= 2)
                                    {
                                        var xml = new System.Xml.XmlDocument();
                                        try
                                        {
                                            xml.LoadXml(exchange.Request.PayloadString);
                                        }
                                        catch (Exception ex)
                                        {
                                            exchange.Respond(StatusCode.BadRequest, $"Can't load xml ,{ex.Message}");
                                        }
                                        keyValues.Add(querys[1], xml);
                                    }
                                    else
                                    {
                                        exchange.Respond(StatusCode.BadRequest, "You did not specify  key name for xml.");
                                        exchange.Reject();
                                    }
                                    break;
                                case MediaType.ApplicationOctetStream:
                                    if (querys.Length >= 2)
                                    {
                                        keyValues.Add(querys[1], exchange.Request.Payload);
                                    }
                                    else
                                    {
                                        exchange.Respond(StatusCode.BadRequest, "You did not specify  key name for binary.");
                                        exchange.Reject();
                                    }
                                    break;
                                default:
                                    break;
                            }
                            var mcr = await _dbContext.DeviceIdentities.Include(d => d.Device).FirstOrDefaultAsync(di => di.IdentityType == IdentityType.AccessToken && di.IdentityId == acctoken);
                            var dev = mcr?.Device;
                            if (mcr != null && dev != null)
                            {
                                switch (_res)
                                {
                                    case CoApRes.Attributes:
                                        _eventBus.PublishAttributeData(new RawMsg() { MsgType = MsgType.CoAP, MsgBody = keyValues, DataCatalog = DataCatalog.AttributeData, DataSide = DataSide.ClientSide, DeviceId = dev.Id });
                                        exchange.Respond(StatusCode.Changed, $"OK");
                                        break;
                                    case CoApRes.Telemetry:
                                        _eventBus.PublishTelemetryData( new RawMsg() { MsgType = MsgType.CoAP, MsgBody = keyValues, DataCatalog = DataCatalog.AttributeData, DataSide = DataSide.ClientSide, DeviceId = dev.Id });
                                        exchange.Respond(StatusCode.Created, $"OK");
                                        break;
                                    default:
                                        break;
                                }
                                exchange.Accept();
                            }
                            else
                            {
                                exchange.Respond(StatusCode.NotFound, "Can't found  device.");
                                exchange.Reject();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    exchange.Respond(StatusCode.BadRequest, ex.Message);
                    exchange.Reject();
                }

            });
        }
        protected override void DoGet(CoapExchange exchange)
        {

        }
    }

}
