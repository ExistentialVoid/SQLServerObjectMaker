using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace SQLServerObjectMaker
{
    internal static partial class Internals
    {
        /// <summary>
        /// String representation of operator in transact-SQL
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string GetOperator(this SQLRelation val)
        {
            return val switch
            {
                SQLRelation.Equal => "=",
                SQLRelation.NEqual => "<>",
                SQLRelation.Greater => ">",
                SQLRelation.Less => "<",
                SQLRelation.GreaterOrEqual => ">=",
                SQLRelation.LessOrEqual => "<=",
                SQLRelation.Like => " LIKE ",
                SQLRelation.NLike => " NOT LIKE ",
                SQLRelation.Null => " IS NULL ",
                SQLRelation.NNull => " IS NOT NULL ",
                _ => string.Empty
            };
        }

        public static List<DataRow> RowList(this DataTable table)
        {
            List<DataRow> list = new();
            foreach (DataRow row in table.Rows) list.Add(row);
            return list;
        }

        public static List<T> ToList<T>(this T[] array)
        {
            List<T> list = new();
            foreach (T t in array) list.Add(t);
            return list;
        }

        /// <summary>
        /// Return a new string with the specific trimmed string removed at the end, else returns original string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="trimText"></param>
        /// <returns></returns>
        public static string TrimEnd(this string text, string trimText)
        {
            string newText = text;
            for (int t = trimText.Length - 1; t >= 0; t--)
            {
                if (newText.TrimEnd(trimText[t]).Equals(newText)) return text;
                else newText = newText.TrimEnd(trimText[t]);
            }
            return newText;
        }
    }

    /// <summary>
    /// Aid SELECT statements to provide additional functionality
    /// </summary>
    public enum SQLModifier
    {
        /// <summary>
        /// Will be accompanied by "ORDER BY"; uses ColumnName
        /// </summary>
        OrderAsc,
        /// <summary>
        /// Will be accompanied by "ORDER BY"; uses ColumnName
        /// </summary>
        OrderDesc,
        /// <summary>
        /// Selects records whose column value is unique; Uses ColumnName
        /// </summary>
        Distinct,
        /// <summary>
        /// Gets the record with the maximum value of a column; uses ColumnName
        /// </summary>
        Max,
        /// <summary>
        /// Gets the record with the minumum value of a column; uses ColumnName
        /// </summary>
        Min,
        /// <summary>
        /// Get only TOP amount specified; uses Int32
        /// </summary>
        Top,
    }

    /// <summary>
    /// SQL relational operator
    /// </summary>
    public enum SQLRelation
    {
        [Description("=")] Equal,
        [Description("<>")] NEqual,
        [Description(">")] Greater,
        [Description("<")] Less,
        [Description(">=")] GreaterOrEqual,
        [Description("<=")] LessOrEqual,
        /// <summary>
        /// Inserts '%' at the beginning and end of value
        /// </summary>
        [Description("LIKE")] Like,
        /// <summary>
        /// Inserts '%' at the beginning and end of value
        /// </summary>
        [Description("NOT LIKE")] NLike,
        [Description("IS NULL")] Null,
        [Description("IS NOT NULL")] NNull,
        /// <summary>
        /// Signal the first value of BETWEEN statment
        /// </summary>
        [Description("BETWEEN front AND back")] BetweenFront,
        /// <summary>
        /// Signal the second value of the BETWEEN statement
        /// </summary>
        [Description("BETWEEN front AND back")] BetweenBack,
        /// <summary>
        /// No constraint
        /// </summary>
        [Description("")] All,
        /// <summary>
        /// Anything with content
        /// </summary>
        [Description("IS NOT NULL AND <> ''")] Any
    }
}
