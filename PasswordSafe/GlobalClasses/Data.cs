using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PasswordSafe.GlobalClasses
{
    namespace Data
    {
        public class Folder
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("expanded")]
            public bool Expanded { get; set; }

            [JsonProperty("children")]
            public List<Folder> Children { get; set; }
        }

        public class Account : ICloneable
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("accountName")]
            public string AccountName { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("password")]
            public string Password { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }

            [JsonProperty("notes")]
            public string Notes { get; set; }

            [JsonProperty("dateCreated")]
            public string DateCreated { get; set; }

            [JsonProperty("dateLastEdited")]
            public string DateLastEdited { get; set; }

            [JsonProperty("backup")]
            public bool Backup { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            /// <summary>
            ///     Creates a clone of the object
            /// </summary>
            /// <returns>Returns a clone of the object</returns>
            public object Clone()
            {
                return MemberwiseClone();
            }

            /// <summary>
            ///     Checks if this is equal to another object
            /// </summary>
            /// <param name="account">The account to check if it is equal to</param>
            /// <returns>Returns true if this is equal to the other account</returns>
            public bool Equals(Account account)
            {
                return (Id == account.Id) && (AccountName == account.AccountName) && (Username == account.Username) &&
                       (Email == account.Email) && (Password == account.Password) && (Url == account.Url) &&
                       (Notes == account.Notes) && (DateCreated == account.DateCreated) &&
                       (DateLastEdited == account.DateLastEdited) && (Backup == account.Backup) &&
                       (Path == account.Path);
            }
        }

        public class RootObject
        {
            [JsonProperty("versionNumber")]
            // ReSharper disable once UnusedAutoPropertyAccessor.Global could be used one day if the data structure changes
            public string VersionNumber { get; set; }

            [JsonProperty("folders")]
            public List<Folder> Folders { get; set; }

            [JsonProperty("accounts")]
            public List<Account> Accounts { get; set; }
        }
    }
}