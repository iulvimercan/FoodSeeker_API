using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodSeekerAPI.Migrations
{
    /// <inheritdoc />
    public partial class DonatorAddressUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortAddress",
                table: "DonatorProfiles");

            migrationBuilder.AddColumn<string>(
                name: "AddressCity",
                table: "DonatorProfiles",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressCountry",
                table: "DonatorProfiles",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AddressStreet",
                table: "DonatorProfiles",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressCity",
                table: "DonatorProfiles");

            migrationBuilder.DropColumn(
                name: "AddressCountry",
                table: "DonatorProfiles");

            migrationBuilder.DropColumn(
                name: "AddressStreet",
                table: "DonatorProfiles");

            migrationBuilder.AddColumn<string>(
                name: "ShortAddress",
                table: "DonatorProfiles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
