# Overview

MediaCloud is a home media cloud app for storing and fast accesing your images.

![gallery](/gallery.png)

### Features

- Crossplatform
- UI layouts support for mobile devices
- Uploading images with configurable quueue and workers
- Arranging images to image collection (like one level depth directory with preview)
- Rearranging of collection images by drag-&-drop (https://github.com/lukasoppermann/html5sortable)
- Gallery of uploaded images with sorting and filtering
- Image tagging with top usage counting
- Tag typing autocompletion
- AI autotagging by JoyTag model (https://github.com/fpgaminer/joytag) with backgound configurable quueue and workers
- Image search by tags
- Multiple account authorization (Cookie Bearer)
- Invite registration system (check for invite code for create account)
- Statistic dashboards (Images and tag count, database size)

### Stack

- .NET Core
- ASP.NET Core
- EF Core
- Python
- PostgreSQL (Actually may be easily replaced by any other database that EF Core supports)
- Bootstrap
- JQuery

### Used libs

- AI JoyTag (https://github.com/fpgaminer/joytag) for AI autotagging
- SixLabors.ImageSharp (https://github.com/SixLabors/ImageSharp) for image processing
- Html5Sortable (https://github.com/lukasoppermann/html5sortable) for image rearranging by drag-&-drop
- JQuery (for html5sortable lib)