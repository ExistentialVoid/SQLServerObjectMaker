namespace SQLServerObjectMaker
{
    /*  Istructions to add a new RecordRepresentor object:
     *  
     *  In Tables enum below, add the name of the datatable as appears in the database.
     *   - include its PrimaryKey extension 
     *   - include its ConnectionString extension
     *   - include an enumerator of columns with names as they appear in the datatable
     *   
     *  Any alteration to a DataTable must be reflected by the following information
     */


    /// <summary>
    /// Names of the tables as appears in the database
    /// </summary>
    public enum Tables { Users }

    internal static partial class Internals
    {
        /// <summary>
        /// The proper string name of the primary key single/combo column name(s)
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static string PrimaryKey(this Tables table)
        {
            return table switch
            {
                Tables.Users => "ID",
                _ => string.Empty
            };
        }

        internal static string ConnectionString(this Tables table)
        {
            return table switch
            {
                Tables.Users => "",
                _ => string.Empty
            };
        }
    }


    #region Table column lists
    /*
     *   Objects' properties of these tables will make use of these enums to reference column row in the 'Me' property
     */

    /// <summary>
    /// Formal column names of User Datatable
    /// </summary>
    internal enum UserColumn
    {
        ID,
        NAME,
        POSITIONTITLE,
        PASSWORD,
        RECOVERYQUESTION,
        RECOVERYANSER,
        LOGINSTATUS,
        LASTLOGINTIME
    }
    #endregion
}
