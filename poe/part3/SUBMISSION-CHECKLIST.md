# EventEase — Part 3 Submission Checklist

Work top to bottom. Don't run teardown until the **Capture proof** section is fully done.

## A. Code & local verification
- [x] EventType lookup table + 8 predefined categories seeded
- [x] Events search filters: text, venue, **event type**, **date range**, **venue availability**
- [x] EventType dropdown on Create/Edit
- [x] EF migration `AddEventType` generated
- [x] Blob service renamed to `AzureBlobStorageService`
- [ ] Ran locally once (`dotnet run`) and confirmed filters + image upload work *(optional — already proven live)*
- [x] Solution builds clean (`dotnet build`, 0 errors)

## B. Azure "Go Live" (done)
- [x] `az login` + active **Azure for Students** subscription
- [x] Resource providers registered (Microsoft.Sql / Storage / Web)
- [x] Provisioned via `azure-provision.sh` (SQL DB, Storage, App Service, in `rg-eventease`)
- [x] Connection strings + admin credentials set as **App Service settings** (not in source)
- [x] Deployed via `azure-deploy.sh`
- [x] Live app loads, login works (`admin` / `admin123`), data + image upload work on Azure

## C. Capture proof — BEFORE teardown  ⚠️
- [ ] **Record the YouTube video** following `VIDEO-SCRIPT.md` (record while live)
- [ ] **Upload to YouTube**, set to Public or Unlisted, copy the link
- [ ] Screenshots saved into `poe/part3/` (so they reach GitHub):
  - [ ] Live site login page (URL bar showing `azurewebsites.net`)
  - [ ] Events page with EventType badges + a filter applied
  - [ ] A venue showing an uploaded image (blob URL visible)
  - [ ] Bookings page
  - [ ] Azure Portal: `rg-eventease` resource group with all resources
  - [ ] App Service configuration showing the live connection strings/app settings
  - [ ] Storage container `venue-images` with the uploaded blob
  - [ ] (optional) SQL query editor showing the tables/seed data
  - [ ] CLI proof: output of `az resource list -g rg-eventease -o table`

## D. Tear down + proof
- [ ] Run `bash poe/part3/azure-teardown.sh`
- [ ] Run `az group exists -n rg-eventease` → confirm `false`
- [ ] Screenshot the proof (the `false` output and/or Portal showing the group is gone),
      saved into `poe/part3/`

## E. Report finalisation (`REPORT.md`)
- [ ] Rewrite the **reflection** (Section 4) in your own voice / from your experience
- [x] Live web app URL recorded
- [ ] Fill in **GitHub repository URL** (after pushing)
- [ ] Fill in **YouTube URL**
- [ ] Read the **theory** answers and adjust wording so they sound like you
- [ ] Confirm the **References** list and code-attribution note are complete and consistent
- [ ] Export REPORT.md to the format your submission requires (e.g. PDF/Word) if needed

## F. Push to GitHub
- [ ] Merge `part3-advanced-filtering` into your main branch (or however you submit)
- [ ] Confirm `poe/part3/` (report, scripts, screenshots) is tracked and pushed
- [ ] Confirm **no secrets** are committed (passwords/keys live only in App Service)
- [ ] Copy the repo URL → paste into REPORT.md and your submission

## G. Final submission package
- [ ] Written report (with referencing — practical + theoretical, as the brief requires)
- [ ] Live web app URL (documented, even though resources were dropped)
- [ ] GitHub repository URL
- [ ] YouTube video link
- [ ] Screenshots of the published app running
- [ ] Proof that all resources were dropped

> Rubric reminders from the brief: include code attribution & traditional referencing
> (practical + theoretical), provide the web app URL and GitHub repo (missing GitHub = 5% off),
> and a detailed video — your lecturer may not be able to connect to your database, so the
> video must clearly demonstrate everything required.
