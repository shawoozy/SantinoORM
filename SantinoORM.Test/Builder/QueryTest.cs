using System.Collections.Generic;
using NSubstitute;
using SantinoORM.Builder;
using Xunit;

namespace SantinoORM.Test.Builder
{
    public class QueryTest
    {
        private readonly Query _sut;

        /// <summary>
        /// Constructor. Creates the mock object(s) and class to test.
        /// </summary>
        public QueryTest()
        {
            _sut = Substitute.ForPartsOf<Query>();
        }

        [Fact]
        public void SingleWhereShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.Where("UserId", 1);
            var actual = model.BuildQuery();

            var expected = " select `devices`.* from `devices` where (`devices`.`UserId` = @UserId)";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void SingleWhereInWithListShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.WhereIn("UserId", new List<int> {1, 2, 3});
            var actual = model.BuildQuery();
            
            var expected = " select `devices`.* from `devices` where (`devices`.`UserId` in @UserId)";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SingleWhereInWithOneItemShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.WhereIn("UserId", new []{1});
            var actual = model.BuildQuery();
            
            var expected = " select `devices`.* from `devices` where (`devices`.`UserId` in @UserId)";

            Assert.Equal(expected, actual);
        }

        
        [Fact]
        public void SingleWhereRawShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.WhereRaw("`UserId` = @UserId", 1);
            var actual = model.BuildQuery();

            var expected = " select `devices`.* from `devices` where (`UserId` = @UserId)";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void NestedWhereShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.Where("UserId", 1)
                .WhereRaw("Nr = @Nr", 1)
                .OrWhere("UserId", 2)
                .WhereNotNull("Id")
                .OrWhere("UserId", 3)
                .WhereIn("Nr", 1, 2, 3);
            var actual = model.BuildQuery();

            var expected = " select `devices`.* " +
                           "from `devices` " +
                           "where (`devices`.`UserId` = @UserId and Nr = @Nr) " +
                           "or (`devices`.`UserId` = @UserId_ and `devices`.`Id` is not null) " +
                           "or (`devices`.`UserId` = @UserId__ and `devices`.`Nr` in @Nr_)";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void SingleJoinShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.Join("table", "UserId", "Id");
            var actual = model.BuildQuery();

            var expected = @" select `devices`.*, "":"" , `table`.* " +
                           "from `devices`  " +
                           "inner join `table` on `devices`.`UserId` = `table`.`Id` ";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void SingleJoinWithWhereShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.Join("table", "UserId", "Id").Where("UserId", 2);
            var actual = model.BuildQuery();

            var expected = @" select `devices`.*, "":"" , `table`.* " +
                           "from `devices`  " +
                           "inner join `table` on `devices`.`UserId` = `table`.`Id`  " +
                           "where (`devices`.`UserId` = @UserId)";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void MultipleJoinsShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.Join("tableA", "UserId", "Id")
                .LeftJoin("tableB", "UserId", "Id")
                .RightJoin("tableC", "UserId", "Id");
            var actual = model.BuildQuery();

            var expected = @" select `devices`.*, "":"" , `tableA`.*, "":"" , `tableB`.*, "":"" , `tableC`.* " +
                           "from `devices`  " +
                           "inner join `tableA` on `devices`.`UserId` = `tableA`.`Id` " +
                           "left join `tableB` on `devices`.`UserId` = `tableB`.`Id` " +
                           "right join `tableC` on `devices`.`UserId` = `tableC`.`Id` ";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void LimitShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.Limit(50);
            var actual = model.BuildQuery();

            var expected = " select `devices`.* from `devices` limit 50";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void SelectShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.Select("UserId", "Nr");
            var actual = model.BuildQuery();

            var expected = " select `devices`.`UserId`,`devices`.`Nr` from `devices`";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void SelectRawShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.SelectRaw("`UserId` as test");
            var actual = model.BuildQuery();

            var expected = " select `UserId` as test from `devices`";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void SelectWithJoinShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.Select("UserId", "Nr")
                .Join("table", "UserId", "Id");
            var actual = model.BuildQuery();

            var expected = " select `devices`.`UserId`,`devices`.`Nr` " +
                           "from `devices`  " +
                           "inner join `table` on `devices`.`UserId` = `table`.`Id` ";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void OrderByAscShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.OrderByAsc("UserId");
            var actual = model.BuildQuery();

            var expected = " select `devices`.* " +
                           "from `devices` " +
                           "order by `devices`.`UserId` asc";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void OrderByDescShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.OrderByDesc("sometable.UserId");
            var actual = model.BuildQuery();

            var expected = " select `devices`.* " +
                           "from `devices` " +
                           "order by `sometable`.`UserId` desc";

            Assert.Equal(expected, actual);
        }         
        
        [Fact]
        public void RandomOrderShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.RandomOrder();
            var actual = model.BuildQuery();

            var expected = " select `devices`.* " +
                           "from `devices` " +
                           "order by rand()";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void FromShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.From("table");
            var actual = model.BuildQuery();

            var expected = " select `table`.* " +
                           "from `table`";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void SingleHavingShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.Having("UserId", 3);
            var actual = model.BuildQuery();

            var expected = " select `devices`.* " +
                           "from `devices` " +
                           "having (`devices`.`UserId` = @UserId)";

            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void NestedHavingShouldBuildCorrectQuery()
        {
            var model = new TestModelA();
            model.Having("UserId", 3)
                .OrHaving("UserId", 4)
                .HavingRaw("`Nr` = @Nr", 5)
                .OrHaving("Nr", 7);
            var actual = model.BuildQuery();

            var expected = " select `devices`.* " +
                           "from `devices` " +
                           "having (`devices`.`UserId` = @UserId) " +
                           "or (`devices`.`UserId` = @UserId_ and `Nr` = @Nr) " +
                           "or (`devices`.`Nr` = @Nr_)";

            Assert.Equal(expected, actual);
        }
        
    }
}