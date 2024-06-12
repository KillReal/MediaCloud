
# Infrastructure             

- 1.6.0 mediacloud
- 1.6.0 mediacloud-dev

# Next release - 1.7.0       
# Next minor - 1.6.1   

Todo:
- Review sql requests (Accordingly to ActorProvider staff)
- Admin check for display actors in Statistic
- Implement all actors statistic dashboards for Admin
- Write unit tests
- Show first uploaded image on uploade page
- Update zabbix metrics from Uploader to TaskScheduler
- Review creator updator linking when updating entities.
- Add check is task executed or queued before adding to queue
- Remove unnecessary dependencies in python .venv on prod
- Implement in task time execution measuring

# Changelog				   

### Version 1.6.0 - 12th Jun, 2024
- Added AI Medias and Collection autotagging with JoyTag AI model
- Added new account personal settings page
- Updated SixLabour.ImageSharp lib to 3.1.4
- Optimized autoloading speed for first page
- Various layout fixes for narrow displays
- A lot of other fixes

### Version 1.5.1 - 08th Apr, 2024
- Fixed exception handling when empty password provided for login
- Removed unused config settings
- Varios fixes and improvements

### Version 1.5.0 - 07th Apr, 2024
- Statistics dashboards now showing personal statistics
- Added personal account settings
- New homepage design
- Added recalculation button to statistics with modal window of recalculation task status
- Massive code cleanup and refactor
- Improved overall page loading speed by 10-40%
- Improved tag and preview saving perfomance
- New universal gallery layout
- Added multiple actor session caching
- Added picture rotating option
- Fixed page layouts for small screens
- Fixed returnUrl problem for Collections (When go to Medias->Collection->Edit and back)
- Improved logging for services
- Minor fixes and local improvements

### Version 1.4.5 - 28th Mar, 2024
- Implemented PageModel debug logging
- Implemented DateService init debug logging
- Fixed Collection removing

### Version 1.4.4 - 04th Mar, 2024
- Minor improvements in Statistic dashboards
- Implemented auth cookie expiration
- Added CookieExpireTime and UploaderWorkersCount settings to appsettings.json

### Version 1.4.3 - 03th Mar, 2024
- Implemented NLog for application logging
- Code cleanup and removed unused extensions

### Version 1.4.2 - 02th Mar, 2024
- Improved content processing perfomance after uploading by 30-50%
- Improved Collection removing perfomance by 20-40%
- Switched preview processing alghorithm from Bicubic to Lanzcos3
- Fixed Collection upload counter in Statistic
- Code cleanup and small improvements

### Version 1.4.1 - 01th Mar, 2024
- Improved content saving perfomance after uploading by 80%
- Fixed Tag editing
- Fixed repeating content upload

### Version 1.4.0 - 13th Jan, 2024
- Switched from SQLite 3 database to PostgreSQL
- Imroved overall app perfomance by 10-40%
- Added new favicon

### Version 1.3.5 - 21th Dec, 2023
- Added collection info to /Collection
- Fixed tag previews count saving

### Version 1.3.4 - 19th Dec, 2023
- Various fixes for UI

### Version 1.3.3 - 09th Oct, 2023
- Add autoloading toggle in /Gallery

### Version 1.3.2 - 08th Oct, 2023
- Fixes and improvements in tag autocomplition

### Version 1.3.1 - 06th Oct, 2023
- Added autocompletion in tag filter input for /Gallery/Collection
- Minor UI changes in /Gallery/Collection

### Version 1.3.0 - 05th Oct, 2023
- Added autocompletion in tag filter input for /Gallery

### Version 1.2.3 - 02th Aug, 2023
- Switched graphics library to crossplatform ImageSharp
- Switched to self made file name comporator

### Version 1.2.2 - 10th Mar, 2023
- Added show similar function in /Details and /Collection

### Version 1.2.1 - 11th Mar, 2023
- Improved perfomance for statistic recalculation
- Statistic recalculation now skip 'empty' days
- Fixes and minor improvements in statistic layout for /Statistic
- Statistic service fixes and perfomance improvements

### Version 1.2.0 - 10th Mar, 2023
- Added statistic service with every day users activity tracking
- Added statistic layout for /Statistic
- Added activity factor tracking
- Small UI fixes

### Version 1.1.6 - 28th Feb, 2023
- Added go to top of layout button
- Fixed mobile layout width and height
- Fixed tags duplication

### Version 1.1.5 - 05th Feb, 2023	
- Window layout now more wide

### Version 1.1.4 - 04th Feb, 2023
- Added prev/next buttons in /Details when other collection medias exist

### Version 1.1.3 - 22th Jan, 2023
- Change onlick event to onauxclick

### Version 1.1.2 - 16th Jan, 2023
- Fixed collection reordering
- Switched sorting library to https://github.com/lukasoppermann/html5sortable (0.13.3)
- Small fixes and improvements

### Version 1.1.1 - 14th Jan, 2023
- Fixed broken links for dynamic loading of previews
- Fixed dynamic loading for /Medias/Collection through https
- Fixed tag editing for /Medias/Collection

### Version 1.1.0 - 13th Jan, 2023	
- Bring dynamic loading of previews in /Medias/Index and /Medias/Collection

### Version 1.0.3 - 12th Jan, 2023
- Fixed existing current actor when doesn't logged in
- Enabled response compression


### Version 1.0.2 - 22th Dec, 2022
- Fixed Current Actor context

### Version 1.0.1 - 15th Dec, 2022	
- small fixes and speed improvement

### Version 1.0.0 - 3th Dec, 2022
- First public release
- Added more universal tag types
- Reworked /Medias/Collection page
- Removed /Medias/CollectionReorder page due to transfer that functional to /Medias/Collection
- Security improvements

### Version 0.7.0 - 24th Nov, 2022
- Added multiple account support
- Added /Actors for admin
- Added logout function
- Added joining in function by invite code

### Version 0.6.0 - 18th Nov, 2022
- Added Authorization

### Version 0.5.0 - 14th Nov, 2022	
- Added negative filters support for Medias with '!' sign

### Version 0.4.0 - 1th Nov, 2022	
- Added new uploader with queue and status api
- Added uploading status to Medias/Upload

### Version 0.3.3 - 22th Oct, 2022
- Added related medias button to (Tags/Detail and Tags)
- Some minor perfomance improvements

### Version 0.3.2 - 21th Oct, 2022
- Increased lists loading speed (Medias and Tags) by 20-50%
- Implemented entity chaching
- Fixed some layouts issues

### Version 0.3.1 - 20th Oct, 2022
- Fixed drag-and-drop when placeholder not hides
- Fixed Medias/Collection image auto sizing and effects

### Version 0.3.0 - 20th Oct, 2022
- Implemented drag-and-drop collection reorder
- Fixed disapearing tags in collection after reordering

### Version 0.2.1 - 19th Oct, 2022
- Increased Medias/Index loading speed by 10-55%
- Some fixes and improvements in layouts

### Version 0.2.0 - 16th Oct, 2022
- Completely reworked collection storing
- Significantly improved speed of collection loading
- Added Medias/Collection view
- Added Medias/CollectionReorder page for reordering collection
- Updated Tag types

### Version 0.1.0 - 14th Oct, 2022
- Added Medias/Index "Random" ordering
- Added Medias/Collection view for Collection medias
- Reworker Medias/Detail for single media view
- Fixed Tags/Index MediasCount sorting
- Fixed Medias/Detail pages return url
- Fixed Medias/Index filter button returnUrl

### Version 0.0.2 - 8th Oct, 2022
- Fixed Medias/Edit ReturnUrls
- Fixed Medias/Index missing tags in filter queryes
- Fixed Medias/Upload unnecessary require validation for tags textarea
- Fixed Tags/Detail tags links
- Fixed overlapping Footer on pages
- Fixed mobile layout and content wrap
- Set minimal layout width to 450px

### Version 0.0.1 - 4th Oct, 2022
- Initial Dev-release
