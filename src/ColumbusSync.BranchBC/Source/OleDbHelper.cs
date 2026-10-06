using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Threading.Tasks;

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
        // mdb 파일이 다른 프로그램(TS2020 등)에 의해 잠겨 있으면 OleDbConnection.Open()이
        // 예외도 없이 무기한 멈출 수 있다 - CommandTimeout은 쿼리 실행 단계에만 적용되고,
        // Jet/ACE 드라이버는 파일 잠금 대기에는 Connect Timeout을 제대로 지키지 않는 경우가
        // 있다. 그대로 두면 이 백그라운드 프로세스가 영원히 멈춰 있어도 예외가 안 나서 로그에
        // 아무것도 안 남고(Program.cs의 catch도 못 잡는다) 동기화가 그냥 멈춰버린다. 별도
        // 스레드에서 실행하고 바깥에서 타임아웃을 걸어, 못 끝내면 예외로 실패 처리해 다음
        // 루프에서 재시도되게 한다.
        private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(90);

        public static DataTable GetDataTable(string connectionString, string sql, IEnumerable<OleDbParam> parameters)
        {
            var parameterList = new List<OleDbParam>(parameters);

            var task = Task.Run(() => ExecuteQuery(connectionString, sql, parameterList));
            if (!task.Wait(OperationTimeout))
            {
                throw new TimeoutException(string.Format(
                    "mdb 쿼리가 {0}초 안에 끝나지 않았습니다(파일이 다른 프로그램에 의해 잠겨 있을 수 있습니다): {1}",
                    OperationTimeout.TotalSeconds, sql));
            }

            return task.Result;
        }

        private static DataTable ExecuteQuery(string connectionString, string sql, IEnumerable<OleDbParam> parameters)
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
