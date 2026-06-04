#!/usr/bin/env bash
#
# EventEase — set the admin login on the live App Service (POE Part 3)
# Use this if the app was provisioned before admin credentials were configured.
# The deployed app reads AdminCredentials:* from configuration; User Secrets
# (used locally) do NOT deploy, so the credentials are supplied as App settings.
#
#   export EE_SUFFIX=lmck0604             # the suffix you provisioned with
#   export EE_ADMIN_PASSWORD='admin123'  # optional, defaults to admin123
#   bash poe/part3/azure-set-admin.sh
#
set -euo pipefail
SUFFIX="${EE_SUFFIX:-}"
ADMIN_PASS="${EE_ADMIN_PASSWORD:-admin123}"
if [[ -z "$SUFFIX" ]]; then echo "ERROR: set EE_SUFFIX first"; exit 1; fi
RG="rg-eventease"
WEBAPP="app-eventease-$SUFFIX"

echo "Setting admin login on $WEBAPP (the app will restart ~30s)..."
az webapp config appsettings set --resource-group "$RG" --name "$WEBAPP" \
  --settings AdminCredentials__Username=admin AdminCredentials__Password="$ADMIN_PASS" \
  --output none
echo "DONE — log in with:  admin / $ADMIN_PASS"
