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

            [JsonProperty("children")]
            public List<Folder> Children { get; set; }
        }

        public class Account
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

            [JsonProperty("path")]
            public string Path { get; set; }
        }

        public class RootObject
        {
            [JsonProperty("folders")]
            public List<Folder> Folders { get; set; }

            [JsonProperty("accounts")]
            public List<Account> Accounts { get; set; }
        }
    }
}