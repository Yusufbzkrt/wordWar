using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KelimeOyunu.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Gold = table.Column<int>(type: "integer", nullable: false),
                    Diamonds = table.Column<int>(type: "integer", nullable: false),
                    TotalWins = table.Column<int>(type: "integer", nullable: false),
                    TotalLosses = table.Column<int>(type: "integer", nullable: false),
                    LastAdRewardTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsPopular = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Answers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddresseeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_AddresseeId",
                        column: x => x.AddresseeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Player1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Player2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WinnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Player1RoundWins = table.Column<int>(type: "integer", nullable: false),
                    Player2RoundWins = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessions_Users_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameSessions_Users_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameSessions_Users_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    GoldAmount = table.Column<int>(type: "integer", nullable: false),
                    DiamondAmount = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameRounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundNumber = table.Column<int>(type: "integer", nullable: false),
                    Player1Score = table.Column<int>(type: "integer", nullable: false),
                    Player2Score = table.Column<int>(type: "integer", nullable: false),
                    RoundWinnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameRounds_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameRounds_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Category", "CreatedAt", "Text" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), "Yiyecek", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir meyve adı söyleyin" },
                    { new Guid("a2222222-2222-2222-2222-222222222222"), "Genel", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir renk adı söyleyin" },
                    { new Guid("a3333333-3333-3333-3333-333333333333"), "Hayvanlar", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir hayvan adı söyleyin" },
                    { new Guid("a4444444-4444-4444-4444-444444444444"), "Coğrafya", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir ülke adı söyleyin" },
                    { new Guid("a5555555-5555-5555-5555-555555555555"), "Spor", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir spor dalı söyleyin" },
                    { new Guid("a6666666-6666-6666-6666-666666666666"), "Meslekler", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir meslek adı söyleyin" },
                    { new Guid("a7777777-7777-7777-7777-777777777777"), "Müzik", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir müzik aleti söyleyin" },
                    { new Guid("a8888888-8888-8888-8888-888888888888"), "Yiyecek", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir sebze adı söyleyin" },
                    { new Guid("a9999999-9999-9999-9999-999999999999"), "Coğrafya", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir şehir adı söyleyin (Türkiye)" },
                    { new Guid("b1111111-1111-1111-1111-111111111111"), "Otomobil", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir araç markası söyleyin" },
                    { new Guid("b2222222-2222-2222-2222-222222222222"), "Yiyecek", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir içecek adı söyleyin" },
                    { new Guid("b3333333-3333-3333-3333-333333333333"), "Doğa", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir çiçek adı söyleyin" },
                    { new Guid("b4444444-4444-4444-4444-444444444444"), "Eğlence", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir film türü söyleyin" },
                    { new Guid("b5555555-5555-5555-5555-555555555555"), "Eğitim", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir okul dersi söyleyin" },
                    { new Guid("b6666666-6666-6666-6666-666666666666"), "Moda", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir giysi türü söyleyin" },
                    { new Guid("b7777777-7777-7777-7777-777777777777"), "Ev", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mutfakta bulunan bir eşya söyleyin" },
                    { new Guid("b8888888-8888-8888-8888-888888888888"), "Bilim", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir gezegen adı söyleyin" },
                    { new Guid("b9999999-9999-9999-9999-999999999999"), "Hayvanlar", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir deniz canlısı söyleyin" },
                    { new Guid("c1111111-1111-1111-1111-111111111111"), "Yiyecek", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir Türk yemeği söyleyin" },
                    { new Guid("c2222222-2222-2222-2222-222222222222"), "Teknoloji", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bir yazılım dili söyleyin" }
                });

            migrationBuilder.InsertData(
                table: "Answers",
                columns: new[] { "Id", "IsPopular", "QuestionId", "Text" },
                values: new object[,]
                {
                    { new Guid("d0000001-0000-0000-0000-000000000000"), true, new Guid("a1111111-1111-1111-1111-111111111111"), "Elma" },
                    { new Guid("d0000002-0000-0000-0000-000000000000"), true, new Guid("a1111111-1111-1111-1111-111111111111"), "Muz" },
                    { new Guid("d0000003-0000-0000-0000-000000000000"), false, new Guid("a1111111-1111-1111-1111-111111111111"), "Portakal" },
                    { new Guid("d0000004-0000-0000-0000-000000000000"), false, new Guid("a1111111-1111-1111-1111-111111111111"), "Çilek" },
                    { new Guid("d0000005-0000-0000-0000-000000000000"), false, new Guid("a1111111-1111-1111-1111-111111111111"), "Karpuz" },
                    { new Guid("d0000006-0000-0000-0000-000000000000"), false, new Guid("a1111111-1111-1111-1111-111111111111"), "Üzüm" },
                    { new Guid("d0000007-0000-0000-0000-000000000000"), false, new Guid("a1111111-1111-1111-1111-111111111111"), "Kiraz" },
                    { new Guid("d0000008-0000-0000-0000-000000000000"), false, new Guid("a1111111-1111-1111-1111-111111111111"), "Şeftali" },
                    { new Guid("d0000009-0000-0000-0000-000000000000"), false, new Guid("a1111111-1111-1111-1111-111111111111"), "Armut" },
                    { new Guid("d0000010-0000-0000-0000-000000000000"), false, new Guid("a1111111-1111-1111-1111-111111111111"), "Kavun" },
                    { new Guid("d0000011-0000-0000-0000-000000000000"), true, new Guid("a2222222-2222-2222-2222-222222222222"), "Kırmızı" },
                    { new Guid("d0000012-0000-0000-0000-000000000000"), true, new Guid("a2222222-2222-2222-2222-222222222222"), "Mavi" },
                    { new Guid("d0000013-0000-0000-0000-000000000000"), false, new Guid("a2222222-2222-2222-2222-222222222222"), "Yeşil" },
                    { new Guid("d0000014-0000-0000-0000-000000000000"), false, new Guid("a2222222-2222-2222-2222-222222222222"), "Sarı" },
                    { new Guid("d0000015-0000-0000-0000-000000000000"), false, new Guid("a2222222-2222-2222-2222-222222222222"), "Siyah" },
                    { new Guid("d0000016-0000-0000-0000-000000000000"), false, new Guid("a2222222-2222-2222-2222-222222222222"), "Beyaz" },
                    { new Guid("d0000017-0000-0000-0000-000000000000"), false, new Guid("a2222222-2222-2222-2222-222222222222"), "Turuncu" },
                    { new Guid("d0000018-0000-0000-0000-000000000000"), false, new Guid("a2222222-2222-2222-2222-222222222222"), "Mor" },
                    { new Guid("d0000019-0000-0000-0000-000000000000"), false, new Guid("a2222222-2222-2222-2222-222222222222"), "Pembe" },
                    { new Guid("d0000020-0000-0000-0000-000000000000"), true, new Guid("a3333333-3333-3333-3333-333333333333"), "Kedi" },
                    { new Guid("d0000021-0000-0000-0000-000000000000"), true, new Guid("a3333333-3333-3333-3333-333333333333"), "Köpek" },
                    { new Guid("d0000022-0000-0000-0000-000000000000"), false, new Guid("a3333333-3333-3333-3333-333333333333"), "At" },
                    { new Guid("d0000023-0000-0000-0000-000000000000"), false, new Guid("a3333333-3333-3333-3333-333333333333"), "Aslan" },
                    { new Guid("d0000024-0000-0000-0000-000000000000"), false, new Guid("a3333333-3333-3333-3333-333333333333"), "Kaplan" },
                    { new Guid("d0000025-0000-0000-0000-000000000000"), false, new Guid("a3333333-3333-3333-3333-333333333333"), "Fil" },
                    { new Guid("d0000026-0000-0000-0000-000000000000"), false, new Guid("a3333333-3333-3333-3333-333333333333"), "Tavuk" },
                    { new Guid("d0000027-0000-0000-0000-000000000000"), false, new Guid("a3333333-3333-3333-3333-333333333333"), "Kuş" },
                    { new Guid("d0000028-0000-0000-0000-000000000000"), false, new Guid("a3333333-3333-3333-3333-333333333333"), "Balık" },
                    { new Guid("d0000029-0000-0000-0000-000000000000"), false, new Guid("a3333333-3333-3333-3333-333333333333"), "Tavşan" },
                    { new Guid("d0000030-0000-0000-0000-000000000000"), true, new Guid("a4444444-4444-4444-4444-444444444444"), "Türkiye" },
                    { new Guid("d0000031-0000-0000-0000-000000000000"), true, new Guid("a4444444-4444-4444-4444-444444444444"), "Almanya" },
                    { new Guid("d0000032-0000-0000-0000-000000000000"), false, new Guid("a4444444-4444-4444-4444-444444444444"), "Fransa" },
                    { new Guid("d0000033-0000-0000-0000-000000000000"), false, new Guid("a4444444-4444-4444-4444-444444444444"), "İngiltere" },
                    { new Guid("d0000034-0000-0000-0000-000000000000"), false, new Guid("a4444444-4444-4444-4444-444444444444"), "İtalya" },
                    { new Guid("d0000035-0000-0000-0000-000000000000"), false, new Guid("a4444444-4444-4444-4444-444444444444"), "İspanya" },
                    { new Guid("d0000036-0000-0000-0000-000000000000"), false, new Guid("a4444444-4444-4444-4444-444444444444"), "ABD" },
                    { new Guid("d0000037-0000-0000-0000-000000000000"), false, new Guid("a4444444-4444-4444-4444-444444444444"), "Japonya" },
                    { new Guid("d0000038-0000-0000-0000-000000000000"), false, new Guid("a4444444-4444-4444-4444-444444444444"), "Brezilya" },
                    { new Guid("d0000039-0000-0000-0000-000000000000"), true, new Guid("a5555555-5555-5555-5555-555555555555"), "Futbol" },
                    { new Guid("d0000040-0000-0000-0000-000000000000"), true, new Guid("a5555555-5555-5555-5555-555555555555"), "Basketbol" },
                    { new Guid("d0000041-0000-0000-0000-000000000000"), false, new Guid("a5555555-5555-5555-5555-555555555555"), "Voleybol" },
                    { new Guid("d0000042-0000-0000-0000-000000000000"), false, new Guid("a5555555-5555-5555-5555-555555555555"), "Tenis" },
                    { new Guid("d0000043-0000-0000-0000-000000000000"), false, new Guid("a5555555-5555-5555-5555-555555555555"), "Yüzme" },
                    { new Guid("d0000044-0000-0000-0000-000000000000"), false, new Guid("a5555555-5555-5555-5555-555555555555"), "Boks" },
                    { new Guid("d0000045-0000-0000-0000-000000000000"), false, new Guid("a5555555-5555-5555-5555-555555555555"), "Atletizm" },
                    { new Guid("d0000046-0000-0000-0000-000000000000"), false, new Guid("a5555555-5555-5555-5555-555555555555"), "Güreş" },
                    { new Guid("d0000047-0000-0000-0000-000000000000"), true, new Guid("a6666666-6666-6666-6666-666666666666"), "Doktor" },
                    { new Guid("d0000048-0000-0000-0000-000000000000"), true, new Guid("a6666666-6666-6666-6666-666666666666"), "Öğretmen" },
                    { new Guid("d0000049-0000-0000-0000-000000000000"), false, new Guid("a6666666-6666-6666-6666-666666666666"), "Mühendis" },
                    { new Guid("d0000050-0000-0000-0000-000000000000"), false, new Guid("a6666666-6666-6666-6666-666666666666"), "Avukat" },
                    { new Guid("d0000051-0000-0000-0000-000000000000"), false, new Guid("a6666666-6666-6666-6666-666666666666"), "Polis" },
                    { new Guid("d0000052-0000-0000-0000-000000000000"), false, new Guid("a6666666-6666-6666-6666-666666666666"), "Hemşire" },
                    { new Guid("d0000053-0000-0000-0000-000000000000"), false, new Guid("a6666666-6666-6666-6666-666666666666"), "Pilot" },
                    { new Guid("d0000054-0000-0000-0000-000000000000"), false, new Guid("a6666666-6666-6666-6666-666666666666"), "Aşçı" },
                    { new Guid("d0000055-0000-0000-0000-000000000000"), true, new Guid("a7777777-7777-7777-7777-777777777777"), "Gitar" },
                    { new Guid("d0000056-0000-0000-0000-000000000000"), true, new Guid("a7777777-7777-7777-7777-777777777777"), "Piyano" },
                    { new Guid("d0000057-0000-0000-0000-000000000000"), false, new Guid("a7777777-7777-7777-7777-777777777777"), "Keman" },
                    { new Guid("d0000058-0000-0000-0000-000000000000"), false, new Guid("a7777777-7777-7777-7777-777777777777"), "Davul" },
                    { new Guid("d0000059-0000-0000-0000-000000000000"), false, new Guid("a7777777-7777-7777-7777-777777777777"), "Flüt" },
                    { new Guid("d0000060-0000-0000-0000-000000000000"), false, new Guid("a7777777-7777-7777-7777-777777777777"), "Saz" },
                    { new Guid("d0000061-0000-0000-0000-000000000000"), false, new Guid("a7777777-7777-7777-7777-777777777777"), "Klarnet" },
                    { new Guid("d0000062-0000-0000-0000-000000000000"), false, new Guid("a7777777-7777-7777-7777-777777777777"), "Ney" },
                    { new Guid("d0000063-0000-0000-0000-000000000000"), true, new Guid("a8888888-8888-8888-8888-888888888888"), "Domates" },
                    { new Guid("d0000064-0000-0000-0000-000000000000"), true, new Guid("a8888888-8888-8888-8888-888888888888"), "Biber" },
                    { new Guid("d0000065-0000-0000-0000-000000000000"), false, new Guid("a8888888-8888-8888-8888-888888888888"), "Salatalık" },
                    { new Guid("d0000066-0000-0000-0000-000000000000"), false, new Guid("a8888888-8888-8888-8888-888888888888"), "Patates" },
                    { new Guid("d0000067-0000-0000-0000-000000000000"), false, new Guid("a8888888-8888-8888-8888-888888888888"), "Soğan" },
                    { new Guid("d0000068-0000-0000-0000-000000000000"), false, new Guid("a8888888-8888-8888-8888-888888888888"), "Havuç" },
                    { new Guid("d0000069-0000-0000-0000-000000000000"), false, new Guid("a8888888-8888-8888-8888-888888888888"), "Patlıcan" },
                    { new Guid("d0000070-0000-0000-0000-000000000000"), false, new Guid("a8888888-8888-8888-8888-888888888888"), "Kabak" },
                    { new Guid("d0000071-0000-0000-0000-000000000000"), false, new Guid("a8888888-8888-8888-8888-888888888888"), "Marul" },
                    { new Guid("d0000072-0000-0000-0000-000000000000"), true, new Guid("a9999999-9999-9999-9999-999999999999"), "İstanbul" },
                    { new Guid("d0000073-0000-0000-0000-000000000000"), true, new Guid("a9999999-9999-9999-9999-999999999999"), "Ankara" },
                    { new Guid("d0000074-0000-0000-0000-000000000000"), false, new Guid("a9999999-9999-9999-9999-999999999999"), "İzmir" },
                    { new Guid("d0000075-0000-0000-0000-000000000000"), false, new Guid("a9999999-9999-9999-9999-999999999999"), "Antalya" },
                    { new Guid("d0000076-0000-0000-0000-000000000000"), false, new Guid("a9999999-9999-9999-9999-999999999999"), "Bursa" },
                    { new Guid("d0000077-0000-0000-0000-000000000000"), false, new Guid("a9999999-9999-9999-9999-999999999999"), "Trabzon" },
                    { new Guid("d0000078-0000-0000-0000-000000000000"), false, new Guid("a9999999-9999-9999-9999-999999999999"), "Adana" },
                    { new Guid("d0000079-0000-0000-0000-000000000000"), false, new Guid("a9999999-9999-9999-9999-999999999999"), "Konya" },
                    { new Guid("d0000080-0000-0000-0000-000000000000"), false, new Guid("a9999999-9999-9999-9999-999999999999"), "Gaziantep" },
                    { new Guid("d0000081-0000-0000-0000-000000000000"), true, new Guid("b1111111-1111-1111-1111-111111111111"), "BMW" },
                    { new Guid("d0000082-0000-0000-0000-000000000000"), true, new Guid("b1111111-1111-1111-1111-111111111111"), "Mercedes" },
                    { new Guid("d0000083-0000-0000-0000-000000000000"), false, new Guid("b1111111-1111-1111-1111-111111111111"), "Audi" },
                    { new Guid("d0000084-0000-0000-0000-000000000000"), false, new Guid("b1111111-1111-1111-1111-111111111111"), "Toyota" },
                    { new Guid("d0000085-0000-0000-0000-000000000000"), false, new Guid("b1111111-1111-1111-1111-111111111111"), "Volkswagen" },
                    { new Guid("d0000086-0000-0000-0000-000000000000"), false, new Guid("b1111111-1111-1111-1111-111111111111"), "Ford" },
                    { new Guid("d0000087-0000-0000-0000-000000000000"), false, new Guid("b1111111-1111-1111-1111-111111111111"), "Honda" },
                    { new Guid("d0000088-0000-0000-0000-000000000000"), false, new Guid("b1111111-1111-1111-1111-111111111111"), "Renault" },
                    { new Guid("d0000089-0000-0000-0000-000000000000"), false, new Guid("b1111111-1111-1111-1111-111111111111"), "Fiat" },
                    { new Guid("d0000090-0000-0000-0000-000000000000"), true, new Guid("b2222222-2222-2222-2222-222222222222"), "Çay" },
                    { new Guid("d0000091-0000-0000-0000-000000000000"), true, new Guid("b2222222-2222-2222-2222-222222222222"), "Kahve" },
                    { new Guid("d0000092-0000-0000-0000-000000000000"), false, new Guid("b2222222-2222-2222-2222-222222222222"), "Su" },
                    { new Guid("d0000093-0000-0000-0000-000000000000"), false, new Guid("b2222222-2222-2222-2222-222222222222"), "Ayran" },
                    { new Guid("d0000094-0000-0000-0000-000000000000"), false, new Guid("b2222222-2222-2222-2222-222222222222"), "Kola" },
                    { new Guid("d0000095-0000-0000-0000-000000000000"), false, new Guid("b2222222-2222-2222-2222-222222222222"), "Limonata" },
                    { new Guid("d0000096-0000-0000-0000-000000000000"), false, new Guid("b2222222-2222-2222-2222-222222222222"), "Süt" },
                    { new Guid("d0000097-0000-0000-0000-000000000000"), false, new Guid("b2222222-2222-2222-2222-222222222222"), "Meyve Suyu" },
                    { new Guid("d0000098-0000-0000-0000-000000000000"), true, new Guid("b3333333-3333-3333-3333-333333333333"), "Gül" },
                    { new Guid("d0000099-0000-0000-0000-000000000000"), true, new Guid("b3333333-3333-3333-3333-333333333333"), "Papatya" },
                    { new Guid("d0000100-0000-0000-0000-000000000000"), false, new Guid("b3333333-3333-3333-3333-333333333333"), "Lale" },
                    { new Guid("d0000101-0000-0000-0000-000000000000"), false, new Guid("b3333333-3333-3333-3333-333333333333"), "Orkide" },
                    { new Guid("d0000102-0000-0000-0000-000000000000"), false, new Guid("b3333333-3333-3333-3333-333333333333"), "Karanfil" },
                    { new Guid("d0000103-0000-0000-0000-000000000000"), false, new Guid("b3333333-3333-3333-3333-333333333333"), "Menekşe" },
                    { new Guid("d0000104-0000-0000-0000-000000000000"), false, new Guid("b3333333-3333-3333-3333-333333333333"), "Ayçiçeği" },
                    { new Guid("d0000105-0000-0000-0000-000000000000"), false, new Guid("b3333333-3333-3333-3333-333333333333"), "Zambak" },
                    { new Guid("d0000106-0000-0000-0000-000000000000"), true, new Guid("b4444444-4444-4444-4444-444444444444"), "Aksiyon" },
                    { new Guid("d0000107-0000-0000-0000-000000000000"), true, new Guid("b4444444-4444-4444-4444-444444444444"), "Komedi" },
                    { new Guid("d0000108-0000-0000-0000-000000000000"), false, new Guid("b4444444-4444-4444-4444-444444444444"), "Korku" },
                    { new Guid("d0000109-0000-0000-0000-000000000000"), false, new Guid("b4444444-4444-4444-4444-444444444444"), "Dram" },
                    { new Guid("d0000110-0000-0000-0000-000000000000"), false, new Guid("b4444444-4444-4444-4444-444444444444"), "Bilim Kurgu" },
                    { new Guid("d0000111-0000-0000-0000-000000000000"), false, new Guid("b4444444-4444-4444-4444-444444444444"), "Romantik" },
                    { new Guid("d0000112-0000-0000-0000-000000000000"), false, new Guid("b4444444-4444-4444-4444-444444444444"), "Animasyon" },
                    { new Guid("d0000113-0000-0000-0000-000000000000"), false, new Guid("b4444444-4444-4444-4444-444444444444"), "Belgesel" },
                    { new Guid("d0000114-0000-0000-0000-000000000000"), true, new Guid("b5555555-5555-5555-5555-555555555555"), "Matematik" },
                    { new Guid("d0000115-0000-0000-0000-000000000000"), true, new Guid("b5555555-5555-5555-5555-555555555555"), "Türkçe" },
                    { new Guid("d0000116-0000-0000-0000-000000000000"), false, new Guid("b5555555-5555-5555-5555-555555555555"), "Fen Bilgisi" },
                    { new Guid("d0000117-0000-0000-0000-000000000000"), false, new Guid("b5555555-5555-5555-5555-555555555555"), "Tarih" },
                    { new Guid("d0000118-0000-0000-0000-000000000000"), false, new Guid("b5555555-5555-5555-5555-555555555555"), "Coğrafya" },
                    { new Guid("d0000119-0000-0000-0000-000000000000"), false, new Guid("b5555555-5555-5555-5555-555555555555"), "İngilizce" },
                    { new Guid("d0000120-0000-0000-0000-000000000000"), false, new Guid("b5555555-5555-5555-5555-555555555555"), "Müzik" },
                    { new Guid("d0000121-0000-0000-0000-000000000000"), false, new Guid("b5555555-5555-5555-5555-555555555555"), "Beden Eğitimi" },
                    { new Guid("d0000122-0000-0000-0000-000000000000"), true, new Guid("b6666666-6666-6666-6666-666666666666"), "Tişört" },
                    { new Guid("d0000123-0000-0000-0000-000000000000"), true, new Guid("b6666666-6666-6666-6666-666666666666"), "Pantolon" },
                    { new Guid("d0000124-0000-0000-0000-000000000000"), false, new Guid("b6666666-6666-6666-6666-666666666666"), "Gömlek" },
                    { new Guid("d0000125-0000-0000-0000-000000000000"), false, new Guid("b6666666-6666-6666-6666-666666666666"), "Etek" },
                    { new Guid("d0000126-0000-0000-0000-000000000000"), false, new Guid("b6666666-6666-6666-6666-666666666666"), "Ceket" },
                    { new Guid("d0000127-0000-0000-0000-000000000000"), false, new Guid("b6666666-6666-6666-6666-666666666666"), "Kazak" },
                    { new Guid("d0000128-0000-0000-0000-000000000000"), false, new Guid("b6666666-6666-6666-6666-666666666666"), "Şort" },
                    { new Guid("d0000129-0000-0000-0000-000000000000"), false, new Guid("b6666666-6666-6666-6666-666666666666"), "Mont" },
                    { new Guid("d0000130-0000-0000-0000-000000000000"), true, new Guid("b7777777-7777-7777-7777-777777777777"), "Tencere" },
                    { new Guid("d0000131-0000-0000-0000-000000000000"), true, new Guid("b7777777-7777-7777-7777-777777777777"), "Bıçak" },
                    { new Guid("d0000132-0000-0000-0000-000000000000"), false, new Guid("b7777777-7777-7777-7777-777777777777"), "Tabak" },
                    { new Guid("d0000133-0000-0000-0000-000000000000"), false, new Guid("b7777777-7777-7777-7777-777777777777"), "Bardak" },
                    { new Guid("d0000134-0000-0000-0000-000000000000"), false, new Guid("b7777777-7777-7777-7777-777777777777"), "Çatal" },
                    { new Guid("d0000135-0000-0000-0000-000000000000"), false, new Guid("b7777777-7777-7777-7777-777777777777"), "Kaşık" },
                    { new Guid("d0000136-0000-0000-0000-000000000000"), false, new Guid("b7777777-7777-7777-7777-777777777777"), "Tava" },
                    { new Guid("d0000137-0000-0000-0000-000000000000"), false, new Guid("b7777777-7777-7777-7777-777777777777"), "Süzgeç" },
                    { new Guid("d0000138-0000-0000-0000-000000000000"), true, new Guid("b8888888-8888-8888-8888-888888888888"), "Mars" },
                    { new Guid("d0000139-0000-0000-0000-000000000000"), true, new Guid("b8888888-8888-8888-8888-888888888888"), "Jüpiter" },
                    { new Guid("d0000140-0000-0000-0000-000000000000"), false, new Guid("b8888888-8888-8888-8888-888888888888"), "Venüs" },
                    { new Guid("d0000141-0000-0000-0000-000000000000"), false, new Guid("b8888888-8888-8888-8888-888888888888"), "Satürn" },
                    { new Guid("d0000142-0000-0000-0000-000000000000"), false, new Guid("b8888888-8888-8888-8888-888888888888"), "Merkür" },
                    { new Guid("d0000143-0000-0000-0000-000000000000"), false, new Guid("b8888888-8888-8888-8888-888888888888"), "Neptün" },
                    { new Guid("d0000144-0000-0000-0000-000000000000"), false, new Guid("b8888888-8888-8888-8888-888888888888"), "Uranüs" },
                    { new Guid("d0000145-0000-0000-0000-000000000000"), false, new Guid("b8888888-8888-8888-8888-888888888888"), "Dünya" },
                    { new Guid("d0000146-0000-0000-0000-000000000000"), true, new Guid("b9999999-9999-9999-9999-999999999999"), "Yunus" },
                    { new Guid("d0000147-0000-0000-0000-000000000000"), true, new Guid("b9999999-9999-9999-9999-999999999999"), "Köpekbalığı" },
                    { new Guid("d0000148-0000-0000-0000-000000000000"), false, new Guid("b9999999-9999-9999-9999-999999999999"), "Balina" },
                    { new Guid("d0000149-0000-0000-0000-000000000000"), false, new Guid("b9999999-9999-9999-9999-999999999999"), "Ahtapot" },
                    { new Guid("d0000150-0000-0000-0000-000000000000"), false, new Guid("b9999999-9999-9999-9999-999999999999"), "Denizanası" },
                    { new Guid("d0000151-0000-0000-0000-000000000000"), false, new Guid("b9999999-9999-9999-9999-999999999999"), "Yengeç" },
                    { new Guid("d0000152-0000-0000-0000-000000000000"), false, new Guid("b9999999-9999-9999-9999-999999999999"), "Karides" },
                    { new Guid("d0000153-0000-0000-0000-000000000000"), false, new Guid("b9999999-9999-9999-9999-999999999999"), "Deniz Kaplumbağası" },
                    { new Guid("d0000154-0000-0000-0000-000000000000"), true, new Guid("c1111111-1111-1111-1111-111111111111"), "Kebap" },
                    { new Guid("d0000155-0000-0000-0000-000000000000"), true, new Guid("c1111111-1111-1111-1111-111111111111"), "Lahmacun" },
                    { new Guid("d0000156-0000-0000-0000-000000000000"), false, new Guid("c1111111-1111-1111-1111-111111111111"), "Pide" },
                    { new Guid("d0000157-0000-0000-0000-000000000000"), false, new Guid("c1111111-1111-1111-1111-111111111111"), "Döner" },
                    { new Guid("d0000158-0000-0000-0000-000000000000"), false, new Guid("c1111111-1111-1111-1111-111111111111"), "Mantı" },
                    { new Guid("d0000159-0000-0000-0000-000000000000"), false, new Guid("c1111111-1111-1111-1111-111111111111"), "İskender" },
                    { new Guid("d0000160-0000-0000-0000-000000000000"), false, new Guid("c1111111-1111-1111-1111-111111111111"), "Köfte" },
                    { new Guid("d0000161-0000-0000-0000-000000000000"), false, new Guid("c1111111-1111-1111-1111-111111111111"), "Çorba" },
                    { new Guid("d0000162-0000-0000-0000-000000000000"), false, new Guid("c1111111-1111-1111-1111-111111111111"), "Dolma" },
                    { new Guid("d0000163-0000-0000-0000-000000000000"), false, new Guid("c1111111-1111-1111-1111-111111111111"), "Baklava" },
                    { new Guid("d0000164-0000-0000-0000-000000000000"), true, new Guid("c2222222-2222-2222-2222-222222222222"), "Python" },
                    { new Guid("d0000165-0000-0000-0000-000000000000"), true, new Guid("c2222222-2222-2222-2222-222222222222"), "JavaScript" },
                    { new Guid("d0000166-0000-0000-0000-000000000000"), false, new Guid("c2222222-2222-2222-2222-222222222222"), "Java" },
                    { new Guid("d0000167-0000-0000-0000-000000000000"), false, new Guid("c2222222-2222-2222-2222-222222222222"), "C#" },
                    { new Guid("d0000168-0000-0000-0000-000000000000"), false, new Guid("c2222222-2222-2222-2222-222222222222"), "C++" },
                    { new Guid("d0000169-0000-0000-0000-000000000000"), false, new Guid("c2222222-2222-2222-2222-222222222222"), "TypeScript" },
                    { new Guid("d0000170-0000-0000-0000-000000000000"), false, new Guid("c2222222-2222-2222-2222-222222222222"), "Go" },
                    { new Guid("d0000171-0000-0000-0000-000000000000"), false, new Guid("c2222222-2222-2222-2222-222222222222"), "Rust" },
                    { new Guid("d0000172-0000-0000-0000-000000000000"), false, new Guid("c2222222-2222-2222-2222-222222222222"), "PHP" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_AddresseeId",
                table: "Friendships",
                column: "AddresseeId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_RequesterId_AddresseeId",
                table: "Friendships",
                columns: new[] { "RequesterId", "AddresseeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameRounds_GameSessionId",
                table: "GameRounds",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameRounds_QuestionId",
                table: "GameRounds",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_Player1Id",
                table: "GameSessions",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_Player2Id",
                table: "GameSessions",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_WinnerId",
                table: "GameSessions",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SentAt",
                table: "Messages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CreatedAt",
                table: "Transactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "GameRounds");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
