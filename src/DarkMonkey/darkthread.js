//Disabled
//Name:Darkthread Add DIV
//UrlMatch:^https://blog\.darkthread\.net
(function () {
    $('<div>Hacking Fun</div>')
        .css({
            'position': 'fixed',
            'top': '6px',
            'left': '6px',
            'color': 'yellow',
            'font-size': '16pt',
            'text-shadow': '1px 1px 1px black'
        })
        .appendTo("body");
})();