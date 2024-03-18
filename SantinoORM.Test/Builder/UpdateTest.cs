using System.Collections.Generic;
using NSubstitute;
using SantinoORM.Builder;
using SantinoORM.Builder.Interfaces;
using SantinoORM.ORM;
using Xunit;

namespace SantinoORM.Test.Builder
{
    public class UpdateTest
    {
        private readonly IExecute _execute;
        private IDataContext _dataContext;

        /// <summary>
        /// Constructor. Creates the mock object(s) and class to test.
        /// </summary>
        public UpdateTest()       
        {   
            // execute query class mock 
           _execute =  Substitute.For<IExecute>();
            Substitute.For<UpdateClause>(_execute);
            
            _dataContext = Substitute.For<IDataContext>();
        }
        
        [Fact]
        public void UpdateCustomPrimaryKeyShouldBuildCorrectQuery()
        {
            var model = new TestModelA {Nr = 1, Userid = 1, DeviceToken = "test"};
            model.SetOriginal(model.Clone());

            model.Userid = 2;
            model.DeviceToken = "changed";
            var columns = TestModelA.GetFields(); 

            _execute.ColumnsFromTable(Arg.Is("`devices`"), null).Returns(columns);
  
            var actual = UpdateClause.BuildUpdateQuery("`devices`", model);
            
            var expected = "update `devices` set `userid` = @userid, `devicetoken` = @devicetoken where Nr = 1";
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void UpdateShouldBuildCorrectQuery()
        {
            var model = new TestModelC {Id = 1, Userid = 3};
            model.SetOriginal(model.Clone());
            var columns = TestModelC.GetFields();

            _execute.ColumnsFromTable(Arg.Is("`test_table_c`"), null).Returns(columns);
            model.Userid = 5;

            var actual = UpdateClause.BuildUpdateQuery("`test_table_c`", model);
            
            var expected = "update `test_table_c` set `userid` = @userid where Id = 1";
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void UpdateShouldNotBuildCorrect()
        {
            var model = new TestModelC {Id = 1, Userid = 3};
            model.SetOriginal(model.Clone());
            var columns = TestModelC.GetFields();

            _execute.ColumnsFromTable(Arg.Is("`test_table_c`"), null).Returns(columns);

            var actual = UpdateClause.BuildUpdateQuery("`test_table_c`", model);
            
            var expected = "";
            
            Assert.Equal(expected, actual);
        }
        
        
                
        [Fact]
        public void UpdateShouldBuildCorrectQueryMultipleKeys()
        {
            var model = new TestModelB {Key = 1, KeyTwo = 1, Userid = 3};
            model.SetOriginal(model.Clone());
            var columns = TestModelB.GetFields();

            model.Userid = 4;

            _execute.ColumnsFromTable(Arg.Is("`test_table`"), null).Returns(columns);

            var actual = UpdateClause.BuildUpdateQuery("`test_table`", model);
            
            var expected = "update `test_table` set `userid` = @userid where Key = 1 and KeyTwo = 1";
            
            Assert.Equal(expected, actual);
        }
       
        
        [Fact]
        public void UpdateShouldBuildCorrectQueryMultipleModels()
        {
            var model = new TestModelA {Nr = 1, Userid = 1, DeviceToken = "test"};
            var model2 = new TestModelA {Nr = 2, Userid = 2, DeviceToken = "test"};
            var list = new List<BaseModel> {model, model2};
            var columns = TestModelA.GetFields();

            _execute.ColumnsFromTable(Arg.Is("`devices`"), null).Returns(columns);

            var actual = UpdateClause.BuildBulkUpdateQuery(list);
            
            var expected = @"insert into `devices` (`Nr`, `userid`, `devicetoken`) values (""1"", ""1"", ""test""), (""2"", ""2"", ""test"") on duplicate key update `userid` = VALUES(`userid`), `devicetoken` = VALUES(`devicetoken`)";
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void UpdateShouldBuildCorrectQueryMultipleModelsMultipleKeys()
        {
            var model = new TestModelB {Key = 1, KeyTwo = 1, Userid = 3};
            var model2 = new TestModelB {Key = 2, KeyTwo = 2, Userid = 4};
            var list = new List<BaseModel> {model, model2};
            var columns = TestModelB.GetFields();

            _execute.ColumnsFromTable(Arg.Is("`test_table`"), null).Returns(columns);

            var actual = UpdateClause.BuildBulkUpdateQuery(list);
            
            var expected = @"insert into `test_table` (`key`, `keytwo`, `userid`) values (""1"", ""1"", ""3""), (""2"", ""2"", ""4"") on duplicate key update `userid` = VALUES(`userid`)";
            
            Assert.Equal(expected, actual);
        }
    }
}