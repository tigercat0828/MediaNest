using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaNest.Shared.Dtos;
public class AccountUpdateRequest() {
    public string Username { get; set; }
    public string Role { get; set; }
    public string Password { get; set; }    // later 
}