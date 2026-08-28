using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ColumbusSync.BranchBC.Hub
{
    /// <summary>통합 허브 DB에 보낼 SQL 파라미터 1개.</summary>
    public class SqlParam
    {
        public string Name { get; }
        public object Value { get; }
        public SqlDbType DbType { get; }

        public SqlParam(string name, object value, SqlDbType dbType = SqlDbType.NVarChar)
        {
            Name = name;
            Value = value;
            DbType = dbType;
        }
    }

    /// <summary>통합 허브 DB(SQL Server) 쓰기 헬퍼. 독립 배포 요건에 따라 다른 프로젝트를
    /// 참조하지 않고 이 프로젝트 안에 별도로 둔다.</summary>
    public static class SqlHelper
    {
        public static int ExecuteNonQuery(string connectionString, string commandText, IEnumerable<SqlParam> parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(commandText, connection) { CommandTimeout = 60 })
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter.Name, parameter.DbType).Value = parameter.Value ?? DBNull.Value;
                }

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
    }
}
