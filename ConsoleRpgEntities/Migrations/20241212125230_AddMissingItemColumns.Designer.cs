using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ConsoleRpgEntities.Data;

namespace ConsoleRpgEntities.Migrations
{
    [DbContext(typeof(GameContext))]
    [Migration("20241212125230_AddMissingItemColumns")]
    public partial class AddMissingItemColumns : Migration
    {
        
    }
}
