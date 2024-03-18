using System.Collections.Generic;
using System.Dynamic;
using SantinoORM.ORM;

namespace SantinoORM.Test
 {
     public class TestModelA : BaseModel
     {
         public new static string Table { get; set; } = "devices";
         
         [PrimaryKey]
         public int Nr { get; set; }
         
         public int Userid { get; set; }
                     
         public string DeviceToken { get; set; }
         
         public TestModelB TestModelB { get; set; }
         
         public IList<TestModelD> TestModelD { get; set; }
 
         public override object GetPrimaryKey()
         {
             return Nr;
         }
         
         public static IList<object> GetFields()
         {
             dynamic a = new ExpandoObject();
             a.Key = "PRI";
             a.Field = "Nr";
             
             dynamic b = new ExpandoObject();
             b.Key = "";
             b.Field = "userid";
             
             dynamic c = new ExpandoObject();
             c.Key = "";
             c.Field = "devicetoken";
             
             return new List<object> {a, b, c};
         }
     }
 }