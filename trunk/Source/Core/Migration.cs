#region License
//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.
#endregion
using Migrator.Providers;
using Migrator.Loggers;
using System.Reflection;
using System;
using System.Collections;
using Migrator.Columns;
namespace Migrator
{
	/// <summary>
	/// A migration is a group of transformation applied to the database schema
	/// (or sometimes data) to port the database from one version to another.
	/// The <c>Up()</c> method must apply the modifications (eg.: create a table)
	/// and the <c>Down()</c> method must revert, or rollback the modifications
	/// (eg.: delete a table).
	/// <para>
	/// Each migration must be decorated with the <c>[Migration(0)]</c> attribute.
	/// Each migration number (0) must be unique, or else a 
	/// <c>DuplicatedVersionException</c> will be trown.
	/// </para>
	/// <para>
	/// All migrations are executed inside a transaction. If an exception is
	/// thrown, the transaction will be rolledback and transformations wont be
	/// applied.
	/// </para>
	/// <para>
	/// It is best to keep a limited number of transformation inside a migration
	/// so you can easely move from one version of to another with fine grain
	/// modifications.
	/// You should give meaningful name to the migration class and prepend the
	/// migration number to the filename so they keep ordered, eg.: 
	/// <c>002_CreateTableTest.cs</c>.
	/// </para>
	/// <para>
	/// Use the <c>Database</c> property to apply transformation and the
	/// <c>Logger</c> property to output informations in the console (or other).
	/// For more details on transformations see
	/// <see cref="TransformationProvider">TransformationProvider</see>.
	/// </para>
	/// </summary>
	/// <example>
	/// The following migration creates a new Customer table.
	/// (File <c>003_AddCustomerTable.cs</c>)
	/// <code>
	/// [Migration(3)]
	/// public class AddCustomerTable : Migration
	/// {
	/// 	public override void Up()
	/// 	{
	/// 		Database.AddTable("Customer",
	///		                  new Column("Name", typeof(string), 50),
	///		                  new Column("Address", typeof(string), 100)
	///		                 );
	/// 	}
	/// 	public override void Down()
	/// 	{
	/// 		Database.RemoveTable("Customer");
	/// 	}
	/// }
	/// </code>
	/// </example>
	public abstract class Migration
	{
		private TransformationProvider _transformationProvider;

		/// <summary>
		/// Defines tranformations to port the database to the current version.
		/// </summary>
		public abstract void Up();

		/// <summary>
		/// Defines transformations to revert things done in <c>Up</c>.
		/// </summary>
		public abstract void Down();

		/// <summary>
		/// Represents the database.
		/// <see cref="TransformationProvider"></see>.
		/// </summary>
		/// <seealso cref="Migration.Transformations">Migration.Transformations</seealso>
		public TransformationProvider Database
		{
			get
			{
				return _transformationProvider;
			}
		}

		/// <summary>
		/// This gets called once on the first migration object.
		/// </summary>
		public virtual void InitializeOnce(string[] args)
		{
			System.Console.WriteLine("Migration.InitializeOnce()");
		}

		/// <summary>
		/// Alias to the <c>Database</c> property.
		/// </summary>
		public TransformationProvider TransformationProvider
		{
			get
			{
				return _transformationProvider;
			}
			set
			{
				_transformationProvider = value;
			}
		}

		/// <summary>
		/// Event logger.
		/// </summary>
		public ILogger Logger
		{
			get
			{
				return _transformationProvider.Logger;
			}
		}

		protected Column String(string name, int size)
		{
			return new Column(name, typeof(string), size);
		}

		protected void AddString(string table, string name, int size)
		{
			Database.AddColumn(table, String(name, size));
		}

		protected Column String(string name, int size, ColumnProperties properties)
		{
			return new Column(name, typeof(string), size, properties);
		}

		protected void AddString(string table, string name, int size, ColumnProperties properties)
		{
			Database.AddColumn(table, String(name, size, properties));
		}

		protected Column String(string name, int size, string defaultValue)
		{
			return new Column(name, typeof(string), size, ColumnProperties.NotNull, defaultValue);
		}

		protected Column String(string name, int size, ColumnProperties properties, string defaultValue)
		{
			return new Column(name, typeof(string), size, properties, defaultValue);
		}

		private const int defaultTextLength = 1024;

		protected Column Text(string name)
		{
			return new Column(name, typeof(string), defaultTextLength);
		}

		protected Column Text(string name, ColumnProperties properties)
		{
			return new Column(name, typeof(string), defaultTextLength, properties);
		}

		protected Column Text(string name, int defaultValue)
		{
			return new Column(name, typeof(string), defaultTextLength, ColumnProperties.NotNull, defaultValue);
		}

		protected Column Text(string name, ColumnProperties properties, int defaultValue)
		{
			return new Column(name, typeof(string), defaultTextLength, properties, defaultValue);
		}

		protected Column Int(string name)
		{
			return new Column(name, typeof(int));
		}

		protected Column Int(string name, ColumnProperties properties)
		{
			return new Column(name, typeof(int), properties);
		}

		protected Column Int(string name, int defaultValue)
		{
			return new Column(name, typeof(int), ColumnProperties.NotNull, defaultValue);
		}

		protected Column Int(string name, ColumnProperties properties, int defaultValue)
		{
			return new Column(name, typeof(int), properties, defaultValue);
		}

		protected Column DateTime(string name)
		{
			return new Column(name, typeof(DateTime));
		}

		protected Column DateTime(string name, ColumnProperties properties)
		{
			return new Column(name, typeof(DateTime), properties);
		}

		protected Column DateTime(string name, int defaultValue)
		{
			return new Column(name, typeof(DateTime), ColumnProperties.NotNull, defaultValue);
		}

		protected Column DateTime(string name, ColumnProperties properties, int defaultValue)
		{
			return new Column(name, typeof(DateTime), properties, defaultValue);
		}

		private const int defaultWhole = 18;
		private const int defaultRemainder = 0;

		protected Column Decimal(string name)
		{
			return new DecimalColumn(name, defaultWhole, defaultWhole);
		}

		protected Column Decimal(string name, ColumnProperties properties)
		{
			return new DecimalColumn(name, defaultWhole, defaultWhole, properties);
		}

		protected Column Decimal(string name, int defaultValue)
		{
			return new DecimalColumn(name, defaultWhole, defaultWhole, ColumnProperties.NotNull, defaultValue);
		}

		protected Column Decimal(string name, ColumnProperties properties, int defaultValue)
		{
			return new DecimalColumn(name, defaultWhole, defaultWhole, properties, defaultValue);
		}

		protected Column Decimal(string name, int whole, int remainder)
		{
			return new DecimalColumn(name, whole, remainder);
		}

		protected Column Decimal(string name, int whole, int remainder, ColumnProperties properties)
		{
			return new DecimalColumn(name, whole, remainder, properties);
		}

		protected Column Decimal(string name, int whole, int remainder, int defaultValue)
		{
			return new DecimalColumn(name, whole, remainder, ColumnProperties.NotNull, defaultValue);
		}

		protected Column Decimal(string name, int whole, int remainder, ColumnProperties properties, int defaultValue)
		{
			return new DecimalColumn(name, whole, remainder, properties, defaultValue);
		}

		protected Column Boolean(string name)
		{
			return new Column(name, typeof(bool));
		}

		protected Column Boolean(string name, ColumnProperties properties)
		{
			return new Column(name, typeof(bool), properties);
		}

		protected Column Boolean(string name, bool defaultValue)
		{
			return new Column(name, typeof(bool), ColumnProperties.NotNull, defaultValue);
		}

		protected Column Boolean(string name, ColumnProperties properties, bool defaultValue)
		{
			return new Column(name, typeof(bool), properties, defaultValue);
		}

		protected void AddString(string table, string name, int size, ColumnProperties properties, string defaultValue)
		{
			Database.AddColumn(table, new Column(name, typeof(string), size, properties, defaultValue));
		}

		protected void AddText(string table, string name)
		{
			Database.AddColumn(table, new Column(name, typeof(string), defaultTextLength));
		}

		protected void AddText(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, new Column(name, typeof(string), defaultTextLength, properties));
		}

		protected void AddText(string table, string name, int defaultValue)
		{
			Database.AddColumn(table, new Column(name, typeof(string), defaultTextLength, ColumnProperties.NotNull, defaultValue));
		}

		protected void AddText(string table, string name, ColumnProperties properties, int defaultValue)
		{
			Database.AddColumn(table, new Column(name, typeof(string), defaultTextLength, properties, defaultValue));
		}

		protected void AddInt(string table, string name)
		{
			Database.AddColumn(table, new Column(name, typeof(int)));
		}

		protected void AddInt(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, new Column(name, typeof(int), properties));
		}

		protected void AddInt(string table, string name, int defaultValue)
		{
			Database.AddColumn(table, new Column(name, typeof(int), ColumnProperties.NotNull, defaultValue));
		}

		protected void AddInt(string table, string name, ColumnProperties properties, int defaultValue)
		{
			Database.AddColumn(table, new Column(name, typeof(int), properties, defaultValue));
		}

		protected void AddDateTime(string table, string name)
		{
			Database.AddColumn(table, new Column(name, typeof(DateTime)));
		}

		protected void AddDateTime(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, new Column(name, typeof(DateTime), properties));
		}

		protected void AddDateTime(string table, string name, int defaultValue)
		{
			Database.AddColumn(table, new Column(name, typeof(DateTime), ColumnProperties.NotNull, defaultValue));
		}

		protected void AddDateTime(string table, string name, ColumnProperties properties, int defaultValue)
		{
			Database.AddColumn(table, new Column(name, typeof(DateTime), properties, defaultValue));
		}

		protected void AddDecimal(string table, string name)
		{
			Database.AddColumn(table, new DecimalColumn(name, defaultWhole, defaultWhole));
		}

		protected void AddDecimal(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, new DecimalColumn(name, defaultWhole, defaultWhole, properties));
		}

		protected void AddDecimal(string table, string name, int defaultValue)
		{
			Database.AddColumn(table, new DecimalColumn(name, defaultWhole, defaultWhole, ColumnProperties.NotNull, defaultValue));
		}

		protected void AddDecimal(string table, string name, ColumnProperties properties, int defaultValue)
		{
			Database.AddColumn(table, new DecimalColumn(name, defaultWhole, defaultWhole, properties, defaultValue));
		}

		protected void AddDecimal(string table, string name, int whole, int remainder)
		{
			Database.AddColumn(table, new DecimalColumn(name, whole, remainder));
		}

		protected void AddDecimal(string table, string name, int whole, int remainder, ColumnProperties properties)
		{
			Database.AddColumn(table, new DecimalColumn(name, whole, remainder, properties));
		}

		protected void AddDecimal(string table, string name, int whole, int remainder, int defaultValue)
		{
			Database.AddColumn(table, new DecimalColumn(name, whole, remainder, ColumnProperties.NotNull, defaultValue));
		}

		protected void AddDecimal(string table, string name, int whole, int remainder, ColumnProperties properties, int defaultValue)
		{
			Database.AddColumn(table, new DecimalColumn(name, whole, remainder, properties, defaultValue));
		}

		protected void AddBoolean(string table, string name)
		{
			Database.AddColumn(table, new Column(name, typeof(bool)));
		}

		protected void AddBoolean(string table, string name, ColumnProperties properties)
		{
			Database.AddColumn(table, new Column(name, typeof(bool), properties));
		}

		protected void AddBoolean(string table, string name, bool defaultValue)
		{
			Database.AddColumn(table, new Column(name, typeof(bool), ColumnProperties.NotNull, defaultValue));
		}

		protected void AddBoolean(string table, string name, ColumnProperties properties, bool defaultValue)
		{
			Database.AddColumn(table, new Column(name, typeof(bool), properties, defaultValue));
		}
	}
}
