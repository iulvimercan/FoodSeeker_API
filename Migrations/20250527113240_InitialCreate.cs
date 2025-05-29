using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodSeekerAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ProfilePhotoUrl = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsDonator = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "DonatorProfiles",
                columns: table => new
                {
                    DonatorId = table.Column<long>(type: "bigint", nullable: false),
                    RestaurantName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShortAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    DonationStarts = table.Column<TimeOnly>(type: "time", nullable: false),
                    DonationEnds = table.Column<TimeOnly>(type: "time", nullable: false),
                    AverageScore = table.Column<float>(type: "real", nullable: false),
                    FavoritesCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonatorProfiles", x => x.DonatorId);
                    table.ForeignKey(
                        name: "FK_DonatorProfiles_Users_DonatorId",
                        column: x => x.DonatorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "NotificationLogs",
                columns: table => new
                {
                    NotificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationLogs", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_NotificationLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "UserDeviceTokens",
                columns: table => new
                {
                    TokenId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DeviceToken = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDeviceTokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_UserDeviceTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteDonators",
                columns: table => new
                {
                    FavouriteId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeekerId = table.Column<long>(type: "bigint", nullable: false),
                    DonatorId = table.Column<long>(type: "bigint", nullable: false),
                    FavoritedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteDonators", x => x.FavouriteId);
                    table.ForeignKey(
                        name: "FK_FavoriteDonators_DonatorProfiles_DonatorId",
                        column: x => x.DonatorId,
                        principalTable: "DonatorProfiles",
                        principalColumn: "DonatorId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_FavoriteDonators_Users_SeekerId",
                        column: x => x.SeekerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromUserId = table.Column<long>(type: "bigint", nullable: false),
                    DonatorId = table.Column<long>(type: "bigint", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedbacks_DonatorProfiles_DonatorId",
                        column: x => x.DonatorId,
                        principalTable: "DonatorProfiles",
                        principalColumn: "DonatorId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Users_FromUserId",
                        column: x => x.FromUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "FoodItems",
                columns: table => new
                {
                    FoodId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonatorId = table.Column<long>(type: "bigint", nullable: false),
                    FoodName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEatIn = table.Column<bool>(type: "bit", nullable: false),
                    IsTakeAway = table.Column<bool>(type: "bit", nullable: false),
                    IsBringPack = table.Column<bool>(type: "bit", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodItems", x => x.FoodId);
                    table.ForeignKey(
                        name: "FK_FoodItems_DonatorProfiles_DonatorId",
                        column: x => x.DonatorId,
                        principalTable: "DonatorProfiles",
                        principalColumn: "DonatorId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "PickupIntents",
                columns: table => new
                {
                    IntentId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FoodId = table.Column<long>(type: "bigint", nullable: false),
                    SeekerId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PickupIntents", x => x.IntentId);
                    table.ForeignKey(
                        name: "FK_PickupIntents_FoodItems_FoodId",
                        column: x => x.FoodId,
                        principalTable: "FoodItems",
                        principalColumn: "FoodId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PickupIntents_Users_SeekerId",
                        column: x => x.SeekerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteDonators_DonatorId",
                table: "FavoriteDonators",
                column: "DonatorId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteDonators_SeekerId",
                table: "FavoriteDonators",
                column: "SeekerId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_DonatorId",
                table: "Feedbacks",
                column: "DonatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_FromUserId",
                table: "Feedbacks",
                column: "FromUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodItems_DonatorId",
                table: "FoodItems",
                column: "DonatorId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationLogs_UserId",
                table: "NotificationLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupIntents_FoodId",
                table: "PickupIntents",
                column: "FoodId");

            migrationBuilder.CreateIndex(
                name: "IX_PickupIntents_SeekerId",
                table: "PickupIntents",
                column: "SeekerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDeviceTokens_UserId",
                table: "UserDeviceTokens",
                column: "UserId");
            
            var passHash = BCrypt.Net.BCrypt.HashPassword("test");
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "FullName", "Email", "PasswordHash", "IsDonator", "CreatedAt", "IsVerified" },
                values: new object[,]
                {
                    { 1L, "John Doe", "john@example.com", passHash, false, DateTime.UtcNow , true }, // Food Seeker
                    { 2L, "Emma Smith", "emma@example.com", passHash, true, DateTime.UtcNow, true }  // Donator
                }
            );
    
            migrationBuilder.InsertData(
                table: "DonatorProfiles",
                columns: new[] { "DonatorId", "RestaurantName", "ShortAddress", "Address", "Latitude", "Longitude", "DonationStarts", "DonationEnds", "AverageScore", "FavoritesCount" },
                values: new object[] { 2L, "Pasta Palace", "123 Pasta St", "123 Pasta St, Food City", 40.7128, -74.0060, new TimeOnly(10, 0), new TimeOnly(20, 0), 4.5f, 100 }
            );

            migrationBuilder.InsertData(
                table: "FoodItems",
                columns: new[] { "FoodId", "DonatorId", "FoodName", "Description", "PhotoUrl", "IsEatIn", "IsTakeAway", "IsBringPack", "IsActive", "CreatedAt" },
                values: new object[] { 1L, 2L, "Pasta", "Delicious hot pasta", null, true, false, true, true, DateTime.UtcNow }
            );

            migrationBuilder.InsertData(
                table: "Feedbacks",
                columns: new[] { "FeedbackId", "FromUserId", "DonatorId", "Rating", "Comment", "CreatedAt" },
                values: new object[] { 1L, 1L, 2L, 5, "Great food!", DateTime.UtcNow }
            );
            
            migrationBuilder.InsertData(
                table: "FavoriteDonators",
                columns: new[] { "FavouriteId", "SeekerId", "DonatorId", "FavoritedAt" },
                values: new object[] { 1L, 1L, 2L, DateTime.UtcNow }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteDonators");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "NotificationLogs");

            migrationBuilder.DropTable(
                name: "PickupIntents");

            migrationBuilder.DropTable(
                name: "UserDeviceTokens");

            migrationBuilder.DropTable(
                name: "FoodItems");

            migrationBuilder.DropTable(
                name: "DonatorProfiles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
