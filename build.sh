#!/bin/bash
# ALX Build Script — Linux/Mac
# Usage: ./build.sh

set -e

echo "Building ALX..."

# Build the solution
dotnet build ALX.sln -c Release

# Run all tests
echo ""
echo "Running tests..."
dotnet test ALX.sln --verbosity minimal

# Create output directory
OUTPUT_DIR="dist"
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR/alx"

# Publish CLI
echo ""
echo "Publishing CLI..."
dotnet publish src/ALX.CLI -c Release --self-contained false -o "$OUTPUT_DIR/alx"

# Copy examples
cp -r examples "$OUTPUT_DIR/alx/"

# Copy documentation
cp -r docs/site "$OUTPUT_DIR/alx/docs"

# Create alx launcher script
cat > "$OUTPUT_DIR/alx/alx" << 'EOF'
#!/bin/bash
dotnet "$(dirname "$0")/ALX.CLI.dll" "$@"
EOF
chmod +x "$OUTPUT_DIR/alx/alx"

echo ""
echo "Build complete!"
echo "Output: $OUTPUT_DIR/alx/"
echo ""
echo "To use ALX:"
echo "  1. Add to PATH: export PATH=\"\$PATH:\$(pwd)/$OUTPUT_DIR/alx\""
echo "  2. Run: alx version"
echo "  3. Run: alx hello.alx"
