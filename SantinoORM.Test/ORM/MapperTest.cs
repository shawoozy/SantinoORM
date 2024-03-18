using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using NSubstitute;
using SantinoORM.ORM;
using SantinoORM.ORM.Interfaces;
using Xunit;

namespace SantinoORM.Test.ORM
{
    public class MapperTest
    {
        private readonly Mapper _sut;

        /// <summary>
        /// Constructor. Creates the mock object(s) and class to test.
        /// </summary>
        public MapperTest()
        {
            _sut = Substitute.ForPartsOf<Mapper>();
        }

        [Fact]
        public void MapRelationsShouldMapRelationsA()
        {
            var modelA = new TestModelA {Nr = 1, Userid = 1, DeviceToken = "Testt"};
            var modelB = new TestModelB {Key = 2, KeyTwo = 2, Userid = 2};
            var modelC = new TestModelC {Id = 3, Userid = 3};

            var row = new IBaseModel[] {modelA, modelB, modelC};

            var actual = _sut.MapRelations(row) as TestModelA;

            Assert.NotNull(actual.TestModelB);
            Assert.Equal(actual.TestModelB.TestModelC.Count, 1);
        }
        
        [Fact]
        public void MapRelationsShouldMapRelationsB()
        {
            var modelA = new TestModelA {Nr = 1, Userid = 1, DeviceToken = "Testt"};
            var modelB = new TestModelB {Key = 2, KeyTwo = 2, Userid = 2};
            var modelC = new TestModelC {Id = 3, Userid = 3};
            var modelD = new TestModelD {Nr = 4, Userid = 4};

            var row = new IBaseModel[] {modelA, modelB, modelC, modelD};

            var actual = _sut.MapRelations(row) as TestModelA;

            Assert.NotNull(actual.TestModelB);
            Assert.Equal(actual.TestModelB.TestModelC.Count, 1);
            Assert.Equal(actual.TestModelD.Count, 1);
        }
        
        [Fact]
        public void MapRelationsShouldMapRelationsC()
        {
            var modelA = new TestModelA {Nr = 1, Userid = 1, DeviceToken = "Testt", TestModelD = new List<TestModelD>
            {
                new TestModelD {Nr = 5, Userid = 5}
            }};
            var modelB = new TestModelB {Key = 2, KeyTwo = 2, Userid = 2};
            var modelC = new TestModelC {Id = 3, Userid = 3};
            var modelD = new TestModelD {Nr = 4, Userid = 4};

            var row = new IBaseModel[] {modelA, modelB, modelC, modelD};

            var actual = _sut.MapRelations(row) as TestModelA;

            Assert.NotNull(actual.TestModelB);
            Assert.Equal(actual.TestModelB.TestModelC.Count, 1);
            Assert.Equal(actual.TestModelD.Count, 2);
        }
        
        [Fact]
        public void MapRelationsShouldMapRelationsD()
        {
            var modelA = new TestModelA {Nr = 1, Userid = 1, DeviceToken = "Testt", TestModelD = new List<TestModelD>
            {
                new TestModelD {Nr = 5, Userid = 5}
            }};
            var modelB = new TestModelB {Key = 2, KeyTwo = 2, Userid = 2, TestModelC =  new List<TestModelC>
            {
                  new TestModelC {Id = 6, Userid = 6}
            }};
            var modelC = new TestModelC {Id = 3, Userid = 3};
            var modelD = new TestModelD {Nr = 4, Userid = 4};

            var row = new IBaseModel[] {modelA, modelB, modelC, modelD};

            var actual = _sut.MapRelations(row) as TestModelA;

            Assert.NotNull(actual.TestModelB);
            Assert.Equal(actual.TestModelB.TestModelC.Count, 2);
            Assert.Equal(actual.TestModelD.Count, 2);
        }
        
        [Fact]
        public void MapRelationsShouldMapRelationsE()
        {
            var modelA = new TestModelA {Nr = 1, Userid = 1, DeviceToken = "Testt"};
            var modelB = new TestModelB {Key = 2, KeyTwo = 2, Userid = 2};

            var row = new IBaseModel[] {modelA, modelB};

            var actual = _sut.MapRelations(row) as TestModelA;

            Assert.NotNull(actual.TestModelB);
        }
        
        [Fact]
        public void MapRelationsShouldMapRelationsF()
        {
            var modelA = new TestModelA {Nr = 1, Userid = 1, DeviceToken = "Testt"};
            var modelC = new TestModelC {Id = 2, Userid = 2};

            var row = new IBaseModel[] {modelA, modelC};

            var actual = _sut.MapRelations(row) as TestModelA;

            Assert.Null(actual.TestModelB);
        }
        
        [Fact]
        public void MapRelationsShouldMapRelationsG()
        {
            var modelA = new TestModelA
            {
               Nr = 1, Userid = 1, DeviceToken = "Testt",
               TestModelD = new List<TestModelD>
               {
                   new TestModelD
                   {
                       Nr = 3, Userid = 21,
                       TestModelE = new List<TestModelE>
                       {
                           new TestModelE
                           {
                              Id = 1, Userid = 23,
                              TestModelF = new List<TestModelF>{ new TestModelF {Id = 4, Userid = 5}}
                           }
                       }
                   }
               }
            };
            
            var modelE = new TestModelE { Id = 1, Userid = 23};
            var modelD = new TestModelD { Nr = 3, Userid = 21};
            var modelF = new TestModelF { Id = 6, Userid = 7};

            var row = new IBaseModel[] {modelA, modelD, modelE, modelF};

            var actual = _sut.MapRelations(row) as TestModelA;

            Assert.Equal(1, actual.TestModelD.Count);
            Assert.Equal(1, actual.TestModelD.First(x => x.Nr == 3).TestModelE.Count);
            Assert.Equal(2, actual.TestModelD.First(x => x.Nr == 3).TestModelE.First(x => x.Id == 1).TestModelF.Count);
        }
    }
}