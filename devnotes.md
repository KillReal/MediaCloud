# Publishing

```bash
dotnet publish -r linux-x64 -p:PublishSingleFile=true --no-self-contained -o D:/Development/MediaCloud-Deploy
```


TODO:
- Admin check for display actors in Statistic
- Implement all actors statistic dashboards for Admin
- Write unit tests 
- Show first uploaded image on upload page
- Fix statistic recalculation when only 1 day recorded
- Fix previews ordering in gallery. Different order with filtering and without.
- Fix worker task execution race condition (Error while running the task, mb cause of taking the task twice)
- Implement blob streaming
- Implement text editing in uploaded files
- Fix autotagging service hung when no respond from external ai model for 2 times in a row
- Add partial work output to CompleteMessage into Task when exception raised