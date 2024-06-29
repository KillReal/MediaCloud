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
- Personal account settings
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

### Deployment

1. Build app by `dotnet` for target OS as example:
    `dotnet publish -r linux-x64 -p:PublishSingleFile=true --no-selft-contained -o C:/MediaCloud-deploy`
2. Setup hosting configuration in `appsettings.json` (Host endpoint, DB connectionString and etc...)
3. Deploy JoyTag AI model if you need it or disable in `appsettings.json`:
    a. Download JoyTag model from https://huggingface.co/fancyfeast/joytag/tree/main (config.json, model.onnx, model.safetensors and top_tags.txt)
    b. Place it to /JoyTag/modelshttps://huggingface.co/fancyfeast/joytag/tree/main
    c. Deploy JoyTag server on some port:
        `python3 joytag.py 5050 & disown`
    d. Add joytag url to `appsettings.json`
4. Start application:
    `./MediaCloud.WebApp`
5. Login in admin account (login: Admin, pass: superadmin)
6. Enjoy!

### Used libs

- AI JoyTag (https://github.com/fpgaminer/joytag) for AI autotagging
- SixLabors.ImageSharp (https://github.com/SixLabors/ImageSharp) for image processing
- Html5Sortable (https://github.com/lukasoppermann/html5sortable) for image rearranging by drag-&-drop
- JQuery (for html5sortable lib)