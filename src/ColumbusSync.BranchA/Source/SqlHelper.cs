using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ColumbusSync.BranchA.Source
{
    /// <summary>단일 저장프로시저 호출 파라미터.</summary>
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

    /// <summary>
    /// 이 프로젝트는 메인 WinForms 프로젝트(ColumbusWeighing)를 참조하지 않고 완전히 독립적으로
    /// 빌드/배포되어야 하므로, DBConn과 같은 역할을 하는 최소한의 헬퍼를 이 프로젝트 안에
    /// 별도로 둔다. (독립 배포 요건: App.config만 채우면 이 폴더 하나로 다른 PC에 복사해서
    /// 그대로 돌릴 수 있어야 한다.)
    /// </summary>
    public static class SqlHelper
    {
        public static DataTable GetDataTable(string connectionString, string spName, IEnumerable<SqlParam> parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(spName, connection) { CommandType = CommandType.StoredProcedure, CommandTimeout = 60 })
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter.Name, parameter.DbType).Value = parameter.Value ?? DBNull.Value;
                }

                connection.Open();

                var table = new DataTable();
                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(table);
                }

                return table;
            }
        }

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
