#!/usr/bin/env bash
#
# EventEase — Azure "Go Live" provisioning script (POE Part 3)
# Creates all live Azure resources inside ONE resource group so the whole stack
# can be torn down with a single command afterwards (see azure-teardown.sh).
#
# Prerequisites:
#   1. Azure CLI installed:  brew install azure-cli
#   2. Logged in:            az login
#   3. An active subscription (Azure for Students / Free) — verify: az account show
#
# Configuration is read from environment variables so that NO secret is ever
# written into this (git-tracked) file:
#
#   export EE_SUFFIX=lmck2026            # globally-unique, lowercase letters+digits
#   export EE_SQL_PASSWORD='Str0ng#Pass' # >= 12 chars: upper+lower+digit+symbol
#   # optional overrides:
#   export EE_LOCATION=southafricanorth
#   export EE_SQL_ADMIN=eventeaseadmin
#
#   bash poe/part3/azure-provision.sh
#
set -euo pipefail

SUFFIX="${EE_SUFFIX:-}"
SQL_PASSWORD="${EE_SQL_PASSWORD:-}"
LOCATION="${EE_LOCATION:-southafricanorth}"
SQL_ADMIN="${EE_SQL_ADMIN:-eventeaseadmin}"

if [[ -z "$SUFFIX" || -z "$SQL_PASSWORD" ]]; then
  echo "ERROR: set EE_SUFFIX and EE_SQL_PASSWORD first, e.g.:" >&2
  echo "  export EE_SUFFIX=lmck2026" >&2
  echo "  export EE_SQL_PASSWORD='Str0ng#Pass2026'" >&2
  exit 1
fi

RG="rg-eventease"
SQL_SERVER="sql-eventease-$SUFFIX"
SQL_DB="EventEaseDb"
STORAGE="steventease$SUFFIX"        # storage names: lowercase, 3-24 chars, no dashes
PLAN="plan-eventease"
WEBAPP="app-eventease-$SUFFIX"

echo "==> [1/6] Resource group: $RG"
az group create --name "$RG" --location "$LOCATION" --output none

echo "==> [2/6] Azure SQL server + database"
az sql server create --name "$SQL_SERVER" --resource-group "$RG" --location "$LOCATION" \
  --admin-user "$SQL_ADMIN" --admin-password "$SQL_PASSWORD" --output none
az sql db create --resource-group "$RG" --server "$SQL_SERVER" --name "$SQL_DB" \
  --service-objective Basic --output none

echo "    - firewall: allow Azure services"
az sql server firewall-rule create --resource-group "$RG" --server "$SQL_SERVER" \
  --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0 --output none
echo "    - firewall: allow this machine"
MYIP="$(curl -s https://api.ipify.org)"
az sql server firewall-rule create --resource-group "$RG" --server "$SQL_SERVER" \
  --name MyMachine --start-ip-address "$MYIP" --end-ip-address "$MYIP" --output none

echo "==> [3/6] Storage account + blob container"
az storage account create --name "$STORAGE" --resource-group "$RG" \
  --location "$LOCATION" --sku Standard_LRS --allow-blob-public-access true --output none
STORAGE_CONN="$(az storage account show-connection-string --name "$STORAGE" \
  --resource-group "$RG" --query connectionString -o tsv)"
az storage container create --name venue-images --connection-string "$STORAGE_CONN" \
  --public-access blob --output none

echo "==> [4/6] App Service plan + web app (.NET 10, Linux)"
az appservice plan create --name "$PLAN" --resource-group "$RG" --sku B1 --is-linux --output none
az webapp create --resource-group "$RG" --plan "$PLAN" --name "$WEBAPP" \
  --runtime "DOTNETCORE:10.0" --output none

echo "==> [5/6] App settings — live connection strings (never committed to source)"
SQL_CONN="Server=tcp:$SQL_SERVER.database.windows.net,1433;Database=$SQL_DB;User ID=$SQL_ADMIN;Password=$SQL_PASSWORD;Encrypt=True;TrustServerCertificate=False;"
az webapp config connection-string set --resource-group "$RG" --name "$WEBAPP" \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="$SQL_CONN" --output none
# Storage connection string is read via GetConnectionString("AzureStorage"),
# which maps to the app-setting key ConnectionStrings__AzureStorage.
# Admin login is read from AdminCredentials:* — supplied here because User Secrets
# (used locally) do not deploy. Override the password via EE_ADMIN_PASSWORD.
ADMIN_PASS="${EE_ADMIN_PASSWORD:-admin123}"
az webapp config appsettings set --resource-group "$RG" --name "$WEBAPP" \
  --settings ConnectionStrings__AzureStorage="$STORAGE_CONN" \
             AdminCredentials__Username=admin \
             AdminCredentials__Password="$ADMIN_PASS" --output none

echo "==> [6/6] DONE"
echo "    Web app URL : https://$WEBAPP.azurewebsites.net"
echo "    SQL server  : $SQL_SERVER.database.windows.net"
echo "    Storage     : $STORAGE (container: venue-images)"
echo ""
echo "Next: publish + deploy (EE_SUFFIX must stay exported, then run azure-deploy.sh)."
