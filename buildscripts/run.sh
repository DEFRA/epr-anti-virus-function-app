#! /usr/bin/env bash

cd "$(dirname "$0")/../" || exit 1

cd EPR.Antivirus/EPR.Antivirus.Function || exit 1

func start --verbose
