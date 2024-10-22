using System;
using MediaCloud.Data.Models;

namespace MediaCloud.WebApp.Services.AutotagService;

public class AutotagResult
{
   public bool IsSuccess {get; set;}
   public List<Tag> Tags {get; set;}

   public AutotagResult(List<Tag> tags, bool isSuccess)
   {
      Tags = tags;
      IsSuccess = isSuccess;
   }
}
