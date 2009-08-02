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

using System.Reflection;
using DbRefactor.Providers.TypeToSqlProviders;
using DbRefactor.Tools.Loggers;

namespace DbRefactor.Providers
{
	public class ProviderFactory
	{
		public TransformationProvider Create(string connectionString)
		{
			return new TransformationProvider(new SqlServerEnvironment(connectionString, Logger.NullLogger), Logger.NullLogger, new ColumnProviderFactory(new CodeGenerationService(), new SqlServerTypes()));
			//string providerName = GuessProviderName(name);

			//return (TransformationProvider)Activator.CreateInstance(
			//    Type.GetType(
			//        String.Format("Migrator.Providers.{0}TransformationProvider, Migrator", 
			//            providerName), true),
			//    new object[] { connectionString });
		}

		public TransformationProvider Create(string connectionString, ILogger logger)
		{
			return new TransformationProvider(new SqlServerEnvironment(connectionString, logger), logger, new ColumnProviderFactory(new CodeGenerationService(), new SqlServerTypes()));
		}

		public Migrator CreateMigrator(string provider, string connectionString, string category, Assembly assembly, bool trace)
		{
			var logger = new Logger(trace);
			logger.Attach(new ConsoleWriter());
			return new Migrator(Create(connectionString, logger), category, assembly, logger);
		}

		private string GuessProviderName(string name)
		{
			if (name == "NHibernate.Driver.MySqlDataDriver")
			{
				return "MySql";
			}
			if (name == "NHibernate.Driver.NpgsqlDriver")
			{
				return "PostgreSQL";
			}
			if (name == "NHibernate.Driver.SqlClientDriver")
			{
				return "SqlServer";
			}
			return name;
		}
	}
}