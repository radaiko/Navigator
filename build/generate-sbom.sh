#!/usr/bin/env bash
set -euo pipefail

# generate-sbom.sh
# Simple SBOM generation using a CycloneDX CLI/tool.
# Prefer an already-installed `cyclonedx` binary, fallback to `dotnet cyclonedx`,
# or install a local dotnet tool into build/.tools and run it.

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
OUT_DIR="$ROOT_DIR/build"
mkdir -p "$OUT_DIR"
SBOM_JSON="$OUT_DIR/sbom.json"
LOCAL_TOOLS="$OUT_DIR/.tools"

# Prefer solution file path if available
SOLUTION_FILE="$ROOT_DIR/Navigator.sln"
if [ -f "$SOLUTION_FILE" ]; then
  TARGET_PATH="$SOLUTION_FILE"
else
  TARGET_PATH="$ROOT_DIR"
fi

echo "Generating CycloneDX SBOM (JSON) at: $SBOM_JSON"

# Run native cyclonedx (binary) with the path and filename options
run_native_cyclonedx() {
  local exe="$1"
  echo "Running: $exe $TARGET_PATH -o $OUT_DIR -fn sbom.json -F json"
  if "$exe" "$TARGET_PATH" -o "$OUT_DIR" -fn sbom.json -F json; then
    echo "SBOM generated: $SBOM_JSON"
    return 0
  elseiod
    return 1
  fi
}

# Try system cyclonedx command first
if command -v cyclonedx >/dev/null 2>&1; then
  echo "Using system 'cyclonedx' command"
  run_native_cyclonedx cyclonedx || true
  exit 0
fi

# Some installations expose the dotnet tool as 'dotnet-CycloneDX' (common on mac when installed as a dotnet tool)
if command -v dotnet-CycloneDX >/dev/null 2>&1; then
  echo "Using 'dotnet-CycloneDX' executable"
  run_native_cyclonedx dotnet-CycloneDX || true
  exit 0
fi

# Try dotnet tool invocation (if installed as global tool exposed through `dotnet`)
if command -v dotnet >/dev/null 2>&1; then
  echo "Trying 'dotnet cyclonedx'"
  # dotnet tool invocation accepts path as first argument too
  if dotnet cyclonedx "$TARGET_PATH" -o "$OUT_DIR" -fn sbom.json -F json >/dev/null 2>&1; then
    echo "SBOM generated: $SBOM_JSON"
    exit 0
  fi
fi

# As a fallback, install a local dotnet tool into build/.tools
echo "Installing local CycloneDX dotnet tool into $LOCAL_TOOLS (if not already installed)"
mkdir -p "$LOCAL_TOOLS"
if [ -x "$LOCAL_TOOLS/cyclonedx" ]; then
  echo "Found local cyclonedx at $LOCAL_TOOLS/cyclonedx"
  run_native_cyclonedx "$LOCAL_TOOLS/cyclonedx" || true
   exit 0
 fi

# Try installing the CycloneDX dotnet tool (best-effort). This may require network access.
# Package ID commonly used: CycloneDX (official dotnet global tool). If your environment uses a different package id,
# install it manually and re-run this script.
if command -v dotnet >/dev/null 2>&1; then
  echo "Installing dotnet tool 'CycloneDX' to $LOCAL_TOOLS"
  if dotnet tool install --tool-path "$LOCAL_TOOLS" CycloneDX >/dev/null 2>&1; then
    echo "Installed local CycloneDX tool"
    if [ -x "$LOCAL_TOOLS/cyclonedx" ]; then
      run_native_cyclonedx "$LOCAL_TOOLS/cyclonedx" || true
      exit 0
    fi
   else
     echo "Failed to install CycloneDX dotnet tool. You can install it manually with:\n  dotnet tool install --global CycloneDX"
     echo "Or run a local install into the repo build dir:\n  dotnet tool install --tool-path build/.tools CycloneDX"
     exit 2
   fi
 fi

 echo "No CycloneDX CLI available. Please install one of the following and re-run this script:\n  - Install global tool: dotnet tool install --global CycloneDX\n  - Install local tool: dotnet tool install --tool-path build/.tools CycloneDX\  - Or install a native 'cyclonedx' CLI and put it on PATH"
 exit 3
