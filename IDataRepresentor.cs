using System.Data;

namespace SQLServerObjectMaker
{
    /// <summary>
    /// An object to represent a single record from a datatable
    /// </summary>
    public interface IDataRepresentor
    {
        /// <summary>
        /// Changing this objects properties' values will cause changes to the datarecord
        /// </summary>
        public bool AllowEdits { get; set; }
        /// <summary>
        /// The record this object represents
        /// </summary>
        internal DataRow Me { get; set; }
        /// <summary>
        /// Current value of this object's primary key (return property that represents primary key)
        /// </summary>
        public string PrimaryKeyValue { get; }
        /// <summary>
        /// Provides the ability to select records
        /// </summary>
        internal Selector Selector { get; }
        /// <summary>
        /// Provides the ability to update records
        /// </summary>
        internal Updater Updater { get; }
        /// <summary>
        /// Provides the ability to insert records
        /// </summary>
        internal Inserter Inserter { get; }
        /// <summary>
        /// Provides the ability to delete records
        /// </summary>
        internal Deleter Deleter { get; }
    }
}
