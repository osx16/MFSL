using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RESTServices.Models
{
    public class UsersViewModel
    {
        public UsersViewModel()
        {
            Users = new List<string>();
        }
        public List<string> Users;
    }
}