define([
    "dojo/_base/declare",
    "dijit/_Widget",
    "dijit/_TemplatedMixin",

    "dojo/text!./Views/AuthorProperty.html",
    "dojo/dom",
    "dojo/domReady!"
],
    function (
        declare,
        _Widget,
        _TemplatedMixin,
        template,
        dom
    ) {
        return declare("Aloy11201/Views/AuthorProperty", [
            _Widget,
            _TemplatedMixin], {
            templateString: template,
            _onFirstNameChange: function (event) {
                if (!this.value) {
                    this.value = { firstName: '', lastName: '' };
                }
                this.value.firstName = event.target.value
                this._set('value', this.value);
                this.onChange(this.value);
            },
            _onLastNameChange: function (event) {
                if (!this.value) {
                    this.value = { firstName: '', lastName: '' };
                }
                this.value.lastName = event.target.value
                this._set('value', this.value);
                this.onChange(this.value);
            },
            _setValueAttr: function (val) {
                if (val) {
                    this.firstName.value = val.firstName;
                    this.lastName.value = val.lastName;
                    this._set('value', val);
                }
            },
            isValid: function () {
                return true;
            }
        }
        );
    });