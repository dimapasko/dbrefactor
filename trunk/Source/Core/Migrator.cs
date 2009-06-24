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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using DbRefactor.Compatibility;
using DbRefactor.Providers;
using ILogger=DbRefactor.Tools.Loggers.ILogger;

namespace DbRefactor
{
	using OldMigrator = global::Migrator;
	using System.Data.SqlClient;
	/// <summary>
	/// Migrations mediator.
	/// </summary>
	public sealed class Migrator
	{
		private readonly TransformationProvider _provider;
		private readonly List<Type> _migrationsTypes = new List<Type>();
		private readonly bool _trace;  // show trace for debugging
		private Tools.Loggers.ILogger _logger = new Tools.Loggers.Logger(false);
		private string[] _args;

		public string[] args
		{
			get { return _args; }
			set { _args = value; }
		}

		private string _category;

		public string Category
		{
			get
			{
				return _category;
			}

			set
			{
				_category = value;
			}
		}

		public Migrator(string providerName, string connectionString, string category, Assembly migrationAssembly, bool trace)
			: this(CreateProvider(connectionString), category, migrationAssembly, trace)
		{ }


		public Migrator(string providerName, string connectionString, Assembly migrationAssembly, bool trace)
			: this(CreateProvider(connectionString), null, migrationAssembly, trace)
		{ }

		public Migrator(string providerName, string connectionString, Assembly migrationAssembly)
			: this(CreateProvider(connectionString), null, migrationAssembly, false)
		{ }

		private OldMigrator.Providers.SqlServerTransformationProvider _oldProvider;

		internal Migrator(TransformationProvider provider, string category, Assembly migrationAssembly, bool trace)
		{
			_provider = provider;
			_provider.Category = category;

			_oldProvider = new OldMigrator.Providers.SqlServerTransformationProvider(((provider as TransformationProvider).Environment as SqlServerEnvironment).Connection as SqlConnection);

			_trace = trace;
			Tools.Loggers.Logger logger = new Tools.Loggers.Logger(_trace);
			logger.Attach(new Tools.Loggers.ConsoleWriter());
			_logger = logger;
			_category = category;

			_migrationsTypes.AddRange(GetMigrationTypes(Assembly.GetExecutingAssembly()));

			if (migrationAssembly != null)
			{
				_migrationsTypes.AddRange(GetMigrationTypes(migrationAssembly));
				_setUpMigration = GetSetUpMigrationType(migrationAssembly);
			}

			_logger.Trace("Loaded migrations:");
			foreach (Type t in _migrationsTypes)
			{
				_logger.Trace("{0} {1}", GetMigrationVersion(t).ToString().PadLeft(5), ToHumanName(t.Name));
			}
			CheckForDuplicatedVersion();
		}

		private void BeginTransaction()
		{
			_provider.BeginTransaction();
			_oldProvider.Transaction = (_provider.Environment as SqlServerEnvironment).Transaction;
		}

		/// <summary>
		/// Migrate the database to a specific version.
		/// Runs all migration between the actual version and the
		/// specified version.
		/// If <c>version</c> is greater then the current version,
		/// the <c>Up()</c> method will be invoked.
		/// If <c>version</c> lower then the current version,
		/// the <c>Down()</c> method of previous migration will be invoked.
		/// </summary>
		/// <param name="version">The version that must became the current one</param>
		public void MigrateTo(int version)
		{
			_provider.Logger = _logger;
			BeginTransaction();

			if (CurrentVersion == version)
			{
				RunGlobalSetUp();
				if (version == LastVersion)
				{
					RunGlobalTearDown();
				}
				return;
			}
			int originalVersion = CurrentVersion;
			bool goingUp = originalVersion < version;
			BaseMigration migration;
			int v;	// the currently running migration number
			bool firstRun = true;

			if (goingUp)
			{
				// When we migrate to an upper version,
				// tranformations of the current version are
				// already applied, so we started at the next version.
				v = CurrentVersion + 1;
			}
			else
			{
				v = CurrentVersion;
			}

			_logger.Started(originalVersion, version);
			RunGlobalSetUp();

			while (true)
			{
				migration = GetMigration(v);

				if (firstRun)
				{
					if (migration is Migration)
					{
						(migration as Migration).InitializeOnce(_args);
					}
					firstRun = false;
				}

				if (migration != null)
				{
					string migrationName = ToHumanName(migration.GetType().Name);

					if (migration is Migration)
					{
						(migration as Migration).TransformationProvider = _provider;
					}
					else
					{
						(migration as OldMigrator.Migration).TransformationProvider = _oldProvider;
					}

					try
					{
						RunSetUp();
						if (goingUp)
						{
							_logger.MigrateUp(v, migrationName);
							migration.Up();
						}
						else
						{
							_logger.MigrateDown(v, migrationName);
							migration.Down();
						}
						RunTearDown();
					}
					catch (Exception ex)
					{
						_logger.Exception(v, migrationName, ex);

						// Oho! error! We rollback changes.
						_logger.RollingBack(originalVersion);
						_provider.Rollback();

						throw;
					}

				}
				else
				{
					// The migration number is not found
					_logger.Skipping(v);
				}

				if (goingUp)
				{
					if (v == version)
						break;
					v++;
				}
				else
				{
					v--;
					// When we go back to previous versions
					// we don't invoke Down() of the current
					// version.
					if (v == version)
						break;
				}
			}

			// Update and commit all changes
			_provider.CurrentVersion = version;
			

			_provider.Commit();
			_logger.Finished(originalVersion, version);

			try
			{
				if (version == LastVersion)
				{
					RunGlobalTearDown();
				}
			}
			catch (Exception ex)
			{
				_logger.Exception(v, "Global Tear down", ex);
				//throw;
			}
		}

		/// <summary>
		/// Run all migrations up to the latest.
		/// </summary>
		public void MigrateToLastVersion()
		{
			MigrateTo(LastVersion);
		}

		/// <summary>
		/// Returns the last version of the migrations.
		/// </summary>
		public int LastVersion
		{
			get
			{
				if (_migrationsTypes.Count == 0)
				{
					return 0;
				}
				return GetMigrationVersion((Type)_migrationsTypes[_migrationsTypes.Count - 1]);
			}
		}

		/// <summary>
		/// Returns the current version of the database.
		/// </summary>
		public int CurrentVersion
		{
			get
			{
				return _provider.CurrentVersion;
			}
		}

		/// <summary>
		/// Returns registered migration <see cref="System.Type">types</see>.
		/// </summary>
		public List<Type> MigrationsTypes
		{
			get
			{
				return _migrationsTypes;
			}
		}

		/// <summary>
		/// Get or set the event logger.
		/// </summary>
		public ILogger Logger
		{
			get
			{
				return _logger;
			}
			set
			{
				_logger = value;
			}
		}


		/// <summary>
		/// Returns the version of the migration
		/// <see cref="MigrationAttribute">MigrationAttribute</see>.
		/// </summary>
		/// <param name="t">Migration type.</param>
		/// <returns>Version number sepcified in the attribute</returns>
		public static int GetMigrationVersion(Type t)
		{
			return MigrationHelper.GetMigrationVersion(t);
		}

		/// <summary>
		/// Check for duplicated version in migrations.
		/// </summary>
		/// <exception cref="CheckForDuplicatedVersion">CheckForDuplicatedVersion</exception>
		private void CheckForDuplicatedVersion()
		{
			List<int> versions = new List<int>();

			foreach (Type t in _migrationsTypes)
			{
				int version = GetMigrationVersion(t);

				if (versions.Contains(version))
					throw new DuplicatedVersionException(version);

				versions.Add(version);
			}
		}

		/// <summary>
		/// Collect migrations in one <c>Assembly</c>.
		/// </summary>
		/// <param name="asm">The <c>Assembly</c> to browse.</param>
		/// <returns>The migrations collection</returns>
		private List<Type> GetMigrationTypes(Assembly asm)
		{
			List<Type> migrations = new List<Type>();

			foreach (Type t in asm.GetTypes())
			{
				if (MigrationHelper.IsMigration(t))
				{
					migrations.Add(t);
				}
			}

			migrations.Sort(new MigrationTypeComparer(true));

			return migrations;
		}

		/// <summary>
		/// Convert a classname to something more readable.
		/// ex.: CreateATable => Create a table
		/// </summary>
		/// <param name="className"></param>
		/// <returns></returns>
		internal static string ToHumanName(string className)
		{
			string name = Regex.Replace(className, "([A-Z])", " $1").Substring(1);
			return name.Substring(0, 1).ToUpper() + name.Substring(1).ToLower();
		}


		#region Helper methods
		private static TransformationProvider CreateProvider(string connectionString)
		{
			return new ProviderFactory().Create(connectionString);
		}

		private BaseMigration GetMigration(int version)
		{
			foreach (Type t in _migrationsTypes)
			{
				if (GetMigrationVersion(t) == version)
				{
					return (BaseMigration)Activator.CreateInstance(t);
				}
			}
			throw new Exception(String.Format("Migration {0} was not found", version));
		}

		#endregion

		private static Type GetSetUpMigrationType(Assembly asm)
		{
			List<Type> setupList = new List<Type>();

			foreach (Type t in asm.GetTypes())
			{
				SetUpMigrationAttribute attrib = (SetUpMigrationAttribute)
					Attribute.GetCustomAttribute(t, typeof(SetUpMigrationAttribute));
				if (attrib != null)
				{
					setupList.Add(t);
				}
			}

			if (setupList.Count > 1)
			{
				throw new Exception("Found more than one classes with SetUpMigrationAttribute");
			}

			if (setupList.Count == 0)
			{
				return null;
			}

			return setupList[0];
		}

		private readonly Type _setUpMigration;

		private void RunGlobalSetUp()
		{
			ExecuteSetupMethodWith(typeof(MigratorSetUp));
		}

		private void RunSetUp()
		{
			ExecuteSetupMethodWith(typeof(MigrationSetUp));
		}

		private void RunGlobalTearDown()
		{
			ExecuteSetupMethodWith(typeof(MigratorTearDown));
		}

		private void RunTearDown()
		{
			ExecuteSetupMethodWith(typeof(MigrationTearDown));
		}

		private void ExecuteSetupMethodWith(Type attributeType)
		{
			if (_setUpMigration == null)
			{
				return;
			}
			object setupObject = Activator.CreateInstance(_setUpMigration);
			MethodInfo[] methods = _setUpMigration.GetMethods();
			MethodInfo globalSetupMethod = null;
			foreach (MethodInfo method in methods)
			{
				if (method.GetCustomAttributes(attributeType, false).Length > 0)
				{
					globalSetupMethod = method;
				}
			}
			if (globalSetupMethod != null)
			{
				globalSetupMethod.Invoke(setupObject, new object[] { });
			}
		}
	}
}