using System.Collections.Generic;
using System.Dynamic;
using SantinoORM.ORM;

namespace SantinoORM.Test
{
    public class TestModelC : BaseModel
    {
        public new static string Table { get; set; } = "test_table_c";

        [PrimaryKey]
        public override int Id { get; set; }

        public int Userid { get; set; }


        public static IList<dynamic> GetFields()
        {
            dynamic a = new ExpandoObject();
            a.Key = "PRI";
            a.Field = "Nr";

            dynamic b = new ExpandoObject();
            b.Key = "";
            b.Field = "userid";

            return new List<dynamic> {a, b};
        }
    }
}