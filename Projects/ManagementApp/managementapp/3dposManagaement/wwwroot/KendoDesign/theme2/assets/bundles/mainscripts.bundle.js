'use strict';
/**
 * @return {undefined}
 */
function initSparkline() {
    $(".sparkline").each(function () {
        var $this = $(this);
        $this.sparkline("html", $this.data());
    });
    $(".bh_visitors").sparkline("html", {
        type: "bar",
        height: "42px",
        barColor: "#a27ce6",
        barWidth: 5
    });
    $(".bh_visits").sparkline("html", {
        type: "bar",
        height: "42px",
        barColor: "#0078ff",
        barWidth: 5
    });
    $(".bh_chats").sparkline("html", {
        type: "bar",
        height: "42px",
        barColor: "#50d38a",
        barWidth: 5
    });
}
/**
 * @return {undefined}
 */
function skinChanger() {
    $(".choose-skin li").on("click", function () {
        var $body = $("body");
        var b = $(this);
        var existTheme = $(".choose-skin li.active").data("theme");
        $(".choose-skin li").removeClass("active");
        $body.removeClass("theme-" + existTheme);
        b.addClass("active");
        $body.addClass("theme-" + b.data("theme"));
    });
}
$(function () {
    skinChanger();
    initSparkline();
    setTimeout(function () {
        $(".page-loader-wrapper").fadeOut();
    }, 50);
}), $(document).ready(function () {
    $(".main-menu").metisMenu();
    $(".cwidget-scroll").slimScroll({
        height: "263px",
        wheelStep: 10,
        touchScrollStep: 50,
        color: "#efefef",
        size: "2px",
        borderRadius: "3px",
        alwaysVisible: false,
        position: "right"
    });
    $(".btn-toggle-fullwidth").on("click", function () {
        if ($("body").hasClass("layout-fullwidth")) {
            $("body").removeClass("layout-fullwidth");
            $(this).find(".fa").toggleClass("fa-arrow-left fa-arrow-right");
        } else {
            $("body").addClass("layout-fullwidth");
            $(this).find(".fa").toggleClass("fa-arrow-left fa-arrow-right");
        }
    });
    $(".btn-toggle-offcanvas").on("click", function () {
        $("body").toggleClass("offcanvas-active");
    });
    $("#main-content").on("click", function () {
        $("body").removeClass("offcanvas-active");
    });
    $(".dropdown").on("show.bs.dropdown", function () {
        $(this).find(".dropdown-menu").first().stop(true, true).animate({
            top: "100%"
        }, 200);
    });
    $(".dropdown").on("hide.bs.dropdown", function () {
        $(this).find(".dropdown-menu").first().stop(true, true).animate({
            top: "80%"
        }, 200);
    });
    $('.navbar-form.search-form input[type="text"]').on("focus", function () {
        $(this).animate({
            width: "+=50px"
        }, 300);
    }).on("focusout", function () {
        $(this).animate({
            width: "-=50px"
        }, 300);
    });
    if ($('[data-toggle="tooltip"]').length > 0) {
        $('[data-toggle="tooltip"]').tooltip();
    }
    if ($('[data-toggle="popover"]').length > 0) {
        $('[data-toggle="popover"]').popover();
    }
    $(window).on("load resize", function () {
        if ($(window).innerWidth() < 420) {
            $(".navbar-brand logo.svg").attr("src", "../assets/images/logo-icon.svg");
        } else {
            $(".navbar-brand logo-icon.svg").attr("src", "../assets/images/logo.svg");
        }
    });
    $(".rightbar_btn").on("click", function () {
        $("#rightbar").toggleClass("open");
    });
    $(".select-all").on("click", function () {
        if (this.checked) {
            $(this).parents("table").find(".checkbox-tick").each(function () {
                /** @type {boolean} */
                this.checked = true;
            });
        } else {
            $(this).parents("table").find(".checkbox-tick").each(function () {
                /** @type {boolean} */
                this.checked = false;
            });
        }
    });
    $(".checkbox-tick").on("click", function () {
        if ($(this).parents("table").find(".checkbox-tick:checked").length == $(this).parents("table").find(".checkbox-tick").length) {
            $(this).parents("table").find(".select-all").prop("checked", true);
        } else {
            $(this).parents("table").find(".select-all").prop("checked", false);
        }
    });
}), $.fn.clickToggle = function (func1, func2) {
    return this.each(function () {
        /** @type {boolean} */
        var done = false;
        $(this).bind("click", function () {
            return done ? (done = false, func2.apply(this, arguments)) : (done = true, func1.apply(this, arguments));
        });
    });
};
var Tawk_API = Tawk_API || {};
/** @type {!Date} */
var Tawk_LoadStart = new Date;
!function () {
    /** @type {!Element} */
    var script = document.createElement("script");
    /** @type {!Element} */
    var wafCss = document.getElementsByTagName("script")[0];
    /** @type {boolean} */
    script.async = true;
    /** @type {string} */
    script.src = "https://embed.tawk.to/5c6d4867f324050cfe342c69/default";
    /** @type {string} */
    script.charset = "UTF-8";
    script.setAttribute("crossorigin", "*");
    wafCss.parentNode.insertBefore(script, wafCss);
}();
