namespace MockupApplication
{
    internal class Account
    {
        #region json

        public const string Jsonfile = @"{
                                    ""filesystem"": {
                                        ""Common"": {},
                                        ""Folders"": {
                                            ""Personal"": {},
                                            ""Work"": {},
                                            ""School"": {
                                                ""High School"": {},
                                                ""Uni"": {}
                                            }
                                        }
                                    },
	                                ""accounts"": [
		                                {
			                                ""id"": ""234"",
			                                ""accountname"": ""Gmail"",
			                                ""username"": ""cam2772@gmail.com"",
			                                ""email"": """",
			                                ""password"": ""password"",
			                                ""url"": ""www.google.com"",
			                                ""notes"": ""This is google""
		                                },
		
		                                {
			                                ""id"": ""3451"",
			                                ""accountname"": ""Steam"",
			                                ""username"": ""Dark_Omega27"",
			                                ""email"": ""cam2772@gmail"",
			                                ""password"": ""password"",
			                                ""url"": ""store.steampowered.com"",
			                                ""notes"": """"
		                                },
		
		                                {
			                                ""id"": ""65"",
			                                ""accountname"": ""reddit"",
			                                ""username"": ""redditusername"",
			                                ""email"": ""cam2772@gmail.com"",
			                                ""password"": ""password"",
			                                ""url"": ""www.reddit.com"",
			                                ""notes"": """"
		                                },
		
		                                {
			                                ""id"": ""987"",
			                                ""accountname"": ""Uni"",
			                                ""username"": ""cms60"",
			                                ""email"": ""cms60@students.waikato.ac.nz"",
			                                ""password"": ""password"",
			                                ""url"": ""www.google.com"",
			                                ""notes"": """"
		                                }
	                                ]
                                }";

        #endregion

        public Account(int id, string accountName, string username, string email, string password, string url,
            string notes)
        {
            Id = id;
            AccountName = accountName;
            Username = username;
            Email = email;
            Password = password;
            Url = url;
            Notes = notes;
        }

        #region Getters and Setters

        public int Id { get; set; }

        public string AccountName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public string Url { get; set; }

        public string Notes { get; set; }

        #endregion
    }
}