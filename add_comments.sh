#!/usr/bin/env bash
set -euo pipefail

# Function to add XML summary to C# public methods
add_csharp_summaries() {
  local file="$1"
  # Use perl to insert summary if not already present
  perl -i -pe '
    if (/^\s*public\s+(async\s+)?(\S+\s+)+(?<name>\w+)\s*\(.*\)\s*{/) {
      my $line = $_;
      # Check previous line for XML comment
      my $prev = $ARGV[0] // "";
      if ($prev !~ /\*\*\s*<summary>/) {
        my $indent = ("$line" =~ /^(\s*)/)[0];
        print "${indent}/// <summary>\n${indent}/// TODO: Describe the purpose of $+{name}.\n${indent}/// </summary>\n";
      }
    }
    $ARGV[0] = $_;
  ' "$file"
}

# Function to add block comments to TS/JS exported functions
add_js_comments() {
  local file="$1"
  perl -i -pe '
    if (/^\s*export\s+(const|function)\s+(\w+)/) {
      my $name = $2;
      my $prev = $ARGV[0] // "";
      if ($prev !~ /\/\*\*/ ) {
        my $indent = ("$._" =~ /^(\s*)/)[0];
        print "${indent}/**\n${indent} * TODO: Describe $name.\n${indent} */\n";
      }
    }
    $ARGV[0] = $_;
  ' "$file"
}

# Process C# files
find "$(pwd)/backend/src" -name "*.cs" | while read -r csfile; do
  add_csharp_summaries "$csfile"
done

# Process TS/JS files in frontend
find "$(pwd)/frontend" -type f \( -name "*.ts" -o -name "*.tsx" -o -name "*.js" \) | while read -r jsfile; do
  add_js_comments "$jsfile"
done

echo "Comments added."
