#!/bin/sh

# Define the output file
OUTPUT_FILE="all_files_content.txt"

# Initialize the output file
echo "File names/paths and their content" > $OUTPUT_FILE

# Find all .cs and .json files and process them
find . -type f \( -name "*.cs" -o -name "*.json" \) | while read -r FILE; do
  echo "Processing file: $FILE"
  # Print the file name/path
  echo "Filename: $FILE" >> $OUTPUT_FILE
  echo "-----------------------------------" >> $OUTPUT_FILE
  # Print the file content
  cat "$FILE" >> $OUTPUT_FILE
  # Add a separator for readability
  echo "\n\n" >> $OUTPUT_FILE
done

echo "All files and their contents have been written to $OUTPUT_FILE"

