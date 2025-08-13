using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService.DTOs.AuthDtos
{
    public class RegisterRequest
    {
        public string FullName;
        public string Gender;
        public string Country;
        public string City;
        public string Address;
        public string Email;
        public string Password;
    }
}