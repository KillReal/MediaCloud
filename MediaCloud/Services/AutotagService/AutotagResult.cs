using System;
using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Services.AutotagService;

public class AutotagResult
{
   public bool IsSuccess {get; set;}
   public List<Tag> Tags {get; set;}
   public string? ErrorMessage {get; set;} = null;

   public AutotagResult(List<Tag> tags, bool isSuccess, string? errorMessage = null)
   {
      Tags = tags;
      IsSuccess = isSuccess;
      ErrorMessage = errorMessage;
   }
}
