using System.Collections.Generic;
using NSubstitute;
using SantinoORM.Builder;
using SantinoORM.Builder.Interfaces;
using SantinoORM.ORM;
using Xunit;

namespace SantinoORM.Test.Builder
{
    public class InsertTest
    {
        private readonly IExecute _execute;
        private IDataContext _dataContext;

        /// <summary>
        /// Constructor. Creates the mock object(s) and class to test.
        /// </summary>
        public InsertTest()
        {        
            // execute query class mock 
           _execute = Substitute.For<IExecute>();
            Substitute.For<InsertClause>(_execute);

            _dataContext = Substitute.For<IDataContext>();
        }
        
        [Fact]
        public void SaveShouldBuildCorrectQuery()
        {
            var model = new TestModelA {Userid = 1, DeviceToken = "test"};
            var columns = TestModelA.GetFields();

            _execute.ColumnsFromTable(Arg.Is("`devices`"), _dataContext).Returns(columns);
            
            var actual = InsertClause.BuildSaveQuery("`devices`", _dataContext);
            
            var expected = "insert into `devices` (`userid`, `devicetoken`) values (@userid, @devicetoken)";
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]                                                                                                         
        public void SaveShouldBuildCorrectQueryDifferentTable()                                                                      
        {           
            var model = new TestModelA {Userid = 1, DeviceToken = "test"};
            var columns = TestModelA.GetFields();

            _execute.ColumnsFromTable(Arg.Is("`test_table`"), _dataContext).Returns(columns);
            
            var actual = InsertClause.BuildSaveQuery("`test_table`", _dataContext);
            
            var expected = "insert into `test_table` (`userid`, `devicetoken`) values (@userid, @devicetoken)";
            
            Assert.Equal(expected, actual);                                                                     
        }      
        
        [Fact]
        public void SaveShouldBuildCorrectQueryMultiplePrimaryKeys()
        {
            var model = new TestModelB {Userid = 1};
            var columns = TestModelB.GetFields();

            _execute.ColumnsFromTable(Arg.Is("`test_table`"), _dataContext).Returns(columns);
            
            var actual = InsertClause.BuildSaveQuery("`test_table`", _dataContext);
            
            var expected = "insert into `test_table` (`userid`) values (@userid)";
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void SaveShouldBuildCorrectQueryMultipleModels()
        {
            var model = new TestModelA {Userid = 1, DeviceToken = "test"};
            var model2 = new TestModelA {Userid = 2, DeviceToken = "test"};
            var modelList = new List<BaseModel> {model, model2};
            var columns = TestModelA.GetFields();

            _execute.ColumnsFromTable(Arg.Is("`devices`"), null).Returns(columns);
            
            var actual = InsertClause.BuildBulkSaveQuery(modelList);
            
            var expected = @"insert into `devices` (`userid`, `devicetoken`) values (""1"", ""test""), (""2"", ""test"")";
            
            Assert.Equal(expected, actual);
        }
     

                                                                                                                       
                                                                                                                       
                                                                                                                       














    }
}