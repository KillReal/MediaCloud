using MediaCloud.Data;
using MediaCloud.Data.Models;
using MediaCloud.MediaUploader.Tasks;
using MediaCloud.Repositories;
using MediaCloud.WebApp.Services.ConfigurationProvider;
using MediaCloud.WebApp.Services.Statistic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Task = MediaCloud.MediaUploader.Tasks.Task;

namespace MediaCloud.WebApp.Services.ActorProvider
{
    public class DummyActorProvider : IActorProvider
    {
        private readonly ActorRepository _actorRepository;
        private readonly Actor _currentActor;

        public DummyActorProvider(Actor actor, ActorRepository actorRepository)
        {
            _actorRepository = actorRepository;
            _currentActor = actor;
        }

        public Actor GetCurrent() => _currentActor;
        
        public Actor? GetCurrentOrDefault() => _currentActor;

        public bool Authorize(AuthData data, HttpContext httpContext) => false;

        public RegistrationResult Register(IConfigProvider configProvider, AuthData data, string inviteCode) 
        => new(false, $"Dummy provider cannot register an actor");

        public bool SaveSettings(string jsonSettings)
        {
             _currentActor.PersonalSettings = jsonSettings;
            _actorRepository.Update(_currentActor);

            return true;
        }
    }
}
