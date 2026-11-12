using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.SonnetDB.Migrations
{
    /// <summary>
    /// 将历史日期列和既有长度声明对齐 SonnetDB 5.0 的原生时间类型与模型约束。
    /// </summary>
    public partial class AlignSonnetDb5Model : Migration
    {
        /// <summary>
        /// 升级日期列以保留 UTC ticks，并为已有长度声明补齐约束；执行前应验证停写备份。
        /// </summary>
        /// <param name="migrationBuilder">当前数据库提供程序的迁移构建器。</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "TelemetryData",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "TelemetryData",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                table: "SubscriptionEvents",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TesterDateTime",
                table: "RuleTaskExecutors",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddDateTime",
                table: "RuleTaskExecutors",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReleaseTasks",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReceiptAt",
                table: "ReleaseTasks",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DispatchedAt",
                table: "ReleaseTasks",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ReleaseTasks",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "ReleaseTasks",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReportedAt",
                table: "ReleaseReceipts",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReceivedAt",
                table: "ReleaseReceipts",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReleasePlans",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "ReleasePlans",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ReleasePlans",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "ReleasePlans",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReleasePackages",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ReleasePackages",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "RefreshTokens",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddedDate",
                table: "RefreshTokens",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                table: "ProductCommands",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TesterDateTime",
                table: "Flows",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Flows",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatTime",
                table: "FlowRules",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddDate",
                table: "FlowOperations",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EdgeTasks",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "EdgeTasks",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "EdgeTasks",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReceiptAt",
                table: "EdgeTasks",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpireAt",
                table: "EdgeTasks",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "EdgeTasks",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "EdgeTasks",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AcceptedAt",
                table: "EdgeTasks",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReportedAt",
                table: "EdgeTaskReceipts",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReceivedAt",
                table: "EdgeTaskReceipts",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EdgeNodes",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastRegistrationDateTime",
                table: "EdgeNodes",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastHeartbeatDateTime",
                table: "EdgeNodes",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "EdgeNodes",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ConfigurationOperationExpiresAt",
                table: "EdgeNodes",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevokedAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastPulledAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastExecutionAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AppliedAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FromCreateDate",
                table: "DynamicFormInfos",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FieldCreateDate",
                table: "DynamicFormFieldValueInfos",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FieldEditDate",
                table: "DynamicFormFieldInfos",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FieldCreateDate",
                table: "DynamicFormFieldInfos",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ConfigDateTime",
                table: "DeviceRules",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "DevicePorts",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "DevicePortMappings",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                table: "DeviceModels",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                table: "DeviceModelCommands",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "DataStorage",
                type: "DATETIME2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "DataStorage",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CollectionTemplates",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CollectionTemplates",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CollectionConfigVersions",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CollectionConfigVersions",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreaterDateTime",
                table: "BaseEvents",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActiveDateTime",
                table: "AuditLog",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "AspNetUsers",
                type: "DATETIMEOFFSET",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIME",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDateTime",
                table: "Alarms",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDateTime",
                table: "Alarms",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClearDateTime",
                table: "Alarms",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AckDateTime",
                table: "Alarms",
                type: "DATETIME2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleaseTasks_InstanceId_MaxLength",
                table: "ReleaseTasks",
                sql: "\"InstanceId\" IS NULL OR char_length(\"InstanceId\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleaseTasks_Message_MaxLength",
                table: "ReleaseTasks",
                sql: "\"Message\" IS NULL OR char_length(\"Message\") <= 1024");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleaseTasks_RuntimeType_MaxLength",
                table: "ReleaseTasks",
                sql: "\"RuntimeType\" IS NULL OR char_length(\"RuntimeType\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleaseTasks_TargetKey_MaxLength",
                table: "ReleaseTasks",
                sql: "\"TargetKey\" IS NULL OR char_length(\"TargetKey\") <= 450");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleaseReceipts_InstanceId_MaxLength",
                table: "ReleaseReceipts",
                sql: "\"InstanceId\" IS NULL OR char_length(\"InstanceId\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleaseReceipts_Message_MaxLength",
                table: "ReleaseReceipts",
                sql: "\"Message\" IS NULL OR char_length(\"Message\") <= 1024");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleaseReceipts_RuntimeType_MaxLength",
                table: "ReleaseReceipts",
                sql: "\"RuntimeType\" IS NULL OR char_length(\"RuntimeType\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleaseReceipts_TargetKey_MaxLength",
                table: "ReleaseReceipts",
                sql: "\"TargetKey\" IS NULL OR char_length(\"TargetKey\") <= 450");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePlans_CreatedBy_MaxLength",
                table: "ReleasePlans",
                sql: "\"CreatedBy\" IS NULL OR char_length(\"CreatedBy\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePlans_Description_MaxLength",
                table: "ReleasePlans",
                sql: "\"Description\" IS NULL OR char_length(\"Description\") <= 1024");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePlans_Name_MaxLength",
                table: "ReleasePlans",
                sql: "\"Name\" IS NULL OR char_length(\"Name\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePlans_UpdatedBy_MaxLength",
                table: "ReleasePlans",
                sql: "\"UpdatedBy\" IS NULL OR char_length(\"UpdatedBy\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_BlobPath_MaxLength",
                table: "ReleasePackages",
                sql: "\"BlobPath\" IS NULL OR char_length(\"BlobPath\") <= 1024");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_ContentType_MaxLength",
                table: "ReleasePackages",
                sql: "\"ContentType\" IS NULL OR char_length(\"ContentType\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_ContractVersion_MaxLength",
                table: "ReleasePackages",
                sql: "\"ContractVersion\" IS NULL OR char_length(\"ContractVersion\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_CreatedBy_MaxLength",
                table: "ReleasePackages",
                sql: "\"CreatedBy\" IS NULL OR char_length(\"CreatedBy\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_DownloadToken_MaxLength",
                table: "ReleasePackages",
                sql: "\"DownloadToken\" IS NULL OR char_length(\"DownloadToken\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_FileName_MaxLength",
                table: "ReleasePackages",
                sql: "\"FileName\" IS NULL OR char_length(\"FileName\") <= 512");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_Name_MaxLength",
                table: "ReleasePackages",
                sql: "\"Name\" IS NULL OR char_length(\"Name\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_PackageKey_MaxLength",
                table: "ReleasePackages",
                sql: "\"PackageKey\" IS NULL OR char_length(\"PackageKey\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_Sha256_MaxLength",
                table: "ReleasePackages",
                sql: "\"Sha256\" IS NULL OR char_length(\"Sha256\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_TargetRuntimeType_MaxLength",
                table: "ReleasePackages",
                sql: "\"TargetRuntimeType\" IS NULL OR char_length(\"TargetRuntimeType\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_TargetRuntimeVersion_MaxLength",
                table: "ReleasePackages",
                sql: "\"TargetRuntimeVersion\" IS NULL OR char_length(\"TargetRuntimeVersion\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_UpdatedBy_MaxLength",
                table: "ReleasePackages",
                sql: "\"UpdatedBy\" IS NULL OR char_length(\"UpdatedBy\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ReleasePackages_Version_MaxLength",
                table: "ReleasePackages",
                sql: "\"Version\" IS NULL OR char_length(\"Version\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeTasks_ContractVersion_MaxLength",
                table: "EdgeTasks",
                sql: "\"ContractVersion\" IS NULL OR char_length(\"ContractVersion\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeTasks_InstanceId_MaxLength",
                table: "EdgeTasks",
                sql: "\"InstanceId\" IS NULL OR char_length(\"InstanceId\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeTasks_Message_MaxLength",
                table: "EdgeTasks",
                sql: "\"Message\" IS NULL OR char_length(\"Message\") <= 1024");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeTasks_RuntimeType_MaxLength",
                table: "EdgeTasks",
                sql: "\"RuntimeType\" IS NULL OR char_length(\"RuntimeType\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeTasks_TargetKey_MaxLength",
                table: "EdgeTasks",
                sql: "\"TargetKey\" IS NULL OR char_length(\"TargetKey\") <= 450");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeTaskReceipts_ContractVersion_MaxLength",
                table: "EdgeTaskReceipts",
                sql: "\"ContractVersion\" IS NULL OR char_length(\"ContractVersion\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeTaskReceipts_InstanceId_MaxLength",
                table: "EdgeTaskReceipts",
                sql: "\"InstanceId\" IS NULL OR char_length(\"InstanceId\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeTaskReceipts_Message_MaxLength",
                table: "EdgeTaskReceipts",
                sql: "\"Message\" IS NULL OR char_length(\"Message\") <= 1024");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeTaskReceipts_RuntimeType_MaxLength",
                table: "EdgeTaskReceipts",
                sql: "\"RuntimeType\" IS NULL OR char_length(\"RuntimeType\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeTaskReceipts_TargetKey_MaxLength",
                table: "EdgeTaskReceipts",
                sql: "\"TargetKey\" IS NULL OR char_length(\"TargetKey\") <= 450");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeNodes_ConfigurationOperationOwner_MaxLength",
                table: "EdgeNodes",
                sql: "\"ConfigurationOperationOwner\" IS NULL OR char_length(\"ConfigurationOperationOwner\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_AppliedConfigurationHash_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"AppliedConfigurationHash\" IS NULL OR char_length(\"AppliedConfigurationHash\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_ConfigurationHash_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"ConfigurationHash\" IS NULL OR char_length(\"ConfigurationHash\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_ContractVersion_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"ContractVersion\" IS NULL OR char_length(\"ContractVersion\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_CreatedBy_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"CreatedBy\" IS NULL OR char_length(\"CreatedBy\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_InstanceId_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"InstanceId\" IS NULL OR char_length(\"InstanceId\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_LastExecutionMessage_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"LastExecutionMessage\" IS NULL OR char_length(\"LastExecutionMessage\") <= 1024");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_RuntimeType_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"RuntimeType\" IS NULL OR char_length(\"RuntimeType\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_SourceId_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"SourceId\" IS NULL OR char_length(\"SourceId\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_SourceType_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"SourceType\" IS NULL OR char_length(\"SourceType\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_SourceVersion_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"SourceVersion\" IS NULL OR char_length(\"SourceVersion\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_TargetKey_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"TargetKey\" IS NULL OR char_length(\"TargetKey\") <= 450");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EdgeCollectionAssignments_UpdatedBy_MaxLength",
                table: "EdgeCollectionAssignments",
                sql: "\"UpdatedBy\" IS NULL OR char_length(\"UpdatedBy\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionTemplates_CreatedBy_MaxLength",
                table: "CollectionTemplates",
                sql: "\"CreatedBy\" IS NULL OR char_length(\"CreatedBy\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionTemplates_Name_MaxLength",
                table: "CollectionTemplates",
                sql: "\"Name\" IS NULL OR char_length(\"Name\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionTemplates_SemanticModelId_MaxLength",
                table: "CollectionTemplates",
                sql: "\"SemanticModelId\" IS NULL OR char_length(\"SemanticModelId\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionTemplates_TemplateKey_MaxLength",
                table: "CollectionTemplates",
                sql: "\"TemplateKey\" IS NULL OR char_length(\"TemplateKey\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionTemplates_UpdatedBy_MaxLength",
                table: "CollectionTemplates",
                sql: "\"UpdatedBy\" IS NULL OR char_length(\"UpdatedBy\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionSamplingPolicies_AggregateHint_MaxLength",
                table: "CollectionSamplingPolicies",
                sql: "\"AggregateHint\" IS NULL OR char_length(\"AggregateHint\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionSamplingPolicies_Group_MaxLength",
                table: "CollectionSamplingPolicies",
                sql: "\"Group\" IS NULL OR char_length(\"Group\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionProtocolTemplates_ProtocolKind_MaxLength",
                table: "CollectionProtocolTemplates",
                sql: "\"ProtocolKind\" IS NULL OR char_length(\"ProtocolKind\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_Address_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"Address\" IS NULL OR char_length(\"Address\") <= 512");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_BindingId_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"BindingId\" IS NULL OR char_length(\"BindingId\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_ConnectionKey_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"ConnectionKey\" IS NULL OR char_length(\"ConnectionKey\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_DisplayName_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"DisplayName\" IS NULL OR char_length(\"DisplayName\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_FieldPath_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"FieldPath\" IS NULL OR char_length(\"FieldPath\") <= 512");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_Name_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"Name\" IS NULL OR char_length(\"Name\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_PointKey_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"PointKey\" IS NULL OR char_length(\"PointKey\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_Quantity_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"Quantity\" IS NULL OR char_length(\"Quantity\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_RawValueType_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"RawValueType\" IS NULL OR char_length(\"RawValueType\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_SemanticId_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"SemanticId\" IS NULL OR char_length(\"SemanticId\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_SourceType_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"SourceType\" IS NULL OR char_length(\"SourceType\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionPointTemplates_Unit_MaxLength",
                table: "CollectionPointTemplates",
                sql: "\"Unit\" IS NULL OR char_length(\"Unit\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionMappingPolicies_DisplayName_MaxLength",
                table: "CollectionMappingPolicies",
                sql: "\"DisplayName\" IS NULL OR char_length(\"DisplayName\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionMappingPolicies_Group_MaxLength",
                table: "CollectionMappingPolicies",
                sql: "\"Group\" IS NULL OR char_length(\"Group\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionMappingPolicies_TargetName_MaxLength",
                table: "CollectionMappingPolicies",
                sql: "\"TargetName\" IS NULL OR char_length(\"TargetName\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionMappingPolicies_Unit_MaxLength",
                table: "CollectionMappingPolicies",
                sql: "\"Unit\" IS NULL OR char_length(\"Unit\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConnectionTemplates_AuthType_MaxLength",
                table: "CollectionConnectionTemplates",
                sql: "\"AuthType\" IS NULL OR char_length(\"AuthType\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConnectionTemplates_ConnectionKey_MaxLength",
                table: "CollectionConnectionTemplates",
                sql: "\"ConnectionKey\" IS NULL OR char_length(\"ConnectionKey\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConnectionTemplates_ConnectionName_MaxLength",
                table: "CollectionConnectionTemplates",
                sql: "\"ConnectionName\" IS NULL OR char_length(\"ConnectionName\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConnectionTemplates_EndpointRef_MaxLength",
                table: "CollectionConnectionTemplates",
                sql: "\"EndpointRef\" IS NULL OR char_length(\"EndpointRef\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConnectionTemplates_Host_MaxLength",
                table: "CollectionConnectionTemplates",
                sql: "\"Host\" IS NULL OR char_length(\"Host\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConnectionTemplates_SerialPort_MaxLength",
                table: "CollectionConnectionTemplates",
                sql: "\"SerialPort\" IS NULL OR char_length(\"SerialPort\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConnectionTemplates_Transport_MaxLength",
                table: "CollectionConnectionTemplates",
                sql: "\"Transport\" IS NULL OR char_length(\"Transport\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConfigVersions_ConfigurationHash_MaxLength",
                table: "CollectionConfigVersions",
                sql: "\"ConfigurationHash\" IS NULL OR char_length(\"ConfigurationHash\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConfigVersions_ContractVersion_MaxLength",
                table: "CollectionConfigVersions",
                sql: "\"ContractVersion\" IS NULL OR char_length(\"ContractVersion\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConfigVersions_CreatedBy_MaxLength",
                table: "CollectionConfigVersions",
                sql: "\"CreatedBy\" IS NULL OR char_length(\"CreatedBy\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConfigVersions_SourceId_MaxLength",
                table: "CollectionConfigVersions",
                sql: "\"SourceId\" IS NULL OR char_length(\"SourceId\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConfigVersions_SourceType_MaxLength",
                table: "CollectionConfigVersions",
                sql: "\"SourceType\" IS NULL OR char_length(\"SourceType\") <= 128");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConfigVersions_SourceVersion_MaxLength",
                table: "CollectionConfigVersions",
                sql: "\"SourceVersion\" IS NULL OR char_length(\"SourceVersion\") <= 64");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CollectionConfigVersions_UpdatedBy_MaxLength",
                table: "CollectionConfigVersions",
                sql: "\"UpdatedBy\" IS NULL OR char_length(\"UpdatedBy\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_Email_MaxLength",
                table: "AspNetUsers",
                sql: "\"Email\" IS NULL OR char_length(\"Email\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_NormalizedEmail_MaxLength",
                table: "AspNetUsers",
                sql: "\"NormalizedEmail\" IS NULL OR char_length(\"NormalizedEmail\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_NormalizedUserName_MaxLength",
                table: "AspNetUsers",
                sql: "\"NormalizedUserName\" IS NULL OR char_length(\"NormalizedUserName\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetUsers_UserName_MaxLength",
                table: "AspNetUsers",
                sql: "\"UserName\" IS NULL OR char_length(\"UserName\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetRoles_Name_MaxLength",
                table: "AspNetRoles",
                sql: "\"Name\" IS NULL OR char_length(\"Name\") <= 256");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AspNetRoles_NormalizedName_MaxLength",
                table: "AspNetRoles",
                sql: "\"NormalizedName\" IS NULL OR char_length(\"NormalizedName\") <= 256");
        }

        /// <summary>
        /// 恢复旧列声明；旧 DATETIME 只有毫秒精度，含亚毫秒新值时不能承诺无损降级。
        /// 应优先恢复升级前的停写备份；多条 DDL 不构成跨表原子回滚。
        /// </summary>
        /// <param name="migrationBuilder">当前数据库提供程序的迁移构建器。</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleaseTasks_InstanceId_MaxLength",
                table: "ReleaseTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleaseTasks_Message_MaxLength",
                table: "ReleaseTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleaseTasks_RuntimeType_MaxLength",
                table: "ReleaseTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleaseTasks_TargetKey_MaxLength",
                table: "ReleaseTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleaseReceipts_InstanceId_MaxLength",
                table: "ReleaseReceipts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleaseReceipts_Message_MaxLength",
                table: "ReleaseReceipts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleaseReceipts_RuntimeType_MaxLength",
                table: "ReleaseReceipts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleaseReceipts_TargetKey_MaxLength",
                table: "ReleaseReceipts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePlans_CreatedBy_MaxLength",
                table: "ReleasePlans");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePlans_Description_MaxLength",
                table: "ReleasePlans");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePlans_Name_MaxLength",
                table: "ReleasePlans");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePlans_UpdatedBy_MaxLength",
                table: "ReleasePlans");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_BlobPath_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_ContentType_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_ContractVersion_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_CreatedBy_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_DownloadToken_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_FileName_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_Name_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_PackageKey_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_Sha256_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_TargetRuntimeType_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_TargetRuntimeVersion_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_UpdatedBy_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ReleasePackages_Version_MaxLength",
                table: "ReleasePackages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeTasks_ContractVersion_MaxLength",
                table: "EdgeTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeTasks_InstanceId_MaxLength",
                table: "EdgeTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeTasks_Message_MaxLength",
                table: "EdgeTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeTasks_RuntimeType_MaxLength",
                table: "EdgeTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeTasks_TargetKey_MaxLength",
                table: "EdgeTasks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeTaskReceipts_ContractVersion_MaxLength",
                table: "EdgeTaskReceipts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeTaskReceipts_InstanceId_MaxLength",
                table: "EdgeTaskReceipts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeTaskReceipts_Message_MaxLength",
                table: "EdgeTaskReceipts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeTaskReceipts_RuntimeType_MaxLength",
                table: "EdgeTaskReceipts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeTaskReceipts_TargetKey_MaxLength",
                table: "EdgeTaskReceipts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeNodes_ConfigurationOperationOwner_MaxLength",
                table: "EdgeNodes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_AppliedConfigurationHash_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_ConfigurationHash_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_ContractVersion_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_CreatedBy_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_InstanceId_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_LastExecutionMessage_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_RuntimeType_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_SourceId_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_SourceType_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_SourceVersion_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_TargetKey_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EdgeCollectionAssignments_UpdatedBy_MaxLength",
                table: "EdgeCollectionAssignments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionTemplates_CreatedBy_MaxLength",
                table: "CollectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionTemplates_Name_MaxLength",
                table: "CollectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionTemplates_SemanticModelId_MaxLength",
                table: "CollectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionTemplates_TemplateKey_MaxLength",
                table: "CollectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionTemplates_UpdatedBy_MaxLength",
                table: "CollectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionSamplingPolicies_AggregateHint_MaxLength",
                table: "CollectionSamplingPolicies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionSamplingPolicies_Group_MaxLength",
                table: "CollectionSamplingPolicies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionProtocolTemplates_ProtocolKind_MaxLength",
                table: "CollectionProtocolTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_Address_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_BindingId_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_ConnectionKey_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_DisplayName_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_FieldPath_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_Name_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_PointKey_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_Quantity_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_RawValueType_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_SemanticId_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_SourceType_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionPointTemplates_Unit_MaxLength",
                table: "CollectionPointTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionMappingPolicies_DisplayName_MaxLength",
                table: "CollectionMappingPolicies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionMappingPolicies_Group_MaxLength",
                table: "CollectionMappingPolicies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionMappingPolicies_TargetName_MaxLength",
                table: "CollectionMappingPolicies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionMappingPolicies_Unit_MaxLength",
                table: "CollectionMappingPolicies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConnectionTemplates_AuthType_MaxLength",
                table: "CollectionConnectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConnectionTemplates_ConnectionKey_MaxLength",
                table: "CollectionConnectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConnectionTemplates_ConnectionName_MaxLength",
                table: "CollectionConnectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConnectionTemplates_EndpointRef_MaxLength",
                table: "CollectionConnectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConnectionTemplates_Host_MaxLength",
                table: "CollectionConnectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConnectionTemplates_SerialPort_MaxLength",
                table: "CollectionConnectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConnectionTemplates_Transport_MaxLength",
                table: "CollectionConnectionTemplates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConfigVersions_ConfigurationHash_MaxLength",
                table: "CollectionConfigVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConfigVersions_ContractVersion_MaxLength",
                table: "CollectionConfigVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConfigVersions_CreatedBy_MaxLength",
                table: "CollectionConfigVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConfigVersions_SourceId_MaxLength",
                table: "CollectionConfigVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConfigVersions_SourceType_MaxLength",
                table: "CollectionConfigVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConfigVersions_SourceVersion_MaxLength",
                table: "CollectionConfigVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CollectionConfigVersions_UpdatedBy_MaxLength",
                table: "CollectionConfigVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_Email_MaxLength",
                table: "AspNetUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_NormalizedEmail_MaxLength",
                table: "AspNetUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_NormalizedUserName_MaxLength",
                table: "AspNetUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetUsers_UserName_MaxLength",
                table: "AspNetUsers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetRoles_Name_MaxLength",
                table: "AspNetRoles");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AspNetRoles_NormalizedName_MaxLength",
                table: "AspNetRoles");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "TelemetryData",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "TelemetryData",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                table: "SubscriptionEvents",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TesterDateTime",
                table: "RuleTaskExecutors",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddDateTime",
                table: "RuleTaskExecutors",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReleaseTasks",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReceiptAt",
                table: "ReleaseTasks",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DispatchedAt",
                table: "ReleaseTasks",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ReleaseTasks",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "ReleaseTasks",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReportedAt",
                table: "ReleaseReceipts",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReceivedAt",
                table: "ReleaseReceipts",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReleasePlans",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "ReleasePlans",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ReleasePlans",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "ReleasePlans",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ReleasePackages",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ReleasePackages",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "RefreshTokens",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddedDate",
                table: "RefreshTokens",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                table: "ProductCommands",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TesterDateTime",
                table: "Flows",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "Flows",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatTime",
                table: "FlowRules",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AddDate",
                table: "FlowOperations",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EdgeTasks",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                table: "EdgeTasks",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "EdgeTasks",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastReceiptAt",
                table: "EdgeTasks",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpireAt",
                table: "EdgeTasks",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "EdgeTasks",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CompletedAt",
                table: "EdgeTasks",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "AcceptedAt",
                table: "EdgeTasks",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReportedAt",
                table: "EdgeTaskReceipts",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReceivedAt",
                table: "EdgeTaskReceipts",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EdgeNodes",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastRegistrationDateTime",
                table: "EdgeNodes",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastHeartbeatDateTime",
                table: "EdgeNodes",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "EdgeNodes",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ConfigurationOperationExpiresAt",
                table: "EdgeNodes",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RevokedAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastPulledAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastExecutionAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AssignedAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AppliedAt",
                table: "EdgeCollectionAssignments",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FromCreateDate",
                table: "DynamicFormInfos",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FieldCreateDate",
                table: "DynamicFormFieldValueInfos",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FieldEditDate",
                table: "DynamicFormFieldInfos",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FieldCreateDate",
                table: "DynamicFormFieldInfos",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ConfigDateTime",
                table: "DeviceRules",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "DevicePorts",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "DevicePortMappings",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                table: "DeviceModels",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                table: "DeviceModelCommands",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Value_DateTime",
                table: "DataStorage",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "DataStorage",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CollectionTemplates",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CollectionTemplates",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "CollectionConfigVersions",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CollectionConfigVersions",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreaterDateTime",
                table: "BaseEvents",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ActiveDateTime",
                table: "AuditLog",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "AspNetUsers",
                type: "DATETIME",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DATETIMEOFFSET",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDateTime",
                table: "Alarms",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDateTime",
                table: "Alarms",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClearDateTime",
                table: "Alarms",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AckDateTime",
                table: "Alarms",
                type: "DATETIME",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "DATETIME2");
        }
    }
}
