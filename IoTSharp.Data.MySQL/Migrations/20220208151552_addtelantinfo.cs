using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoTSharp.Data.MySql.Migrations
{
    public partial class addtelantinfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SubscriptionTasks",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "SubscriptionEvents",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "SubscriptionEvents",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "RuleTaskExecutors",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "RuleTaskExecutors",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Flows",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Flows",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "FlowRules",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "FlowRules",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<double>(
                name: "Version",
                table: "FlowRules",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormInfos",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormInfos",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormFieldValueInfos",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormFieldValueInfos",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DynamicFormFieldInfos",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DynamicFormFieldInfos",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DeviceGraphToolBoxes",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DeviceGraphToolBoxes",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DeviceGraphs",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DeviceGraphs",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "DeviceDiagrams",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "DeviceDiagrams",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "BaseEvents",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "BaseEvents",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTasks_TenantId",
                table: "SubscriptionTasks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionEvents_CustomerId",
                table: "SubscriptionEvents",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionEvents_TenantId",
                table: "SubscriptionEvents",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleTaskExecutors_CustomerId",
                table: "RuleTaskExecutors",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleTaskExecutors_TenantId",
                table: "RuleTaskExecutors",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_CustomerId",
                table: "Flows",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Flows_TenantId",
                table: "Flows",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowRules_CustomerId",
                table: "FlowRules",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowRules_TenantId",
                table: "FlowRules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormInfos_CustomerId",
                table: "DynamicFormInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormInfos_TenantId",
                table: "DynamicFormInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldValueInfos_CustomerId",
                table: "DynamicFormFieldValueInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldValueInfos_TenantId",
                table: "DynamicFormFieldValueInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldInfos_CustomerId",
                table: "DynamicFormFieldInfos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicFormFieldInfos_TenantId",
                table: "DynamicFormFieldInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_CustomerId",
                table: "DeviceGraphToolBoxes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphToolBoxes_TenantId",
                table: "DeviceGraphToolBoxes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_CustomerId",
                table: "DeviceGraphs",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceGraphs_TenantId",
                table: "DeviceGraphs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDiagrams_CustomerId",
                table: "DeviceDiagrams",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceDiagrams_TenantId",
                table: "DeviceDiagrams",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEvents_CustomerId",
                table: "BaseEvents",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BaseEvents_TenantId",
                table: "BaseEvents",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEvents_Customer_CustomerId",
                table: "BaseEvents",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseEvents_Tenant_TenantId",
                table: "BaseEvents",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDiagrams_Customer_CustomerId",
                table: "DeviceDiagrams",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceDiagrams_Tenant_TenantId",
                table: "DeviceDiagrams",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphs_Customer_CustomerId",
                table: "DeviceGraphs",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphs_Tenant_TenantId",
                table: "DeviceGraphs",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphToolBoxes_Customer_CustomerId",
                table: "DeviceGraphToolBoxes",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceGraphToolBoxes_Tenant_TenantId",
                table: "DeviceGraphToolBoxes",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldInfos_Customer_CustomerId",
                table: "DynamicFormFieldInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldInfos_Tenant_TenantId",
                table: "DynamicFormFieldInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldValueInfos_Customer_CustomerId",
                table: "DynamicFormFieldValueInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormFieldValueInfos_Tenant_TenantId",
                table: "DynamicFormFieldValueInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormInfos_Customer_CustomerId",
                table: "DynamicFormInfos",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DynamicFormInfos_Tenant_TenantId",
                table: "DynamicFormInfos",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlowRules_Customer_CustomerId",
                table: "FlowRules",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlowRules_Tenant_TenantId",
                table: "FlowRules",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_Customer_CustomerId",
                table: "Flows",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_Tenant_TenantId",
                table: "Flows",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RuleTaskExecutors_Customer_CustomerId",
                table: "RuleTaskExecutors",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RuleTaskExecutors_Tenant_TenantId",
                table: "RuleTaskExecutors",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionEvents_Customer_CustomerId",
                table: "SubscriptionEvents",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionEvents_Tenant_TenantId",
                table: "SubscriptionEvents",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionTasks_Tenant_TenantId",
                table: "SubscriptionTasks",
                column: "TenantId",
                principalTable: "Tenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_Customer_CustomerId",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseEvents_Tenant_TenantId",
                table: "BaseEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDiagrams_Customer_CustomerId",
                table: "DeviceDiagrams");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceDiagrams_Tenant_TenantId",
                table: "DeviceDiagrams");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphs_Customer_CustomerId",
                table: "DeviceGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphs_Tenant_TenantId",
                table: "DeviceGraphs");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphToolBoxes_Customer_CustomerId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_DeviceGraphToolBoxes_Tenant_TenantId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldInfos_Customer_CustomerId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldInfos_Tenant_TenantId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldValueInfos_Customer_CustomerId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormFieldValueInfos_Tenant_TenantId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormInfos_Customer_CustomerId",
                table: "DynamicFormInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_DynamicFormInfos_Tenant_TenantId",
                table: "DynamicFormInfos");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowRules_Customer_CustomerId",
                table: "FlowRules");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowRules_Tenant_TenantId",
                table: "FlowRules");

            migrationBuilder.DropForeignKey(
                name: "FK_Flows_Customer_CustomerId",
                table: "Flows");

            migrationBuilder.DropForeignKey(
                name: "FK_Flows_Tenant_TenantId",
                table: "Flows");

            migrationBuilder.DropForeignKey(
                name: "FK_RuleTaskExecutors_Customer_CustomerId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropForeignKey(
                name: "FK_RuleTaskExecutors_Tenant_TenantId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionEvents_Customer_CustomerId",
                table: "SubscriptionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionEvents_Tenant_TenantId",
                table: "SubscriptionEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionTasks_Tenant_TenantId",
                table: "SubscriptionTasks");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionTasks_TenantId",
                table: "SubscriptionTasks");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionEvents_CustomerId",
                table: "SubscriptionEvents");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionEvents_TenantId",
                table: "SubscriptionEvents");

            migrationBuilder.DropIndex(
                name: "IX_RuleTaskExecutors_CustomerId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropIndex(
                name: "IX_RuleTaskExecutors_TenantId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropIndex(
                name: "IX_Flows_CustomerId",
                table: "Flows");

            migrationBuilder.DropIndex(
                name: "IX_Flows_TenantId",
                table: "Flows");

            migrationBuilder.DropIndex(
                name: "IX_FlowRules_CustomerId",
                table: "FlowRules");

            migrationBuilder.DropIndex(
                name: "IX_FlowRules_TenantId",
                table: "FlowRules");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormInfos_CustomerId",
                table: "DynamicFormInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormInfos_TenantId",
                table: "DynamicFormInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldValueInfos_CustomerId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldValueInfos_TenantId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldInfos_CustomerId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropIndex(
                name: "IX_DynamicFormFieldInfos_TenantId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropIndex(
                name: "IX_DeviceGraphToolBoxes_CustomerId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropIndex(
                name: "IX_DeviceGraphToolBoxes_TenantId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropIndex(
                name: "IX_DeviceGraphs_CustomerId",
                table: "DeviceGraphs");

            migrationBuilder.DropIndex(
                name: "IX_DeviceGraphs_TenantId",
                table: "DeviceGraphs");

            migrationBuilder.DropIndex(
                name: "IX_DeviceDiagrams_CustomerId",
                table: "DeviceDiagrams");

            migrationBuilder.DropIndex(
                name: "IX_DeviceDiagrams_TenantId",
                table: "DeviceDiagrams");

            migrationBuilder.DropIndex(
                name: "IX_BaseEvents_CustomerId",
                table: "BaseEvents");

            migrationBuilder.DropIndex(
                name: "IX_BaseEvents_TenantId",
                table: "BaseEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SubscriptionTasks");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "SubscriptionEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SubscriptionEvents");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "RuleTaskExecutors");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Flows");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "FlowRules");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DynamicFormInfos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DynamicFormInfos");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DynamicFormFieldValueInfos");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DynamicFormFieldInfos");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DeviceGraphToolBoxes");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DeviceGraphs");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DeviceGraphs");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "DeviceDiagrams");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "DeviceDiagrams");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "BaseEvents");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "BaseEvents");
        }
    }
}
