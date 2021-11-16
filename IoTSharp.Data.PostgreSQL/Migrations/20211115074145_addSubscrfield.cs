using Microsoft.EntityFrameworkCore.Migrations;

namespace IoTSharp.Migrations
{
    public partial class addSubscrfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTasks_CustomerId",
                table: "SubscriptionTasks",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionTasks_Customer_CustomerId",
                table: "SubscriptionTasks",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionTasks_Customer_CustomerId",
                table: "SubscriptionTasks");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionTasks_CustomerId",
                table: "SubscriptionTasks");
        }
    }
}
