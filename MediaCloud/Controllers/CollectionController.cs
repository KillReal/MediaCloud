﻿using MediaCloud.Builders.List;
using MediaCloud.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaCloud.WebApp.Controllers
{
    [Authorize]
    public class CollectionController(CollectionRepository collectionRepository) : Controller
    {
        private readonly CollectionRepository _collectionRepository = collectionRepository;

        public List<object> PreviewsBatch(Guid id, ListRequest listRequest)
        {
            var previews = _collectionRepository.GetList(id, listRequest);

            var jsonPreviews = new List<object>();
            foreach (var preview in previews)
            {
                jsonPreviews.Add(new
                {
                    preview.Id,
                    preview.Content,
                    preview.Order,
                    preview.BlobName,
                    preview.BlobType
                });
            }

            return jsonPreviews;
        }
    }
}
