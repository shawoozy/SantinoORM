using System.Collections.Generic;
using System.Dynamic;
using SantinoORM.ORM;

namespace SantinoORM.Test
{
    public class TestModelB : BaseModel
    {
        public new static string Table { get; set; } = "test_table";

        [PrimaryKey]
        public int Key { get; set; }

        [PrimaryKey]
        public int KeyTwo { get; set; }

        public int Userid { get; set; }

        public IList<TestModelC> TestModelC { get; set; }
        
        public override object GetPrimaryKey()
        {
            return new {Key, KeyTwo};
        }
        
        public static IList<dynamic> GetFields()
        {
           
                dynamic a = new ExpandoObject();
                a.Key = "PRI";
                a.Field = "key";
             
                dynamic b = new ExpandoObject();
                b.Key = "";
                b.Field = "userid";
             
                dynamic c = new ExpandoObject();
                c.Key = "PRI";
                c.Field = "keytwo";
             
                return new List<dynamic> {a, c, b};
            
        }
    }
}