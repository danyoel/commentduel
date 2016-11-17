//
// Copyright 2016 Daniel J. Liebling <dan@liebling.org>
// MIT License: see ../LICENSE for full details.
//

var utils = require('utils');
var casper = require('casper').create({
    //verbose: true,
    //logLevel: "debug",
    viewportSize: { width: 1024, height: 768 },
    waitTimeout: 10000
});

var x = casper.selectXPath;

function getLinks() {
    var links = document.querySelectorAll('#documents > tbody > tr');
    return Array.prototype.map.call(links, function (row) {
        var link = row.querySelector('td a');
        return {
            "title": row.cells[1].innerText,
            "link": link ? link.getAttribute('href') : null,
            "type": row.cells[2].innerText,
            "captueDate": row.cells[4].innerText
        };
    });
}


// got to the DPD search page
casper.start('http://www.seattle.gov/dpd/toolsresources/default.htm', function () {
    this.waitForSelector('#searchBox');
    this.log('found #searchBox');
    this.sendKeys("#searchBox", casper.cli.args[0].toString()); ///"3020991");
    //this.capture('before-search.png');
    this.log('clicking search');
    this.click('#searchButton');
});


casper.waitUntilVisible('#permitSearchSlideOut', function () {
    this.log('popup present');
    this.click('#permitSearchSlideOut > div.modal-footer > p > a');
});

casper.waitForPopup(/Map\/detail\/default\.htm/);

casper.withPopup(/Map\/detail\/default\.htm/, function() {
    this.log('on map detail page');
    casper.viewport(1024, 768);
    //this.waitForSelector('#parcel-id-link'); // proxy for AJAX finishing load
    this.waitFor(function () {
        return this.evaluate(function () {
            var elem = document.querySelector('#parcel-id-link'); // proxy for AJAX finishing load
            var attr = elem ? elem.getAttribute('href') : null;
            return attr ? attr.length > 0 : false;
        });
    });
    //this.capture('mapdetail.png');
    this.click('#tabs li:nth-child(3) a');
});

casper.withPopup(/Map\/detail\/default\.htm/, function () {
    this.waitWhileVisible('#page-loading');
});

casper.withPopup(/Map\/detail\/default\.htm/, function () {
    this.waitForSelector('#documents > tbody > tr > td');
    this.capture('documents.png');
    var links = this.evaluate(getLinks);
    this.log('links obtained');
    utils.dump(links);
});
   

casper.run();