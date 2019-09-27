using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Extensions;
namespace IoTSharp.Handlers
{
    public class RetainedMessageHandler : IMqttServerStorage
    {
        private ApplicationDbContext _context;
        private ILogger _logger;

        public RetainedMessageHandler(ILogger<RetainedMessageHandler> logger, IServiceScopeFactory scopeFactor)
        {
            _context = scopeFactor.GetRequiredService<ApplicationDbContext>();
            _logger = logger;
        }

        public async Task<IList<MqttApplicationMessage>> LoadRetainedMessagesAsync()
        {
            await Task.CompletedTask;
            try
            {
                var lst = from m in _context.RetainedMessage select (MqttApplicationMessage)m;

                return await lst.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"load RetainedMessage error {ex.Message} ");
                return new List<MqttApplicationMessage>();
            }
        }

        public Task SaveRetainedMessagesAsync(IList<MqttApplicationMessage> messages)
        {
            Task.Factory.StartNew(() =>
          {
              _context.Database.BeginTransaction();
              try
              {
                  DateTime dateTime = DateTime.Now;
                  var needsave = from mam in messages select new RetainedMessage(mam);
                  var ids = needsave.Select(x => x.Id).ToList();
                  var dbids = _context.RetainedMessage.Select(x => x.Id).ToArray();
                  var needdelete = dbids.Except(ids);//.Except(dbids);
                  var del = from f in _context.RetainedMessage where needdelete.Contains(f.Id) select f;
                  var needadd = ids.Except(dbids);
                  var add = from f in needsave where needadd.Contains(f.Id) select f;
                  if (del.Any()) _context.RetainedMessage.RemoveRange(del);
                  if (add.Any()) _context.RetainedMessage.AddRange(add);
                  int ret = _context.SaveChanges();
                  _context.Database.CommitTransaction();
                  _logger.LogInformation($"{ret} pieces of data were saved and took {DateTime.Now.Subtract(dateTime).TotalSeconds} seconds.");
              }
              catch (Exception ex)
              {
                  _context.Database.RollbackTransaction();
                  _logger.LogError(ex, $" An exception was encountered,{ex.Message}");
              }
          }, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            return Task.CompletedTask;
        }
    }
}