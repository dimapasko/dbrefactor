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

namespace DbRefactor
{
	/// <summary>
	/// Comparer of Migration by their version attribute.
	/// </summary>
	sealed class MigrationTypeComparer : IComparer<Type>
	{
		private readonly bool ascending = true;

		public MigrationTypeComparer(bool ascending)
		{
			this.ascending = ascending;
		}

		public int Compare(Type x, Type y)
		{
			int xVersion = MigrationHelper.GetMigrationVersion(x);
			int yVersion = MigrationHelper.GetMigrationVersion(y);

			return ascending 
				? xVersion - yVersion 
				: yVersion - xVersion;
		}
	}
}