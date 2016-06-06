using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockupApplication
{
    namespace Data
    {

        public class Folder
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public List<Folder> Children { get; set; }
        }

        public class Account
        {
            public int Id { get; set; }
            public string AccountName { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Url { get; set; }
            public string Notes { get; set; }
        }

        public class RootObject
        {
            public List<Folder> Folders { get; set; }
            public List<Account> Accounts { get; set; }
        }

    }
}
