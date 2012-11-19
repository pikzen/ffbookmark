# ffbookmark
ffbookmark is a tool to convert Shaarli links to the format Firefox uses for its bookmarks.
It takes an HTML file exported from Shaarli and outputs a JSON file suitable for import in Firefox.
Link tags and descriptions are preserved!

## Requirements
ffbookmark needs HTMLAgilityPack to work. 
If you're running it on Mono, please install the ```libmono-system-runtime-serialization4.0-cil``` and ```libmono-system-xml4.0-cil``` packages.

##Usage
ffbookmark is a command-line program. Run it from windows command line prompt, or your favorite linux terminal emulator.

###On Windows
    ffbookmark.exe exported_shaarli_bookmarks_file.html

###On Linux
    mono ffbookmark.exe exported_shaarli_bookmarks_file.html

