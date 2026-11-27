#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IoTSharp.Data;
using Microsoft.EntityFrameworkCore;

namespace IoTSharp.Services;

internal static class OnlineDemoCleanup
{
    private const int RowLimit = 4096;

    /// <summary>在同一个关系事务中删除演示租户的业务记录，关联表从已核对的上级 ID 追溯归属。</summary>
    internal static async Task RemoveBusinessAsync(ApplicationDbContext db, Guid tenantId, Guid[] devices, CancellationToken token)
    {
        var products = await KeysAsync(Owned(db.Products, tenantId).Select(p => p.Id), token);
        var assets = await KeysAsync(Owned(db.Assets, tenantId).Select(a => a.Id), token);
        var templates = await KeysAsync(Owned(db.CollectionTemplates, tenantId).Select(c => c.Id), token);
        var points = await KeysAsync(db.CollectionPointTemplates.Where(p => templates.Contains(p.CollectionTemplateId)).Select(p => p.Id), token);
        var rules = await KeysAsync(Owned(db.FlowRules, tenantId).Select(r => r.RuleId), token);
        var flows = await KeysAsync(Owned(db.Flows, tenantId).Select(f => f.FlowId), token);
        var events = await KeysAsync(Owned(db.BaseEvents, tenantId).Select(e => e.EventId), token);
        var subscriptions = await KeysAsync(Owned(db.SubscriptionEvents, tenantId).Select(s => s.EventId), token);
        var plans = await KeysAsync(Owned(db.ReleasePlans, tenantId).Select(p => p.Id), token);
        var releases = await KeysAsync(Owned(db.ReleasePackages, tenantId).Select(p => p.Id), token);
        var edgeTasks = await KeysAsync(Owned(db.EdgeTasks, tenantId).Select(t => t.Id), token);
        var releaseTasks = await KeysAsync(Owned(db.ReleaseTasks, tenantId).Select(t => t.Id), token);
        var versions = await KeysAsync(Owned(db.CollectionConfigurationVersions, tenantId).Select(v => v.Id), token);

        // 先检查跨租户引用。即使目标实体属于 Demo，也不能通过级联删除影响其它租户。
        await RejectForeignAsync(db.Device.Where(d => devices.Contains(EF.Property<Guid?>(d, "OwnerId") ?? Guid.Empty)
            || products.Contains(EF.Property<Guid?>(d, "ProductId") ?? Guid.Empty)), tenantId, token);
        await RejectForeignAsync(db.CollectionTemplates.Where(c => products.Contains(c.ProductId)), tenantId, token);
        await RejectForeignAsync(db.CollectionConfigurationVersions.Where(c => devices.Contains(c.GatewayId)), tenantId, token);
        await RejectForeignAsync(db.EdgeCollectionAssignments.Where(c => devices.Contains(c.GatewayId)
            || versions.Contains(c.CollectionConfigurationVersionId ?? Guid.Empty)), tenantId, token);
        await RejectForeignAsync(db.EdgeNodes.Where(n => devices.Contains(n.GatewayId)), tenantId, token);
        await RejectForeignAsync(db.EdgeTasks.Where(t => devices.Contains(t.GatewayId)), tenantId, token);
        await RejectForeignAsync(db.EdgeTaskReceipts.Where(t => devices.Contains(t.GatewayId) || edgeTasks.Contains(t.TaskId)), tenantId, token);
        await RejectForeignAsync(db.ReleasePlans.Where(p => releases.Contains(p.PackageId ?? Guid.Empty)
            || releases.Contains(p.RollbackPackageId ?? Guid.Empty)), tenantId, token);
        await RejectForeignAsync(db.ReleaseTasks.Where(t => plans.Contains(t.PlanId) || releases.Contains(t.PackageId ?? Guid.Empty)
            || edgeTasks.Contains(t.EdgeTaskId ?? Guid.Empty)), tenantId, token);
        await RejectForeignAsync(db.ReleaseReceipts.Where(r => plans.Contains(r.PlanId) || releaseTasks.Contains(r.ReleaseTaskId)), tenantId, token);
        await RejectForeignAsync(db.Flows.Where(f => rules.Contains(EF.Property<Guid?>(f, "FlowRuleRuleId") ?? Guid.Empty)), tenantId, token);
        await RejectForeignAsync(db.BaseEvents.Where(e => rules.Contains(EF.Property<Guid?>(e, "FlowRuleRuleId") ?? Guid.Empty)), tenantId, token);
        if (await db.DeviceRules.AnyAsync(r =>
                (devices.Contains(r.Device.Id) || rules.Contains(r.FlowRule.RuleId))
                && !(devices.Contains(r.Device.Id) && rules.Contains(r.FlowRule.RuleId)), token)
            || await db.DevicePortMappings.AnyAsync(m => (devices.Contains(m.SourceDeviceId) || devices.Contains(m.TargetDeviceId))
                && !(devices.Contains(m.SourceDeviceId) && devices.Contains(m.TargetDeviceId)), token))
            throw new InvalidOperationException("演示设备存在跨租户关联。");

        await RemoveAsync(db, Owned(db.ReleaseReceipts, tenantId), token);
        await RemoveAsync(db, Owned(db.ReleaseTasks, tenantId), token);
        await RemoveAsync(db, Owned(db.ReleasePlans, tenantId), token);
        await RemoveAsync(db, Owned(db.ReleasePackages, tenantId), token);
        await RemoveAsync(db, Owned(db.EdgeTaskReceipts, tenantId), token);
        await RemoveAsync(db, Owned(db.EdgeCollectionAssignments, tenantId), token);
        await RemoveAsync(db, Owned(db.EdgeTasks, tenantId), token);
        await RemoveAsync(db, Owned(db.CollectionConfigurationVersions, tenantId), token);
        await RemoveAsync(db, Owned(db.EdgeNodes, tenantId), token);
        await RemoveAsync(db, db.CollectionTransformTemplates.Where(t => points.Contains(t.PointTemplateId)), token);
        await RemoveAsync(db, db.CollectionSamplingPolicies.Where(t => points.Contains(t.PointTemplateId)), token);
        await RemoveAsync(db, db.CollectionMappingPolicies.Where(t => points.Contains(t.PointTemplateId)), token);
        await RemoveAsync(db, db.CollectionPointTemplates.Where(p => points.Contains(p.Id)), token);
        await RemoveAsync(db, db.CollectionProtocolTemplates.Where(t => templates.Contains(t.CollectionTemplateId)), token);
        await RemoveAsync(db, db.CollectionConnectionTemplates.Where(t => templates.Contains(t.CollectionTemplateId)), token);
        await RemoveAsync(db, Owned(db.CollectionTemplates, tenantId), token);
        await RemoveAsync(db, db.FlowOperations.Where(f => flows.Contains(f.Flow.FlowId) || rules.Contains(f.FlowRule.RuleId)
            || events.Contains(f.BaseEvent.EventId)), token);
        await RemoveAsync(db, Owned(db.BaseEvents, tenantId), token);
        await RemoveAsync(db, db.DeviceRules.Where(r => devices.Contains(r.Device.Id) && rules.Contains(r.FlowRule.RuleId)), token);
        await RemoveAsync(db, Owned(db.Flows, tenantId), token);
        await RemoveAsync(db, Owned(db.FlowRules, tenantId), token);
        await RemoveAsync(db, db.SubscriptionTasks.Where(s => subscriptions.Contains(s.EventId)), token);
        await RemoveAsync(db, Owned(db.SubscriptionEvents, tenantId), token);
        await RemoveAsync(db, Owned(db.RuleTaskExecutors, tenantId), token);
        await RemoveAsync(db, Owned(db.DynamicFormFieldValueInfos, tenantId), token);
        await RemoveAsync(db, Owned(db.DynamicFormFieldInfos, tenantId), token);
        await RemoveAsync(db, Owned(db.DynamicFormInfos, tenantId), token);
        await RemoveAsync(db, Owned(db.Alarms, tenantId), token);
        await RemoveAsync(db, db.AssetRelations.Where(a => assets.Contains(a.AssetId ?? Guid.Empty)), token);
        await RemoveAsync(db, Owned(db.Assets, tenantId), token);
        await RemoveAsync(db, db.TelemetryData.Where(d => devices.Contains(d.DeviceId)), token);
        await RemoveAsync(db, db.DataStorage.Where(d => devices.Contains(d.DeviceId) || products.Contains(d.DeviceId)), token);
        await RemoveAsync(db, db.DevicePortMappings.Where(m => devices.Contains(m.SourceDeviceId) && devices.Contains(m.TargetDeviceId)), token);
        await RemoveAsync(db, db.DevicePorts.Where(p => devices.Contains(p.DeviceId)), token);
        await RemoveAsync(db, db.DeviceIdentities.Where(p => devices.Contains(p.DeviceId)), token);
        await RemoveAsync(db, db.ProductDataMappings.Where(m => products.Contains(m.Product.Id)), token);
        await RemoveAsync(db, db.ProductDictionaries.Where(d => products.Contains(EF.Property<Guid?>(d, "ProductId") ?? Guid.Empty)), token);
        await RemoveAsync(db, db.ProductCommands.Where(d => products.Contains(d.ProductId)), token);
        await RemoveAsync(db, db.Device.Where(d => d.TenantId == tenantId), token);
        await RemoveAsync(db, Owned(db.Products, tenantId), token);
        await RemoveAsync(db, db.InboxMessages.Where(m => m.TenantId == tenantId), token);
        await RemoveAsync(db, db.AuditLog.Where(a => a.TenantId == tenantId), token);
        await RemoveAsync(db, db.Set<DashboardTrendSample>().Where(s => s.TenantId == tenantId), token);
        await db.SaveChangesAsync(token);
    }

    internal static IQueryable<T> Owned<T>(IQueryable<T> rows, Guid tenantId) where T : class =>
        rows.Where(row => EF.Property<Guid?>(row, "TenantId") == tenantId);

    internal static async Task<Guid[]> KeysAsync(IQueryable<Guid> keys, CancellationToken token)
    {
        var ids = await keys.Take(RowLimit + 1).ToArrayAsync(token);
        if (ids.Length > RowLimit) throw new InvalidOperationException("演示记录超出单批处理上限。");
        return ids;
    }

    internal static async Task RemoveAsync<T>(ApplicationDbContext db, IQueryable<T> rows, CancellationToken token) where T : class
    {
        var batch = await rows.Take(RowLimit + 1).ToListAsync(token);
        if (batch.Count > RowLimit) throw new InvalidOperationException($"演示记录超出处理上限 ({typeof(T).Name})。");
        db.RemoveRange(batch);
    }

    private static async Task RejectForeignAsync<T>(IQueryable<T> rows, Guid tenantId, CancellationToken token) where T : class
    {
        if (await rows.AnyAsync(row => EF.Property<Guid?>(row, "TenantId") == null
            || EF.Property<Guid?>(row, "TenantId") != tenantId, token))
            throw new InvalidOperationException($"存在跨租户引用 ({typeof(T).Name})。");
    }
}
