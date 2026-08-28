using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace ColumbusSync.BranchBC.Source
{
    /// <summary>
    /// mdb(Access) 쿼리 파라미터 1개. OLE DB(Jet/ACE)는 SQL Server와 달리 이름이 아니라
    /// "?" 자리표시자가 SQL 문에 나오는 순서로 값을 채운다 — 이름은 로그/디버깅용일 뿐,
    /// 실제로는 Params 리스트에 넣은 순서가 그대로 "?" 순서와 일치해야 한다.
    /// </summary>
    public class OleDbParam
    {
        public string Name { get; }
        public object Value { get; }
        public OleDbType DbType { get; }

        public OleDbParam(string name, object value, OleDbType dbType = OleDbType.VarWChar)
        {
            Name = name;
            Value = value;
            DbType = dbType;
        }
    }

    /// <summary>
    /// mdb 파일 쿼리 헬퍼. ColumbusSync.BranchA의 SqlHelper와 같은 역할이지만 Access(OLE DB)용이다.
    /// 이 프로젝트도 독립 배포 요건에 따라 다른 프로젝트를 참조하지 않고 자체적으로 둔다.
    /// </summary>
    public static class OleDbHelper
    {
        public static DataTable GetDataTable(string connectionString, string sql, IEnumerable<OleDbParam> parameters)
        {
            using (var connection = new OleDbConnection(connectionString))
            using (var command = new OleDbCommand(sql, connection) { CommandTimeout = 60 })
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(new OleDbParameter(parameter.Name, parameter.DbType) { Value = parameter.Value ?? DBNull.Value });
                }

                connection.Open();

                var table = new DataTable();
                using (var adapter = new OleDbDataAdapter(command))
                {
                    adapter.Fill(table);
                }

                return table;
            }
        }
    }
}
