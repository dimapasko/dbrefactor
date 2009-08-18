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
using DbRefactor.Engines.SqlServer;
using DbRefactor.Infrastructure;
using DbRefactor.Infrastructure.Loggers;

namespace DbRefactor.Providers
{
	public class ProviderFactory
	{
		public TransformationProvider Create(string connectionString)
		{
			return Create(connectionString, Logger.NullLogger);
		}

		internal static ColumnProviderFactory ColumnProviderFactory
		{
			get
			{
				return columnProviderFactory;
			}
		}

		private static ColumnProviderFactory columnProviderFactory;

		internal static ColumnPropertyProviderFactory columnPropertyProviderFactory;

		internal static ColumnPropertyProviderFactory ColumnPropertyProviderFactory
		{
			get
			{
				return columnPropertyProviderFactory;
			}
		}

		public TransformationProvider Create(string connectionString, ILogger logger)
		{
			var sqlServerEnvironment = new SqlServerEnvironment(connectionString, logger);
			var codeGenerationService = new CodeGenerationService();
			var sqlGenerationService = new SQLGenerationService();
			var sqlServerTypes = new SqlServerTypes();
			var columnPropertyProviderFactory = new ColumnPropertyProviderFactory(new SqlServerColumnProperties());
			var sqlServerColumnMapper = new SqlServerColumnMapper(codeGenerationService, sqlServerTypes, sqlGenerationService, columnPropertyProviderFactory);
			columnProviderFactory = new ColumnProviderFactory(codeGenerationService, sqlServerTypes, sqlGenerationService, columnPropertyProviderFactory);
			var constraintNameService = new ConstraintNameService();
			return new TransformationProvider(sqlServerEnvironment, sqlServerColumnMapper, columnPropertyProviderFactory, constraintNameService);
		}

		public Migrator CreateMigrator(string provider, string connectionString, string category, Assembly assembly, bool trace)
		{
			var logger = new Logger(trace);
			logger.Attach(new ConsoleWriter());
			return new Migrator(Create(connectionString, logger), category, assembly, logger);
		}
	}
}