#!/bin/bash
set -e

kubectl set env daemonset aws-node -n kube-system ENABLE_PREFIX_DELEGATION=true
kubectl set env ds aws-node -n kube-system WARM_PREFIX_TARGET=1