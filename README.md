# DarkMonkey For IE

A simple GreaseMonkey/TamperMonkey-like tool for IE

[中文介紹](https://blog.darkthread.net/blog/darkmonkey-for-ie)

Every hacker need a tool to inject script to fix the web page which is not satisfying him.  Firefox has GreaseMonkey, Chrome has TamperMonkey, but IE has nothing.

I need to inject some script to IE-only web pages so I wrote my own toy, DarkMonkey for IE.

## How It Works

1. Save the script you want to inject as a .js file.  Add //Name: and //UrlMatch: (regular expression pattern) in the beginning of the script file.  "//Disabled" can mark the script as disabled.
2. When DarkMonkey starts, it loads all the scripts in same folder and show the list.
3. DarkMonkey detect and compare IE windows's url only when IE gets focus to reduce performance impact.  So the script will be injected only after you switch to IE.
4. The detecting interval is 1 second.
5. When the script is injected, the item in list will show yellow background for 1 second to highlight.
6. You can disable or enable script item in list.  Remember to disabled unnecessary scirpts to avoid computing waste.

### Demo

Demostration video: [YouTube](https://www.youtube.com/watch?v=2v9YZKgG9Wc)

Have fun!

