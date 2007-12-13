using System;
using System.Collections.Generic;
using System.Text;
using Migrator.Providers.ColumnPropertiesMappers;

namespace Migrator.Providers.TypeToSqlProviders
{
	public class SQLServerTypeToSqlProvider
	{

		#region ITypeToSqlProvider Members

		public ColumnPropertiesMapper PrimaryKey
		{
			get { return Integer; }
		}

		public ColumnPropertiesMapper Char(byte size)
		{
			return new ColumnPropertiesMapper(string.Format("nchar({0})", size));
		}

		public ColumnPropertiesMapper String(ushort size)
		{
			return new ColumnPropertiesMapper(string.Format("nvarchar({0})", size));
		}

		public ColumnPropertiesMapper Text
		{
			get { return new ColumnPropertiesMapper("ntext"); }
		}

		public ColumnPropertiesMapper LongText
		{
			get { return new ColumnPropertiesMapper("nvarchar(max)"); }
		}

		public ColumnPropertiesMapper Binary(byte size)
		{
			return new ColumnPropertiesMapper(string.Format("VARBINARY({0})", size));
		}

		public ColumnPropertiesMapper Blob
		{
			get { return new ColumnPropertiesMapper("image"); }
		}

		public ColumnPropertiesMapper LongBlob
		{
			get { return new ColumnPropertiesMapper("image"); }
		}

		public ColumnPropertiesMapper Integer
		{
			get { return new ColumnPropertiesMapper("int"); }
		}

		public ColumnPropertiesMapper Long
		{
			get { return new ColumnPropertiesMapper("bigint"); }
		}

		public ColumnPropertiesMapper Float
		{
			get { return new ColumnPropertiesMapper("real"); }
		}

		public ColumnPropertiesMapper Double
		{
			get { return new ColumnPropertiesMapper("float"); }
		}

		public ColumnPropertiesMapper Decimal(int whole)
		{
			return new ColumnPropertiesMapper(string.Format("numeric({0})", whole));
		}

		public ColumnPropertiesMapper Decimal(int whole, int part)
		{
			return new ColumnPropertiesMapper(string.Format("numeric({0}, {1})", whole, part));
		}

		public ColumnPropertiesMapper Bool
		{
			get
			{
				ColumnPropertiesMapper mapper = new ColumnPropertiesMapper("bit");
				mapper.Default("0");
				return mapper;
			}
		}

		public ColumnPropertiesMapper DateTime
		{
			get { return new ColumnPropertiesMapper("datetime"); }
		}

		#endregion

	}
}
