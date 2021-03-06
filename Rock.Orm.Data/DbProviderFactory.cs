using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Rock.Orm.Common;

namespace Rock.Orm.Data
{
    /// <summary>
    /// The db provider factory.
    /// </summary>
    public sealed class DbProviderFactory
    {
        #region Private Members

        private static Dictionary<string, DbProvider> providerCache = new Dictionary<string, DbProvider>();

        private DbProviderFactory()
        {
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Creates the db provider.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <param name="className">Name of the class.</param>
        /// <param name="connStr">The conn STR.</param>
        /// <returns>The db provider.</returns>
        public static DbProvider CreateDbProvider(string assemblyName, string className, string connStr)
        {
            #region 原始版本
            ////Check.Require(!string.IsNullOrEmpty(className), "className could not be null.");
            //Check.Require(!string.IsNullOrEmpty(connStr), "connStr could not be null.");

            //if (connStr.ToLower().Contains("microsoft.jet.oledb") || connStr.ToLower().Contains(".db3"))
            //{
            //    string mdbPath = connStr.Substring(connStr.ToLower().IndexOf("data source") + "data source".Length + 1).TrimStart(' ', '=');
            //    if (mdbPath.ToLower().StartsWith("|datadirectory|"))
            //    {
            //        mdbPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\App_Data" + mdbPath.Substring("|datadirectory|".Length);
            //    }
            //    else if (mdbPath.StartsWith("~/") || mdbPath.StartsWith("~\\"))
            //    {
            //        mdbPath = mdbPath.Replace("/", "\\").Replace("~\\", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\");
            //    }
            //    else if (mdbPath.StartsWith("./") || mdbPath.StartsWith(".\\"))
            //    {
            //        mdbPath = mdbPath.Replace("/", "\\").Replace(".\\", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\");
            //    }

            //    connStr = connStr.Substring(0, connStr.ToLower().IndexOf("data source")) + "Data Source=" + mdbPath;
            //}

            ////by default, using sqlserver db provider
            //if (string.IsNullOrEmpty(className))
            //{
            //    className = typeof(SqlServer.SqlDbProvider).ToString();
            //}
            //else if (className.ToLower().IndexOf("System.Data.SqlClient".ToLower()) >= 0 || className.ToLower().Trim() == "sql" || className.ToLower().Trim() == "sqlserver")
            //{
            //    className = typeof(SqlServer.SqlDbProvider).ToString();
            //}
            //else if (className.ToLower().Trim() == "sql9" || className.ToLower().Trim() == "sqlserver9" || className.ToLower().Trim() == "sqlserver2005" || className.ToLower().Trim() == "sql2005")
            //{
            //    className = typeof(SqlServer9.SqlDbProvider9).ToString();
            //}
            //else if (className.ToLower().IndexOf("oracle".ToLower()) >= 0)
            //{
            //    className = typeof(Oracle.OracleDbProvider).ToString();
            //}
            //else if (className.ToLower().IndexOf("access".ToLower()) >= 0)
            //{
            //    className = typeof(MsAccess.AccessDbProvider).ToString();
            //}
            //else if (className.ToLower().IndexOf("mysql".ToLower()) >= 0)
            //{
            //    className = typeof(MySql.MySqlDbProvider).ToString();
            //}
            //else if (className.ToLower().IndexOf("sqlite".ToLower()) >= 0)
            //{
            //    className = typeof(Sqlite.SqliteDbProvider).ToString();
            //}

            //string cacheKey = string.Concat(assemblyName, className, connStr);
            //lock (providerCache)
            //{
            //    if (providerCache.TryGetValue(cacheKey))
            //    {
            //        return providerCache[cacheKey];
            //    }
            //    else
            //    {
            //        System.Reflection.Assembly ass;

            //        if (assemblyName == null)
            //        {
            //            ass = typeof(DbProvider).Assembly;
            //        }
            //        else
            //        {
            //            ass = System.Reflection.Assembly.Load(assemblyName);
            //        }

            //        DbProvider retProvider = ass.CreateInstance(className, false, System.Reflection.BindingFlags.Default, null, new object[] { connStr }, null, null) as DbProvider;
            //        providerCache.Add(cacheKey, retProvider);
            //        return retProvider;
            //    }
            //}
            #endregion

            //Check.Require(!string.IsNullOrEmpty(className), "className could not be null.");
            Check.Require(!string.IsNullOrEmpty(connStr), "connStr could not be null.");

            if (connStr.ToLower().Contains("microsoft.jet.oledb") || connStr.ToLower().Contains(".db3"))
            {
                string mdbPath = connStr.Substring(connStr.ToLower().IndexOf("data source") + "data source".Length + 1).TrimStart(' ', '=');
                if (mdbPath.ToLower().StartsWith("|datadirectory|"))
                {
                    mdbPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\App_Data" + mdbPath.Substring("|datadirectory|".Length);
                }
                else if (mdbPath.StartsWith("~/") || mdbPath.StartsWith("~\\"))
                {
                    mdbPath = mdbPath.Replace("/", "\\").Replace("~\\", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\");
                }
                else if (mdbPath.StartsWith("./") || mdbPath.StartsWith(".\\"))
                {
                    mdbPath = mdbPath.Replace("/", "\\").Replace(".\\", AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\");
                }

                connStr = connStr.Substring(0, connStr.ToLower().IndexOf("data source")) + "Data Source=" + mdbPath;
            }

            //by default, using sqlserver db provider
            if (string.IsNullOrEmpty(className))
            {
                className = typeof(SqlServer.SqlDbProvider).ToString();
            }
            else if (className.ToLower().IndexOf("System.Data.SqlClient".ToLower()) >= 0 || className.ToLower().Trim() == "sql" || className.ToLower().Trim() == "sqlserver")
            {
                className = typeof(SqlServer.SqlDbProvider).ToString();

                string[] connStrs = connStr.Split(';');
                string[] pwdStr = connStrs[3].Split('=');
                string decryptResult = ECDA.SecretUtil.Decrypt(pwdStr[1]);
                connStr = connStrs[0] + ";" + connStrs[1] + ";" + connStrs[2] + ";" + pwdStr[0] + "=" + decryptResult;

            }
            else if (className.ToLower().Trim() == "sql9" || className.ToLower().Trim() == "sqlserver9" || className.ToLower().Trim() == "sqlserver2005" || className.ToLower().Trim() == "sql2005")
            {
                className = typeof(SqlServer9.SqlDbProvider9).ToString();
            }
            else if (className.ToLower().IndexOf("oracle".ToLower()) >= 0)
            {
                className = typeof(Oracle.OracleDbProvider).ToString();

                string[] connStrs = connStr.Split(';');
                string[] pwdStr = connStrs[2].Split('=');
                string decryptResult = ECDA.SecretUtil.Decrypt(pwdStr[1]);
                connStr = connStrs[0] + ";" + connStrs[1] + ";" + pwdStr[0] + "=" + decryptResult + ";" + connStrs[3];

            }
            else if (className.ToLower().IndexOf("access".ToLower()) >= 0)
            {
                className = typeof(MsAccess.AccessDbProvider).ToString();
            }
            else if (className.ToLower().IndexOf("mysql".ToLower()) >= 0)
            {
                className = typeof(MySql.MySqlDbProvider).ToString();
            }
            else if (className.ToLower().IndexOf("sqlite".ToLower()) >= 0)
            {
                className = typeof(Sqlite.SqliteDbProvider).ToString();
            }

            string cacheKey = string.Concat(assemblyName, className, connStr);
            lock (providerCache)
            {
                DbProvider dbPropvider = null;
                if (providerCache.TryGetValue(cacheKey, out dbPropvider))
                {
                    return dbPropvider;
                }
                else
                {
                    System.Reflection.Assembly ass;

                    if (assemblyName == null)
                    {
                        ass = typeof(DbProvider).Assembly;
                    }
                    else
                    {
                        ass = System.Reflection.Assembly.Load(assemblyName);
                    }

                    dbPropvider = ass.CreateInstance(className, false, System.Reflection.BindingFlags.Default, null, new object[] { connStr }, null, null) as DbProvider;
                    providerCache.Add(cacheKey, dbPropvider);
                    return dbPropvider;
                }
            }
        }

        /// <summary>
        /// Gets the default db provider.
        /// </summary>
        /// <value>The default.</value>
        public static DbProvider Default
        {
            get
            {
                try
                {
                    ConnectionStringSettings connStrSetting = ConfigurationManager.ConnectionStrings[ConfigurationManager.ConnectionStrings.Count - 1];
                    string[] assAndClass = connStrSetting.ProviderName.Split(',');
                    if (assAndClass.Length > 1)
                    {
                        return CreateDbProvider(assAndClass[1].Trim(), assAndClass[0].Trim(), connStrSetting.ConnectionString);
                    }
                    else
                    {
                        return CreateDbProvider(null, assAndClass[0].Trim(), connStrSetting.ConnectionString);
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Creates the db provider.
        /// </summary>
        /// <param name="connStrName">Name of the conn STR.</param>
        /// <returns>The db provider.</returns>
        public static DbProvider CreateDbProvider(string connStrName)
        {
            #region 原始版本
            //Check.Require(connStrName != null, "connStrName could not be null.");

            //ConnectionStringSettings connStrSetting = ConfigurationManager.ConnectionStrings[connStrName];
            //string[] connStr = connStrSetting.ConnectionString.Split(';');
            //string[] pwdStr = connStr[3].Split('=');
            //string decryptResult = ECDA.SecretUtil.Decrypt(pwdStr[1]);
            //string connectStr = connStr[0] + ";" + connStr[1] + ";" + connStr[2] + ";" + pwdStr[0] + "=" + decryptResult;
            //ConnectionStringSettings c = new ConnectionStringSettings(connStrName, connectStr);

            //Check.Invariant(c != null, null, new ConfigurationErrorsException(string.Concat("Cannot find specified connection string setting named as ", connStrName, " in application config file's ConnectionString section.")));
            //string[] assAndClass = c.ProviderName.Split(',');
            //if (assAndClass.Length > 1)
            //{
            //    return CreateDbProvider(assAndClass[0].Trim(), assAndClass[1].Trim(), c.ConnectionString);
            //}
            //else
            //{
            //    return CreateDbProvider(null, assAndClass[0].Trim(), c.ConnectionString);
            //}
            #endregion

            Check.Require(connStrName != null, "connStrName could not be null.");

            ConnectionStringSettings connStrSetting = ConfigurationManager.ConnectionStrings[connStrName];
            Check.Invariant(connStrSetting != null && string.IsNullOrEmpty(connStrSetting.ToString()), null, new ConfigurationErrorsException(string.Concat("Cannot find specified connection string setting named as ", connStrName, " in application config file's ConnectionString section.")));
            Check.Invariant(connStrSetting.ProviderName == null, null, new ConfigurationErrorsException(string.Concat("Cannot find specified connection string setting named as ", connStrName, " in application config file's ConnectionString section.")));
            
            string[] assAndClass = connStrSetting.ProviderName.Split(',');
            if (assAndClass.Length > 1)
            {
                return CreateDbProvider(assAndClass[0].Trim(), assAndClass[1].Trim(), connStrSetting.ConnectionString);
            }
            else
            {
                return CreateDbProvider(null, assAndClass[0].Trim(), connStrSetting.ConnectionString);
            }
        }

        public static string CreateConnectionString(string connStrName)
        {
            Check.Require(connStrName != null, "connStrName could not be null.");

            ConnectionStringSettings connStrSetting = ConfigurationManager.ConnectionStrings[connStrName];
            string[] connStr = connStrSetting.ConnectionString.Split(';');
            string[] pwdStr = connStr[3].Split('=');
            string decryptResult = ECDA.SecretUtil.Decrypt(pwdStr[1]);
            string connectStr = connStr[0] + ";" + connStr[1] + ";" + connStr[2] + ";" + pwdStr[0] + "=" + decryptResult;

            return connectStr;
        }

        #endregion
    }
}
