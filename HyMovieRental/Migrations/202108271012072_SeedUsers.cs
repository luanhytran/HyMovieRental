namespace HyMovieRental.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SeedUsers : DbMigration
    {
        public override void Up()
        {
            Sql(@"
INSERT [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName]) VALUES (N'78a76a08-753f-4fa3-b87c-fa5b463d90e4', N'admin@hymovierental.com', 0, N'ACkQnuq9a6LWmir63R/Uwn18WsyvINSGiT8ezvYRwT5onjOQlrSQVFseNMGBPlNHsw==', N'eb59b4f5-7d22-4f9a-8e4a-f86d61e50008', NULL, 0, 0, NULL, 1, 0, N'admin@hymovierental.com')
INSERT [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName]) VALUES (N'c6fb057e-39ae-486e-988e-a7b3c26e2078', N'guest@hymovierental.com', 0, N'AFeZlcdEileYS3yuRWDIXOtPFLoRk9LRmBg5MFNjvvbN2FVpGljGvz/en5OUU+8HdA==', N'446af46f-830f-429b-9369-c106213adf1b', NULL, 0, 0, NULL, 1, 0, N'guest@hymovierental.com')

INSERT [dbo].[AspNetRoles] ([Id], [Name]) VALUES (N'392da8a2-f135-45bb-9e42-ec3ac37b50c4', N'CanManageMovies')

INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'78a76a08-753f-4fa3-b87c-fa5b463d90e4', N'392da8a2-f135-45bb-9e42-ec3ac37b50c4')
");
        }
        
        public override void Down()
        {
        }
    }
}
