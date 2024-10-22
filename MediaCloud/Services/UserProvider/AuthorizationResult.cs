using System;

namespace MediaCloud.WebApp.Services.UserProvider;

public class AuthorizationResult : RegistrationResult
{
   public AuthorizationResult(bool isSuccess, string message) : base(isSuccess, message) 
   {

   }

   public AuthorizationResult()
   {

   }
}
