using System.Data;

namespace ColumbusWeighing.ComnLib
{
    /// <summary>저장프로시저 호출 파라미터 1개(이름/값/SQL 타입). VisionIns 솔루션의 Parameter 클래스와 동일하다.</summary>
    public class Parameter
    {
        public string Key { get; set; }

        public object Value { get; set; }

        public SqlDbType DbType { get; set; }

        public Parameter(string key, object value)
            : this(key, value, SqlDbType.NVarChar)
        {
        }

        public Parameter(string key, object value, SqlDbType dbType)
        {
            Key = key;
            Value = value;
            DbType = dbType;
        }
    }
}
