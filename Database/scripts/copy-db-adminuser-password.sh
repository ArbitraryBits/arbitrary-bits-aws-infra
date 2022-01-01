#!/bin/bash
set -e

aws secretsmanager get-secret-value --secret-id ArbitrraryBitsDatabaseAdminuserSecret --query SecretString --output text | jq -r ".password" | pbcopy