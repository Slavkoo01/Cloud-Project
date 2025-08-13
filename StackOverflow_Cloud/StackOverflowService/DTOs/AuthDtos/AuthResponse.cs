using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StackOverflowService.DTOs.AuthDtos
{
    public class AuthResponse 
    { 
        public string UserId; 
        public string FullName; 
        public string Email; 
        public string Token; 
    }
}