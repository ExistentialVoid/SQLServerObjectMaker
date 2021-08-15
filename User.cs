using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLServerObjectMaker
{
    /// <summary>
    /// An object to exemplify the functionality of this dll
    /// </summary>
    /// <summary>
    /// Object used to reflect a record in Personnel
    /// </summary>
    public class User : IDataRepresentor, IDisposable
    {
        #region Implementations
        public bool AllowEdits { get => allowEdits; set => allowEdits = value; }
        private bool allowEdits = false;
        DataRow IDataRepresentor.Me { get => me; set => me = value; }
        private DataRow me;
        public string PrimaryKeyValue => ID;
        Selector IDataRepresentor.Selector => selector; // throw new NotImplementedException() if unwanted
        private readonly Selector selector = new(Tables.Users);
        Updater IDataRepresentor.Updater => updater; // throw new NotImplementedException() if unwanted
        private readonly Updater updater = new(Tables.Users);
        Inserter IDataRepresentor.Inserter => inserter; // throw new NotImplementedException() if unwanted
        private readonly Inserter inserter = new(Tables.Users);
        Deleter IDataRepresentor.Deleter => deleter; // throw new NotImplementedException() if unwanted
        private readonly Deleter deleter = new(Tables.Users);
        #endregion


        #region Columns as properties
        /*  Columns listed as enum in 'Table enum' region in 'Customize Region.cs'
         * 
         *  0   (string)    ID
         *  1   (string)    NAME
         *  2   (smallint)  POSITIONTITLE
         *  3   (string)    PASSWORD
         *  4   (string)    RECOVERYQUESTION
         *  5   (string)    RECOVERYANSER
         *  7   (bit)       LOGINSTATUS
         *  8   (date)      LASTLOGINTIME
         */

        /// <summary>
        /// Unique employee ID and primary key
        /// </summary>
        public string ID
        {
            get => (string)me[(int)UserColumn.ID];
            set
            {
                if (value == null) return;

                string storedVal = value;
                
                bool hasUpdated = true;
                if (allowEdits)
                {
                    // update record if allowedits is true
                    int updatedRecords = updater.Update(ID, new() { new(UserColumn.ID.ToString(), storedVal) });
                    hasUpdated = updatedRecords > 0;
                }
                if (hasUpdated)
                {
                    // update Me
                    me[(int)UserColumn.ID] = storedVal;
                }
            }
        }
        /// <summary>
        /// Formatted name: LName_FName
        /// </summary>
        public NameItem Name
        {
            get => Convert.IsDBNull(me[(int)UserColumn.NAME]) ? new(string.Empty) :
                new((string)me[(int)UserColumn.NAME]);
            set
            {
                string storedVal = value.Stored;

                bool hasUpdated = true;
                if (allowEdits)
                {
                    // update record if allowedits is true
                    int updatedRecords = updater.Update(ID, new() { new(UserColumn.NAME.ToString(), storedVal) });
                    hasUpdated = updatedRecords > 0;
                }
                if (hasUpdated)
                {
                    // update Me
                    me[(int)UserColumn.NAME] = storedVal;
                }
            }
        }
        /// <summary>
        /// General position title
        /// </summary>
        public PositionTitle Title
        {
            get => Convert.IsDBNull(me[(int)UserColumn.POSITIONTITLE]) ? PositionTitle.None :
                Enum.Parse<PositionTitle>((string)me[(int)UserColumn.POSITIONTITLE]);
            set
            {
                if (value.StoredText().Equals("Unspecified")) return;

                string storedVal = value.StoredText();

                bool hasUpdated = true;
                if (allowEdits)
                {
                    // update record if allowedits is true
                    int updatedRecords = updater.Update(ID, new() { new(UserColumn.POSITIONTITLE.ToString(), storedVal) });
                    hasUpdated = updatedRecords > 0;
                }
                if (hasUpdated)
                {
                    // update Me
                    me[(int)UserColumn.POSITIONTITLE] = storedVal;
                }
            }
        }
        /// <summary>
        /// WorkOrder System login password
        /// </summary>
        public string Password
        {
            get => Convert.IsDBNull(me[(int)UserColumn.PASSWORD]) ? temporaryPW :
                (string)me[(int)UserColumn.PASSWORD];
            set
            {
                if (value == null) return;
                string storedVal = value;

                if (EnforcePasswordVerification && !PasswordVerifier.Verify(storedVal, out _)) return;

                bool hasUpdated = true;
                if (allowEdits)
                {
                    // update record if allowedits is true
                    int updatedRecords = updater.Update(ID, new() { new(UserColumn.PASSWORD.ToString(), storedVal) });
                    hasUpdated = updatedRecords > 0;
                }
                if (hasUpdated)
                {
                    // update Me
                    me[(int)UserColumn.PASSWORD] = storedVal;
                    needsPw = storedVal.Equals(temporaryPW);
                }
            }
        }
        /// <summary>
        /// User's chosen question for password recovery
        /// </summary>
        public string RecoveryQuestion
        {
            get => Convert.IsDBNull(me[(int)UserColumn.RECOVERYQUESTION]) ? string.Empty :
                (string)me[(int)UserColumn.RECOVERYQUESTION];
            set
            {
                if (value == null) return;

                string storedVal = value;

                bool hasUpdated = true;
                if (allowEdits)
                {
                    // update record if allowedits is true
                    int updatedRecords = updater.Update(ID, new() { new(UserColumn.RECOVERYQUESTION.ToString(), storedVal) });
                    hasUpdated = updatedRecords > 0;
                }
                if (hasUpdated)
                {
                    // update Me
                    me[(int)UserColumn.RECOVERYQUESTION] = storedVal;
                }
            }
        }
        /// <summary>
        /// User's provided answer to security question*
        /// </summary>
        public string RecoveryAnswer
        {
            get => Convert.IsDBNull(me[(int)UserColumn.RECOVERYANSER]) ? string.Empty :
                (string)me[(int)UserColumn.RECOVERYANSER];
            set
            {
                if (value == null) return;

                string storedVal = value;

                bool hasUpdated = true;
                if (allowEdits)
                {
                    // update record if allowedits is true
                    int updatedRecords = updater.Update(ID, new() { new(UserColumn.RECOVERYANSER.ToString(), storedVal) });
                    hasUpdated = updatedRecords > 0;
                }
                if (hasUpdated)
                {
                    // update Me
                    me[(int)UserColumn.RECOVERYANSER] = storedVal;
                }
            }
        }
        /// <summary>
        /// Current Workorder System working status
        /// </summary>
        public bool IsCurrentlyLoggedIn
        {
            get => !Convert.IsDBNull(me[(int)UserColumn.LOGINSTATUS]) && (bool)me[(int)UserColumn.LOGINSTATUS];
            private set
            {
                string storedVal = value.ToString();

                int updatedRecords = updater.Update(ID, new() { new(UserColumn.LOGINSTATUS.ToString(), storedVal) });
                bool hasUpdated = updatedRecords > 0;
                if (hasUpdated)
                {
                    // update Me
                    me[(int)UserColumn.LOGINSTATUS] = storedVal;
                }
            }
        }
        /// <summary>
        /// The Last login timestamp with seconds percision
        /// </summary>
        public DateTime LastLoginTime
        {
            get => Convert.IsDBNull(me[(int)UserColumn.LASTLOGINTIME]) ? new() :
                (DateTime)me[(int)UserColumn.LASTLOGINTIME];
            private set
            {
                string storedVal = value.ToString();

                int updatedRecords = updater.Update(ID, new() { new(UserColumn.LASTLOGINTIME.ToString(), storedVal) });
                bool hasUpdated = updatedRecords > 0;
                if (hasUpdated)
                {
                    // update Me
                    me[(int)UserColumn.LASTLOGINTIME] = storedVal;
                }
            }
        }
        #endregion


        public enum PositionTitle { None, Tech, Tester, Dev, SrDev, Owner, Manager, SrManager, Admin }

        /// <summary>
        /// Adhere to PasswordVerify when addressing the Password property
        /// </summary>
        public bool EnforcePasswordVerification = true;
        /// <summary>
        /// Initialized to generic password standards
        /// </summary>
        public PasswordAuthenticator PasswordVerifier = new();
        /// <summary>
        /// True if the saved answer (assigned during registration) is "unset"
        /// </summary>
        public bool NeedsToSetPassword { get => needsPw; }
        private bool needsPw;
        /// <summary>
        /// App-dependent setting
        /// </summary>
        public static string TemporaryPassword { get => temporaryPW; set => temporaryPW = value; }
        private static string temporaryPW = "password123";
        private int failedLoginAttempts = 0;
        public static readonly int LoginAttemptLimit = 4;


        internal User(DataRow userRow)
        {
            me = userRow;
            Initialize();
        }
        /// <summary>
        /// Create a user
        /// </summary>
        /// <param name="id">The employee id of the user found in Personnel primary key</param>
        /// <param name="attempt">Will still contain info about data errors</param>
        public User(string id)
        {
            try
            {
                me = selector.Select(id);
                Initialize();
            }
            catch { }
        }


        /// <summary>
        /// Initialize any feild relying on current data values 
        /// </summary>
        private void Initialize()
        {
            needsPw = Password.Equals(temporaryPW);
        }
        /// <summary>
        /// Confirm credential and perform login triggers
        /// </summary>
        /// <param name="password"></param>
        /// <returns>True if user can login without issue</returns>
        public bool Login(string password)
        {
            if (failedLoginAttempts >= LoginAttemptLimit)
            {
                // Handle logic for how to proceed when user fails to login 4 time within same app window.
                //  (using this method, if the user resets his/her app-dependent, User object then the 
                //   failed attempts will be reset)

                return false;
            }
            else if (!Password.Equals(password))
            {
                failedLoginAttempts++;
                return false;
            }

            IsCurrentlyLoggedIn = true;
            LastLoginTime = DateTime.Now;
            return true;
        }
        public void Logout()
        {
            IsCurrentlyLoggedIn = false;
            // Caution:  setting me to null could cause errors if not handled properly on client side

            Dispose();
        }

        /// <summary>
        /// Try getting a user by name information
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A user if a unique match, otherwise null</returns>
        public static User Lookup(NameItem name)
        {
            // Another useful way to lookup a user

            Selector selector = new(Tables.Users);
            List<Criteria> criteria = new()
            {
                new(UserColumn.NAME.ToString(), SQLRelation.Like, name.Stored)
            };
            DataTable results = selector.Select(criteria, null);

            if (results.Rows.Count == 1)
            {
                User user = new(results.Rows[(int)UserColumn.ID]);
                return user;
            }
            else
            {
                return null;
            }
        }

        public void Dispose() 
        {
            // Dispose of unmanaged resources when applied
        }
        /// <summary>
        /// The User's employee ID
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Name.ToString();

        /// <summary>
        /// Define a standard set of criteria for password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public class PasswordAuthenticator
        {
            public readonly string SpecialCharacterSet = "~!@#$%^&*_=+|<>?";
            public int MinimumLength = 8;
            public readonly int MaximumLength = 14;
            /// <summary>
            /// Must contain both a single capital and single lower-case letter
            /// </summary>
            public bool IncludeBothLetterCasing = true;
            /// <summary>
            /// Must contain numbers
            /// </summary>
            public bool IncludeNumeric
            {
                get => includeNum;
                set
                {
                    includeNum = value;
                    int min = (includeNum ? 1 : 0) + (includeSpecial ? 1 : 0);
                    NumberOfNonAlphaCharacters = Math.Max(min, NumberOfNonAlphaCharacters);
                }
            }
            private bool includeNum = true;
            /// <summary>
            /// Force complexity by requiring more non-alphabet characters
            /// </summary>
            public int NumberOfNonAlphaCharacters = 1;
            /// <summary>
            /// Must contain special characters
            /// </summary>
            public bool IncludeSpecialChar
            {
                get => includeSpecial;
                set
                {
                    includeSpecial = value;
                    int min = (includeNum ? 1 : 0) + (includeSpecial ? 1 : 0);
                    NumberOfNonAlphaCharacters = Math.Max(min, NumberOfNonAlphaCharacters);
                }
            }
            private bool includeSpecial = false;

            /// <summary>
            /// Test provided password against the requirements
            /// </summary>
            /// <param name="password"></param>
            /// <returns></returns>
            public bool Verify(string password, out FailureCode code)
            {
                int numCount = password.ToCharArray().ToList().Count(c => int.TryParse(c.ToString(), out _));
                int spclCount = password.ToCharArray().ToList().Count(c => SpecialCharacterSet.Contains(c));

                if (password.Length < MinimumLength || password.Length > MaximumLength)
                    code = FailureCode.Length;
                else if (includeNum && numCount == 0)
                    code = FailureCode.NumericChar;
                else if (includeNum && spclCount == 0)
                    code = FailureCode.SpecialChar;
                else if (numCount + spclCount < NumberOfNonAlphaCharacters)
                    code = FailureCode.Complexity;
                else if (password.ToLower().Equals(password) || password.ToUpper().Equals(password))
                    code = FailureCode.Casing;
                else if (password.ToLower().ToCharArray().ToList().Exists(c => $"abcdefghijklmnopqrstuvwxyz0123456789{SpecialCharacterSet}".Contains(c)))
                    code = FailureCode.InvalidChar;
                else
                    code = FailureCode.None;

                return code == FailureCode.None;
            }

            public enum FailureCode { None, Length, Complexity, Casing, NumericChar, SpecialChar, InvalidChar }
        }

        /// <summary>
        /// Name conventions
        /// </summary>
        public struct NameItem
        {
            public const char Separator = '_';

            public string AbbreviatedFirst => $"{FirstInitial}. {Last}";
            public readonly string First;
            public string FirstInitial => First[0].ToString();
            public readonly string Last;
            public string Normal => $"{First} {Last}";
            public string Stored => $"{Last}{Separator}{First}";

            public NameItem(string dataName)
            {
                string[] parts = dataName.Split('_');
                First = parts[1];
                Last = parts[0];
            }
            public NameItem(string firstName, string lastName)
            {
                First = firstName;
                Last = lastName;
            }

            public override string ToString() => Normal;
        }
    }

    public static class UserHelper
    {
        public static string StoredText(this User.PositionTitle val)
        {
            return val switch
            {
                User.PositionTitle.None => string.Empty,
                User.PositionTitle.Tech => "Technician",
                User.PositionTitle.Tester => "Tester",
                User.PositionTitle.Dev => "Developer",
                User.PositionTitle.SrDev => "Sr.Developer",
                User.PositionTitle.Owner => "ProjectOwner",
                User.PositionTitle.Manager => "Manager",
                User.PositionTitle.SrManager => "SrManager",
                User.PositionTitle.Admin => "Administrator",
                _ => "Unspecified"
            };
        }
    }
}
