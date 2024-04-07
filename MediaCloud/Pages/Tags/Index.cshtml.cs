using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.Builders.List;
using MediaCloud.Services;
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaCloud.WebApp.Pages;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using MediaCloud.WebApp.Services.ActorProvider;

namespace MediaCloud.Pages.Tags
{
    public class TagListModel : AuthorizedPageModel
    {
        private readonly TagRepository _tagRepository;
        private readonly IConfigProvider _configProvider;

        [BindProperty]
        public List<Tag> Tags { get; set; } = new();
        [BindProperty]
        public ListBuilder<Tag> ListBuilder { get; set; }
        [BindProperty]
        public bool IsAutoloadEnabled { get; set; } = false;

        public TagListModel(IActorProvider actorProvider, IConfigProvider configProvider, TagRepository tagRepository) 
            : base(actorProvider)
        {
            _configProvider = configProvider;
            _tagRepository = tagRepository;

            ListBuilder = new(new(), _configProvider.ActorSettings);
        }

        public async Task<IActionResult> OnGetAsync(ListRequest request)
        {
            ListBuilder = new(request, _configProvider.ActorSettings);
            Tags = await ListBuilder.BuildAsync(_tagRepository);

            return Page();
        }
    }
}
