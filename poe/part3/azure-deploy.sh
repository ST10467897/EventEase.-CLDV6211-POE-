#!/usr/bin/env bash
#
# EventEase — publish & deploy to Azure App Service (POE Part 3)
# Run AFTER azure-provision.sh, from the repo root (where EventEaseLocal.csproj is).
#
# Uses the same EE_SUFFIX you provisioned with:
#   export EE_SUFFIX=lmck2026      # (still set from provisioning)
#   bash poe/part3/azure-deploy.sh
#
set -euo pipefail

SUFFIX="${EE_SUFFIX:-}"
if [[ -z "$SUFFIX" ]]; then
  echo "ERROR: set EE_SUFFIX (the same value used in azure-provision.sh), e.g.:" >&2
  echo "  export EE_SUFFIX=lmck2026" >&2
  exit 1
fi
RG="rg-eventease"
WEBAPP="app-eventease-$SUFFIX"

echo "==> Publishing (Release)"
rm -rf ./publish ./app.zip
dotnet publish -c Release -o ./publish

echo "==> Zipping"
( cd publish && zip -r ../app.zip . >/dev/null )

echo "==> Deploying to $WEBAPP"
az webapp deploy --resource-group "$RG" --name "$WEBAPP" --src-path app.zip --type zip

echo "==> DONE — open: https://$WEBAPP.azurewebsites.net"
echo "    On first request the app runs db.Database.Migrate(): it creates the"
echo "    schema, the vw_BookingDetails view, and seeds (venues, events, EventTypes)."
