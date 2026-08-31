using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>
    /// MSSQL 조회 헬퍼. VisionIns 솔루션의 DBConn과 같은 역할이며, SqlWeighingRepository 등
    /// DB 연동 리포지토리가 통합 허브 DB(COLUMBUS_WEIGH_HUB)를 조회할 때 사용한다.
    ///
    /// 원본과 달리 연결 문자열을 소스에 하드코딩하지 않고 App.config의 connectionStrings에서 읽으며,
    /// "기존 커넥션 재사용" / "커넥션 자동 생성" 두 갈래로 중복 구현되어 있던 원본 오버로드는
    /// 커넥션을 매 호출마다 열고 닫는 자동 생성 방식 하나로 통합했다.
    /// </summary>
    public static class DBConn
    {
        public static DataTable GetDataTable(string commandText, List<Parameter> parameters, CommandType commandType = CommandType.StoredProcedure)
        {
            using (var connection = new SqlConnection(ComnString.ConnectionString))
            using (var command = new SqlCommand(commandText, connection) { CommandType = commandType })
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(string.Format("@{0}", parameter.Key), parameter.DbType).Value = parameter.Value ?? DBNull.Value;
                }

                connection.Open();

                var dataSet = new DataSet();
                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataSet);
                }

                return dataSet.Tables.Count > 0 ? dataSet.Tables[0] : new DataTable();
            }
        }

        public static DataTable GetDataTable(string sql)
        {
            using (var connection = new SqlConnection(ComnString.ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                connection.Open();

                var dataSet = new DataSet();
                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataSet);
                }

                return dataSet.Tables.Count > 0 ? dataSet.Tables[0] : new DataTable();
            }
        }

        public static int ExecuteNonQuery(string spName, List<Parameter> parameters)
        {
            using (var connection = new SqlConnection(ComnString.ConnectionString))
            using (var command = new SqlCommand(spName, connection) { CommandType = CommandType.StoredProcedure })
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(string.Format("@{0}", parameter.Key), parameter.DbType).Value = parameter.Value ?? DBNull.Value;
                }

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
    }
}
