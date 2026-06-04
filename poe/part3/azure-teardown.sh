#!/usr/bin/env bash
#
# EventEase — tear down ALL Azure resources (POE Part 3 "drop all resources")
# Deletes the entire resource group, which removes the SQL server/DB, storage
# account, App Service plan and web app in one operation.
#
# Take your screenshots of the running app + the populated resource group BEFORE
# running this. Afterwards, screenshot the empty/absent resource group as proof.
#
set -euo pipefail
RG="rg-eventease"

echo "==> Deleting resource group: $RG (this removes everything inside it)"
az group delete --name "$RG" --yes --no-wait

echo "==> Deletion started. Check status with:"
echo "    az group exists --name $RG     # prints 'false' once complete"
echo "    az group list --output table   # confirm rg-eventease is gone"
