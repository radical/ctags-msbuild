# ctags-msbuild
ctags file generator for MSBuild files

Build:

OSX (this requires Mono 4.4.x+)

`$ xbuild`

Usage:

`$ ./bin/Debug/ctags-msbuild -h`

It generates `msb-tags` file for vim, by default.

In vim:

`:set tags=msb-tags`

Or to use it in addition to an existing file

`:set tags=tags,msb-tags`

