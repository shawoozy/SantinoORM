using System.Linq;
using System.Reflection;
using Moq;
using SantinoORM.Builder;
using Xunit;

namespace SantinoORM.Test.Builder
{
    public class FieldsTest
    {  
        [Fact]
        public void GetPrimaryKeyPropertiesShouldReturnProperties()
        {
            var actual = Fields.PrimaryKeyProperties(new TestModelB());
            var expected = new TestModelB().GetType().GetProperties().Where(x => x.Name == "Key" || x.Name == "KeyTwo");
            Assert.Equal(actual, expected);
        }
        
        [Fact]
        public void GetPrimaryKeyPropertiesShouldReturnWrongProperty()
        {
            var actual = Fields.PrimaryKeyProperties(new TestModelA());
            var expected = new TestModelA().GetType().GetProperties().Where(x => x.Name == "Nr");
            Assert.Equal(actual, expected);
        }
    }
}