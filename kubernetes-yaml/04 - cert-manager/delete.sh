#!/bin/bash
set -e

kubectl delete -f 02-issuers.yaml
sleep 5
kubectl delete -f 01-cert-manager.yaml
