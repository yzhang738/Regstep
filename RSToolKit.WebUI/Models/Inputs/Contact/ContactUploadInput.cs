using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RSToolKit.Domain.Data;

namespace RSToolKit.WebUI.Models.Inputs.Contact
{
    /// <summary>
    /// Holds information about a contact list upload.
    /// </summary>
    public class ContactUploadListInput
    {
        /// <summary>
        /// The contact headers being uploaded.
        /// </summary>
        public List<ContactUploadHeaderInput> Headers { get; set; }
        /// <summary>
        /// The contacts being uploaded.
        /// </summary>
        public List<ContactUploadInput> Contacts { get; set; }
        /// <summary>
        /// The path the file is saved at.
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// The company key where the cantacts are going.
        /// </summary>
        public Guid CompanyKey { get; set; }
        /// <summary>
        /// The id of the saved list if being used.
        /// </summary>
        public Guid? SavedListKey { get; set; }
        /// <summary>
        /// The progress information for the upload.
        /// </summary>
        public ProgressInfo Progress { get; set; }
        /// <summary>
        /// Set to true if needs rectified.
        /// </summary>
        public bool NeedsRectified { get; set; }
        /// <summary>
        /// Needs a sheet to be selected.
        /// </summary>
        public bool NeedsSheetSelection { get; set; }
        /// <summary>
        /// Failure occured and the sheet cannot be recovered.
        /// </summary>
        public bool CriticalFailure { get; set; }
        /// <summary>
        /// A flag for setting the upload as processed and complete.
        /// </summary>
        public bool JobDone { get; set; }
        /// <summary>
        /// The sheet being used.
        /// </summary>
        public int SheetSelection { get; set; }
        /// <summary>
        /// The available sheets.
        /// </summary>
        public Dictionary<int, string> Sheets { get; set; }
        /// <summary>
        /// The unique token for this upload.
        /// </summary>
        public Guid Token { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public ContactUploadListInput()
        {
            Headers = new List<ContactUploadHeaderInput>();
            Contacts = new List<ContactUploadInput>();
            Progress = new ProgressInfo();
            FilePath = null;
            SavedListKey = null;
            CriticalFailure = false;
            SheetSelection = -1;
            Sheets = new Dictionary<int, string>();
            JobDone = false;
        }
    }

    /// <summary>
    /// Holds information about the contact being uploaded.
    /// </summary>
    public class ContactUploadInput
    {
        /// <summary>
        /// The unique email of the contact.
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// The contact key. Null if the contact email does not exists in the database.
        /// </summary>
        public Guid? ContactKey { get; set; }
        /// <summary>
        /// Flag for overiding data.
        /// </summary>
        public bool Override { get; set; }
        /// <summary>
        /// The data of the uploading contact.
        /// </summary>
        public List<ContactDataUploadInput> Data { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public ContactUploadInput()
        {
            Email = null;
            Data = new List<ContactDataUploadInput>();
            ContactKey = null;
            Override = false;
        }
    }

    /// <summary>
    /// Holds information about the uploaded header.
    /// </summary>
    public class ContactUploadHeaderInput
    {
        /// <summary>
        /// The key of the header in the database or null if it does not exist yet.
        /// </summary>
        public Guid? HeaderKey { get; set; }
        /// <summary>
        /// The name of the header.
        /// </summary>
        public string HeaderName { get; set; }
        /// <summary>
        /// The row index in the excel sheet.
        /// </summary>
        public int CellIndex { get; set; }
        /// <summary>
        /// Flag for setting the header as the email column in the actual contact column.
        /// </summary>
        public bool Email { get; set; }
        /// <summary>
        /// Initializes the clas.
        /// </summary>
        public ContactUploadHeaderInput()
        {
            HeaderKey = null;
            HeaderName = null;
            CellIndex = 1;
            Email = false;
        }

        /// <summary>
        /// Checks to see if the two objects are equal.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ContactUploadHeaderInput)
            {
                var header = obj as ContactUploadHeaderInput;
                if (HeaderKey.HasValue)
                    return header.HeaderKey == HeaderKey;
                return HeaderName.Equals(header.HeaderName);
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// Gets the hashcode.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    /// <summary>
    /// Holds information about the contact data for upload.
    /// </summary>
    public class ContactDataUploadInput
    {
        /// <summary>
        /// The uploaded header.
        /// </summary>
        public ContactUploadHeaderInput Header { get; set; }
        /// <summary>
        /// The uploaded contact.
        /// </summary>
        public ContactUploadInput Contact { get; set; }
        /// <summary>
        /// The value being uploaded.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public ContactDataUploadInput()
        {
            Value = null;
        }
    }

    /// <summary>
    /// Holds information about how to rectify a contact list upload.
    /// </summary>
    public class RectifyContactUploadListInput
    {
        /// <summary>
        /// The token of the upload.
        /// </summary>
        public Guid Token { get; set; }
        /// <summary>
        /// The problem headers.
        /// </summary>
        public List<ContactUploadHeaderInput> Headers { get; set; }
        /// <summary>
        /// Flag for overwriting existing data.
        /// </summary>
        public bool Overwrite { get; set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        public RectifyContactUploadListInput()
        {
            Headers = new List<ContactUploadHeaderInput>();
            Overwrite = false;
        }
    }

}