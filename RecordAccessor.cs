using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SQLServerObjectMaker
{
    internal abstract class RecordAccessor
    {
        private string ConnectionString => Table.ConnectionString();
        public Log Log { get; protected set; } = new();
        public Tables Table { get; private set; }


        public RecordAccessor(Tables tbl) => Table = tbl;


        /// <summary>
        /// Try selecting record(s)
        /// </summary>
        /// <param name="statement">A command-readied statement</param>
        /// <returns>Matching records</returns>
        protected virtual DataTable ExecuteRead(string statement)
        {
            //  Unmanaged resource are closed/disposed as to avoid unclosed connections over the network

            DataTable results = new();

            Log.AppendLine($"Establishing a connection [ConnectionString provided?={ConnectionString != null}]");
            SqlConnection connection = new(ConnectionString);
            try
            {
                Log.AppendLine($"Opening...");
                connection.Open();
                SqlCommand command = new(statement, connection);
                try
                {
                    Log.AppendLine($"Executing command reader:\t{statement}");
                    using SqlDataReader dataReader = command.ExecuteReader();
                    results.Load(dataReader);
                    Log.AppendLine($"Number of rows retrieved:\t{results.Rows.Count}");
                }
                catch (Exception exCmnd)
                {
                    // InvalidCastException - incorrect dbType type was used when setting value
                    // SqlException - executing agains a locked row -or- timeout during a streaming operation
                    // InvalidOperationException - current connection is closed -or- connection closed/dropped during streaming operation
                    // IOException - error in Stream, XmlReader or TextReader object during streaming operation
                    // ObjectDisposedException - Stream, XmlReader or TextReader object was closed during streaming operation

                    Log.AppendLine($"{exCmnd.Message}");
                }
                finally
                {
                    command.Dispose();
                    Log.AppendLine($"Command disposed");
                }
            }
            catch (Exception exConn)
            {
                // InvalidOperationException - data source or server not specified -or- connection already open
                // SqlException - connection-level error while opening. (If Number property has 18487 or 18488 then PW expired)
                // ConfigurationErrorsException - Multiple entries with the same name in the <localdbinstances> section

                Log.AppendLine($"{exConn.Message}");
            }
            finally
            {
                _ = connection.CloseAsync(); // aka Dispose
                Log.AppendLine($"Connection closed");
            }

            return results;
        }
        /// <summary>
        /// Try affecting record(s) using either Insert, Delete or Update
        /// </summary>
        /// <param name="statement">A command-readied statement</param>
        /// <returns>Number of records affected</returns>
        protected virtual int ExecuteWrite(string statement)
        {
            //  Unmanaged resource are closed/disposed as to avoid unclosed connections over the network
            
            int affectedRows = 0;

            Log.AppendLine($"Establishing a connection [ConnectionString provided?={ConnectionString != null}]");
            SqlConnection connection = new(ConnectionString);
            try
            {
                Log.AppendLine($"Opening...");
                connection.Open();
                SqlCommand command = new(statement, connection);
                try
                {
                    Log.AppendLine($"Executing command:\t{statement}");
                    affectedRows = command.ExecuteNonQuery();
                    Log.AppendLine($"Number of rows affected:\t{affectedRows}");
                }
                catch (Exception exCmnd)
                {
                    // InvalidCastException - setting a dbType not recognized by the column
                    // SqlException - executing command on a locked row -or- timeout during a streaming operation
                    // IOException - error from a Stream, XmlReader or TextReader object during a streaming operation
                    // InvalidOperationException - connection closed or dropped during a streaming operation
                    // ObjectDisposedException - Stream, XmlReader or TextReader object closed during a streaming operation

                    Log.AppendLine($"{exCmnd.Message}");
                }
                finally
                {
                    command.Dispose();
                    Log.AppendLine($"Command disposed");
                }
            }
            catch (Exception exConn)
            {
                // InvalidOperationException - data source or server not specified -or- connection already open
                // SqlException - connection-level error while opening. (If Number property has 18487 or 18488 then PW expired)
                // ConfigurationErrorsException - Multiple entries with the same name in the <localdbinstances> section

                Log.AppendLine($"{exConn.Message}");
            }
            finally
            {
                _ = connection.CloseAsync(); // aka Dispose
                Log.AppendLine($"Connection closed");
            }

            return affectedRows;
        }
    }

    #region Statement creators
    /*
     *  Bellow are the Statement creators that provide standard SQL command statements
     *  used by RecordAccessor Execute methods.
     *  
     */

    /// <summary>
    /// Object focused on retrieving records
    /// </summary>
    internal sealed class Selector : RecordAccessor
    {
        public Selector(Tables table) : base(table) { }


        /// <summary>
        /// Select a single record by primary key
        /// </summary>
        /// <param name="pkValue">Desired pk value of record</param>
        /// <returns>The datarow with pk value; otherwise null</returns>
        public DataRow Select(string pkValue)
        {
            List<Criteria> crit = new()
            {
                new(Table.PrimaryKey(), SQLRelation.Equal, pkValue)
            };

            string statement = BuildStatement(crit, null);
            DataTable result = ExecuteRead(statement);

            return result.Rows.Count == 1 ? result.Rows[0] : null;
        }
        /// <summary>
        /// Get records matching specified conditions
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="mods"></param>
        /// <returns></returns>
        public DataTable Select(List<Criteria> criteria, ModifierList mods)
        {
            string statement = BuildStatement(criteria, mods);
            DataTable results = ExecuteRead(statement);
            return results;
        }

        /// <summary>
        /// Transforms criteria and mods into complete SELECT statement
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="mods"></param>
        /// <returns></returns>
        private string BuildStatement(List<Criteria> criteria, ModifierList mods)
        {
            // SELECT {columns} {applied mods} FROM {table} {applied mods} WHERE {criteria}

            if (mods == null) mods = new();

            // Build WHERE portion of statement
            if (criteria == null || criteria.Count == 0) return string.Empty;

            string WHRStr = "WHERE ";
            int index;
            criteria.ForEach(c =>
            {
                index = criteria.IndexOf(c);

                // Same, non-date column indicates OR: "IN (val1, ..., valN)"
                if (index < criteria.Count - 1 && c.ColumnName.Equals(criteria[index + 1].ColumnName) && c.Relation != SQLRelation.BetweenFront)
                {
                    if (index == 0 || (index > 0 && !criteria[index - 1].ColumnName.Equals(c.ColumnName))) WHRStr += $"{c.SQLColumnName} IN ({c.SQLValue}, ";
                    else if (index > 0) WHRStr += $"{c.SQLValue}, ";
                }
                else if (index > 0 && c.ColumnName.Equals(criteria[index - 1].ColumnName) && c.Relation != SQLRelation.BetweenBack) WHRStr += $"{c.SQLValue}) AND ";
                else WHRStr += $"{c} AND ";
            });


            // Present final string
            return $"SELECT * {mods.FrontString()} FROM {Table} {WHRStr.TrimEnd(" AND ")} {mods.BackString()}";
        }

        protected override int ExecuteWrite(string statement) => throw new NotImplementedException();
    }

    /// <summary>
    /// Object focused on updating recorders
    /// </summary>
    internal sealed class Updater : RecordAccessor
    {
        public Updater(Tables table) : base(table) { }


        /// <summary>
        /// Update a single record by primary key
        /// </summary>
        /// <param name="pkValue">Desired record to update</param>
        /// <param name="values">Values to set as Setter objects</param>
        /// <returns>Number of records affected</returns>
        public int Update(string pkValue, List<Setter> values)
        {
            List<Criteria> crit = new()
            {
                new(Table.PrimaryKey(), SQLRelation.Equal, pkValue)
            };

            string statement = BuildStatement(values, crit);
            int result = ExecuteWrite(statement);
            return result;
        }

        /// <summary>
        /// Transforms setters and criteria into a complete UPDATE statement
        /// </summary>
        /// <param name="values"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private string BuildStatement(List<Setter> values, List<Criteria> criteria)
        {
            // Build SET portion of statement
            string SETStr = "SET ";
            values.ForEach(s => SETStr += $"{s}, ");

            // Build WHERE portion of statement
            if (criteria == null || criteria.Count == 0) return string.Empty;

            string WHRStr = "WHERE ";
            int index;
            criteria.ForEach(c =>
            {
                index = criteria.IndexOf(c);

                // Same, non-date column indicates OR: "IN (val1, ..., valN)"
                if (index < criteria.Count - 1 && c.ColumnName.Equals(criteria[index + 1].ColumnName) && c.Relation != SQLRelation.BetweenFront)
                {
                    if (index == 0 || (index > 0 && !criteria[index - 1].ColumnName.Equals(c.ColumnName))) WHRStr += $"{c.SQLColumnName} IN ({c.SQLValue}, ";
                    else if (index > 0) WHRStr += $"{c.SQLValue}, ";
                }
                else if (index > 0 && c.ColumnName.Equals(criteria[index - 1].ColumnName) && c.Relation != SQLRelation.BetweenBack) WHRStr += $"{c.SQLValue}) AND ";
                else WHRStr += $"{c} AND ";
            });


            // Present final string
            return $"UPDATE {Table} {SETStr.TrimEnd(", ")} {WHRStr.TrimEnd(" AND ")}";
        }

        protected override DataTable ExecuteRead(string statement) => throw new NotImplementedException();
    }

    /// <summary>
    /// Object focused on inserting recorders
    /// </summary>
    internal sealed class Inserter : RecordAccessor
    {
        public Inserter(Tables table) : base(table) { }


        /// <summary>
        /// Insert a single record
        /// </summary>
        /// <param name="values">desired values to set (will not adjust for non-nullable)</param>
        /// <returns>Number of records affected</returns>
        public int Insert(List<Setter> values)
        {
            string statement = BuildStatement(values);
            int result = ExecuteWrite(statement);
            return result;
        }

        /// <summary>
        /// Transforms values into a complete INSERT statement
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private string BuildStatement(List<Setter> values)
        {
            // Build column listor as column1, column2, ...
            // Build values to be set as Val1, Val2, ...
            string columnsStr = "(";
            string valuesStr = "VALUES (";
            values.ForEach(i =>
            {
                columnsStr += $"{i.ColumnName}, ";
                valuesStr += $"{i.SQLValue}, ";
            });

            return $"INSERT INTO {Table} {columnsStr.TrimEnd(", ")}) {valuesStr.TrimEnd(", ")})";
        }

        protected override DataTable ExecuteRead(string statement) => throw new NotImplementedException();
    }

    /// <summary>
    /// Object focused on deleting recorders
    /// </summary>
    internal sealed class Deleter : RecordAccessor
    {
        public Deleter(Tables table) : base(table) { }


        /// <summary>
        /// Delete a single record by primary key
        /// </summary>
        /// <param name="pkValue">Desired pk value to delete</param>
        /// <returns>Number of records affected</returns>
        public int Delete(string pkValue)
        {
            List<Criteria> crit = new()
            {
                new(Table.PrimaryKey(), SQLRelation.Equal, pkValue)
            };

            string statement = BuildStatement(crit);
            int result = ExecuteWrite(statement);
            return result;
        }

        /// <summary>
        /// Transform criteria into complete statement
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private string BuildStatement(List<Criteria> criteria)
        {
            // Build WHERE portion of statement
            if (criteria == null || criteria.Count == 0) return string.Empty;

            string WHRStr = "WHERE ";
            int index;
            criteria.ForEach(c =>
            {
                index = criteria.IndexOf(c);

                // Same, non-date column indicates OR: "IN (val1, ..., valN)"
                if (index < criteria.Count - 1 && c.ColumnName.Equals(criteria[index + 1].ColumnName) && c.Relation != SQLRelation.BetweenFront)
                {
                    if (index == 0 || (index > 0 && !criteria[index - 1].ColumnName.Equals(c.ColumnName))) WHRStr += $"{c.SQLColumnName} IN ({c.SQLValue}, ";
                    else if (index > 0) WHRStr += $"{c.SQLValue}, ";
                }
                else if (index > 0 && c.ColumnName.Equals(criteria[index - 1].ColumnName) && c.Relation != SQLRelation.BetweenBack) WHRStr += $"{c.SQLValue}) AND ";
                else WHRStr += $"{c} AND ";
            });


            // Present final statement
            return $"DELETE FROM {Table} WHERE {WHRStr.TrimEnd(" AND ")}";
        }

        protected override DataTable ExecuteRead(string statement) => throw new NotImplementedException();
    }
    #endregion 

    #region Elements
    /*  
     *  Following are objects used to couple information necessary to execute
     *    statements in an SQLCommand. As example, consider SELECT [Clmn1, Clmn2, ...]
     *    section of a SELECT statements. The object 'Picker', below, holds column names only. 
     *    Whereas the 'Criteria' object matches WHERE statement's [Clmn1 = Val1 AND Clmn2 < Val2 ...]
     *    parts by coupling column names, relations and values.
     *    
     *    The derived classes of RecordAccessor will make use
     *    of lists of these objects to build full statement strings used by SQLCommands.
     *    
     *    To maintain integrity, developement of this dll discourages users to utilize these objects directly,
     *    but instead, directing control of IDataRepresentors which can call methods through derived RecordAccessors
     */

    /// <summary>
    /// Modular modification statement in a SQL statement
    /// </summary>
    internal struct Modifier
    {
        public SQLModifier Type;
        public string Value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="val">Enter the value without quotations</param>
        public Modifier(SQLModifier keyword, string val)
        {
            Type = keyword;
            Value = val.ToUpper();
        }

        /// <summary>
        /// Output the string as should appear in a query statement
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Type switch
            {
                SQLModifier.OrderAsc => $"{Value} ASC",
                SQLModifier.OrderDesc => $"{Value} DESC",
                SQLModifier.Distinct => $"DISTINCT({Value}) ",
                SQLModifier.Max => $"MAX({Value}) ",
                SQLModifier.Min => $"MIN({Value}) ",
                SQLModifier.Top => $"TOP {Value} ",
                _ => string.Empty
            };
        }
    }
    internal class ModifierList : List<Modifier>
    {
        /// <summary>
        /// Filter excess, or query-breaking, items from being added
        /// </summary>
        /// <param name="modifier"></param>
        public new void Add(Modifier modifier)
        {
            switch (modifier.Type)
            {
                case SQLModifier.OrderAsc:
                case SQLModifier.OrderDesc:
                case SQLModifier.Distinct:
                    base.Add(modifier);
                    break;
                case SQLModifier.Max:
                    if (!Exists(m => m.Type == SQLModifier.Max)) base.Add(modifier);
                    break;
                case SQLModifier.Min:
                    if (!Exists(m => m.Type == SQLModifier.Min)) base.Add(modifier);
                    break;
                case SQLModifier.Top:
                    if (!Exists(m => m.Type == SQLModifier.Top)) base.Add(modifier);
                    break;
            }
        }

        /// <summary>
        /// Prepare statement from all items appearing before FROM
        /// </summary>
        /// <returns>A string to append to SELECT query section</returns>
        public string FrontString()
        {
            string str = string.Empty;
            FindAll(m => m.Type == SQLModifier.Top).ForEach(m => str += m.ToString());
            FindAll(m => m.Type == SQLModifier.Distinct).ForEach(m => str += m.ToString());
            return str.TrimEnd();
        }

        /// <summary>
        /// Prepare statement from all items appearing after WHERE
        /// </summary>
        /// <returns>A string to append to WHERE query section</returns>
        public string BackString()
        {
            string str = string.Empty;
            FindAll(m => m.Type == SQLModifier.Min).ForEach(m => str += m.ToString());
            FindAll(m => m.Type == SQLModifier.Max).ForEach(m => str += m.ToString());
            List<SQLModifier> orderings = new() { SQLModifier.OrderAsc, SQLModifier.OrderDesc };
            FindAll(m => orderings.Contains(m.Type)).ForEach(m => str += (str.Contains(" BY ") ? $", " : "ORDER BY ") + m.ToString());
            return str.TrimEnd();
        }
    }

    /// <summary>
    /// Single coupling information of an UPDATE/INSERT column to value
    /// </summary>
    internal class Setter
    {
        public readonly string ColumnName;
        private string setValue;
        public string SetValue
        {
            get => setValue;
            set
            {
                setValue = value;
                SQLValue = $"'{value}'";
            }
        }
        internal readonly string SQLColumnName;
        internal string SQLValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName">May enter column name with or without brackets</param>
        /// <param name="value">Enter exact string to be stored (\' will be applied)</param>
        public Setter(string columnName, string value)
        {
            ColumnName = columnName.Trim('[', ']');
            SQLColumnName = $"[{ColumnName}]";
            setValue = $"{value}";
            SQLValue = $"'{value}'";
        }

        /// <summary>
        /// Output a single set statment: [ColumnName]='SetValue'
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{SQLColumnName}={SQLValue}";
    }

    /// <summary>
    /// Single coupling information of a WHERE column relation to value
    /// </summary>
    internal class Criteria
    {
        /*  Supplying multiple Criteria object with the same ColumnName to the same List<Criteria>
         *    will result in building a WHERE statement section with use of 'OR', but only when
         *    placing these items back to back*    
         */

        public readonly string ColumnName;
        public readonly SQLRelation Relation;
        private string conditionValue;
        public string ConditionValue
        {
            get => conditionValue;
            set
            {
                conditionValue = value;
                SQLValue = $"'{value}'";
            }
        }
        internal readonly string SQLColumnName;
        internal string SQLValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName">May enter column name with or without brackets</param>
        /// <param name="relation"></param>
        /// <param name="value">Enter exact string to be searched (\' will be applied)</param>
        public Criteria(string columnName, SQLRelation relation, string value)
        {
            ColumnName = columnName.Trim('[', ']');
            SQLColumnName = $"[{ColumnName}]";
            Relation = relation;
            conditionValue = value;
            SQLValue = relation switch
            {
                SQLRelation.Like => $"'%{value}%'",
                SQLRelation.NLike => $"'%{value}%'",
                _ => $"'{value}'"
            };
        }

        /// <summary>
        /// Output a sinlge WHERE condition formed from these fields
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Relation == SQLRelation.Any) return $"({SQLColumnName} IS NOT NULL AND {SQLColumnName}<>'')";
            else if (Relation == SQLRelation.All) return string.Empty;
            else
            {
                return Relation switch
                {
                    SQLRelation.Null => $"{SQLColumnName} IS NULL",
                    SQLRelation.NNull => $"{SQLColumnName} IS NOT NULL",
                    SQLRelation.BetweenFront => $"{SQLColumnName} BETWEEN (CAST({SQLValue} AS DATE))",
                    SQLRelation.BetweenBack => $"(CAST({SQLValue} AS DATE))",
                    _ => $"{SQLColumnName}{Relation.GetOperator()}{SQLValue}"
                };
            }
        }
    }
    #endregion
}
