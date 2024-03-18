using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Newtonsoft.Json;
using SantinoORM.Configuration;
using SantinoORM.ORM;
using SantinoORM.ORM.Interfaces;

namespace SantinoORM.Builder
{
    public abstract class Query
    {

        /// <summary>
        /// The table which the query is targeting.
        /// </summary>
        [JsonIgnore] protected internal string Table;

        /// <summary>
        /// The joins for the query.
        /// </summary>
        private IList<Tuple<string, IList<string>>> Joins { get; } = new List<Tuple<string, IList<string>>>();

        /// <summary>
        /// Indicates if the query returns distinct results.
        /// </summary>
        private bool SelectDistinct { get; set; }

        /// <summary>
        /// The Value bindings.
        /// </summary>
        [JsonIgnore]
        protected DynamicParameters ValueBindings { get; } = new DynamicParameters();


        /// <summary>
        ///  All of the available clause operators.
        /// </summary>
        private static string[] Operators { get; set; } =
        {
            "=", "<", ">", "<=", ">=", "<>", "!=", "<=>",
            "like", "like binary", "not like", "ilike",
            "&", "|", "^", "<<", ">>", "in", "not in",
            "rlike", "regexp", "not regexp", "is null", "is not null",
            "~", "~*", "!~", "!~*", "similar to",
            "not similar to", "~~*", "!~~*",
        };
        public Query() { }

        /// <summary>
        /// The where constraints for the query.
        /// </summary>
        private IList<IList<string>> Wheres { get; } = new List<IList<string>>
        {
            new List<string>()
        };

        /// <summary>
        /// The havings constraints for the query.
        /// </summary>
        private IList<IList<string>> Havings { get; } = new List<IList<string>>
        {
            new List<string>()
        };

        /// <summary>
        /// The columns that should be returned.
        /// </summary>
        private List<string> Columns { get; } = new List<string>();

        /// <summary>
        /// The join columns that should be returned.
        /// </summary>
        private List<string> JoinColumns { get; } = new List<string>();

        /// <summary>
        /// The current query value bindings.
        /// </summary>
        private IList<Tuple<string, List<string>>> Bindings { get; } =
            new List<Tuple<string, List<string>>>()
            {
                new Tuple<string, List<string>>("select", new List<string>()),
                new Tuple<string, List<string>>("from", new List<string>()),
                new Tuple<string, List<string>>("join", new List<string>()),
                new Tuple<string, List<string>>("where", new List<string>()),
                new Tuple<string, List<string>>("group by", new List<string>()),
                new Tuple<string, List<string>>("having", new List<string>()),
                new Tuple<string, List<string>>("order by", new List<string>()),
                new Tuple<string, List<string>>("union", new List<string>()),
                new Tuple<string, List<string>>("limit", new List<string>()),
                new Tuple<string, List<string>>("offset", new List<string>()),
            };

        /// <summary>
        /// These indexes are for the list of tuples above.
        /// </summary>
        protected const int SelectIndex = 0;
        protected const int FromIndex = 1;
        protected const int JoinIndex = 2;
        protected const int WhereIndex = 3;
        protected const int GroupByIndex = 4;
        protected const int HavingIndex = 5;
        protected const int OrderByIndex = 6;
        protected const int UnionIndex = 7;
        protected const int LimitIndex = 8;
        protected const int OffsetIndex = 9;

        /// <summary>
        /// Returns the parameters of the query, usefull for debugging a query.
        /// </summary>
        /// <returns></returns>
        public DynamicParameters GetBindingsParameters()
        {
            return ValueBindings;
        }

        /// <summary>
        /// Returns a single row of a model by primary key.
        /// </summary>
        /// <param name="id"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T Find<T>(int id) where T : BaseModel
        {
            var primaryKey = Fields.PrimaryKeyProperties(this as IBaseModel);
            
            if (primaryKey.Count > 1)
                throw new Exception("Do not use Find method if model has multiple primary keys");

            return Where(primaryKey.First().Name, id).First<T>();
        }

        /// <summary>
        /// Add a where clause to the query
        /// </summary>
        /// <param name="column"> Column </param>
        /// <param name="sqlOperator"> Operator for the clause eg. '=', 'like'</param>
        /// <param name="value"> Value </param>
        /// <param name="or"> If its an or clause </param>
        /// <returns></returns>
        public Builder Where(string column, object sqlOperator, object value = null, bool or = false)
        {
            // when no operator is given, set operator to '='
            if (!Operators.Contains(sqlOperator))
            {
                value = sqlOperator;
                sqlOperator = value == null ? "is null" : "=";
            }

            // create new list of where statements 
            if (or)
                Wheres.Add(new List<string>());

            // the binding key for the column and value
            var paramBindingKey = column;

            // if column already has alias (users.id), do not add table        // .Where("UserId", 4).Where("notifications.Send", 1)
            column = CreateColumnString(column, Table);

            var statement = new StringBuilder(column + " " + sqlOperator);

            // add value to the statement and also to the value bindings list, this is not always the case like where null or not null clauses
            if (value != null)
            {
                // check if bindingkey exists and replace with a unique binding key
                paramBindingKey = BindingKey(paramBindingKey);

                statement.Append(" @" + paramBindingKey);
                ValueBindings.Add(paramBindingKey, value);
            }

            // add the where statement to the list of where statements
            Wheres.Last().Add(statement.ToString());

            return this as Builder;
        }

        /// <summary>
        /// Add a "where null" clause to the query
        /// </summary>
        /// <param name="columns"> Can be multiple columns</param>
        /// <returns></returns>
        public Builder WhereNull(params string[] columns)
        {
            foreach (var column in columns)
            {
                Where(column, "is null");
            }

            return this as Builder;
        }

        /// <summary>
        /// Add a "where not null" clause to the query
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Builder WhereNotNull(params string[] columns)
        {
            foreach (var column in columns)
            {
                Where(column, "is not null");
            }

            return this as Builder;
        }

        /// <summary>
        /// Add an "or where null" clause to the query
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Builder OrWhereNull(params string[] columns)
        {
            foreach (var column in columns)
            {
                Where(column, "is null", null, true);
            }

            return this as Builder;
        }

        /// <summary>
        /// Add an "or where not null" clause to the query
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Builder OrWhereNotNull(params string[] columns)
        {
            foreach (var column in columns)
            {
                Where(column, "is not null", null, true);
            }

            return this as Builder;
        }

        /// <summary>
        /// Add an "or where" clause to the query
        /// </summary>
        /// <param name="column"></param>
        /// <param name="sqlOperator"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Builder OrWhere(string column, object sqlOperator, object value = null)
        {
            return Where(column, sqlOperator, value, or: true);
        }

        /// <summary>
        /// Add a "where in" clause to the query
        /// </summary>
        /// <param name="column"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public Builder WhereIn(string column, params object[] values)
        {
            return Where(column, "in", values[0] is IEnumerable && !(values[0] is string) ? values[0] : values);
        }

        /// <summary>
        /// Add a "or where in" clause to the query.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public Builder OrWhereIn(string column, params object[] values)
        {
            return Where(column, "in", values[0] is IEnumerable && !(values[0] is string) ? values[0] : values,
                or: true);
        }

        /// <summary>
        /// Add a "where not in" clause to the query.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public Builder WhereNotIn(string column, params object[] values)
        {
            return Where(column, "not in", values[0] is IEnumerable && !(values[0] is string) ? values[0] : values);
        }


        /// <summary>
        /// Add a "or where not in" clause to the query.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public Builder OrWhereNotIn(string column, params object[] values)
        {
            return Where(column, "not in", values[0] is IEnumerable && !(values[0] is string) ? values[0] : values,
                or: true);
        }

        /// <summary>
        ///  Add a raw where clause to the query
        /// </summary>
        /// <param name="rawQuery"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public Builder WhereRaw(string rawQuery, params object[] bindings)
        {
            // add the where statement to the list of where statements
            Wheres.Last().Add(rawQuery);

            // split the raw query to a list and remove everything except the @fields to get the keys for binding the values
            var splitted = rawQuery.Split(' ').ToList();
            splitted.RemoveAll(x => !x.StartsWith("@"));

            // add the value bindings
            for (var i = 0; i < bindings.Length; i++)
            {
                var key = splitted[i];

                // if param binding already exists with the same key ( when there are multiple clause's of the same column )
                if (ValueBindings.ParameterNames.Any(x => x.Equals(key)))
                    key = key + "_";

                ValueBindings.Add(key, bindings[i]);
            }

            return this as Builder;
        }
        
        /// <summary>
        ///  Add a raw where clause to the query
        /// </summary>
        /// <param name="rawQuery"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public Builder OrWhereRaw(string rawQuery, params object[] bindings)
        {
            // create new list of where statements 
            Wheres.Add(new List<string>());
            return WhereRaw(rawQuery, bindings);
        }

        /// <summary>
        /// Add a join clause to the query
        /// </summary>
        /// <param name="table"> Table to join </param>
        /// <param name="first"> column to join of original table </param>
        /// <param name="second"> column to join on joined table </param>
        /// <param name="sqlOperator"></param>
        /// <param name="extraConstraints"> extra join constraints, as raw sql</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Builder Join(string table, string first, string second, string sqlOperator = "=",
            string extraConstraints = "", string type = "inner")
        {
            // get the last word in table string as alias is any is given
            var alias = table.Split(' ').Last();
            alias = AddBackTicks(alias);
            
            table = AddBackTicks(table.Split(' ').First());
            
            
            first =  CreateColumnString(first, Table);
            second = CreateColumnString(second, alias);
          
            // add to columns in select statement, add ':' to seperate tables
            JoinColumns.Add($@" "":"" , {alias}.*");

            if (!Operators.Contains(sqlOperator))
            {
                extraConstraints = " " + sqlOperator;
                sqlOperator = "=";
            }

            // combine everyting to make the sql statement
            var statement = table + " on " + first + " " + sqlOperator + " " + second + extraConstraints;

            // the join type already exists, so add this to the existing list of the same join type 
            var joinTuple = Joins.FirstOrDefault(x => x.Item1 == type);

            if (joinTuple != null)
                joinTuple.Item2.Add(statement);

            // create new list of this join type
            else
                Joins.Add(new Tuple<string, IList<string>>(type, new List<string>() {statement}));

            return this as Builder;
        }

        /// <summary>
        ///  Add a left join to the query
        /// </summary>
        /// <param name="table"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="sqlOperator"></param>
        /// <param name="extraConstraints"></param>
        /// <returns></returns>
        public Builder LeftJoin(string table, string first, string second, string sqlOperator = "=",
            string extraConstraints = null)
        {
            return Join(table, first, second, sqlOperator, extraConstraints, "left");
        }

        /// <summary>
        ///  Add a right join to the query
        /// </summary>
        /// <param name="table"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="sqlOperator"></param>
        /// <param name="extraConstraints"></param>
        /// <returns></returns>
        public Builder RightJoin(string table, string first, string second, string sqlOperator = "=",
            string extraConstraints = null)
        {
            return Join(table, first, second, sqlOperator, extraConstraints, "right");
        }

        /// <summary>
        /// Add a having clause to the query 
        /// </summary>
        /// <returns></returns>
        public Builder Having(string column, object sqlOperator, object value = null, bool or = false)
        {
            // when no operator is given, set operator to '='
            if (!Operators.Contains(sqlOperator))
            {
                value = sqlOperator;
                sqlOperator = "=";
            }

            // create new list of where statements 
            if (or)
            {
                Havings.Add(new List<string>());
            }

            // the binding key for the column and value
            var paramBindingKey = column;

            // if column already has alias (users.id), do not add table      
            column = CreateColumnString(column, Table);

            paramBindingKey = BindingKey(paramBindingKey);

            var statement = new StringBuilder(column + " " + sqlOperator);

            // add value to the statement and also to the value bindings list, this is not always the case like where null or not null clauses
            if (value != null)
            {
                statement.Append(" @" + paramBindingKey);
                ValueBindings.Add(paramBindingKey, value);
            }

            // add the where statement to the list of where statements
            Havings.Last().Add(statement.ToString());

            return this as Builder;
        }

        public Builder OrHaving(string column, object sqlOperator, object value = null)
        {
            return Having(column, sqlOperator, value, true);
        }

        /// <summary>
        /// Add a raw sql having clause to the query 
        /// </summary>
        /// <param name="rawQuery"></param>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public Builder HavingRaw(string rawQuery, params object[] bindings)
        {
            // add the where statement to the list of where statements
            Havings.Last().Add(rawQuery);

            // split the raw query to a list and remove everything except the @fields to get the keys for binding the values
            var splitted = rawQuery.Split(' ').ToList();
            splitted.RemoveAll(x => !x.StartsWith("@"));

            // add the value bindings
            for (var i = 0; i < bindings.Length; i++)
            {
                var key = splitted[i];

                // if param binding already exists with the same key ( when there are multiple clause's of the same column )
                if (ValueBindings.ParameterNames.Any(x => x.Equals(key)))
                    key = key + "_";

                ValueBindings.Add(key, bindings[i]);
            }

            return this as Builder;
        }

        /// <summary>
        /// Add a limit clause to the query
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public Builder Limit(int limit)
        {
            ReplaceBinding(new List<string> {$"{limit}"}, LimitIndex, "limit");
            return this as Builder;
        }

        /// <summary>
        /// Add a limit clause to the query
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Builder Limit(int limit, int offset)
        {
            ReplaceBinding(new List<string> {$"{offset}"}, LimitIndex, "offset");
            Limit(limit);
            return this as Builder;
        }

        /// <summary>
        /// Change the table to fetch from
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public Builder From(string table)
        {
            Table = AddBackTicks(table);
            return this as Builder;
        }

        /// <summary>
        /// Add columns to the select clause
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public Builder Select(params string[] column)
        {
            Columns.AddRange(column.ToList().Select(x => Table + "." + AddBackTicks(x)));
            ReplaceBinding(new List<string> {string.Join(",", Columns)}, SelectIndex, "select");
            return this as Builder;
        }

        /// <summary>
        /// Add raw sql statement to the select clause, eg functions
        /// </summary>
        /// <param name="rawQuery"></param>
        /// <returns></returns>
        public Builder SelectRaw(string rawQuery)
        {
            Columns.Add(rawQuery);
            ReplaceBinding(new List<string> {string.Join(",", Columns)}, SelectIndex, "select");
            return this as Builder;
        }

        /// <summary>
        /// Add an  descending "order by" clause to the query
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public Builder OrderByDesc(string column)
        {
            return OrderBy(column);
        }

        /// <summary>
        /// Add an  ascending "order by" clause to the query
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public Builder OrderByAsc(string column)
        {
            return OrderBy(column, " asc");
        }

        private Builder OrderBy(string column, string by = " desc")
        {
            column = column.Contains('.')
                ? string.Join(".", column.Split('.').Select(AddBackTicks))
                : AddBackTicks(column);

            column = column.Contains('.') ? column : Table + "." + column;
            Bindings.First(x => x.Item1 == "order by").Item2.Add(column + by);
            return this as Builder;
        }


        /// <summary>
        /// Return results in random order
        /// </summary>
        /// <returns></returns>
        public Builder RandomOrder()
        {
            Bindings.First(x => x.Item1 == "order by").Item2.Add("rand()");
            return this as Builder;
        }


        /// <summary>
        ///  Force the query to only return distinct results
        /// </summary>
        /// <returns></returns>
        public Builder Distinct()
        {
            SelectDistinct = true;

            return this as Builder;
        }

        /// <summary>
        /// Add a "group by" clause to the query.
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public Builder GroupBy(params string[] columns)
        {
            Bindings.First(x => x.Item1 == "group by").Item2.Add(string.Join(", ", columns));
            return this as Builder;
        }

        /// <summary>
        /// Builds the query string from the bindings
        /// </summary>
        /// <returns></returns>
        public string BuildQuery(bool select = true)
        {
            // if its a select statement, build the 'from' and 'select' parts of the query
            if (select)
            {
                BuildFrom();
                BuildSelect();
            }

            BuildJoin();

            BuildWhere();

            BuildHaving();

            var query = new StringBuilder();

            foreach (var binding in Bindings)
            {
                // Item2 is the list in the Tuple and Item1 is the key eg. "select", "join".
                if (binding.Item2.Count >= 1)
                    query.Append(" " + binding.Item1 + " " + string.Join("", binding.Item2));
            }

            return query.ToString();
        }

        /// <summary>
        /// Builds the from clause
        /// </summary>
        private void BuildFrom()
        {
            ReplaceBinding(new List<string> {Table}, FromIndex, "from");
        }

        /// <summary>
        /// Builds the join clause
        /// </summary>
        private void BuildJoin()
        {
            if (Joins.Count < 1)
                return;

            var joined = new StringBuilder();

            // for each kind of join, create the statement and add it to the string
            foreach (var join in Joins)
            {
                //                   '     inner          join   ['b on a.id = b.aid', 'c on b.cid on c.bid']'
                var st = string.Join(" " + join.Item1 + " join ", join.Item2.ToArray()) + " ";
                joined.Append(join.Item1 + " join " + st);
            }

            // replace the values and also replace the statement key 'join' to '' or else we will have 'join join'
            ReplaceBinding(new List<string> {joined.ToString()}, JoinIndex, "");
        }

        /// <summary>
        /// Builds the select clause
        /// </summary>
        private void BuildSelect()
        {
            // there are no joins and no custom selects
            if (JoinColumns.Count < 1 && Columns.Count < 1)
                ReplaceBinding(new List<string> {Table + ".*"}, SelectIndex, "select");

            // there are joins
            if (JoinColumns.Count >= 1)
                ReplaceBinding(new List<string> {Table + ".*,", string.Join(",", JoinColumns)}, SelectIndex, "select");

            // there are custom selects
            if (Columns.Count >= 1)
                ReplaceBinding(new List<string> {string.Join(",", Columns)}, SelectIndex, "select");

            // if distinct is enabled, replace 'select' with 'select distinct'
            if (!SelectDistinct)
                return;

            var selects = Bindings.First(x => x.Item1 == "select").Item2;
            ReplaceBinding(selects, SelectIndex, "select distinct");
        }

        /// <summary>
        /// Builds the where clause
        /// </summary>
        private void BuildWhere()
        {
            if (Wheres[0].Count < 1)
                return;

            var str = new StringBuilder();
            for (var i = 0; i < Wheres.Count; i++)
            {
                // add '(' and ')' for each list of wheres 
                str.Append("(");

                foreach (var statement in Wheres[i])
                {
                    if (Wheres[i].IndexOf(statement) >= 1)
                        str.Append(" and ");

                    str.Append(statement);
                }

                str.Append(")");

                if (i < Wheres.Count - 1)
                    str.Append(" or ");
            }

            ReplaceBinding(new List<string>() {str.ToString()}, WhereIndex, "where");
        }

        /// <summary>
        /// Builds the having clause
        /// </summary>
        private void BuildHaving()
        {
            if (Havings[0].Count < 1)
                return;

            var str = new StringBuilder();
            for (var i = 0; i < Havings.Count; i++)
            {
                // add '(' and ')' for each list of havings 
                str.Append("(");

                foreach (var statement in Havings[i])
                {
                    if (Havings[i].IndexOf(statement) >= 1)
                        str.Append(" and ");

                    str.Append(statement);
                }

                str.Append(")");

                if (i < Havings.Count - 1)
                    str.Append(" or ");
            }

            ReplaceBinding(new List<string>() {str.ToString()}, HavingIndex, "having");
        }


        /// <summary>
        /// Returns an unique binding key for the parameter
        /// </summary>
        /// <param name="paramBindingKey"></param>
        /// <returns></returns>
        private string BindingKey(string paramBindingKey)
        {
            // loop, until paramBindingKey is unique
            while (true)
            {
                if (!ValueBindings.ParameterNames.Contains(paramBindingKey)) return paramBindingKey;

                // if param binding already exists with the same key (when there are multiple clause's of the same column)
                var key = paramBindingKey;
                paramBindingKey = ValueBindings.ParameterNames.First(x => x.Equals(key)) + "_";
            }
        }

        /// <summary>
        /// Replace an item in the Bindings list
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="newKey"></param>
        protected void ReplaceBinding(List<string> value, int index, string key)
        {
            Bindings[index] = new Tuple<string, List<string>>(key, value);
        }

        /// <summary>
        /// UpdateClause a field with a query
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public Builder Update(Dictionary<string, object> fields)
        {
            // replace the 'select' part of the query string to 'update'
            ReplaceBinding(new List<string>() {Table}, SelectIndex, "update");

            var sb = new StringBuilder();

            // for each field, create the sql part and add value to valuebindings
            foreach (var item in fields)
            {
                var bindingKey = BindingKey(item.Key);
                ValueBindings.Add(bindingKey, item.Value);

                sb.AppendFormat("`{0}` = @{1}", item.Key, bindingKey);

                if (item.Key != fields.Last().Key)
                    sb.Append(", ");
            }

            // replace the 'from' part of the query string to 'set'
            ReplaceBinding(new List<string>() {sb.ToString()}, FromIndex, "set");

            // build the query
            var query = BuildQuery(@select: false);

            // run the query
            new Execute(MysqlConnectionString.GetConnectionString()).RunQuery(query, ValueBindings);

            return this as Builder;
        }

        /// <summary>
        /// Directly update a model
        /// </summary>
        /// <returns></returns>
        public Builder Update()
        {
            new UpdateClause(new Execute(MysqlConnectionString.GetConnectionString())).UpdateModel(Table, this as BaseModel);
            return this as Builder;
        }

        /// <summary>
        /// Save a model in the database
        /// </summary>
        /// <returns></returns>
        public Builder Insert()
        {
            return new InsertClause(new Execute(MysqlConnectionString.GetConnectionString())).Save(Table, (IBaseModel) this) as Builder;
        }

        /// <summary>
        /// Delete row(s) from database
        //  </summary>
        /// <returns></returns>
        public bool Delete()
        {
            // only create the where statement if there are no where statements yet
            if (Wheres[0].Count < 1)
                BuildDeleteWheres();

            // replace the 'select' part of the query string to 'delete from'
            ReplaceBinding(new List<string>() {Table}, SelectIndex, "delete from");

            // remove the 'from' part of the query
            ReplaceBinding(new List<string>(), FromIndex, "");

            // build the query
            var query = BuildQuery(@select: false);

            new Execute( MysqlConnectionString.GetConnectionString()).RunQuery(query,  ValueBindings);

            return true;
        }

        /// <summary>
        /// Create the where statement to delete a model
        /// </summary>
        private void BuildDeleteWheres()
        {
            // the where statement is being build with the primary keys of the model
            var primaryKeys = Fields.PrimaryKeyProperties(this as IBaseModel);
            foreach (var prop in primaryKeys)
            {
                Where(prop.Name, prop.GetValue(this));
            }
        }

        private static string CreateColumnString(string column, string table)
        {
            // if column already has alias (users.id), do not add table
            // .Where("UserId", 4).Where("notifications.Send", 1)
            if (!column.Contains('.')) 
                return table + "." + AddBackTicks(column);
            
            var split = column.Split('.').ToList();
            for (var i = 0; i < split.Count; i++)
            {
                split[i] = AddBackTicks(split[i]);
            }

            return string.Join(".", split);
        }

        /// <summary>
        /// Adds ` ` to a column or table to make sure the query is valid
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string AddBackTicks(string value)
        {
            return BackTicks.AddBackTicks(value);
        }

        /// <summary>
        /// Empty the join columns (tableA.*. ':', tableB.*)
        /// </summary>
        protected void NoRelations()
        {
            Distinct();
            JoinColumns.Clear();
        }
    }
}