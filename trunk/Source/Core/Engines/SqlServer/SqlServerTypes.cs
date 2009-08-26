using System;

namespace DbRefactor.Engines.SqlServer
{
	internal sealed class SqlServerTypes : ISqlTypes
	{
		public string Binary()
		{
			return "varbinary(max)";
		}

		public string BinaryValue(byte[] value)
		{
			throw new NotSupportedException("Couldn't set default value for binary");
		}

		public string Boolean()
		{
			return "bit";
		}

		public string BooleanValue(bool value)
		{
			return value ? "1" : "0";
		}

		public string DateTime()
		{
			return "datetime";
		}

		public string DateTimeValue(DateTime dateTime)
		{
			return string.Format("'{0:0000}-{1:00}-{2:00}T{3:00}:{4:00}:{5:00}'", dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour,
			                     dateTime.Minute, dateTime.Second);
		}

		public string Decimal(int precision, int radix)
		{
			return string.Format("decimal({0},{1})", precision, radix);
		}

		public string DecimalValue(decimal value)
		{
			return value.ToString();
		}

		public string Double()
		{
			return "float";
		}

		public string DoubleValue(double value)
		{
			return value.ToString();
		}

		public string Float()
		{
			return "real";
		}

		public string FloatValue(float value)
		{
			return value.ToString();
		}

		public string Int()
		{
			return "integer";
		}

		public string IntValue(int value)
		{
			return value.ToString();
		}

		public string Long()
		{
			return "bigint";
		}

		public string LongValue(long value)
		{
			return value.ToString();
		}

		public string String(int size)
		{
			return string.Format("varchar({0})", size);
		}

		public string StringValue(string value)
		{
			return string.Format("'{0}'", value.Replace("'", "''"));
		}

		public string Text()
		{
			return "text";
		}

		public string TextValue(string value)
		{
			return StringValue(value);
		}

		public string NullValue()
		{
			return "null";
		}
	}
}