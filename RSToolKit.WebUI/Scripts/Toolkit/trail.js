/// <reference path="b_toolkit.intellisense.js" />
var trail = (function (_my) {
    var _pheromoneDOM = document.getElementById('pheromone');
    _my.current = null;
    if (_pheromoneDOM !== null) {
        _my.current = JSON.parse(_pheromoneDOM.value);
    }
    _my.updatePheromone = function () {
        if (_my.current === null) {
            return;
        }
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/Trail/Put', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { RESTFUL.handleError(this); };
        xhr.onload = function () {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                _my.current = result.pheromone;
                var t_pheromone = document.getElementById(result.id);
                if (t_pheromone != null) {
                    t_pheromone.innerHTML = _my.current.label;
                    var t_parameters = '';
                    var t_paramFirst = true;
                    for (var key in _my.current.parameters) {
                        if (p.hasOwnProperty(key)) {
                            if (!t_paramFirst) {
                                t_parameters += '&';
                            } else {
                                t_paramFirst = false;
                            }
                            t_parameters += key + '=' + _my.current.parameters[key];
                        }
                    }
                    var t_href = window.location.origin + '/' + _my.current.controller + '/' + _my.current.action + '?' + t_parameters;
                    t_pheromone.setAttribute('href', t_href);
                }
            } else if (this.status === 500) {
                RESTFUL.showError(result.message);
            }
        };
        xhr.send(JSON.stringify(toolkit.addJsonAntiForgeryToken(_my.current)));
        xhr = null;
    };
    _my.setLabel = function (label) {
        if (_my.current !== null) {
            _my.current.label = label;
            _my.updatePheromone();
        }
    };
    return _my;
}(trail || {}));

var Pheromone = function () {
    this.action = 'Unknown';
    this.controller = 'Unknown';
    this.parameters = {};
    this.label = 'Unknown';
    this.actionDate = new moment();
    this.id = '';
};