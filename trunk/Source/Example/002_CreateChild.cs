﻿using DbRefactor;

namespace Example
{
	[Migration(2)]
	public class CreateChild : Migration
	{
		public override void Up()
		{
			CreateTable(
				"Child", Columns
				.Int("ID").PrimaryKeyWithIdentity()
				.String("Name", 50)
				.Int("ParentID", ColumnProperties.NotNull));

			AddForeignKey("FK_Child_Parent", "Child", "ParentID", "Parent", "ID", OnDelete.Cascade);
		}

		public override void Down()
		{
			DropForeignKey("Child", "FK_Child_Parent");

			DropTable("Child");
		}
	}
}
