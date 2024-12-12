using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace ConsoleRpgEntities.Migrations
{
    public partial class AddMissingItemColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns 
                               WHERE object_id = OBJECT_ID(N'[dbo].[Items]') 
                               AND name = 'Type')
                BEGIN
                    ALTER TABLE [Items] 
                    ADD [Type] nvarchar(max) NOT NULL DEFAULT N'';
                END;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns 
                               WHERE object_id = OBJECT_ID(N'[dbo].[Items]') 
                               AND name = 'Attack')
                BEGIN
                    ALTER TABLE [Items] 
                    ADD [Attack] int NOT NULL DEFAULT 0;
                END;
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns 
                               WHERE object_id = OBJECT_ID(N'[dbo].[Items]') 
                               AND name = 'Defense')
                BEGIN
                    ALTER TABLE [Items] 
                    ADD [Defense] int NOT NULL DEFAULT 0;
                END;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Type", table: "Items");
            migrationBuilder.DropColumn(name: "Attack", table: "Items");
            migrationBuilder.DropColumn(name: "Defense", table: "Items");
        }
    }
}
