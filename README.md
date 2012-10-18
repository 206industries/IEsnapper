IEsnapper
=========
Command line tool for taking a snapshot of an HTML page and saving it to a bitmap image

Download
--------
Download the latest binary package: [IEsnapper-v1.0.zip][1]

Usage
-----
**Quickstart**: `IEsnapper -u http://www.bing.com/ -o bing.png`

* **Usage**. Invoke with no parameters to see a usage options.
* **Dimensions**. Specify resulting image size with the `-w` and `-h` parameters
* **Input** The `-u` parameter specifies the URL to load
* **Output** The `-o` parameter specifies the image filename
* **Timers** Sane timeouts are used to wait for page load and page render. Override these with `-f` for the time to first byte and `-l` for rendering. Both timers are measured in milliseconds.
* **Logging** Use `-v` for more verbose output

[1]: https://github.com/downloads/andyoakley/IEsnapper/IEsnapper-1.0.zip