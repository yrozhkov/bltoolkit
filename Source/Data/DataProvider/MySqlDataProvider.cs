// MySql Connector/Net
// http://dev.mysql.com/downloads/connector/net/
//
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using BLToolkit.Mapping;
using MySql.Data.MySqlClient;

namespace BLToolkit.Data.DataProvider
{
	using Sql.SqlProvider;
	using Common;

	public class MySqlDataProvider :  DataProviderBase
	{
		#region Static configuration

		public static char ParameterSymbol
		{
			get { return MySqlSqlProvider.ParameterSymbol;  }
			set { MySqlSqlProvider.ParameterSymbol = value; }
		}

		public static bool TryConvertParameterSymbol
		{
			get { return MySqlSqlProvider.TryConvertParameterSymbol;  }
			set { MySqlSqlProvider.TryConvertParameterSymbol = value; }
		}

		public  static string  CommandParameterPrefix
		{
			get { return MySqlSqlProvider.CommandParameterPrefix;  }
			set { MySqlSqlProvider.CommandParameterPrefix = value; }
		}

		public  static string  SprocParameterPrefix
		{
			get { return MySqlSqlProvider.SprocParameterPrefix;  }
			set { MySqlSqlProvider.SprocParameterPrefix = value; }
		}

		public  static List<char>  ConvertParameterSymbols
		{
			get { return MySqlSqlProvider.ConvertParameterSymbols;  }
			set { MySqlSqlProvider.ConvertParameterSymbols = value; }
		}

		[Obsolete("Use CommandParameterPrefix or SprocParameterPrefix instead.")]
		public  static string  ParameterPrefix
		{
			get { return MySqlSqlProvider.SprocParameterPrefix; }
			set { SprocParameterPrefix = CommandParameterPrefix = string.IsNullOrEmpty(value) ? string.Empty : value; }
		}

		public static void ConfigureOldStyle()
		{
			ParameterSymbol           = '?';
			ConvertParameterSymbols   = new List<char>(new[] { '@' });
			TryConvertParameterSymbol = true;
		}

		public static void ConfigureNewStyle()
		{
			ParameterSymbol           = '@';
			ConvertParameterSymbols   = null;
			TryConvertParameterSymbol = false;
		}

		static MySqlDataProvider()
		{
			ConfigureOldStyle();
		}
		
		#endregion

		public override IDbConnection CreateConnectionObject()
		{
			return new MySqlConnection();
		}

		public override DbDataAdapter CreateDataAdapterObject()
		{
			return new MySqlDataAdapter();
		}

		private void ConvertParameterNames(IDbCommand command)
		{
			foreach (IDataParameter p in command.Parameters)
			{
				if (p.ParameterName[0] != ParameterSymbol)
					p.ParameterName =
						Convert(
							Convert(p.ParameterName, ConvertType.SprocParameterToName),
							command.CommandType == CommandType.StoredProcedure ? ConvertType.NameToSprocParameter : ConvertType.NameToCommandParameter).ToString();
			}
		}

		public override bool DeriveParameters(IDbCommand command)
		{
			if (command is MySqlCommand)
			{
				MySqlCommandBuilder.DeriveParameters((MySqlCommand)command);

				if (TryConvertParameterSymbol && ConvertParameterSymbols.Count > 0)
					ConvertParameterNames(command);

				return true;
			}

			return false;
		}

		public override IDbDataParameter GetParameter(
			IDbCommand command,
			NameOrIndexParameter nameOrIndex)
		{
			if (nameOrIndex.ByName)
			{
				// if we have a stored procedure, then maybe command paramaters were formatted
				// (SprocParameterPrefix added). In this case we need to format given parameter name first
				// and only then try to take parameter by formatted parameter name
				var parameterName = command.CommandType == CommandType.StoredProcedure
					? Convert(nameOrIndex.Name, ConvertType.NameToSprocParameter).ToString()
					: nameOrIndex.Name;

				return (IDbDataParameter)(command.Parameters[parameterName]);
			}
			return (IDbDataParameter)(command.Parameters[nameOrIndex.Index]);
		}

		public override object Convert(object value, ConvertType convertType)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			switch (convertType)
			{
				case ConvertType.ExceptionToErrorNumber:
					if (value is MySqlException)
						return ((MySqlException)value).Number;
					break;

				case ConvertType.ExceptionToErrorMessage:
					if (value is MySqlException)
						return ((MySqlException)value).Message;
					break;
			}

			return SqlProvider.Convert(value, convertType);
		}

		public override DataExceptionType ConvertErrorNumberToDataExceptionType(int number)
		{
			switch (number)
			{
				case 1213: return DataExceptionType.Deadlock;
				case 1205: return DataExceptionType.Timeout;
				case 1216:
				case 1217: return DataExceptionType.ForeignKeyViolation;
				case 1169: return DataExceptionType.UniqueIndexViolation;
			}

			return DataExceptionType.Undefined;
		}

		public override Type ConnectionType
		{
			get { return typeof(MySqlConnection); }
		}

		public override string Name
		{
			get { return DataProvider.ProviderName.MySql; }
		}

		public override ISqlProvider CreateSqlProvider()
		{
			return new MySqlSqlProvider();
		}

		public override void Configure(System.Collections.Specialized.NameValueCollection attributes)
		{
			var paremeterPrefix = attributes["ParameterPrefix"];
			if (paremeterPrefix != null)
				CommandParameterPrefix = SprocParameterPrefix = paremeterPrefix;

			paremeterPrefix = attributes["CommandParameterPrefix"];
			if (paremeterPrefix != null)
				CommandParameterPrefix = paremeterPrefix;

			paremeterPrefix = attributes["SprocParameterPrefix"];
			if (paremeterPrefix != null)
				SprocParameterPrefix = paremeterPrefix;

			var configName = attributes["ParameterSymbolConfig"];
			if (configName != null)
			{
				switch (configName)
				{
					case "OldStyle":
						ConfigureOldStyle();
						break;
					case "NewStyle":
						ConfigureNewStyle();
						break;
				}
			}

			var parameterSymbol = attributes["ParameterSymbol"];
			if (parameterSymbol != null && parameterSymbol.Length == 1)
				ParameterSymbol = parameterSymbol[0];

			var convertParameterSymbols = attributes["ConvertParameterSymbols"];
			if (convertParameterSymbols != null)
				ConvertParameterSymbols = new List<char>(convertParameterSymbols.ToCharArray());

			var tryConvertParameterSymbol = attributes["TryConvertParameterSymbol"];
			if (tryConvertParameterSymbol != null)
				TryConvertParameterSymbol = BLToolkit.Common.Convert.ToBoolean(tryConvertParameterSymbol);

			base.Configure(attributes);
		}


        #region InsertBatch

        public override int InsertBatch<T>(
          DbManager db,
          string insertText,
          IEnumerable<T> collection,
          MemberMapper[] members,
          int maxBatchSize, DbManager.ParameterProvider<T> getParameters)
        {
            if (collection == null)
                return 0;

            var sb = new StringBuilder();
            var sp =   (MySqlSqlProvider)CreateSqlProvider();
            int n = 0;
            int cnt = 0;

            string insertStatement = insertText.Substring(0, insertText.IndexOf(") VALUES (")).Replace("INSERT", "INSERT IGNORE").Replace("\r", "")
                .Replace("\n", "")
                .Replace("\t", " ")
                .Replace("( ", "(") + ") VALUES ";

            foreach (T item in collection)
            {
                if (sb.Length == 0)
                    sb.AppendLine(insertStatement);

                sb.Append("(");

                foreach (MemberMapper member in members)
                {
                    object value = member.GetValue(item);

                    if (value is DateTime?)
                        value = ((DateTime?)value).Value;


                    sp.BuildValue(sb, value);

                    sb.Append(", ");
                }

                sb.Length -= 2;
                sb.AppendLine("),");

                n++;

                if (n >= maxBatchSize)
                {


                    sb.Length -= 3;
                    sb.Append(";");
                    string sql = sb.ToString();

                    if (DbManager.TraceSwitch.TraceInfo)
                        DbManager.WriteTraceLine("\n" + sql.Replace("\r", ""), DbManager.TraceSwitch.DisplayName);

                    cnt += db.SetCommand(sql).ExecuteNonQuery();

                    n = 0;
                    sb.Length = 0;
                }
            }

            if (n > 0)
            {

                sb.Length -= 3;
                sb.Append(";");
                string sql = sb.ToString();

                if (DbManager.TraceSwitch.TraceInfo)
                    DbManager.WriteTraceLine("\n" + sql.Replace("\r", ""), DbManager.TraceSwitch.DisplayName);

                cnt += db.SetCommand(sql).ExecuteNonQuery();
            }

            return cnt;
        }
        #endregion
    }
}
